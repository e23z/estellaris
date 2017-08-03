using System;
using System.Text;

namespace Estellaris.Core.Extensions {
  public static class StringExtensions {
    public static byte[ ] HexToBytes(this string hex) {
      var bytes = new byte[hex.Length / 2];
      for (var i = 0; i < hex.Length; i += 2)
        bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
      return bytes;
    }

    public static byte[ ] ToBytes(this string str) {
      return Encoding.UTF8.GetBytes(str);
    }

    public static byte[ ] FromBase64(this string str) {
      return Convert.FromBase64String(str);
    }
  }
}