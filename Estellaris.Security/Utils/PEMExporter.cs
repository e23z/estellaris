using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Estellaris.Security.Utils {
  internal static class PEMExporter {
    public static string Export(RSA rsa, bool exportPrivateKey) {
      var parameters = rsa.ExportParameters(includePrivateParameters : exportPrivateKey);
      var key = new StringBuilder();
      using(var stream = new MemoryStream()) {
        var writer = new BinaryWriter(stream);
        writer.Write((byte) 0x30); // SEQUENCE
        using(var innerStream = new MemoryStream()) {
          var innerWriter = new BinaryWriter(innerStream);
          EncodeIntegerBigEndian(innerWriter, new byte[] { 0x00 }); // Version
          EncodeIntegerBigEndian(innerWriter, parameters.Modulus);
          EncodeIntegerBigEndian(innerWriter, parameters.Exponent);
          EncodeIntegerBigEndian(innerWriter, exportPrivateKey ? parameters.D : parameters.Exponent);
          EncodeIntegerBigEndian(innerWriter, exportPrivateKey ? parameters.P : parameters.Exponent);
          EncodeIntegerBigEndian(innerWriter, exportPrivateKey ? parameters.Q : parameters.Exponent);
          EncodeIntegerBigEndian(innerWriter, exportPrivateKey ? parameters.DP : parameters.Exponent);
          EncodeIntegerBigEndian(innerWriter, exportPrivateKey ? parameters.DQ : parameters.Exponent);
          EncodeIntegerBigEndian(innerWriter, exportPrivateKey ? parameters.InverseQ : parameters.Exponent);
          var length = (int) innerStream.Length;
          EncodeLength(writer, length);

          ArraySegment<byte> innerBuffer = new ArraySegment<byte>();
          innerStream.TryGetBuffer(out innerBuffer);
          writer.Write(innerBuffer.Array, 0, length);
        }

        ArraySegment<byte> buffer = new ArraySegment<byte>();
        stream.TryGetBuffer(out buffer);
        var base64 = Convert.ToBase64String(buffer.Array, 0, (int) stream.Length);
        key.AppendLine(exportPrivateKey ? "-----BEGIN RSA PRIVATE KEY-----" : "-----BEGIN PUBLIC KEY-----");
        for (var i = 0; i < base64.Length; i += 64) {
          key.AppendLine(base64.Substring(i, Math.Min(64, base64.Length - i)));
        }
        key.AppendLine(exportPrivateKey ? "-----END RSA PRIVATE KEY-----" : "-----END PUBLIC KEY-----");
      }
      return key.ToString();
    }

    static void EncodeLength(BinaryWriter stream, int length) {
      if (length < 0) throw new ArgumentOutOfRangeException("length", "Length must be non-negative");
      if (length < 0x80) { // Short form
        stream.Write((byte) length);
      } else { // Long form
        var temp = length;
        var bytesRequired = 0;
        while (temp > 0) {
          temp >>= 8;
          bytesRequired++;
        }
        stream.Write((byte)(bytesRequired | 0x80));
        for (var i = bytesRequired - 1; i >= 0; i--) {
          stream.Write((byte)(length >>(8 * i) & 0xff));
        }
      }
    }

    static void EncodeIntegerBigEndian(BinaryWriter stream, byte[] value, bool forceUnsigned = true) {
      stream.Write((byte) 0x02); // INTEGER
      var prefixZeros = 0;
      for (var i = 0; i < value.Length; i++) {
        if (value[i] != 0) break;
        prefixZeros++;
      }
      if (value.Length - prefixZeros == 0) {
        EncodeLength(stream, 1);
        stream.Write((byte) 0);
      } else {
        if (forceUnsigned && value[prefixZeros] > 0x7f) {
          // Add a prefix zero to force unsigned if the MSB is 1
          EncodeLength(stream, value.Length - prefixZeros + 1);
          stream.Write((byte) 0);
        } else {
          EncodeLength(stream, value.Length - prefixZeros);
        }
        for (var i = prefixZeros; i < value.Length; i++) {
          stream.Write(value[i]);
        }
      }
    }
  }
}