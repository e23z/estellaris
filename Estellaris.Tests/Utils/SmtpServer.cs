using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Estellaris.Tests.Utils {
  public class MailMessage {
    public string From { get; set; }
    public string Date { get; set; }
    public string Subject { get; set; }
    public string To { get; set; }
    public string Body { get; set; }
    public string Sender { get; set; }
  }

  public interface ISmtpServer : IDisposable {
    void Start();
    IList<MailMessage> Messages { get; }
  }

  // REFS:
  // https://github.com/thesheps/selfishsmtp
  // https://github.com/mczachurski/MailRetriever
  public class SmtpServer : ISmtpServer {
    readonly int _port;

    IList<MailMessage> _messages = new List<MailMessage>();
    TcpListener _listener;

    public IList<MailMessage> Messages {
      get { lock (_messages) { return _messages; } }
    }

    public SmtpServer(int port) {
      _port = port;
    }

    public void Start() {
      ThreadPool.QueueUserWorkItem(StartListening);
    }

    public void Dispose() {
      ReleaseUnmanagedResources();
      GC.SuppressFinalize(this);
    }

    ~SmtpServer() {
      ReleaseUnmanagedResources();
    }

    void StartListening(object state) {
      StartListeningAsync(state).Wait();
    }

    async Task StartListeningAsync(object state) {
      try {
        _listener = new TcpListener(IPAddress.Loopback, _port);
        _listener.Start();

        var client = await _listener.AcceptTcpClientAsync();

        Write(client, "220 localhost -- Smtp Server");

        while (true) {
          try {
            var data = Read(client);

            if (data.Length <= 0)
              continue;

            if (data.StartsWith("QUIT")) {
              client.Client.Dispose();
              break;
            }

            if (data.StartsWith("EHLO"))
              Write(client, "250 AUTH=PLAIN LOGIN");
            
            if (data.StartsWith("HELLO"))
              Write(client, "250 AUTH=PLAIN LOGIN");
            
            if (data.StartsWith("AUTH PLAIN"))
              Write(client, "235 Authentication successful");

            if (data.StartsWith("RCPT TO"))
              Write(client, "250 OK");

            if (data.StartsWith("MAIL FROM"))
              Write(client, "250 OK");

            if (!data.StartsWith("DATA"))
              continue;

            lock (_messages) {
              Write(client, "354 Start mail input; end with");
              data = Read(client);

              var message = data.Split(new [] { Environment.NewLine }, StringSplitOptions.None);
              Write(client, "250 OK");

              data = Read(client);

              _messages.Add(new MailMessage {
                From = ParseField(message, "From"),
                Date = ParseField(message, "Date"),
                Subject = ParseField(message, "Subject"),
                To = ParseField(message, "To"),
                Sender = ParseField(message, "Sender"),
                Body = data.Replace(Environment.NewLine, string.Empty)
              });
            }
          }
          catch (Exception) {
            break;
          }
        }
      }
      catch (Exception ex) {
        Console.WriteLine(ex.Message);
      }
    }

    static string ParseField(IEnumerable<string> data, string field) {
      return data.FirstOrDefault(d => d.Contains($"{field}:"))?.Replace($"{field}:", string.Empty).Trim();
    }

    static void Write(TcpClient client, string strMessage) {
      var clientStream = client.GetStream();
      var encoder = new ASCIIEncoding();
      var buffer = encoder.GetBytes(strMessage + "\r\n");

      clientStream.Write(buffer, 0, buffer.Length);
      clientStream.Flush();
    }

    static string Read(TcpClient client) {
      var messageBytes = new byte [8192];
      var bytesRead = 0;
      var clientStream = client.GetStream();
      var encoder = new ASCIIEncoding();
      bytesRead = clientStream.Read(messageBytes, 0, 8192);

      var strMessage = encoder.GetString(messageBytes, 0, bytesRead);

      return strMessage;
    }

    void ReleaseUnmanagedResources() {
      _listener.Stop();
    }
  }
}