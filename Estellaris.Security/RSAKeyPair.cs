namespace Estellaris.Security {
  public class RSAKeyPair {
    public readonly string PrivateKey;
    public readonly string PublicKey;

    public RSAKeyPair(string privateKey, string publicKey) {
      PrivateKey = privateKey;
      PublicKey = publicKey;
    }
  }
}