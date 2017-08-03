using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Estellaris.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Estellaris.Tests {
  [TestClass]
  public class RandomizerTests {
    [TestMethod]
    public void TestRandomBytes() {
      var randomBytes = Randomizer.RandomBytes(5);
      Assert.IsTrue(randomBytes.Any());
      Assert.AreEqual(randomBytes.Length, 5);
    }

    [TestMethod]
    public void TestRandomHex() {
      var randomHex = Randomizer.RandomHex(6);
      Assert.IsNotNull(randomHex);
      Assert.AreNotEqual(randomHex, "");
      Assert.AreEqual(randomHex.Length, 12);
      Assert.IsFalse(Regex.IsMatch(randomHex, @"[^0-9a-f]", RegexOptions.IgnoreCase));
    }

    [TestMethod]
    public void TestRandomUniqueToken() {
      var tokens = new Collection<string>();
      for (var i = 0; i < 100; i++)
        tokens.Add(Randomizer.RandomUniqueToken());
      Assert.IsNotNull(tokens);
      Assert.AreEqual(tokens.Distinct().Count(), 100);
    }
  }
}