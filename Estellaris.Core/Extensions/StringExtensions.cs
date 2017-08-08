using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Estellaris.Core.Extensions {
  public static class StringExtensions {
    static readonly RegexOptions _options = RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline;

    public static byte[] HexToBytes(this string hex) {
      var bytes = new byte[hex.Length / 2];
      for (var i = 0; i < hex.Length; i += 2)
        bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
      return bytes;
    }

    public static byte[] ToBytes(this string str) {
      return Encoding.UTF8.GetBytes(str);
    }

    public static byte[] FromBase64(this string str) {
      return Convert.FromBase64String(str);
    }

    public static string Substitute(this string input, string pattern, string replacement) {
      return input.Substitute(pattern, replacement, _options);
    }

    public static string Substitute(this string input, string pattern, string replacement, RegexOptions options) {
      return Regex.Replace(input, pattern, replacement, options);
    }
  }
}