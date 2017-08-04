using Microsoft.Extensions.Logging;

namespace Estellaris.EF.Logging {
  internal class EFLoggerProvider : ILoggerProvider {
    public ILogger CreateLogger(string categoryName) { return new EFLogger(); }
    public void Dispose() { }
  }
}