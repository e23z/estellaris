using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
      if (string.IsNullOrWhiteSpace(pattern) || string.IsNullOrWhiteSpace(replacement))
        return input;
      return Regex.Replace(input, pattern, replacement, options);
    }

    public static Match Match(this string input, string pattern) {
      return input.Match(pattern, _options);
    }

    public static Match Match(this string input, string pattern, RegexOptions options) {
      if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(pattern))
        return null;
      return Regex.Match(input, pattern, options);
    }

    public static IEnumerable<Match> Matches(this string input, string pattern) {
      return input.Matches(pattern, _options);
    }

    public static IEnumerable<Match> Matches(this string input, string pattern, RegexOptions options) {
      if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(pattern))
        return Enumerable.Empty<Match>();
      return Regex.Matches(input, pattern, options).OfType<Match>();
    }

    public static bool IsMatch(this string input, string pattern) {
      return input.IsMatch(pattern, _options);
    }

    public static bool IsMatch(this string input, string pattern, RegexOptions options) {
      if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(pattern))
        return false;
      return Regex.IsMatch(input, pattern, options);
    }

    public static string Truncate(this string input, int maxLength, string ending = "...") {
      if (string.IsNullOrWhiteSpace(input) || input.Length <= maxLength)
        return input;
      return input.Substring(0, maxLength) + ending;
    }

    public static DateTime ToDateTime(this string str, CultureInfo culture = null) {
      return str.ToDateTime("", culture);
    }

    public static DateTime ToDateTime(this string str, string format, CultureInfo culture = null) {
      if (culture == null)
        culture = CultureInfo.CurrentCulture;

      var date = new DateTime();

      if (string.IsNullOrWhiteSpace(format))
        DateTime.TryParse(str, culture, DateTimeStyles.None, out date);
      else
        DateTime.TryParseExact(str, format, culture, DateTimeStyles.None, out date);

      return date;
    }
  }
}