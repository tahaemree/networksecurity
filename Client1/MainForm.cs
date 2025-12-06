using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Shared;
using System.Security.Cryptography;

namespace Client1
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

        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            btnStart.Text = "Client 1 Running...";
            Task.Run(() => StartClient1());
        }

        private void StartClient1()
        {
            try
            {
                Log("=== Client 1 ===");

                // 1. Anahtarları Üret
                var keys = CryptoHelper.GenerateRSAKeys();
                string myPublicKey = keys.PublicKey;
                string myPrivateKey = keys.PrivateKey;
                Log("Keys Generated.");

                // 2. CA Public Key'ini Al
                string caPublicKey = GetCAPublicKey();
                Log("Got CA Public Key.");

                // 3. Sertifika İste
                Certificate myCert = RequestCertificate("Client1", myPublicKey);
                Log($"Certificate Received. Serial: {myCert.SerialNumber}");

                // 4. Client 2'yi Bekle
                int port = 6001;
                TcpListener listener = new TcpListener(System.Net.IPAddress.Any, port);
                listener.Start();
                Log($"Client 1 listening for Client 2 on port {port}...");

                using TcpClient client2 = listener.AcceptTcpClient();
                using NetworkStream stream = client2.GetStream();
                Log("Client 2 Connected.");

                // 5. El Sıkışma - Sertifika Değişimi
                string myCertJson = JsonSerializer.Serialize(myCert);
                byte[] myCertBytes = Encoding.UTF8.GetBytes(myCertJson);
                byte[] lengthPrefix = BitConverter.GetBytes(myCertBytes.Length);
                stream.Write(lengthPrefix, 0, 4);
                stream.Write(myCertBytes, 0, myCertBytes.Length);
                Log("Sent Certificate to Client 2.");

                byte[] lenBuf = new byte[4];
                stream.ReadExactly(lenBuf, 0, 4);
                int len = BitConverter.ToInt32(lenBuf, 0);
                byte[] certBuf = new byte[len];
                stream.ReadExactly(certBuf, 0, len);
                string client2CertJson = Encoding.UTF8.GetString(certBuf);
                Certificate client2Cert = JsonSerializer.Deserialize<Certificate>(client2CertJson)!;
                Log($"Received Certificate from {client2Cert.SubjectID}");

                // 6. Sertifika Doğrulama
                if (!CryptoHelper.VerifyData(client2Cert.GetDataToSign(), client2Cert.Signature!, caPublicKey))
                {
                    Log("Client 2 Certificate Verification FAILED!");
                    return;
                }
                Log("Client 2 Certificate Verified.");

                // 7. Anahtar Değişimi (Stage 2: Master Key Establishment)
                Log("--- Stage 2: Master Key Establishment ---");
                byte[] nonce1 = new byte[32];
                RandomNumberGenerator.Fill(nonce1);
                string nonce1Str = Convert.ToBase64String(nonce1);

                string payload1 = $"Client1|{nonce1Str}";
                byte[] payload1Bytes = Encoding.UTF8.GetBytes(payload1);

                byte[] encryptedPayload1 = CryptoHelper.EncryptRSA(payload1Bytes, client2Cert.PublicKey);

                byte[] encPayload1Len = BitConverter.GetBytes(encryptedPayload1.Length);
                stream.Write(encPayload1Len, 0, 4);
                stream.Write(encryptedPayload1, 0, encryptedPayload1.Length);
                Log("Sent Encrypted ID and Nonce1 (N1).");

                stream.ReadExactly(lenBuf, 0, 4);
                len = BitConverter.ToInt32(lenBuf, 0);
                byte[] encryptedPayload2 = new byte[len];
                stream.ReadExactly(encryptedPayload2, 0, len);

                byte[] decryptedPayload2Bytes = CryptoHelper.DecryptRSA(encryptedPayload2, myPrivateKey);
                string decryptedPayload2 = Encoding.UTF8.GetString(decryptedPayload2Bytes);
                Log($"Received Encrypted Payload: {decryptedPayload2}");

                string[] parts = decryptedPayload2.Split('|');
                if (parts.Length != 2 || parts[0] != "Client2")
                {
                    Log("Invalid Payload from Client 2!");
                    return;
                }
                byte[] nonce2 = Convert.FromBase64String(parts[1]);

                byte[] combinedNonces = new byte[nonce1.Length + nonce2.Length];
                Buffer.BlockCopy(nonce1, 0, combinedNonces, 0, nonce1.Length);
                Buffer.BlockCopy(nonce2, 0, combinedNonces, nonce1.Length, nonce2.Length);

                byte[] masterKey = CryptoHelper.DeriveKey(combinedNonces);
                Log($"Master Key (Km) Generated: {Convert.ToBase64String(masterKey)}");

                // 8. Oturum Anahtarı Değişimi (Stage 3: Session Key Establishment)
                Log("--- Stage 3: Session Key Establishment ---");
                byte[] nonce3 = new byte[32];
                RandomNumberGenerator.Fill(nonce3);

                byte[] ivStage3 = new byte[16];
                RandomNumberGenerator.Fill(ivStage3);
                byte[] encryptedNonce3 = CryptoHelper.EncryptAES(Convert.ToBase64String(nonce3), masterKey, ivStage3);

                byte[] stage3MsgLen = BitConverter.GetBytes(ivStage3.Length + encryptedNonce3.Length);
                stream.Write(stage3MsgLen, 0, 4);
                stream.Write(ivStage3, 0, ivStage3.Length);
                stream.Write(encryptedNonce3, 0, encryptedNonce3.Length);
                Log("Sent Encrypted Nonce3 (N3) with Master Key.");

                stream.ReadExactly(lenBuf, 0, 4);
                len = BitConverter.ToInt32(lenBuf, 0);
                byte[] receivedStage3Bytes = new byte[len];
                stream.ReadExactly(receivedStage3Bytes, 0, len);

                byte[] receivedIvStage3 = new byte[16];
                byte[] receivedCipherStage3 = new byte[len - 16];
                Buffer.BlockCopy(receivedStage3Bytes, 0, receivedIvStage3, 0, 16);
                Buffer.BlockCopy(receivedStage3Bytes, 16, receivedCipherStage3, 0, len - 16);

                string nonce4Str = CryptoHelper.DecryptAES(receivedCipherStage3, masterKey, receivedIvStage3);
                byte[] nonce4 = Convert.FromBase64String(nonce4Str);
                Log("Received and Decrypted Nonce4 (N4).");

                byte[] combinedSessionNonces = new byte[nonce3.Length + nonce4.Length];
                Buffer.BlockCopy(nonce3, 0, combinedSessionNonces, 0, nonce3.Length);
                Buffer.BlockCopy(nonce4, 0, combinedSessionNonces, nonce3.Length, nonce4.Length);

                byte[] sessionKey = CryptoHelper.DeriveKey(combinedSessionNonces);
                Log($"Session Key (Ks) Generated: {Convert.ToBase64String(sessionKey)}");

                // 9. Güvenli İletişim
                string message = "Hello Client 2, this is a secure message from Client 1!";
                byte[] iv = new byte[16];
                RandomNumberGenerator.Fill(iv);

                byte[] encryptedMessage = CryptoHelper.EncryptAES(message, sessionKey, iv);

                byte[] msgLen = BitConverter.GetBytes(iv.Length + encryptedMessage.Length);
                stream.Write(msgLen, 0, 4);
                stream.Write(iv, 0, iv.Length);
                stream.Write(encryptedMessage, 0, encryptedMessage.Length);
                Log($"Sent Encrypted Message: {message}");

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
            string request = $"REQUEST|{clientId}|{publicKey}";
            byte[] requestBytes = Encoding.UTF8.GetBytes(request);
            stream.Write(requestBytes, 0, requestBytes.Length);

            byte[] buffer = new byte[4096];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            return JsonSerializer.Deserialize<Certificate>(response)!;
        }
    }
}
