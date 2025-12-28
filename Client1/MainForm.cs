using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Shared;

namespace Client1
{
    public partial class MainForm : Form
    {
        private TcpListener? listener;
        private TcpClient? peerClient;
        private NetworkStream? peerStream;
        private CancellationTokenSource? cancellationTokenSource;

        private string myClientId = string.Empty;
        private string myPublicKey = string.Empty;
        private string myPrivateKey = string.Empty;
        private string caPublicKey = string.Empty;
        private Certificate? myCertificate;
        private Certificate? peerCertificate;
        private byte[]? sessionKey;

        private bool isConnected = false;
        private bool isListening = false;

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
                string timestamp = DateTime.Now.ToString("HH:mm:ss");
                txtLog.AppendText($"[{timestamp}] {message}" + Environment.NewLine);
                txtLog.ScrollToCaret();
            }
        }

        private void AddChatMessage(string message, bool isSent)
        {
            if (richTextChat.InvokeRequired)
            {
                richTextChat.Invoke(new Action<string, bool>(AddChatMessage), message, isSent);
            }
            else
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss");
                int startIndex = richTextChat.TextLength;

                if (isSent)
                {
                    richTextChat.AppendText($"[{timestamp}] You: ");
                    richTextChat.SelectionStart = richTextChat.TextLength;
                    richTextChat.SelectionLength = 0;
                    richTextChat.SelectionColor = Color.Blue;
                    richTextChat.AppendText(message + Environment.NewLine);
                    richTextChat.SelectionColor = richTextChat.ForeColor;
                }
                else
                {
                    richTextChat.AppendText($"[{timestamp}] {peerCertificate?.SubjectID ?? "Peer"}: ");
                    richTextChat.SelectionStart = richTextChat.TextLength;
                    richTextChat.SelectionLength = 0;
                    richTextChat.SelectionColor = Color.Green;
                    richTextChat.AppendText(message + Environment.NewLine);
                    richTextChat.SelectionColor = richTextChat.ForeColor;
                }

                richTextChat.ScrollToCaret();
            }
        }

        private void UpdateConnectionStatus(string status, Color color)
        {
            if (lblConnectionStatus.InvokeRequired)
            {
                lblConnectionStatus.Invoke(new Action<string, Color>(UpdateConnectionStatus), status, color);
            }
            else
            {
                lblConnectionStatus.Text = status;
                lblConnectionStatus.ForeColor = color;
            }
        }

        private void UpdateCertificateStatus(string status, Color color)
        {
            if (lblCertificateStatus.InvokeRequired)
            {
                lblCertificateStatus.Invoke(new Action<string, Color>(UpdateCertificateStatus), status, color);
            }
            else
            {
                lblCertificateStatus.Text = $"Certificate: {status}";
                lblCertificateStatus.ForeColor = color;
            }
        }

        private void UpdateKeyStatus(string status, Color color)
        {
            if (lblKeyStatus.InvokeRequired)
            {
                lblKeyStatus.Invoke(new Action<string, Color>(UpdateKeyStatus), status, color);
            }
            else
            {
                lblKeyStatus.Text = $"Session: {status}";
                lblKeyStatus.ForeColor = color;
            }
        }

        private void EnableMessaging(bool enable)
        {
            if (txtMessage.InvokeRequired)
            {
                txtMessage.Invoke(new Action<bool>(EnableMessaging), enable);
            }
            else
            {
                txtMessage.Enabled = enable;
                btnSend.Enabled = enable;
                if (enable)
                {
                    txtMessage.Focus();
                }
            }
        }

        private void btnInitialize_Click(object sender, EventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtClientId.Text))
            {
                MessageBox.Show("Please enter a Client ID.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtCAPort.Text, out int caPort) || caPort < 1024 || caPort > 65535)
            {
                MessageBox.Show("CA Port must be between 1024 and 65535.", "Invalid Port", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtListenPort.Text, out int listenPort) || listenPort < 1024 || listenPort > 65535)
            {
                MessageBox.Show("Listen Port must be between 1024 and 65535.", "Invalid Port", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            myClientId = txtClientId.Text.Trim();

            // Disable controls
            txtClientId.Enabled = false;
            txtCAServer.Enabled = false;
            txtCAPort.Enabled = false;
            txtListenPort.Enabled = false;
            btnInitialize.Enabled = false;
            btnDisconnect.Enabled = true;

            cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => InitializeClient());
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            DisconnectClient();
        }

        private void DisconnectClient()
        {
            try
            {
                Log("Disconnecting...");
                cancellationTokenSource?.Cancel();
                listener?.Stop();
                peerStream?.Close();
                peerClient?.Close();

                isConnected = false;
                isListening = false;

                UpdateConnectionStatus("⚫ Disconnected", Color.Red);
                UpdateCertificateStatus("Pending", Color.Gray);
                UpdateKeyStatus("Pending", Color.Gray);
                EnableMessaging(false);

                if (txtClientId.InvokeRequired)
                {
                    txtClientId.Invoke(new Action(() =>
                    {
                        txtClientId.Enabled = true;
                        txtCAServer.Enabled = true;
                        txtCAPort.Enabled = true;
                        txtListenPort.Enabled = true;
                        btnInitialize.Enabled = true;
                        btnDisconnect.Enabled = false;
                    }));
                }
                else
                {
                    txtClientId.Enabled = true;
                    txtCAServer.Enabled = true;
                    txtCAPort.Enabled = true;
                    txtListenPort.Enabled = true;
                    btnInitialize.Enabled = true;
                    btnDisconnect.Enabled = false;
                }

                Log("Disconnected successfully.");
            }
            catch (Exception ex)
            {
                Log($"Error during disconnect: {ex.Message}");
            }
        }

        private async Task InitializeClient()
        {
            try
            {
                Log($"=== {myClientId} Initialization ===");
                UpdateConnectionStatus("⚠ Initializing", Color.Orange);

                // 1. Generate RSA Keys
                Log("Generating RSA key pair...");
                var keys = CryptoHelper.GenerateRSAKeys();
                myPublicKey = keys.PublicKey;
                myPrivateKey = keys.PrivateKey;
                Log("✓ RSA keys generated");

                // 2. Get CA Public Key
                Log("Fetching CA public key...");
                caPublicKey = await Task.Run(() => GetCAPublicKey());
                Log("✓ CA public key received");

                // 3. Request Certificate
                Log("Requesting certificate from CA...");
                myCertificate = await Task.Run(() => RequestCertificate(myClientId, myPublicKey));
                Log($"✓ Certificate received (Serial: {myCertificate.SerialNumber.Substring(0, 8)}...)");
                UpdateCertificateStatus("✓ Verified", Color.Green);

                // 4. Start Listening for Peer
                int listenPort = int.Parse(txtListenPort.Text);
                listener = new TcpListener(IPAddress.Any, listenPort);
                listener.Start();
                isListening = true;
                Log($"✓ Listening for peer on port {listenPort}");
                UpdateConnectionStatus("🟡 Waiting for peer", Color.Orange);

                // 5. Accept Peer Connection
                await Task.Run(() => AcceptPeerConnection());

            }
            catch (Exception ex)
            {
                Log($"✗ Initialization Error: {ex.Message}");
                MessageBox.Show($"Initialization failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DisconnectClient();
            }
        }

        private void AcceptPeerConnection()
        {
            try
            {
                if (listener == null || !isListening) return;

                Log("Waiting for peer to connect...");
                peerClient = listener.AcceptTcpClient();
                peerStream = peerClient.GetStream();
                peerStream.ReadTimeout = 60000; // 60 second timeout

                Log($"✓ Peer connected from {peerClient.Client.RemoteEndPoint}");
                UpdateConnectionStatus("🟢 Connected", Color.Green);
                isConnected = true;

                // Start secure handshake
                PerformSecureHandshake();

            }
            catch (Exception ex)
            {
                if (!cancellationTokenSource!.Token.IsCancellationRequested)
                {
                    Log($"✗ Peer connection error: {ex.Message}");
                    DisconnectClient();
                }
            }
        }

        private void PerformSecureHandshake()
        {
            try
            {
                if (peerStream == null) return;

                Log("=== Starting Secure Handshake ===");

                // 5. Certificate Exchange
                Log("Exchanging certificates...");

                // Send my certificate
                string myCertJson = JsonSerializer.Serialize(myCertificate);
                byte[] myCertBytes = Encoding.UTF8.GetBytes(myCertJson);
                byte[] lengthPrefix = BitConverter.GetBytes(myCertBytes.Length);
                peerStream.Write(lengthPrefix, 0, 4);
                peerStream.Write(myCertBytes, 0, myCertBytes.Length);
                Log("✓ Sent certificate to peer");

                // Receive peer certificate
                byte[] lenBuf = new byte[4];
                peerStream.ReadExactly(lenBuf, 0, 4);
                int len = BitConverter.ToInt32(lenBuf, 0);
                byte[] certBuf = new byte[len];
                peerStream.ReadExactly(certBuf, 0, len);
                string peerCertJson = Encoding.UTF8.GetString(certBuf);
                peerCertificate = JsonSerializer.Deserialize<Certificate>(peerCertJson)!;
                Log($"✓ Received certificate from {peerCertificate.SubjectID}");

                // 6. Verify Peer Certificate
                if (!CryptoHelper.VerifyData(peerCertificate.GetDataToSign(), peerCertificate.Signature!, caPublicKey))
                {
                    Log("✗ Peer certificate verification FAILED!");
                    MessageBox.Show("Peer certificate is invalid!", "Security Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    DisconnectClient();
                    return;
                }
                Log("✓ Peer certificate verified");

                // 7. Stage 2: Master Key Establishment
                Log("=== Stage 2: Master Key Establishment ===");
                UpdateKeyStatus("Establishing Master Key", Color.Orange);

                byte[] nonce1 = new byte[32];
                RandomNumberGenerator.Fill(nonce1);
                string nonce1Str = Convert.ToBase64String(nonce1);

                string payload1 = $"{myClientId}|{nonce1Str}";
                byte[] payload1Bytes = Encoding.UTF8.GetBytes(payload1);
                byte[] encryptedPayload1 = CryptoHelper.EncryptRSA(payload1Bytes, peerCertificate.PublicKey);

                byte[] encPayload1Len = BitConverter.GetBytes(encryptedPayload1.Length);
                peerStream.Write(encPayload1Len, 0, 4);
                peerStream.Write(encryptedPayload1, 0, encryptedPayload1.Length);
                Log("✓ Sent encrypted nonce N1");

                peerStream.ReadExactly(lenBuf, 0, 4);
                len = BitConverter.ToInt32(lenBuf, 0);
                byte[] encryptedPayload2 = new byte[len];
                peerStream.ReadExactly(encryptedPayload2, 0, len);

                byte[] decryptedPayload2Bytes = CryptoHelper.DecryptRSA(encryptedPayload2, myPrivateKey);
                string decryptedPayload2 = Encoding.UTF8.GetString(decryptedPayload2Bytes);
                Log("✓ Received encrypted nonce N2");

                string[] parts = decryptedPayload2.Split('|');
                if (parts.Length != 2)
                {
                    Log("✗ Invalid payload from peer!");
                    DisconnectClient();
                    return;
                }
                byte[] nonce2 = Convert.FromBase64String(parts[1]);

                byte[] combinedNonces = new byte[nonce1.Length + nonce2.Length];
                Buffer.BlockCopy(nonce1, 0, combinedNonces, 0, nonce1.Length);
                Buffer.BlockCopy(nonce2, 0, combinedNonces, nonce1.Length, nonce2.Length);

                byte[] masterKey = CryptoHelper.DeriveKey(combinedNonces);
                Log($"✓ Master Key (Km) generated");

                // 8. Stage 3: Session Key Establishment
                Log("=== Stage 3: Session Key Establishment ===");
                UpdateKeyStatus("Establishing Session Key", Color.Orange);

                byte[] nonce3 = new byte[32];
                RandomNumberGenerator.Fill(nonce3);

                byte[] ivStage3 = new byte[16];
                RandomNumberGenerator.Fill(ivStage3);
                byte[] encryptedNonce3 = CryptoHelper.EncryptAES(Convert.ToBase64String(nonce3), masterKey, ivStage3);

                byte[] stage3MsgLen = BitConverter.GetBytes(ivStage3.Length + encryptedNonce3.Length);
                peerStream.Write(stage3MsgLen, 0, 4);
                peerStream.Write(ivStage3, 0, ivStage3.Length);
                peerStream.Write(encryptedNonce3, 0, encryptedNonce3.Length);
                Log("✓ Sent encrypted nonce N3");

                peerStream.ReadExactly(lenBuf, 0, 4);
                len = BitConverter.ToInt32(lenBuf, 0);
                byte[] receivedStage3Bytes = new byte[len];
                peerStream.ReadExactly(receivedStage3Bytes, 0, len);

                byte[] receivedIvStage3 = new byte[16];
                byte[] receivedCipherStage3 = new byte[len - 16];
                Buffer.BlockCopy(receivedStage3Bytes, 0, receivedIvStage3, 0, 16);
                Buffer.BlockCopy(receivedStage3Bytes, 16, receivedCipherStage3, 0, len - 16);

                string nonce4Str = CryptoHelper.DecryptAES(receivedCipherStage3, masterKey, receivedIvStage3);
                byte[] nonce4 = Convert.FromBase64String(nonce4Str);
                Log("✓ Received encrypted nonce N4");

                byte[] combinedSessionNonces = new byte[nonce3.Length + nonce4.Length];
                Buffer.BlockCopy(nonce3, 0, combinedSessionNonces, 0, nonce3.Length);
                Buffer.BlockCopy(nonce4, 0, combinedSessionNonces, nonce3.Length, nonce4.Length);

                sessionKey = CryptoHelper.DeriveKey(combinedSessionNonces);
                Log($"✓ Session Key (Ks) generated");
                UpdateKeyStatus("✓ Established", Color.Green);

                // 9. Enable Secure Communication
                Log("=== Secure Channel Established ===");
                EnableMessaging(true);
                AddChatMessage("🔒 Secure communication channel established. You can now send messages.", false);

                // Start listening for incoming messages
                Task.Run(() => ListenForMessages());

            }
            catch (Exception ex)
            {
                Log($"✗ Handshake error: {ex.Message}");
                MessageBox.Show($"Secure handshake failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DisconnectClient();
            }
        }

        private void ListenForMessages()
        {
            try
            {
                if (peerStream == null || sessionKey == null) return;

                byte[] lenBuf = new byte[4];

                while (isConnected && !cancellationTokenSource!.Token.IsCancellationRequested)
                {
                    try
                    {
                        // Read message length
                        int bytesRead = peerStream.Read(lenBuf, 0, 4);
                        if (bytesRead == 0) break; // Connection closed

                        int len = BitConverter.ToInt32(lenBuf, 0);
                        byte[] receivedBytes = new byte[len];
                        peerStream.ReadExactly(receivedBytes, 0, len);

                        // Decrypt message
                        byte[] receivedIv = new byte[16];
                        byte[] receivedCipher = new byte[len - 16];
                        Buffer.BlockCopy(receivedBytes, 0, receivedIv, 0, 16);
                        Buffer.BlockCopy(receivedBytes, 16, receivedCipher, 0, len - 16);

                        string decryptedMessage = CryptoHelper.DecryptAES(receivedCipher, sessionKey, receivedIv);
                        Log($"✓ Received encrypted message");
                        AddChatMessage(decryptedMessage, false);
                    }
                    catch (Exception ex)
                    {
                        if (isConnected)
                        {
                            Log($"✗ Error receiving message: {ex.Message}");
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"✗ Message listener error: {ex.Message}");
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                SendMessage();
            }
        }

        private void SendMessage()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtMessage.Text)) return;
                if (peerStream == null || sessionKey == null || !isConnected) return;

                string message = txtMessage.Text.Trim();

                // Encrypt and send message
                byte[] iv = new byte[16];
                RandomNumberGenerator.Fill(iv);
                byte[] encryptedMessage = CryptoHelper.EncryptAES(message, sessionKey, iv);

                byte[] msgLen = BitConverter.GetBytes(iv.Length + encryptedMessage.Length);
                peerStream.Write(msgLen, 0, 4);
                peerStream.Write(iv, 0, iv.Length);
                peerStream.Write(encryptedMessage, 0, encryptedMessage.Length);

                Log($"✓ Sent encrypted message");
                AddChatMessage(message, true);

                if (txtMessage.InvokeRequired)
                {
                    txtMessage.Invoke(new Action(() => txtMessage.Clear()));
                }
                else
                {
                    txtMessage.Clear();
                }
            }
            catch (Exception ex)
            {
                Log($"✗ Error sending message: {ex.Message}");
                MessageBox.Show($"Failed to send message: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetCAPublicKey()
        {
            try
            {
                using TcpClient client = new TcpClient(txtCAServer.Text, int.Parse(txtCAPort.Text));
                client.ReceiveTimeout = 10000;
                using NetworkStream stream = client.GetStream();
                byte[] request = Encoding.UTF8.GetBytes("GET_CA_KEY");
                stream.Write(request, 0, request.Length);

                byte[] buffer = new byte[4096];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                return Encoding.UTF8.GetString(buffer, 0, bytesRead);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get CA public key: {ex.Message}");
            }
        }

        private Certificate RequestCertificate(string clientId, string publicKey)
        {
            try
            {
                using TcpClient client = new TcpClient(txtCAServer.Text, int.Parse(txtCAPort.Text));
                client.ReceiveTimeout = 10000;
                using NetworkStream stream = client.GetStream();
                string request = $"REQUEST|{clientId}|{publicKey}";
                byte[] requestBytes = Encoding.UTF8.GetBytes(request);
                stream.Write(requestBytes, 0, requestBytes.Length);

                byte[] buffer = new byte[8192];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                return JsonSerializer.Deserialize<Certificate>(response)!;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to request certificate: {ex.Message}");
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DisconnectClient();
        }
    }
}
