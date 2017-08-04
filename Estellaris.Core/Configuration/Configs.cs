using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;

namespace Estellaris.Core {
  public class Configs {
    static bool _isLoaded { get; set; }
    public static IConfigurationRoot Configuration { get; private set; }
    public static string Environment { get; protected set; } = "dev";
    public static string BasePath { get; protected set; } = Directory.GetCurrentDirectory();

    public static void Init(string environment = null, string basePath = null, IDictionary<string, string> inMemoryConfigs = null) {
      var builder = new ConfigurationBuilder();
      var memoryProvider = new MemoryConfigurationProvider(new MemoryConfigurationSource());

      if (inMemoryConfigs != null) {
        foreach(var config in inMemoryConfigs) {
          memoryProvider.Add(config.Key, config.Value);
        }
      }

      if (!string.IsNullOrWhiteSpace(environment))
        Environment = environment;

      if (!string.IsNullOrWhiteSpace(basePath))
        BasePath = basePath;

      builder.SetBasePath(BasePath);
      builder.AddInMemoryCollection(memoryProvider);
      builder.AddJsonFile($"appsettings.{Environment}.json", optional : true, reloadOnChange : true);
      builder.AddJsonFile($"appsettings.json", optional : true, reloadOnChange : true);
      Configuration = builder.Build();
    }

    public static T GetValue<T>(string key) {
      CheckConfiguration();
      var value = Configuration[key];
      return value != null ? (T) Convert.ChangeType(value, typeof (T)) : default(T);
    }

    public static T GetValueOrDefault<T>(string key, T _default) {
      CheckConfiguration();
      var value = GetValue<T>(key);
      return value != null ? value : _default;
    }

    public static IEnumerable<T> GetArray<T>(string key) {
      CheckConfiguration();
      return Configuration.GetSection(key).GetChildren().Select(_ => _.Value).OfType<T>();
    }

    public static string GetConnectionString(string name = "Default") {
      CheckConfiguration();
      return Configuration.GetConnectionString(name);
    }

    public static DirectoryInfo GetDirectory(string key, string basePath = null) {
      CheckConfiguration();
      if (string.IsNullOrWhiteSpace(basePath))
        basePath = Directory.GetCurrentDirectory();

      var localPath = Path.Combine(basePath, Configuration[key]);
      return !Directory.Exists(localPath) ? Directory.CreateDirectory(localPath) : new DirectoryInfo(localPath);
    }

    static void CheckConfiguration() {
      if (Configuration == null)
        Init();
    }
  }
}