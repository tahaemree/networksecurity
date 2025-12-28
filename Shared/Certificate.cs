using System;
using System.Text.Json;

namespace Shared
{
    // Sertifika sınıfı: Dijital kimlik kartı görevi görür.
    public class Certificate
    {
        // Sertifikanın kime ait olduğu (Örn: "Client1")
        public string SubjectID { get; set; } = string.Empty;

        // Sertifika sahibinin Public Key'i (Base64 formatında)
        public string PublicKey { get; set; } = string.Empty;

        // Sertifikanın geçerlilik süresi
        public DateTime Validity { get; set; }

        // Sertifikaya özel benzersiz seri numarası
        public string SerialNumber { get; set; } = string.Empty;

        // CA tarafından atılan dijital imza (Verinin bütünlüğünü ve kaynağını doğrular)
        public byte[]? Signature { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }

        // İmzalanacak veriyi hazırlar (SubjectID + PublicKey + Validity + SerialNumber)
        // Bu veri CA'nın Private Key'i ile imzalanır.
        public byte[] GetDataToSign()
        {
            // Basitlik için alanları aralarına ':' koyarak birleştiriyoruz.
            var data = $"{SubjectID}:{PublicKey}:{Validity:O}:{SerialNumber}";
            return System.Text.Encoding.UTF8.GetBytes(data);
        }
    }
}