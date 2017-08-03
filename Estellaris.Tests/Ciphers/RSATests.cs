using Estellaris.Security;
using Estellaris.Security.Ciphers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Estellaris.Tests {
  [TestClass]
  public class RSATests {
    [TestMethod]
    public void TestKeyPairGeneration() {
      var keys = RSACipher.GenerateKeys();
      Assert.IsNotNull(keys);
      Assert.IsNotNull(keys.PrivateKey);
      Assert.IsNotNull(keys.PublicKey);
      Assert.AreNotEqual(keys.PrivateKey, "");
      Assert.AreNotEqual(keys.PublicKey, "");
    }

    [TestMethod]
    public void TestEncryption() {
      var keys = RSACipher.GenerateKeys();
      var text = "test of encryption";
      var encryptedText = RSACipher.EncryptString(keys, text);
      var decryptedText = RSACipher.DecryptString(keys, encryptedText);
      Assert.IsFalse(string.IsNullOrWhiteSpace(encryptedText));
      Assert.IsFalse(string.IsNullOrWhiteSpace(decryptedText));
      Assert.AreEqual(text, decryptedText);
    }

    [TestMethod]
    public void TestDecryption() {
      var privateKey = @"
        -----BEGIN RSA PRIVATE KEY-----
        MIIEowIBAAKCAQEA3BGLEjYSMZGI9EELAbq9/teH/EPA5aa+XTfOoe6PBRDmPIvl
        vCuy+u37DLDqry1zf5spaDSLYVMhiiLDcisLAb0n1TDOHTdSPMTWq7k9ACe0lPOT
        d0uPaFwWRBtKXdj/ZmzfEqkwMyUFGTQXeol6c4R3ddnp6fnkZKm1neI+cQ2emvrU
        FFbH1NsrC8Kb0F1ysBvePGIvpsf2xc42+pl6tkyhGdnu41Tg78qeYBJWJuxSgcqV
        /uq9RB5vwfAKefpQK25eyUXX1cFGOOOJk4ZKepVkKXYwrkjlg6YUczTeOd4ai0Z5
        x28d8o3pDv44gd7g1nHmeHiMDePQSzW8kHWbtwIDAQABAoIBAGAS3Jb3uhufwJ15
        o9d+ciHGcFyGK7lWgTbq/S+emRuKFCmMnZ/3p+x6ZqZUui/99LVZxMr0XYEArNzE
        bnTyK5z3umMNKn6Av0s+V8WiWeoua1y3tcJX32SdBy92hpHQATfzAbQA3sUFPWOS
        ZUmeqTGzO//cQY1fBgGYQWyK792kV+ZcgrfPprohuX4vMkZgiq9X8bSQZeyllnFl
        Ve5a3lBfehENN5T9oAO5Z79zskKczsuC8ENF8sue8y2hGVWcP1wxynl16N7a/Q7c
        7ZBeeaEyzgbwaBD+uOqgRJ95+9aVSp0vv95lO4hevNvHzNjBcjnxZA1yul54qzpF
        9s61RGkCgYEA+7zIa1HQv/B3YD1uDhuaUj3kKVr0gvFmUZ38OBWJWuvAO4Fp/Plh
        PNBpVdzBF+gxld0xjieEPYD86MYWNUZbXb/EL9tkWs5MRCuNPtHabXXc1KD6lO6h
        M/6goD5zIisq9DksfylpUNlVDJttkVDh5veYCOXcET/Z8iIHbJ8ZM4MCgYEA38t7
        2bh0RiI2t6+NpOSmxW6fvW4Q5HzNFlM1j83WN933c3ftly7rA/vvTe1zLM/1G4Cs
        7qAKJnN5AoSqkmbCnMUmANZ9A0Njr6w83jgqBk49UsEbvmGZhW5/n1Fkg/7ezaVI
        vRnmwOykiNu1lUzUnRJGv74pRZ5UXkg8gKEy3L0CgYAPRrh+0rsxOp1z6KZqEsC9
        puXWoJ9f5thHsaehm6T3MIE82zCcWmHNN/R8cmYBVmTT7FVahAlhVheC5KpqVzeq
        K73zDYQ8gnJcXKw4mLGBnPpmqNIsAYMkzZnfv4prE2WK5oFNwiyS7G1d03zFqbvb
        sUC3oYNGRLKsH+aFb+4ukwKBgCj/eBLsrbBjeC6yZHp+wOaOW4kybrnM+y1J1Rgh
        F0toyHpI0CfQGpHCR5a5F05wUnDVY9jbTMC6isKhVzY2yRQ7MbkZkhFU+SAnp4b2
        NwysrKNKaTC5ZIyDL7IaERX1TQ/TZb3uzs0tDP9dcKiHSLP1syxRQ/JTDRWwmhTe
        cxjpAoGBAITD5UmUoKA9Ve/yGhtd7fAlOoXs+zNuRhzEUnaSAORh/mV57hZQhdDA
        HsCuVMNxscvownXuSm73Jl3cTO4SpgyDwo9KUGDBvKf+21WaHQ0d+c06TGqi36Yt
        1BNpV0hZC1+jws+de01q6iBK9RZ2iQVRMPL+xFpqNHj8PAtPl8A6
        -----END RSA PRIVATE KEY-----
      ";
      var keys = new RSAKeyPair(privateKey, "");
      var encryptedText = "mPSaaHi5dUg9i+C0+q/PHPJVCv8nVambyaCZIg5bgwgtUKbxu04N9X0QqjAwVz3rm/tDcrz3++nV2KUTrcBlpoJxOSlnZzs9t9lQ5OIebtOys2E5/4HOkZjgpT3o4HWkeCd4YYcwZ8RhQuyv6GdE/iMNkxgS2O93pjI3l1MGhCRS9WMoIHq9vaia4RFSZEvS+7QC+fy7Xx6SXr+rBiAldxGrUy2+rnV20KJK2iXOPCy0IIWuWLMl+uH3A55nMO8ZmoYpzWyhmEEBij5AErg2utA0Nl8rJKtUxQIx5U+IJ/O13RAIZOVe6ziXJzt8922HaWSy2/5e4FjPzfXyHjOz8A==";
      var decryptedText = RSACipher.DecryptString(keys, encryptedText);
      Assert.IsFalse(string.IsNullOrWhiteSpace(decryptedText));
      Assert.AreEqual("test of encryption", decryptedText);
    }

    [TestMethod]
    public void TestKeyLoadingAndEncrypting() {
      var privateKey = @"
        -----BEGIN RSA PRIVATE KEY-----
        MIIEowIBAAKCAQEA3BGLEjYSMZGI9EELAbq9/teH/EPA5aa+XTfOoe6PBRDmPIvl
        vCuy+u37DLDqry1zf5spaDSLYVMhiiLDcisLAb0n1TDOHTdSPMTWq7k9ACe0lPOT
        d0uPaFwWRBtKXdj/ZmzfEqkwMyUFGTQXeol6c4R3ddnp6fnkZKm1neI+cQ2emvrU
        FFbH1NsrC8Kb0F1ysBvePGIvpsf2xc42+pl6tkyhGdnu41Tg78qeYBJWJuxSgcqV
        /uq9RB5vwfAKefpQK25eyUXX1cFGOOOJk4ZKepVkKXYwrkjlg6YUczTeOd4ai0Z5
        x28d8o3pDv44gd7g1nHmeHiMDePQSzW8kHWbtwIDAQABAoIBAGAS3Jb3uhufwJ15
        o9d+ciHGcFyGK7lWgTbq/S+emRuKFCmMnZ/3p+x6ZqZUui/99LVZxMr0XYEArNzE
        bnTyK5z3umMNKn6Av0s+V8WiWeoua1y3tcJX32SdBy92hpHQATfzAbQA3sUFPWOS
        ZUmeqTGzO//cQY1fBgGYQWyK792kV+ZcgrfPprohuX4vMkZgiq9X8bSQZeyllnFl
        Ve5a3lBfehENN5T9oAO5Z79zskKczsuC8ENF8sue8y2hGVWcP1wxynl16N7a/Q7c
        7ZBeeaEyzgbwaBD+uOqgRJ95+9aVSp0vv95lO4hevNvHzNjBcjnxZA1yul54qzpF
        9s61RGkCgYEA+7zIa1HQv/B3YD1uDhuaUj3kKVr0gvFmUZ38OBWJWuvAO4Fp/Plh
        PNBpVdzBF+gxld0xjieEPYD86MYWNUZbXb/EL9tkWs5MRCuNPtHabXXc1KD6lO6h
        M/6goD5zIisq9DksfylpUNlVDJttkVDh5veYCOXcET/Z8iIHbJ8ZM4MCgYEA38t7
        2bh0RiI2t6+NpOSmxW6fvW4Q5HzNFlM1j83WN933c3ftly7rA/vvTe1zLM/1G4Cs
        7qAKJnN5AoSqkmbCnMUmANZ9A0Njr6w83jgqBk49UsEbvmGZhW5/n1Fkg/7ezaVI
        vRnmwOykiNu1lUzUnRJGv74pRZ5UXkg8gKEy3L0CgYAPRrh+0rsxOp1z6KZqEsC9
        puXWoJ9f5thHsaehm6T3MIE82zCcWmHNN/R8cmYBVmTT7FVahAlhVheC5KpqVzeq
        K73zDYQ8gnJcXKw4mLGBnPpmqNIsAYMkzZnfv4prE2WK5oFNwiyS7G1d03zFqbvb
        sUC3oYNGRLKsH+aFb+4ukwKBgCj/eBLsrbBjeC6yZHp+wOaOW4kybrnM+y1J1Rgh
        F0toyHpI0CfQGpHCR5a5F05wUnDVY9jbTMC6isKhVzY2yRQ7MbkZkhFU+SAnp4b2
        NwysrKNKaTC5ZIyDL7IaERX1TQ/TZb3uzs0tDP9dcKiHSLP1syxRQ/JTDRWwmhTe
        cxjpAoGBAITD5UmUoKA9Ve/yGhtd7fAlOoXs+zNuRhzEUnaSAORh/mV57hZQhdDA
        HsCuVMNxscvownXuSm73Jl3cTO4SpgyDwo9KUGDBvKf+21WaHQ0d+c06TGqi36Yt
        1BNpV0hZC1+jws+de01q6iBK9RZ2iQVRMPL+xFpqNHj8PAtPl8A6
        -----END RSA PRIVATE KEY-----
      ";
      var publicKey = @"
        -----BEGIN PUBLIC KEY-----
        MIIBKwIBAAKCAQEA3BGLEjYSMZGI9EELAbq9/teH/EPA5aa+XTfOoe6PBRDmPIvl
        vCuy+u37DLDqry1zf5spaDSLYVMhiiLDcisLAb0n1TDOHTdSPMTWq7k9ACe0lPOT
        d0uPaFwWRBtKXdj/ZmzfEqkwMyUFGTQXeol6c4R3ddnp6fnkZKm1neI+cQ2emvrU
        FFbH1NsrC8Kb0F1ysBvePGIvpsf2xc42+pl6tkyhGdnu41Tg78qeYBJWJuxSgcqV
        /uq9RB5vwfAKefpQK25eyUXX1cFGOOOJk4ZKepVkKXYwrkjlg6YUczTeOd4ai0Z5
        x28d8o3pDv44gd7g1nHmeHiMDePQSzW8kHWbtwIDAQABAgMBAAECAwEAAQIDAQAB
        AgMBAAECAwEAAQIDAQAB
        -----END PUBLIC KEY-----
      ";
      var keys = new RSAKeyPair(privateKey, publicKey);
      var text = "test of encryption";
      var encryptedText = RSACipher.EncryptString(keys, text);
      var decryptedText = RSACipher.DecryptString(keys, encryptedText);
      Assert.IsFalse(string.IsNullOrWhiteSpace(encryptedText));
      Assert.IsFalse(string.IsNullOrWhiteSpace(decryptedText));
      Assert.AreEqual(text, decryptedText);
    }
  }
}