using Estellaris.Core.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Estellaris.Tests {
  [TestClass]
  public class BytesExtensionsTests {
    [TestMethod]
    public void TestToHex() {
      var hex = new byte[] { 49, 50, 51, 52 }.ToHex();
      Assert.AreEqual("31323334", hex);
    }

    [TestMethod]
    public void TestToBytes() {
      var utf8Text = new byte[] { 116, 101, 115, 116 }.ToEncoding();
      Assert.AreEqual("test", utf8Text);
    }

    [TestMethod]
    public void TestToBase64() {
      var base64 = new byte[] { 116, 101, 115, 116 }.ToBase64();
      Assert.AreEqual("dGVzdA==", base64);
    }
  }
}