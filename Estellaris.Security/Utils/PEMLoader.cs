using System;
using System.IO;
using System.Security.Cryptography;
using Estellaris.Core.Extensions;

namespace Estellaris.Security.Utils {
  internal static class PEMLoader {
    const string PrivateHeader = "-----BEGIN RSA PRIVATE KEY-----";
    const string PrivateFooter = "-----END RSA PRIVATE KEY-----";
    const string PublicHeader = "-----BEGIN PUBLIC KEY-----";
    const string PublicFooter = "-----END PUBLIC KEY-----";

    public static RSA GetRSAFromPEM(string pemStr) {
      pemStr = pemStr.Trim();
      var isPrivateKey = pemStr.StartsWith(PrivateHeader) && pemStr.EndsWith(PrivateFooter);
      var keyBytes = isPrivateKey ? DecodePrivateKey(pemStr) : DecodePublicKey(pemStr);
      if (keyBytes == null)
        return null;
      return ConvertToRSA(keyBytes);
    }

    static byte[] DecodePublicKey(string publicKey) {
      publicKey = publicKey.Trim();
      if (!publicKey.StartsWith(PublicHeader) || !publicKey.EndsWith(PublicFooter))
        return null;
      publicKey = publicKey.Replace(PublicHeader, "").Replace(PublicFooter, "").Trim();

      byte[] keyBytes = null;
      try { keyBytes = publicKey.FromBase64(); } catch { }
      return keyBytes;
    }

    static byte[] DecodePrivateKey(string privateKey) {
      privateKey = privateKey.Trim();
      if (!privateKey.StartsWith(PrivateHeader) || !privateKey.EndsWith(PrivateFooter))
        return null;
      privateKey = privateKey.Replace(PrivateHeader, "").Replace(PrivateFooter, "").Trim();

      byte[] keyBytes = null;
      try { keyBytes = privateKey.FromBase64(); } catch { }
      return keyBytes;
    }

    static RSA ConvertToRSA(byte[] key) {
      RSA rsa = null;
      using(var ms = new MemoryStream(key))
      using(var br = new BinaryReader(ms)) {
        byte[] MODULUS, E, D, P, Q, DP, DQ, IQ;
        byte bt = 0;
        ushort twobytes = 0;
        int elems = 0;

        try {
          twobytes = br.ReadUInt16();
          if (twobytes == 0x8130) br.ReadByte();
          else if (twobytes == 0x8230) br.ReadInt16();
          else throw new InvalidDataException();

          twobytes = br.ReadUInt16();
          if (twobytes != 0x0102) throw new InvalidDataException();

          bt = br.ReadByte();
          if (bt != 0x00) throw new InvalidDataException();

          elems = GetIntegerSize(br);
          MODULUS = br.ReadBytes(elems);

          elems = GetIntegerSize(br);
          E = br.ReadBytes(elems);

          elems = GetIntegerSize(br);
          D = br.ReadBytes(elems);

          elems = GetIntegerSize(br);
          P = br.ReadBytes(elems);

          elems = GetIntegerSize(br);
          Q = br.ReadBytes(elems);

          elems = GetIntegerSize(br);
          DP = br.ReadBytes(elems);

          elems = GetIntegerSize(br);
          DQ = br.ReadBytes(elems);

          elems = GetIntegerSize(br);
          IQ = br.ReadBytes(elems);

          rsa = RSA.Create();
          rsa.ImportParameters(new RSAParameters {
            Modulus = MODULUS,
              Exponent = E,
              D = D,
              P = P,
              Q = Q,
              DP = DP,
              DQ = DQ,
              InverseQ = IQ
          });
        } catch { }
      }
      return rsa;
    }

    static int GetIntegerSize(BinaryReader br) {
      byte bt = 0;
      byte lowbyte = 0x00;
      byte highbyte = 0x00;
      int count = 0;
      bt = br.ReadByte();
      if (bt != 0x02) return 0;
      bt = br.ReadByte();

      if (bt == 0x81) count = br.ReadByte();
      else if (bt == 0x82) {
        highbyte = br.ReadByte();
        lowbyte = br.ReadByte();
        byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
        count = BitConverter.ToInt32(modint, 0);
      } else count = bt;

      while (br.ReadByte() == 0x00) { count -= 1; }
      br.BaseStream.Seek(-1, SeekOrigin.Current);
      return count;
    }
  }
}