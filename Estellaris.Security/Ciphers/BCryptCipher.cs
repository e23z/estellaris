namespace Estellaris.Security.Ciphers {
  public static class BCryptCipher {
    /// <summary>
    /// Generates a bcrypt hashed and salted version of the input string.
    /// </summary>
    /// <param name="str">String to be hashed.</param>
    /// <returns>Base64 string of the hashed input.</returns>
    public static string Hash(string str) {
      return BCrypt.Net.BCrypt.HashPassword(str, BCrypt.Net.BCrypt.GenerateSalt());
    }

    /// <summary>
    /// Checks if the input matches the hashed versions.
    /// </summary>
    /// <param name="str">Unhashed input to be compared with the hashed string.</param>
    /// <param name="hash">Base64 bcrypt hash to be compared to.</param>
    /// <returns>True/False if the input matches the hash or not.</returns>
    public static bool Equals(string str, string hash) {
      return BCrypt.Net.BCrypt.Verify(str, hash);
    }
  }
}