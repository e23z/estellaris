using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;

namespace Estellaris.Core {
  public class Configs {
    static bool _isLoaded { get; set; }
    public static IConfigurationRoot Configuration { get; private set; }
    public static string Environment { get; protected set; }
    public static string BasePath { get; protected set; }

    public static void Init(string environment = null, string basePath = null, IDictionary<string, string> inMemoryConfigs = null) {
      var builder = new ConfigurationBuilder();
      var memoryProvider = new MemoryConfigurationProvider(new MemoryConfigurationSource());

      if (inMemoryConfigs != null) {
        foreach(var config in inMemoryConfigs) {
          memoryProvider.Add(config.Key, config.Value);
        }
      }

      Environment = !string.IsNullOrWhiteSpace(environment) ? environment : "dev";
      BasePath = !string.IsNullOrWhiteSpace(basePath) ? basePath : Directory.GetCurrentDirectory();

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
      return EqualityComparer<T>.Default.Equals(value, default(T)) ? _default : value;
    }

    public static IEnumerable<T> GetArray<T>(string key) {
      CheckConfiguration();
      var section = Configuration.GetSection(key);
      return GetArrayOfElements<T>(section);
    }

    public static string GetConnectionString(string name = "Default") {
      CheckConfiguration();
      return Configuration.GetConnectionString(name);
    }

    public static DirectoryInfo GetDirectory(string key, string basePath = null) {
      CheckConfiguration();
      if (string.IsNullOrWhiteSpace(basePath))
        basePath = Directory.GetCurrentDirectory();

      var dirname = Configuration[key];

      if (string.IsNullOrWhiteSpace(dirname))
        return null;

      var localPath = Path.Combine(basePath, dirname);
      return !Directory.Exists(localPath) ? Directory.CreateDirectory(localPath) : new DirectoryInfo(localPath);
    }

    public static T BindTo<T>(string key) {
      CheckConfiguration();
      var model = Activator.CreateInstance<T>();
      Configuration.GetSection(key).Bind(model);
      return model;
    }

    static IEnumerable<T> GetArrayOfElements<T>(IConfigurationSection section) {
      CheckConfiguration();
      if (section == null)
        return Enumerable.Empty<T>();
      return section
        .GetChildren()
        .Select(_ =>(T) Convert.ChangeType(_.Value, typeof (T)))
        .ToList();
    }

    public static void Dispose() {
      _isLoaded = false;
      Configuration = null;
    }

    static void CheckConfiguration() {
      if (Configuration == null)
        Init();
    }
  }
}