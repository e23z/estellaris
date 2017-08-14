using CommandLine;

namespace Estellaris.Terminal {
  public class BaseOptions {
    [Option('e', "environment", Default = "dev")]
    public string Environment { get; set; }
  }
}