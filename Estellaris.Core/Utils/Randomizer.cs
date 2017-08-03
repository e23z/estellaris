using System;
using Estellaris.Core.Extensions;

namespace Estellaris.Core {
  public static class Randomizer {
    static readonly Random random = new Random( );
    static readonly object locker = new object( );

    public static byte[ ] RandomBytes(int length) {
      var bytes = new byte[length];
      random.NextBytes(bytes);
      return bytes;
    }

    public static string RandomHex(int length) {
      return RandomBytes(length).ToHex( );
    }

    public static string RandomUniqueToken( ) {
      byte[ ] tickBytes;
      lock(locker) {
        tickBytes = BitConverter.GetBytes(DateTime.Now.Ticks);
      }
      return tickBytes.ToHex( );
    }
  }
}