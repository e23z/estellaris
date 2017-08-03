using System;
using System.Linq;
using System.Security.Cryptography;
using Estellaris.Core.Extensions;

namespace Estellaris.Security.Ciphers {
  public static class AESCipher {
    public static string EncryptString(string key, string str) {
      string encryptedStr = null;
      ExecWithAES(key, (aes) => {
        using(var encryptor = aes.CreateEncryptor(aes.Key, aes.IV)) {
          var strBytes = str.ToBytes();
          var encryptedBytes = encryptor.TransformFinalBlock(strBytes, 0, strBytes.Length);
          encryptedStr = aes.IV.Concat(encryptedBytes).ToArray().ToBase64();
        }
      });
      return encryptedStr;
    }

    public static string DecryptString(string key, string encryptedStr) {
      var decryptedStr = "";
      ExecWithAES(key, (aes) => {
        var strBytes = encryptedStr.FromBase64();
        var iv = new byte[16];
        Array.Copy(strBytes, iv, 16);
        strBytes = strBytes.Skip(16).ToArray();
        aes.IV = iv;
        using(var decryptor = aes.CreateDecryptor(aes.Key, aes.IV)) {
          var decryptedBytes = decryptor.TransformFinalBlock(strBytes, 0, strBytes.Length);
          decryptedStr = decryptedBytes.ToEncoding();
        }
      });
      return decryptedStr;
    }

    static void ExecWithAES(string key, Action<Aes> action) {
      const int keySize = 32;

      if (key.Length < keySize)
        key += string.Join("", Enumerable.Repeat("A", keySize - key.Length));
      if (key.Length > keySize)
        key = key.Substring(0, keySize);

      using(var aes = Aes.Create()) {
        var keyBytes = key.ToBytes();

        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.KeySize = 256;
        aes.BlockSize = 128;
        aes.Key = keyBytes;

        action?.Invoke(aes);
      }
    }
  }
}