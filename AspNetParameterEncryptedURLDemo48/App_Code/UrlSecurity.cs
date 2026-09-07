using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;

public static class UrlSecurity
{
    private const string TokenParameterName = "token";

    private static readonly byte[] EncryptionKey;
    private static readonly byte[] ValidationKey;

    static UrlSecurity()
    {
        string encryptionKey = ConfigurationManager.AppSettings["UrlSecurity.EncryptionKey"];
        string validationKey = ConfigurationManager.AppSettings["UrlSecurity.ValidationKey"];

        if (string.IsNullOrWhiteSpace(encryptionKey))
            throw new ConfigurationErrorsException(
                "UrlSecurity.EncryptionKey is missing from web.config.");

        if (string.IsNullOrWhiteSpace(validationKey))
            throw new ConfigurationErrorsException(
                "UrlSecurity.ValidationKey is missing from web.config.");

        EncryptionKey = Convert.FromBase64String(encryptionKey);
        ValidationKey = Convert.FromBase64String(validationKey);

        if (EncryptionKey.Length != 32)
            throw new ConfigurationErrorsException(
                "UrlSecurity.EncryptionKey must be exactly 32 bytes.");

        if (ValidationKey.Length < 32)
            throw new ConfigurationErrorsException(
                "UrlSecurity.ValidationKey must be at least 32 bytes.");
    }

    public static string EncryptParameters(NameValueCollection parameters)
    {
        if (parameters == null)
            throw new ArgumentNullException("parameters");

        StringBuilder plainText = new StringBuilder();

        foreach (string key in parameters.AllKeys)
        {
            if (string.IsNullOrEmpty(key))
                continue;

            string[] values = parameters.GetValues(key);

            if (values == null)
                continue;

            foreach (string value in values)
            {
                plainText.Append(Escape(key));
                plainText.Append('=');
                plainText.Append(Escape(value ?? string.Empty));
                plainText.Append('&');
            }
        }

        if (plainText.Length > 0)
            plainText.Length--;

        return Encrypt(plainText.ToString());
    }

    public static NameValueCollection DecryptParameters(string token)
    {
        string plainText = Decrypt(token);

        NameValueCollection result = new NameValueCollection();

        if (string.IsNullOrEmpty(plainText))
            return result;

        string[] pairs = plainText.Split('&');

        foreach (string pair in pairs)
        {
            int index = pair.IndexOf('=');

            if (index <= 0)
                continue;

            string key = Unescape(pair.Substring(0, index));
            string value = Unescape(pair.Substring(index + 1));

            result.Add(key, value);
        }

        return result;
    }

    public static string Encrypt(string plainText)
    {
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

        byte[] iv = new byte[16];

        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(iv);
        }

        byte[] cipherBytes;

        using (Aes aes = Aes.Create())
        {
            aes.Key = EncryptionKey;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (MemoryStream ms = new MemoryStream())
            using (ICryptoTransform encryptor = aes.CreateEncryptor())
            using (CryptoStream cs =
                new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                cs.Write(plainBytes, 0, plainBytes.Length);
                cs.FlushFinalBlock();

                cipherBytes = ms.ToArray();
            }
        }

        // Payload = IV + CipherText
        byte[] payload = new byte[iv.Length + cipherBytes.Length];

        Buffer.BlockCopy(iv, 0, payload, 0, iv.Length);
        Buffer.BlockCopy(
            cipherBytes,
            0,
            payload,
            iv.Length,
            cipherBytes.Length);

        // HMAC provides tamper protection
        byte[] signature;

        using (HMACSHA256 hmac = new HMACSHA256(ValidationKey))
        {
            signature = hmac.ComputeHash(payload);
        }

        // Final payload = IV + CipherText + HMAC
        byte[] finalPayload =
            new byte[payload.Length + signature.Length];

        Buffer.BlockCopy(
            payload,
            0,
            finalPayload,
            0,
            payload.Length);

        Buffer.BlockCopy(
            signature,
            0,
            finalPayload,
            payload.Length,
            signature.Length);

        return Base64UrlEncode(finalPayload);
    }

    public static string Decrypt(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new CryptographicException("Empty URL security token.");

        byte[] finalPayload = Base64UrlDecode(token);

        if (finalPayload.Length < 16 + 32)
            throw new CryptographicException("Invalid URL security token.");

        int hmacLength = 32;
        int payloadLength = finalPayload.Length - hmacLength;

        byte[] payload = new byte[payloadLength];
        byte[] receivedSignature = new byte[hmacLength];

        Buffer.BlockCopy(
            finalPayload,
            0,
            payload,
            0,
            payloadLength);

        Buffer.BlockCopy(
            finalPayload,
            payloadLength,
            receivedSignature,
            0,
            hmacLength);

        byte[] expectedSignature;

        using (HMACSHA256 hmac = new HMACSHA256(ValidationKey))
        {
            expectedSignature = hmac.ComputeHash(payload);
        }

        if (!FixedTimeEquals(
            expectedSignature,
            receivedSignature))
        {
            throw new CryptographicException(
                "Invalid URL security signature.");
        }

        byte[] iv = new byte[16];
        byte[] cipherBytes = new byte[payload.Length - 16];

        Buffer.BlockCopy(payload, 0, iv, 0, 16);

        Buffer.BlockCopy(
            payload,
            16,
            cipherBytes,
            0,
            cipherBytes.Length);

        using (Aes aes = Aes.Create())
        {
            aes.Key = EncryptionKey;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (MemoryStream ms =
                new MemoryStream(cipherBytes))
            using (ICryptoTransform decryptor =
                aes.CreateDecryptor())
            using (CryptoStream cs =
                new CryptoStream(
                    ms,
                    decryptor,
                    CryptoStreamMode.Read))
            using (MemoryStream output =
                new MemoryStream())
            {
                byte[] buffer = new byte[4096];
                int read;

                while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    output.Write(buffer, 0, read);
                }

                return Encoding.UTF8.GetString(output.ToArray());
            }
        }
    }

    public static string TokenParameter
    {
        get { return TokenParameterName; }
    }

    private static string Escape(string value)
    {
        return Uri.EscapeDataString(value ?? string.Empty);
    }

    private static string Unescape(string value)
    {
        return Uri.UnescapeDataString(value ?? string.Empty);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static byte[] Base64UrlDecode(string value)
    {
        string base64 = value
            .Replace('-', '+')
            .Replace('_', '/');

        switch (base64.Length % 4)
        {
            case 2:
                base64 += "==";
                break;

            case 3:
                base64 += "=";
                break;
        }

        return Convert.FromBase64String(base64);
    }

    private static bool FixedTimeEquals(byte[] a, byte[] b)
    {
        if (a == null || b == null || a.Length != b.Length)
            return false;

        int result = 0;

        for (int i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }

        return result == 0;
    }
}