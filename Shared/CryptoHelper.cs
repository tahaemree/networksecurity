using System;
using System.Security.Cryptography;
using System.Text;

namespace Shared
{
    // Kriptografik işlemler için yardımcı sınıf
    public static class CryptoHelper
    {
        // RSA Anahtar Çifti (Public & Private) Üretimi
        // 2048 bitlik güvenli bir anahtar çifti oluşturur.
        public static (string PublicKey, string PrivateKey) GenerateRSAKeys()
        {
            using var rsa = RSA.Create(2048);
            var pubKey = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo());
            var privKey = Convert.ToBase64String(rsa.ExportPkcs8PrivateKey());
            return (pubKey, privKey);
        }

        // Veriyi İmzalar (Dijital İmza)
        // Veriyi SHA256 ile hashler ve Private Key ile şifreler (imzalar).
        public static byte[] SignData(byte[] data, string privateKey)
        {
            using var rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(privateKey), out _);
            return rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        // İmzayı Doğrular
        // Veriyi Public Key kullanarak imza ile karşılaştırır.
        // Eğer true dönerse, veri değiştirilmemiştir ve doğru kişiden gelmiştir.
        public static bool VerifyData(byte[] data, byte[] signature, string publicKey)
        {
            using var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKey), out _);
            return rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        // RSA ile Şifreleme (Asimetrik)
        // Veriyi karşı tarafın Public Key'i ile şifreler. Sadece karşı tarafın Private Key'i açabilir.
        // Anahtar değişimi (Key Exchange) sırasında kullanılır.
        public static byte[] EncryptRSA(byte[] data, string publicKey)
        {
            using var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKey), out _);
            return rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA256);
        }

        // RSA ile Şifre Çözme (Asimetrik)
        // Kendi Private Key'imiz ile bize gelen şifreli veriyi çözeriz.
        public static byte[] DecryptRSA(byte[] data, string privateKey)
        {
            using var rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(privateKey), out _);
            return rsa.Decrypt(data, RSAEncryptionPadding.OaepSHA256);
        }

        // AES ile Şifreleme (Simetrik)
        // Hızlıdır, büyük verileri (mesajlaşma) şifrelemek için kullanılır.
        // Session Key (Oturum Anahtarı) burada kullanılır.
        public static byte[] EncryptAES(string plainText, byte[] key, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            return encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        }

        // AES ile Şifre Çözme (Simetrik)
        public static string DecryptAES(byte[] cipherText, byte[] key, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            var plainBytes = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }

        // Anahtar Türetme Fonksiyonu (KDF)
        // Gelen veriyi (örn: birleştirilmiş Nonce'lar) SHA256 ile hashleyerek sabit uzunlukta bir anahtar üretir.
        public static byte[] DeriveKey(byte[] input)
        {
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(input);
        }
    }
}