using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace Estellaris.Web {
  public class WebResponse {
    public readonly HttpStatusCode StatusCode;
    public readonly byte [] Data;
    public readonly WebRequest Request;
    public readonly HttpResponseHeaders Headers;

    public WebResponse(WebRequest request, HttpResponseHeaders headers, HttpStatusCode statusCode, byte [] data) {
      Request = request;
      Headers = headers;
      StatusCode = statusCode;
      Data = data;
    }

    override public string ToString() {
      return ToString(Request.Encoding, Encoding.UTF8);
    }

    public string ToString(Encoding encoding) {
      return ToString(Request.Encoding, encoding);
    }

    public string ToString(Encoding fromEncoding, Encoding toEncoding) {
      if (fromEncoding == null || toEncoding == null || Data == null)
        return "";

      var dataArr = Data;
      if (fromEncoding != toEncoding)
        dataArr = Encoding.Convert(fromEncoding, toEncoding, Data);
      return toEncoding.GetString(dataArr);
    }

    public T Deserialize<T>() {
      try { return JsonConvert.DeserializeObject<T>(ToString().Trim()); }
      catch { return default(T); }
    }
  }
}