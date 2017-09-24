using System;
using System.Linq;
using System.Threading;
using Estellaris.Tests.Utils;
using Estellaris.Web.Email;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Estellaris.Tests {
  [TestClass]
  public class MailerTests {
    [TestMethod]
    public void TestSend() {
      var random = new Random();
      var port = random.Next(3389, 20000);
      var to = new [] { "test.one@test.com", "test.two@test.com" };
      var from = "no-reply@teste.com";
      var subject = "Hello, Test!";
      var body = "<html><body>Hello, Unit Test!</body></html>";

      using (var server = new SmtpServer(port)) {
        server.Start();

        var credentials = new MailCredentials {
          Server = "localhost",
          Port = port,
          Username = "user",
          Password = "password",
          UseSsl = false
        };
        using (var mailer = new Mailer(credentials)) {
          mailer.SendMail(to, from, subject, body);
          Thread.Sleep(100);
        }

        var message = server.Messages.FirstOrDefault();
        Assert.IsNotNull(message);
        Assert.IsTrue(message.To.Contains(to[0]));
        Assert.IsTrue(message.To.Contains(to[1]));
        Assert.AreEqual(from, message.From);
        Assert.AreEqual(subject, message.Subject);
        Assert.AreEqual(body, message.Body.Trim());
      }
    }
  }
}