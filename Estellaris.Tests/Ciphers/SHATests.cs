using Estellaris.Security.Ciphers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Estellaris.Tests {
  [TestClass]
  public class SHATests {
    [TestMethod]
    public void TestHash() {
      var hashed = SHACipher.Hash512("test");
      Assert.IsNotNull(hashed);
      Assert.AreEqual(hashed, "EE26B0DD4AF7E749AA1A8EE3C10AE9923F618980772E473F8819A5D4940E0DB27AC185F8A0E1D5F84F88BC887FD67B143732C304CC5FA9AD8E6F57F50028A8FF");
    }
  }
}