using System;

namespace Estellaris.EF.Cache {
  public class Cache {
    public string Key { get; set; }
    public byte[] Value { get; set; }
    public DateTime? ExpiresOn { get; set; }

    public Cache() { }

    public Cache(string key, byte[] value) {
      Key = key;
      Value = value;
    }

    public Cache(string key, byte[] value, DateTime expiresOn) : this(key, value) {
      ExpiresOn = expiresOn;
    }
  }
}