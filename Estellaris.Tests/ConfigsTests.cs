using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Estellaris.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Estellaris.Tests {
  [TestClass]
  public class ConfigsTests {
    [TestMethod]
    public void TestEnvironment() {
      Configs.Init(environment: "invalid");
      Assert.IsNull(Configs.GetValue<string>("EnvName"));
      Configs.Dispose();

      Configs.Init(environment: "dev");
      Assert.AreEqual("Dev", Configs.GetValue<string>("EnvName"));
      Configs.Dispose();

      Configs.Init(environment: "hml");
      Assert.AreEqual("Hml", Configs.GetValue<string>("EnvName"));
      Configs.Dispose();
    }

    [TestMethod]
    public void TestBasePath() {
      var executingPath = Directory.GetCurrentDirectory();
      Configs.Init();
      Assert.AreEqual(executingPath, Configs.BasePath);
      Assert.AreEqual("OldValue", Configs.GetValue<string>("BasePathDiff"));
      Configs.Dispose();

      var newBasePath = Path.Combine(Directory.GetCurrentDirectory(), "Test");
      var appsettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
      var appSettings = File.ReadAllText(appsettingsPath).Replace("OldValue", "NewValue");
      Directory.CreateDirectory(newBasePath);
      File.WriteAllText(appsettingsPath.Replace("appsettings", "Test/appsettings"), appSettings);
      Configs.Init(basePath : newBasePath);
      Assert.AreEqual(newBasePath, Configs.BasePath);
      Assert.AreEqual("Hello World!", Configs.GetValue<string>("GlobalKey"));
      Assert.AreEqual("NewValue", Configs.GetValue<string>("BasePathDiff"));
      Directory.Delete(newBasePath, true);
      Configs.Dispose();
    }

    [TestMethod]
    public void TestInMemoryData() {
      Configs.Init();
      Assert.IsNull(Configs.GetValue<string>("InMemoryKey"));
      Configs.Dispose();

      var memoryConfigs = new Dictionary<string, string>();
      memoryConfigs.Add("InMemoryKey", "MemoryValue");
      Configs.Init(inMemoryConfigs : memoryConfigs);
      Assert.AreEqual("MemoryValue", Configs.GetValue<string>("InMemoryKey"));
      Configs.Dispose();
    }

    [TestMethod]
    public void TestConfigReload() {
      Configs.Init();
      var oldValue = Configs.GetValue<string>("Reloaded");
      var appsettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
      var appsettings = File.ReadAllText(appsettingsPath);
      var pattern = "\"Reloaded\":\\s\"(.+?)\"";
      var replaceValue = Regex.Match(appsettings, pattern).Groups[1].Value;
      var guid = Guid.NewGuid().ToString("N");
      appsettings = appsettings.Replace(replaceValue, guid);
      File.WriteAllText(appsettingsPath, appsettings);
      Thread.Sleep(1000); // force wait to system detect changes
      var newValue = Configs.GetValue<string>("Reloaded");
      Assert.AreNotEqual(oldValue, newValue);
      Assert.AreEqual(newValue, guid);
      Configs.Dispose();
    }

    [TestMethod]
    public void TestOverride() {
      Configs.Init();
      Assert.AreEqual("GlobalValue", Configs.GetValue<string>("Overridden"));
      Configs.Dispose();
    }

    [TestMethod]
    public void TestGetValue() {
      Assert.AreEqual(10, Configs.GetValue<int>("IntValue"));
      Assert.IsTrue(Configs.GetValue<bool>("BoolValue"));
      Assert.AreEqual("Test", Configs.GetValue<string>("StringValue"));
      Assert.AreEqual(new DateTime(2017, 01, 02, 12, 13, 14),
        Configs.GetValue<DateTime>("DateTimeValue"));
      Configs.Dispose();
    }

    [TestMethod]
    public void TestInvalidGetValue() {
      Assert.IsNull(Configs.GetValue<string>("InvalidKey"));
      Assert.IsFalse(Configs.GetValue<bool>("InvalidKey"));
      Configs.Dispose();
    }

    [TestMethod]
    public void TestGetValueOrDefault() {
      var defaultDate = new DateTime(2017, 01, 01);

      Assert.AreEqual(10, Configs.GetValueOrDefault<int>("IntValue", 0));
      Assert.IsTrue(Configs.GetValueOrDefault<bool>("BoolValue", false));
      Assert.AreEqual("Test", Configs.GetValueOrDefault<string>("StringValue", "default"));
      Assert.AreEqual(new DateTime(2017, 01, 02, 12, 13, 14),
        Configs.GetValueOrDefault<DateTime>("DateTimeValue", defaultDate));

      Assert.AreEqual(5, Configs.GetValueOrDefault<int>("InvalidKey", 5));
      Assert.IsTrue(Configs.GetValueOrDefault<bool>("InvalidKey", true));
      Assert.AreEqual("default", Configs.GetValueOrDefault<string>("InvalidKey", "default"));
      Assert.AreEqual(defaultDate, Configs.GetValueOrDefault<DateTime>("InvalidKey", defaultDate));
      Configs.Dispose();
    }

    [TestMethod]
    public void TestGetArray() {
      var intArray = new [] { 1, 2, 3 };
      var strArray = new [] { "Value1", "Value2" };
      Assert.IsTrue(intArray.SequenceEqual(Configs.GetArray<int>("IntArray")));
      Assert.IsTrue(strArray.SequenceEqual(Configs.GetArray<string>("StringArray")));
      Configs.Dispose();
    }

    [TestMethod]
    public void TestGetConnectionString() {
      var _default = @"Server=(localdb)\mssqllocaldb;Database=EFProviders.InMemory;Trusted_Connection=True;";
      var mysql = "Server=localhost;Port=3306;Database=mydatabase-dev;Uid=root;Pwd=root;";
      Assert.AreEqual(_default, Configs.GetConnectionString());
      Assert.AreEqual(mysql, Configs.GetConnectionString("MySql"));
      Configs.Dispose();
    }

    [TestMethod]
    public void TestGetDirectory() {
      var dirpath = Path.Combine(Directory.GetCurrentDirectory(), "TestDir");
      if (Directory.Exists(dirpath))
        Directory.Delete(dirpath, recursive : true);
      var dir = Configs.GetDirectory("DirName");
      Assert.IsNull(Configs.GetDirectory(""));
      Assert.AreEqual(new DirectoryInfo(dirpath).FullName, dir.FullName);
      Assert.IsTrue(Directory.Exists(dirpath));
      var relativeBasePath = Path.Combine(dirpath, "TestDir");
      var relativeDirPath = Path.Combine(relativeBasePath, "TestDir");
      var relativeDir = Configs.GetDirectory("DirName", basePath : relativeBasePath);
      Assert.AreEqual(new DirectoryInfo(relativeDirPath).FullName, relativeDir.FullName);
      Assert.IsTrue(Directory.Exists(relativeDirPath));
      Configs.Dispose();
    }

    internal class MyOptions {
      public int IntValue { get; set; }
      public bool BoolValue { get; set; }
      public string StringValue { get; set; }
      public DateTime DateTimeValue { get; set; }
      public IEnumerable<string> StringArray { get; set; }
      public int[] IntArray { get; set; }
      public MySubOptions MySubOptions { get; set; }
    }

    internal class MySubOptions {
      public string Option1 { get; set; }
      public string Option2 { get; set; }
      public string Option3 { get; set; }
    }

    [TestMethod]
    public void TestBinding() {
      var intArray = new [] { 1, 2, 3 };
      var strArray = new [] { "Value1", "Value2" };
      var model = Configs.BindTo<MyOptions>("MyOptions");
      Assert.AreEqual(model.IntValue, 10);
      Assert.IsTrue(model.BoolValue);
      Assert.AreEqual(model.StringValue, "Test");
      Assert.AreEqual(model.DateTimeValue, new DateTime(2017, 01, 02, 12, 13, 14));
      Assert.IsNotNull(model.StringArray);
      Assert.AreEqual(model.StringArray.Count(), 2);
      Assert.IsTrue(strArray.SequenceEqual(model.StringArray));
      Assert.IsNotNull(model.IntArray);
      Assert.AreEqual(model.IntArray.Count(), 3);
      Assert.IsTrue(intArray.SequenceEqual(model.IntArray));
      Assert.IsNotNull(model.MySubOptions);
      Assert.AreEqual(model.MySubOptions.Option1, "Lorem");
      Assert.AreEqual(model.MySubOptions.Option2, "Ipsum");
      Assert.IsNull(model.MySubOptions.Option3);
    }

    [TestMethod]
    public void TestCulture() {
      Assert.AreEqual("iv", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
      Assert.IsNull(CultureInfo.DefaultThreadCurrentCulture);
      Configs.SetGlobalCulture(new CultureInfo("pt-BR"));
      Assert.IsNotNull(CultureInfo.DefaultThreadCurrentCulture);
      Assert.AreEqual("pt", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
      Assert.AreEqual("pt", CultureInfo.DefaultThreadCurrentCulture.TwoLetterISOLanguageName);
      Configs.SetGlobalCulture(new CultureInfo("iv"));
    }
  }
}