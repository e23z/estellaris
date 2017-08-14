using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Estellaris.Core.Extensions;
using Estellaris.Terminal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Estellaris.Tests {
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
  }
}