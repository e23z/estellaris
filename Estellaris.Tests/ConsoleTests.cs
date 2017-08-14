using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CommandLine;
using Estellaris.Core.Extensions;
using Estellaris.Terminal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Estellaris.Tests {
  internal class TestOptions : BaseOptions {
    [Option]
    public string OptionA { get; set; }
    [Option]
    public bool OptionB { get; set; }
  }

  [Verb("verb")]
  internal class VerbOptions : BaseOptions {
    [Option]
    public string OptionA { get; set; }
  }

  [TestClass]
  public class ConsoleTests {
    [TestMethod]
    public void TestProgressBar() {
      using (var progressBar = new ProgressBar()) {
        for (var i = 0; i < 11; i++) {
          var progressText = "";
          progressBar.Report(((i * 100f) / 10f) / 100f, out progressText);
          var totalDashes = 10 - i;
          var totalSharps = i;
          var validationPattern = $@"(\[#{{{totalSharps}}}\-{{{totalDashes}}}\]\s+{i * 10}%\s[\|/\-\\])";
          Assert.IsTrue(progressText.IsMatch(validationPattern));
        }
      }
    }

    [TestMethod]
    public void TestArgsParserWithVerb() {
      var verbArgs = new [] { "verb", "-e", "prod", "--optiona", "lorem ipsum" };
      var assembly = Assembly.Load(new AssemblyName("Estellaris.Tests"));
      ArgsParser.LoadOptionsFromAssembly(assembly);
      ArgsParser.Parse(verbArgs);
      var options = ArgsParser.GetParsed<VerbOptions>();
      Assert.AreEqual("prod", options.Environment);
      Assert.AreEqual("lorem ipsum", options.OptionA);
    }
    
    [TestMethod]
    public void TestArgsParserWithVerbAction() {
      var verbArgs = new [] { "verb", "-e", "prod", "--optiona", "lorem ipsum" };
      var assembly = Assembly.Load(new AssemblyName("Estellaris.Tests"));
      ArgsParser.LoadOptionsFromAssembly(assembly);
      ArgsParser.Parse(verbArgs);
      ArgsParser.Bind<VerbOptions>(o => {
        Assert.AreEqual("prod", o.Environment);
        Assert.AreEqual("lorem ipsum", o.OptionA);
      });
    }
    
    [TestMethod]
    public void TestArgsParser() {
      var args = new [] { "-e", "prod", "--optiona", "lorem ipsum", "--optionb" };
      var assembly = Assembly.Load(new AssemblyName("Estellaris.Tests"));
      ArgsParser.LoadOptionsFromAssembly(assembly);
      var options = ArgsParser.Parse<TestOptions>(args);
      Assert.AreEqual("prod", options.Environment);
      Assert.AreEqual("lorem ipsum", options.OptionA);
      Assert.IsTrue(options.OptionB);
    }
  }
}