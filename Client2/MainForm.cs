using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Shared;
using System.Security.Cryptography;

namespace Client2
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void Log(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action<string>(Log), message);
            }
            else
            {
                txtLog.AppendText(message + Environment.NewLine);
                txtLog.ScrollToCaret();
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            btnConnect.Enabled = false;
            btnConnect.Text = "Client 2 Running...";
            Task.Run(() => StartClient2());
        }

        private void StartClient2()
        {
            try
            {
                Log("=== Client 2 ===");

                // 1. Anahtarları Üret
                var keys = CryptoHelper.GenerateRSAKeys();
                string myPublicKey = keys.PublicKey;
                string myPrivateKey = keys.PrivateKey;
                Log("Keys Generated.");

                // 2. CA Public Key'ini Al
                string caPublicKey = GetCAPublicKey();
                Log("Got CA Public Key.");

                // 3. Sertifika İste
                Certificate myCert = RequestCertificate("Client2", myPublicKey);
                Log($"Certificate Received. Serial: {myCert.SerialNumber}");

                // 4. Client 1'e Bağlan
                Log("Connecting to Client 1...");
                Thread.Sleep(2000); // Wait for Client 1 to start listening
                using TcpClient client1 = new TcpClient("localhost", 6001);
                using NetworkStream stream = client1.GetStream();
                Log("Connected to Client 1.");

                // 5. El Sıkışma - Sertifika Değişimi
                byte[] lenBuf = new byte[4];
                stream.ReadExactly(lenBuf, 0, 4);
                int len = BitConverter.ToInt32(lenBuf, 0);
                byte[] certBuf = new byte[len];
                stream.ReadExactly(certBuf, 0, len);
                string client1CertJson = Encoding.UTF8.GetString(certBuf);
                Certificate client1Cert = JsonSerializer.Deserialize<Certificate>(client1CertJson)!;
                Log($"Received Certificate from {client1Cert.SubjectID}");

                string myCertJson = JsonSerializer.Serialize(myCert);
                byte[] myCertBytes = Encoding.UTF8.GetBytes(myCertJson);
                byte[] lengthPrefix = BitConverter.GetBytes(myCertBytes.Length);
                stream.Write(lengthPrefix, 0, 4);
                stream.Write(myCertBytes, 0, myCertBytes.Length);
                Log("Sent Certificate to Client 1.");

                // 6. Sertifika Doğrulama
                if (!CryptoHelper.VerifyData(client1Cert.GetDataToSign(), client1Cert.Signature!, caPublicKey))
                {
                    Log("Client 1 Certificate Verification FAILED!");
                    return;
                }
                Log("Client 1 Certificate Verified.");

                // 7. Anahtar Değişimi (Stage 2: Master Key Establishment)
                Log("--- Stage 2: Master Key Establishment ---");
                stream.ReadExactly(lenBuf, 0, 4);
                len = BitConverter.ToInt32(lenBuf, 0);
                byte[] encryptedPayload1 = new byte[len];
                stream.ReadExactly(encryptedPayload1, 0, len);

                byte[] decryptedPayload1Bytes = CryptoHelper.DecryptRSA(encryptedPayload1, myPrivateKey);
                string decryptedPayload1 = Encoding.UTF8.GetString(decryptedPayload1Bytes);
                Log($"Received Encrypted Payload: {decryptedPayload1}");

                string[] parts = decryptedPayload1.Split('|');
                if (parts.Length != 2 || parts[0] != "Client1")
                {
                    Log("Invalid Payload from Client 1!");
                    return;
                }
                byte[] nonce1 = Convert.FromBase64String(parts[1]);

                byte[] nonce2 = new byte[32];
                RandomNumberGenerator.Fill(nonce2);
                string nonce2Str = Convert.ToBase64String(nonce2);

                string payload2 = $"Client2|{nonce2Str}";
                byte[] payload2Bytes = Encoding.UTF8.GetBytes(payload2);

                byte[] encryptedPayload2 = CryptoHelper.EncryptRSA(payload2Bytes, client1Cert.PublicKey);

                byte[] encPayload2Len = BitConverter.GetBytes(encryptedPayload2.Length);
                stream.Write(encPayload2Len, 0, 4);
                stream.Write(encryptedPayload2, 0, encryptedPayload2.Length);
                Log("Sent Encrypted ID and Nonce2 (N2).");

                byte[] combinedNonces = new byte[nonce1.Length + nonce2.Length];
                Buffer.BlockCopy(nonce1, 0, combinedNonces, 0, nonce1.Length);
                Buffer.BlockCopy(nonce2, 0, combinedNonces, nonce1.Length, nonce2.Length);

                byte[] masterKey = CryptoHelper.DeriveKey(combinedNonces);
                Log($"Master Key (Km) Generated: {Convert.ToBase64String(masterKey)}");

                // 8. Oturum Anahtarı Değişimi (Stage 3: Session Key Establishment)
                Log("--- Stage 3: Session Key Establishment ---");
                stream.ReadExactly(lenBuf, 0, 4);
                len = BitConverter.ToInt32(lenBuf, 0);
                byte[] receivedStage3Bytes = new byte[len];
                stream.ReadExactly(receivedStage3Bytes, 0, len);

                byte[] receivedIvStage3 = new byte[16];
                byte[] receivedCipherStage3 = new byte[len - 16];
                Buffer.BlockCopy(receivedStage3Bytes, 0, receivedIvStage3, 0, 16);
                Buffer.BlockCopy(receivedStage3Bytes, 16, receivedCipherStage3, 0, len - 16);

                string nonce3Str = CryptoHelper.DecryptAES(receivedCipherStage3, masterKey, receivedIvStage3);
                byte[] nonce3 = Convert.FromBase64String(nonce3Str);
                Log("Received and Decrypted Nonce3 (N3).");

                byte[] nonce4 = new byte[32];
                RandomNumberGenerator.Fill(nonce4);

                byte[] ivStage3 = new byte[16];
                RandomNumberGenerator.Fill(ivStage3);
                byte[] encryptedNonce4 = CryptoHelper.EncryptAES(Convert.ToBase64String(nonce4), masterKey, ivStage3);

                byte[] stage3MsgLen = BitConverter.GetBytes(ivStage3.Length + encryptedNonce4.Length);
                stream.Write(stage3MsgLen, 0, 4);
                stream.Write(ivStage3, 0, ivStage3.Length);
                stream.Write(encryptedNonce4, 0, encryptedNonce4.Length);
                Log("Sent Encrypted Nonce4 (N4) with Master Key.");

                byte[] combinedSessionNonces = new byte[nonce3.Length + nonce4.Length];
                Buffer.BlockCopy(nonce3, 0, combinedSessionNonces, 0, nonce3.Length);
                Buffer.BlockCopy(nonce4, 0, combinedSessionNonces, nonce3.Length, nonce4.Length);

                byte[] sessionKey = CryptoHelper.DeriveKey(combinedSessionNonces);
                Log($"Session Key (Ks) Generated: {Convert.ToBase64String(sessionKey)}");

                // 9. Güvenli İletişim
                stream.ReadExactly(lenBuf, 0, 4);
                len = BitConverter.ToInt32(lenBuf, 0);
                byte[] receivedBytes = new byte[len];
                stream.ReadExactly(receivedBytes, 0, len);

                byte[] receivedIv = new byte[16];
                byte[] receivedCipher = new byte[len - 16];
                Buffer.BlockCopy(receivedBytes, 0, receivedIv, 0, 16);
                Buffer.BlockCopy(receivedBytes, 16, receivedCipher, 0, len - 16);

                string decryptedMessage = CryptoHelper.DecryptAES(receivedCipher, sessionKey, receivedIv);
                Log($"Received Encrypted Message: {decryptedMessage}");

                string message = "Hello Client 1, I received your message securely!";
                byte[] iv = new byte[16];
                RandomNumberGenerator.Fill(iv);

                byte[] encryptedMessage = CryptoHelper.EncryptAES(message, sessionKey, iv);

                byte[] msgLen = BitConverter.GetBytes(iv.Length + encryptedMessage.Length);
                stream.Write(msgLen, 0, 4);
                stream.Write(iv, 0, iv.Length);
                stream.Write(encryptedMessage, 0, encryptedMessage.Length);
                Log($"Sent Encrypted Reply: {message}");
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
            }
        }

        private string GetCAPublicKey()
        {
            using TcpClient client = new TcpClient("localhost", 5000);
            using NetworkStream stream = client.GetStream();
            byte[] request = Encoding.UTF8.GetBytes("GET_CA_KEY");
            stream.Write(request, 0, request.Length);

            byte[] buffer = new byte[4096];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }

        private Certificate RequestCertificate(string clientId, string publicKey)
        {
            using TcpClient client = new TcpClient("localhost", 5000);
            using NetworkStream stream = client.GetStream();
            string req = $"REQUEST|{clientId}|{publicKey}";
            byte[] request = Encoding.UTF8.GetBytes(req);
            stream.Write(request, 0, request.Length);

            byte[] buffer = new byte[8192];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            return JsonSerializer.Deserialize<Certificate>(response)!;
        }
    }
}
