using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Shared;

namespace CA
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Certificate Authority (CA) ===");

            // 1. CA Anahtarlarını Üret (Generate CA Keys)
            // CA, sertifikaları imzalamak için kendi Private Key'ini kullanır.
            Console.WriteLine("Generating CA Keys...");
            var keys = CryptoHelper.GenerateRSAKeys();
            string caPublicKey = keys.PublicKey;
            string caPrivateKey = keys.PrivateKey;
            Console.WriteLine("CA Keys Generated.");

            // Sunucuyu Başlat (Start Listener)
            // CA, 5000 portundan gelen istekleri dinler.
            int port = 5000;
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"CA listening on port {port}...");

            while (true)
            {
                try
                {
                    using TcpClient client = listener.AcceptTcpClient();
                    using NetworkStream stream = client.GetStream();

                    // İsteği Oku (Read Request)
                    byte[] buffer = new byte[4096];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    // Protokol Formatı: REQUEST|ClientID|PublicKey
                    string[] parts = request.Split('|');
                    if (parts.Length == 3 && parts[0] == "REQUEST")
                    {
                        string clientId = parts[1];
                        string clientPublicKey = parts[2];

                        Console.WriteLine($"Received Certificate Request from {clientId}");

                        // Sertifika Oluştur (Create Certificate)
                        var cert = new Certificate
                        {
                            SubjectID = clientId,
                            PublicKey = clientPublicKey,
                            Validity = DateTime.UtcNow.AddYears(1),
                            SerialNumber = Guid.NewGuid().ToString()
                        };

                        // Sertifikayı İmzala (Sign Certificate)
                        // CA, sertifika verisini kendi Private Key'i ile imzalar.
                        byte[] dataToSign = cert.GetDataToSign();
                        cert.Signature = CryptoHelper.SignData(dataToSign, caPrivateKey);

                        // İmzalı Sertifikayı Gönder (Send Certificate back)
                        string response = JsonSerializer.Serialize(cert);
                        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                        stream.Write(responseBytes, 0, responseBytes.Length);
                        Console.WriteLine($"Certificate issued to {clientId}");
                    }
                    else if (parts.Length > 0 && parts[0] == "GET_CA_KEY")
                    {
                        // Client'ların sertifikaları doğrulayabilmesi için CA Public Key'ini dağıtıyoruz.
                        byte[] responseBytes = Encoding.UTF8.GetBytes(caPublicKey);
                        stream.Write(responseBytes, 0, responseBytes.Length);
                        Console.WriteLine("Sent CA Public Key.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }
}