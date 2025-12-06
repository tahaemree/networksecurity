using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Shared;

namespace CA
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
            btnStart.Text = "CA Server Running...";
            Task.Run(() => StartServer());
        }

        private void StartServer()
        {
            try
            {
                Log("=== Certificate Authority (CA) ===");

                // 1. CA Anahtarlarını Üret
                Log("Generating CA Keys...");
                var keys = CryptoHelper.GenerateRSAKeys();
                string caPublicKey = keys.PublicKey;
                string caPrivateKey = keys.PrivateKey;
                Log("CA Keys Generated.");

                // Sunucuyu Başlat
                int port = 5000;
                TcpListener listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                Log($"CA listening on port {port}...");

                while (true)
                {
                    try
                    {
                        // Blocking call, but we are in a Task.Run, so UI is fine.
                        TcpClient client = listener.AcceptTcpClient();
                        // Handle client in a separate task to not block other incoming connections
                        Task.Run(() => HandleClient(client, caPublicKey, caPrivateKey));
                    }
                    catch (Exception ex)
                    {
                        Log($"Accept Error: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Critical Error: {ex.Message}");
            }
        }

        private void HandleClient(TcpClient client, string caPublicKey, string caPrivateKey)
        {
            try
            {
                using (client)
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    string[] parts = request.Split('|');
                    if (parts.Length == 3 && parts[0] == "REQUEST")
                    {
                        string clientId = parts[1];
                        string clientPublicKey = parts[2];

                        Log($"Received Certificate Request from {clientId}");

                        var cert = new Certificate
                        {
                            SubjectID = clientId,
                            PublicKey = clientPublicKey,
                            Validity = DateTime.UtcNow.AddYears(1),
                            SerialNumber = Guid.NewGuid().ToString()
                        };

                        byte[] dataToSign = cert.GetDataToSign();
                        cert.Signature = CryptoHelper.SignData(dataToSign, caPrivateKey);

                        string response = JsonSerializer.Serialize(cert);
                        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                        stream.Write(responseBytes, 0, responseBytes.Length);
                        Log($"Certificate issued to {clientId}");
                    }
                    else if (parts.Length > 0 && parts[0] == "GET_CA_KEY")
                    {
                        byte[] responseBytes = Encoding.UTF8.GetBytes(caPublicKey);
                        stream.Write(responseBytes, 0, responseBytes.Length);
                        Log("Sent CA Public Key.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Client Handling Error: {ex.Message}");
            }
        }
    }
}
