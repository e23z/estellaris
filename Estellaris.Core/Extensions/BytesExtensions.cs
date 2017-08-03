using System;
using System.Text;

namespace Estellaris.Core.Extensions {
  public static class BytesExtensions {
    public static string ToHex(this byte[ ] bytes) {
      return BitConverter.ToString(bytes).Replace("-", "");
    }

    public static string ToBase64(this byte[ ] bytes) {
      return Convert.ToBase64String(bytes);
    }

    public static string ToEncoding(this byte[ ] bytes, Encoding encoding = null) {
      if (encoding == null)
        encoding = Encoding.UTF8;
      return encoding.GetString(bytes);
    }
  }
}