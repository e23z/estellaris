using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Estellaris.Web {
  public class WebClient : IDisposable {
    readonly HttpClient _httpClient;
    public int MaxAttempts { get; set; } = 5;
    public HttpRequestHeaders Headers => _httpClient.DefaultRequestHeaders;

    public WebClient(int maxConnections = 5, TimeSpan? timeout = null, string userAgent = null) {
      _httpClient = new HttpClient(new HttpClientHandler {
        CookieContainer = new CookieContainer(),
        MaxConnectionsPerServer = maxConnections,
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
        Proxy = null,
        UseProxy = false,
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }
      }) { Timeout = timeout.HasValue ? timeout.Value : TimeSpan.FromMinutes(1) };

      if (!string.IsNullOrEmpty(userAgent))
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
      _httpClient.DefaultRequestHeaders.Pragma.ParseAdd("no-cache");
    }

    public void Dispose() { _httpClient.Dispose(); }

    public WebResponse Get(string url) {
      return GetAsync(url).Result;
    }

    public async Task<WebResponse> GetAsync(string url) {
      return await Fetch(new WebRequest(url));
    }

    public WebResponse Post(string url, HttpContent content) {
      return PostAsync(url, content).Result;
    }

    public async Task<WebResponse> PostAsync(string url, HttpContent content) {
      return await Fetch(new WebRequest(url) {
        Method = HttpMethod.Post,
        Content = content
      });
    }
    
    public WebResponse Put(string url, HttpContent content) {
      return PutAsync(url, content).Result;
    }

    public async Task<WebResponse> PutAsync(string url, HttpContent content) {
      return await Fetch(new WebRequest(url) {
        Method = HttpMethod.Put,
        Content = content
      });
    }
    
    public WebResponse Delete(string url) {
      return DeleteAsync(url).Result;
    }

    public async Task<WebResponse> DeleteAsync(string url) {
      return await Fetch(new WebRequest(url) { Method = HttpMethod.Delete });
    }

    public void CancelPendingRequests() {
      _httpClient.CancelPendingRequests();
    }

    async Task<WebResponse> Fetch(WebRequest request) {
      var response = new WebResponse(request, null, 0, null);

      try {
        using (var httpRequest = new HttpRequestMessage(request.Method, request.Url)) {
          if (request.Method != HttpMethod.Get)
            httpRequest.Content = request.Content;

          using (var httpResponse = await _httpClient.SendAsync(httpRequest)) {
            if (httpResponse.StatusCode != HttpStatusCode.OK)
              throw new HttpRequestException();

            var httpData = await httpResponse.Content.ReadAsByteArrayAsync();
            response = new WebResponse(request, httpResponse.Headers, HttpStatusCode.OK, httpData);
          }
        }
      }
      catch (Exception ex) {
        Console.WriteLine(ex);
        request.Fail();
        if (request.Attempts <= MaxAttempts)
          return await Fetch(request);
      }

      request.Callback?.Invoke(response);
      return response;
    }
  }
}