using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace Estellaris.Web {
  public class WebRequest {
    public Action<WebResponse> Callback { get; set; }
    public HttpMethod Method { get; set; }
    public HttpContent Content { get; set; }
    public string Url { get; private set; }
    public int Attempts { get; private set; }
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    public WebRequest() { Method = HttpMethod.Get; }
    public WebRequest(string url) : this() { Url = url; }
    public WebRequest(string url, HttpContent content) : this(url) { Content = content; }
    public WebRequest(string url, HttpMethod method) : this(url) { Method = method; }
    public WebRequest(string url, HttpMethod method, HttpContent content) : this (url, method) {
      Content = content;
    }

    public void Fail() { Attempts++; }

    public static HttpContent FormContent(IDictionary<string, string> data) {
      return new FormUrlEncodedContent(data);
    }

    public static HttpContent JsonContent(object data) {
      return JsonContent(data, Encoding.UTF8);
    }

    public static HttpContent JsonContent(object data, Encoding encoding) {
      var json = JsonConvert.SerializeObject(data);
      var mime = "application/json";
      return new StringContent(json, Encoding.UTF8, mime);
    }
  }
}