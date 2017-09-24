using System;
using System.Collections.Generic;
using MailKit.Net.Smtp;
using MimeKit;

namespace Estellaris.Web.Email {
  public class Mailer : IDisposable {
    readonly MailCredentials _credentials;
    readonly SmtpClient _smtpClient;

    public Mailer(MailCredentials credentials) {
      _credentials = credentials;
      _smtpClient = new SmtpClient();
      _smtpClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
      _smtpClient.Connect(_credentials.Server, _credentials.Port, _credentials.UseSsl);
      _smtpClient.AuthenticationMechanisms.Remove("XOAUTH2");
      _smtpClient.Authenticate(_credentials.Username, _credentials.Password);
    }

    public void Dispose() {
      _smtpClient.Disconnect(true);
      _smtpClient.Dispose();
    }

    public bool SendMail(IEnumerable<string> to, string from, string subject, string body, MailType type = MailType.Html) {
      var message = new MimeMessage {
        Subject = subject,
        Body = new TextPart("html") { Text = body }
      };
      message.From.Add(new MailboxAddress("", from));
      foreach (var email in to)
        message.To.Add(new MailboxAddress("", email));

      try {
        _smtpClient.Send(message);
        return true;
      }
      catch { return false; }
    }

    public bool SendPlain(IEnumerable<string> to, string from, string subject, string body) {
      return SendMail(to, from, subject, body, MailType.PlainText);
    }
    
    public bool SendPlain(string to, string from, string subject, string body) {
      return SendMail(new [] { to }, from, subject, body, MailType.PlainText);
    }
    
    public bool SendHtml(IEnumerable<string> to, string from, string subject, string body) {
      return SendMail(to, from, subject, body, MailType.Html);
    }
    
    public bool SendHtml(string to, string from, string subject, string body) {
      return SendMail(new [] { to }, from, subject, body, MailType.Html);
    }

    string GetSubtype(MailType type) {
      switch (type) {
        case MailType.Html:
          return "html";
        default:
          return "plain";
      }
    }
  }
}
