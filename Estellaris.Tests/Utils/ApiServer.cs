using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Estellaris.Core.Extensions;

namespace Estellaris.Web {
  public class ApiServerResponse {
    public HttpMethod Method { get; set; } = HttpMethod.Get;
    public string Route { get; set; } = "/";
    public string Body { get; set; } = "";
    public string MimeType { get; set; } = "text/plain";
  }

  public class ApiServer : IDisposable {
    const string MethodPattern = @"^(DELETE|GET|HEAD|OPTIONS|POST|PUT|TRACE)\s";
    const string ResponseKeyPattern = @"^((DELETE|GET|HEAD|OPTIONS|POST|PUT|TRACE)\s([a-z0-9\-_/]*))\sHTTPS?/1\.1";
    const string PayloadPattern = @"(\r\n){2}(.*)$";
    const string ContentTypePattern = @"Content\-Type:\s(.+?)\r\n";

    readonly int _port;
    readonly IDictionary<string, ApiServerResponse> _responses;

    TcpListener _listener { get; set; }

    public ApiServer(int port, IEnumerable<ApiServerResponse> responses = null) {
      _port = port;
      _responses = new Dictionary<string, ApiServerResponse>();
      if (responses != null)
        foreach (var response in responses) {
          var key = GetResponseKey(response);
          if (!_responses.ContainsKey(key))
            _responses.Add(key, response);
        }
    }

    public void Start() { ThreadPool.QueueUserWorkItem(state => StartListeningAsync(state).Wait()); }
    public void Dispose() { _listener.Stop(); }

    async Task StartListeningAsync(object state) {
      try {
        _listener = new TcpListener(IPAddress.Loopback, _port);
        _listener.Start();

        while (true) {
          using (var tcpClient = await _listener.AcceptTcpClientAsync()) {
            try {
              var request = await Read(tcpClient);

              if (request.Length <= 0)
                Close(tcpClient);
              else {
                var method = request.Match(MethodPattern).Groups [1].Value;
                var responseKey = request.Match(ResponseKeyPattern).Groups [1].Value;
                var contentType = request.Match(ContentTypePattern);
                var payload = request.Match(PayloadPattern).Groups [2].Value;
                var mimeType = contentType != null ? contentType.Groups [1].Value : "text/plain";
                var responseHeader = $"HTTP/1.1 200 OK\r\nServer: Api-Server\r\nDate:{DateTime.Now.ToString("R")}\r\nMethod: {method}\r\n";
                var responseBody = !string.IsNullOrWhiteSpace(payload) ? payload : "SUCCESS";

                if ((request.IsMatch("^(POST|PUT)") && string.IsNullOrWhiteSpace(payload)) || request.Contains("100-continue")) {
                  await Send(tcpClient, "HTTP/1.1 100 Continue\r\n");
                  responseBody = await Read(tcpClient);
                }

                if (_responses.ContainsKey(responseKey)) {
                  var response = _responses [responseKey];
                  mimeType = response.MimeType;
                  responseBody = response.Body;
                }

                responseHeader += $"Content-Type: {mimeType}\r\nConnection: close\r\nContent-Length: {responseBody.Length}\r\n\r\n";
                await SendAndClose(tcpClient, responseHeader + responseBody);
              }
            }
            catch (Exception ex) {
              Console.WriteLine(ex);
              break;
            }
          }
        }
      }
      catch (Exception ex) {
        Console.WriteLine(ex);
      }
    }

    async Task Send(TcpClient client, string message) {
      var stream = client.GetStream();
      var buffer = Encoding.UTF8.GetBytes(message + "\r\n");
      await stream.WriteAsync(buffer, 0, buffer.Length);
      await stream.FlushAsync();
    }

    async Task<string> Read(TcpClient client) {
      var bytes = new byte [1024 * 1024 * 4];
      var readBytes = 0;
      var stream = client.GetStream();
      readBytes = await stream.ReadAsync(bytes, 0, bytes.Length);
      return Encoding.UTF8.GetString(bytes, 0, readBytes);
    }

    async Task SendAndClose(TcpClient tcpClient, string response) {
      await Send(tcpClient, response);
      Close(tcpClient);
    }

    void Close(TcpClient tcpClient) {
      tcpClient.Client.Shutdown(SocketShutdown.Both);
      tcpClient.Client.Dispose();
    }

    string GetResponseKey(ApiServerResponse response) {
      return response.Method.ToString().ToUpper() + " " + response.Route;
    }
  }
}