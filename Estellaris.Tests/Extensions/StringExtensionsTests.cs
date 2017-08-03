using System.Linq;
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
  }
}