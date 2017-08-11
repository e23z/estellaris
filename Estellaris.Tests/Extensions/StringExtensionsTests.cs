using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Estellaris.Core.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Estellaris.Tests {
  [TestClass]
  public class StringExtensionsTests {
    [TestMethod]
    public void TestHexToBytes() {
      var comparisonBytes = new byte[] { 49, 50, 51, 52 };
      var bytesFromStr = "31323334".HexToBytes();
      Assert.IsTrue(bytesFromStr.SequenceEqual(comparisonBytes));
    }

    [TestMethod]
    public void TestToBytes() {
      var comparisonBytes = new byte[] { 116, 101, 115, 116 };
      var bytesFromStr = "test".ToBytes();
      Assert.IsTrue(bytesFromStr.SequenceEqual(comparisonBytes));
    }

    [TestMethod]
    public void TestFromBase64() {
      var comparisonBytes = new byte[] { 116, 101, 115, 116 };
      var bytesFromStr = "dGVzdA==".FromBase64();
      Assert.IsTrue(bytesFromStr.SequenceEqual(comparisonBytes));
    }
    
    [TestMethod]
    public void TestSubstitute() {
      var str = "My replaceable text";
      var replaced = str.Substitute(@"\bREPlaceable\b", "beautiful");
      var notReplaced = str.Substitute(@"\bREPlaceable\b", "beautiful", RegexOptions.None);
      Assert.AreEqual(replaced, "My beautiful text");
      Assert.AreEqual(notReplaced, str);
    }

    [TestMethod]
    public void TestTruncate() {
      var untruncated = "My huge text line to be truncated";
      var truncated = untruncated.Truncate(10);
      var truncatedEnding = untruncated.Truncate(10, "[...]");
      Assert.IsFalse(string.IsNullOrWhiteSpace(truncated));
      Assert.IsFalse(string.IsNullOrWhiteSpace(truncatedEnding));
      Assert.AreEqual("My huge te...", truncated);
      Assert.AreEqual("My huge te[...]", truncatedEnding);
    }
    
    [TestMethod]
    public void TestMatch() {
      var text = "It's a beautiful world full of Hello World's tests.";
      var match = text.Match(@"(\bbeautiful\b\s[a-z]+)");
      var nullMatch = text.Match("");
      Assert.IsNull(nullMatch);
      Assert.IsNotNull(match);
      Assert.AreEqual("beautiful world", match.Groups[1].Value);
    }
    
    [TestMethod]
    public void TestMatches() {
      var text = "It's a beautiful world full of Hello World's tests.";
      var matches = text.Matches(@"world");
      var emptyMatches = text.Matches("");
      Assert.IsTrue(emptyMatches.IsEmpty());
      Assert.IsFalse(matches.IsEmpty());
      Assert.AreEqual(2, matches.Count());
      Assert.AreEqual("world", matches.First().Value);
      Assert.AreEqual("World", matches.Last().Value);
    }
    
    [TestMethod]
    public void TestIsMatch() {
      var text = "It's a beautiful world full of Hello World's tests.";
      Assert.IsFalse(text.IsMatch(""));
      Assert.IsTrue(text.IsMatch(@"tests\.$"));
    }
    
    [TestMethod]
    public void TestToDateTime() {
      var culture = new CultureInfo("pt-BR");
      var date = "2017-02-01".ToDateTime();
      var localizedDate = "31/03/2017".ToDateTime(culture);
      var customFormat = "July, 02 2017".ToDateTime("MMMM, dd yyyy");
      var localizedCustomFormat = "20 de janeiro de 2017".ToDateTime("dd 'de' MMMM 'de' yyyy", culture);
      var invalidDate = "31/31/1254".ToDateTime();

      Assert.AreEqual(new DateTime(2017, 02, 01), date);
      Assert.AreEqual(new DateTime(2017, 03, 31), localizedDate);
      Assert.AreEqual(new DateTime(2017, 07, 02), customFormat);
      Assert.AreEqual(new DateTime(2017, 01, 20), localizedCustomFormat);
      Assert.AreEqual(new DateTime(), invalidDate);
    }
  }
}