using System.Security.Cryptography;
using Estellaris.Core.Extensions;

namespace Estellaris.Security.Ciphers {
  public static class SHACipher {
    public static string Hash512(string str) {
      using (var sha = SHA512.Create()) {
        var buffer = str.ToBytes();
        var hashed = sha.ComputeHash(buffer);
        return hashed.ToHex();
      }
    }
  }
}