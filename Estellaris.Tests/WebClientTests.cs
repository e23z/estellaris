using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Estellaris.Tests.Utils;
using Estellaris.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Estellaris.Tests {
  internal class MyPostData {
    public string Str { get; set; }
    public int Int { get; set; }
  }

  [TestClass]
  public class WebClientTests {
    static string _url { get; set; }
    static ApiServer _server { get; set; }


    [ClassInitialize]
    static public void Init(TestContext context) {
      var random = new Random();
      var port = random.Next(3000, 7000);
      _url = "http://localhost:" + port;
      _server = new ApiServer(port);
      _server.Start();
    }

    [ClassCleanup]
    static public void Clean() {
      _server.Dispose();
    }

    [TestMethod]
    public void TestMethods() {
      var data = new Dictionary<string, string> { { "MyKey", "MyValue" } };
      using (var client = new WebClient()) {
        var getHttpResponse = client.Get(_url);
        var getHttpData = getHttpResponse.ToString().Trim();
        var getMethod = getHttpResponse.Headers.GetValues("Method").FirstOrDefault();
        Assert.AreEqual("GET", getMethod);
        Assert.AreEqual("SUCCESS", getHttpData);

        var postContent = WebRequest.JsonContent(data);
        var postContentStr = postContent.ReadAsStringAsync().Result;
        var postHttpResponse = client.Post(_url, postContent);
        var postData = postHttpResponse.ToString().Trim();
        var postMethod = postHttpResponse.Headers.GetValues("Method").FirstOrDefault();
        Assert.AreEqual("POST", postMethod);
        Assert.AreEqual(postContentStr, postData);

        var putContent = WebRequest.FormContent(data);
        var putContentStr = putContent.ReadAsStringAsync().Result;
        var putHttpResponse = client.Put(_url, putContent);
        var putData = putHttpResponse.ToString().Trim();
        var putMethod = putHttpResponse.Headers.GetValues("Method").FirstOrDefault();
        Assert.AreEqual("PUT", putMethod);
        Assert.AreEqual(putContentStr, putData);

        var deleteResponse = client.Delete(_url);
        var deleteData = deleteResponse.ToString().Trim();
        var deleteMethod = deleteResponse.Headers.GetValues("Method").FirstOrDefault();
        Assert.AreEqual("DELETE", deleteMethod);
        Assert.AreEqual("SUCCESS", deleteData);
      }
    }

    [TestMethod]
    public void TestDeserialize() {
      var data = new MyPostData { Int = 1, Str = "Lorem Ipsum" };
      using (var client = new WebClient()) {
        var postContent = WebRequest.JsonContent(data);
        var postHttpResponse = client.Post(_url, postContent);
        var postData = postHttpResponse.Deserialize<MyPostData>();

        Assert.IsNotNull(postData);
        Assert.AreEqual(data.Int, postData.Int);
        Assert.AreEqual(data.Str, postData.Str);
      }
    }
  }
}