using Estellaris.Security.Ciphers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Estellaris.Tests {
  [TestClass]
  public class AESTests {
    [TestMethod]
    public void TestEncrypt() {
      var text = "lorem ipsum dolor siamet";
      var key = "MySecretKey";
      var encryptedText = AESCipher.EncryptString(key, text);
      var decryptedText = AESCipher.DecryptString(key, encryptedText);
      Assert.IsNotNull(encryptedText);
      Assert.IsNotNull(decryptedText);
      Assert.AreNotEqual(encryptedText, "");
      Assert.AreNotEqual(decryptedText, "");
      Assert.AreEqual(decryptedText, text);
    }

    [TestMethod]
    public void TestDecrypt() {
      var key = "MyPassword!1234";
      var text = "test of encrytion";
      var encryptedText = "VNGO76kMiac6VhO1PhMnlHfT0m7Je7zV4u4Pau68iG/0xuVZ7mugtqeiF4RMfGFx";
      var decryptedText = AESCipher.DecryptString(key, encryptedText);
      Assert.IsNotNull(decryptedText);
      Assert.AreNotEqual(decryptedText, "");
      Assert.AreEqual(decryptedText, text);
    }
  }
}