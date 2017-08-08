using System.IO;
using System.Security.Cryptography;
using Estellaris.Core.Extensions;
using Estellaris.Security.Utils;

namespace Estellaris.Security.Ciphers {
  public static class RSACipher {
    public static RSAKeyPair GenerateKeys(int keySize = 2048) {
      RSAKeyPair keys = null;
      using(var rsa = RSA.Create()) {
        rsa.KeySize = keySize;
        var privateKey = PEMExporter.Export(rsa, true);
        var publicKey = PEMExporter.Export(rsa, false);
        keys = new RSAKeyPair(privateKey, publicKey);
      }
      return keys;
    }

    public static string EncryptString(RSAKeyPair keys, string str) {
      var encryptedStr = "";
      try {
        using(var rsa = PEMLoader.GetRSAFromPEM(keys.PublicKey)) {
          var encryptedBytes = rsa.Encrypt(str.ToBytes(), RSAEncryptionPadding.Pkcs1);
          encryptedStr = encryptedBytes.ToBase64();
        }
      } catch { }
      return encryptedStr;
    }

    public static string DecryptString(RSAKeyPair keys, string encryptedStr) {
      var decryptedStr = "";
      try {
        using(var rsa = PEMLoader.GetRSAFromPEM(keys.PrivateKey)) {
          var encryptedBytes = encryptedStr.FromBase64();
          var decryptedBytes = rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.Pkcs1);
          decryptedStr = decryptedBytes.ToEncoding();
        }
      } catch { }
      return decryptedStr;
    }
  }
}