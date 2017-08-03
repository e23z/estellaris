using System;
using Estellaris.Security.Ciphers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Estellaris.Tests {
  [TestClass]
  public class BCryptTests {
    [TestMethod]
    public void TestHashValidity() {
      var strToHash = "My hash text";
      var hashed = BCryptCipher.Hash(strToHash);
      Assert.IsTrue(BCryptCipher.Equals(strToHash, hashed));
    }

    [TestMethod]
    public void TestHashFromAnotherPlatform() {
      var unhashed = "test from another platform";
      var hashed = "$2a$10$1VCl1KwOpTscBMM0MfgEdugbvMbwr5/ekb1t6lqO55mhhlYteqqD2";
      Assert.IsTrue(BCryptCipher.Equals(unhashed, hashed));
    }
    
    [TestMethod]
    public void TestInvalidHash() {
      var hashed = BCryptCipher.Hash("test");
      Assert.IsFalse(BCryptCipher.Equals("testing", hashed));
    }
  }
}