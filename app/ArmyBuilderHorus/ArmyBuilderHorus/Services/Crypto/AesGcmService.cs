using System.Security.Cryptography;

namespace ArmyBuilderHorus.Services.Crypto;

/// Blob: salt(16) | nonce(12) | ciphertext | tag(16)
public static class AesGcmService
{
    public static byte[] Decrypt(byte[] blob, string passphrase)
    {
        if (blob.Length < 16 + 12 + 16) throw new InvalidDataException("Blob too small");
        var salt = blob.AsSpan(0, 16).ToArray();
        var nonce = blob.AsSpan(16, 12).ToArray();
        var tag = blob.AsSpan(blob.Length - 16, 16).ToArray();
        var cipher = blob.AsSpan(28, blob.Length - 28 - 16).ToArray();

        using var kdf = new Rfc2898DeriveBytes(passphrase, salt, 210_000, HashAlgorithmName.SHA256);
        var key = kdf.GetBytes(32);

        var plain = new byte[cipher.Length];
#if NET9_0_OR_GREATER
        using var aes = new AesGcm(key, 16);
#else
#pragma warning disable SYSLIB0053
        using var aes = new AesGcm(key);
#pragma warning restore SYSLIB0053
#endif
        aes.Decrypt(nonce, cipher, tag, plain);
        return plain;
    }
}
