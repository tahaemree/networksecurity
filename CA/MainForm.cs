using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Shared;

namespace CA
{
    public partial class MainForm : Form
    {
        private TcpListener? listener;
        private CancellationTokenSource? cancellationTokenSource;
        private int certificatesIssued = 0;
        private int activeConnections = 0;
        private string caPublicKey = string.Empty;
        private string caPrivateKey = string.Empty;

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

        private void UpdateStats()
        {
            if (lblCertsIssued.InvokeRequired)
            {
                lblCertsIssued.Invoke(new Action(UpdateStats));
            }
            else
            {
                lblCertsIssued.Text = $"Certificates Issued: {certificatesIssued}";
                lblActiveConnections.Text = $"Active Connections: {activeConnections}";
            }
        }

        private void UpdateStatus(string status, System.Drawing.Color color)
        {
            if (lblStatus.InvokeRequired)
            {
                lblStatus.Invoke(new Action<string, System.Drawing.Color>(UpdateStatus), status, color);
            }
            else
            {
                lblStatus.Text = $"Status: {status}";
                lblStatus.ForeColor = color;
            }
        }

        private void AddClientToList(string clientId, string ipAddress, string status)
        {
            if (listViewClients.InvokeRequired)
            {
                listViewClients.Invoke(new Action<string, string, string>(AddClientToList), clientId, ipAddress, status);
            }
            else
            {
                var item = new ListViewItem(clientId);
                item.SubItems.Add(ipAddress);
                item.SubItems.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                item.SubItems.Add(status);
                listViewClients.Items.Add(item);
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPort.Text))
            {
                MessageBox.Show("Please enter a valid port number.", "Invalid Port", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtPort.Text, out int port) || port < 1024 || port > 65535)
            {
                MessageBox.Show("Port must be between 1024 and 65535.", "Invalid Port", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnStart.Enabled = false;
            btnStop.Enabled = true;
            txtPort.Enabled = false;
            UpdateStatus("STARTING", System.Drawing.Color.Orange);
            cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => StartServer(port));
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            StopServer();
        }

        private void StartServer(int port)
        {
            try
            {
                Log("=== Certificate Authority (CA) ===");

                // 1. CA Anahtarlarını Üret
                Log("Generating CA Keys...");
                var keys = CryptoHelper.GenerateRSAKeys();
                caPublicKey = keys.PublicKey;
                caPrivateKey = keys.PrivateKey;
                Log("CA Keys Generated Successfully.");

                // Sunucuyu Başlat
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                Log($"CA Server started on port {port}");
                UpdateStatus("RUNNING", System.Drawing.Color.Green);

                while (!cancellationTokenSource!.Token.IsCancellationRequested)
                {
                    try
                    {
                        if (listener.Pending())
                        {
                            TcpClient client = listener.AcceptTcpClient();
                            Interlocked.Increment(ref activeConnections);
                            UpdateStats();
                            Task.Run(() => HandleClient(client, caPublicKey, caPrivateKey));
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (!cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            Log($"Accept Error: {ex.Message}");
                        }
                    }
                }

                Log("Server stopped.");
            }
            catch (Exception ex)
            {
                Log($"Critical Error: {ex.Message}");
                MessageBox.Show($"Server Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("ERROR", System.Drawing.Color.Red);
            }
            finally
            {
                listener?.Stop();
                if (btnStart.InvokeRequired)
                {
                    btnStart.Invoke(new Action(() =>
                    {
                        btnStart.Enabled = true;
                        btnStop.Enabled = false;
                        txtPort.Enabled = true;
                    }));
                }
                else
                {
                    btnStart.Enabled = true;
                    btnStop.Enabled = false;
                    txtPort.Enabled = true;
                }
            }
        }

        private void StopServer()
        {
            try
            {
                Log("Stopping server...");
                UpdateStatus("STOPPING", System.Drawing.Color.Orange);
                cancellationTokenSource?.Cancel();
                listener?.Stop();
                UpdateStatus("STOPPED", System.Drawing.Color.Red);
            }
            catch (Exception ex)
            {
                Log($"Error stopping server: {ex.Message}");
            }
        }

        private void HandleClient(TcpClient client, string caPublicKey, string caPrivateKey)
        {
            string? clientEndPoint = client.Client.RemoteEndPoint?.ToString();
            try
            {
                using (client)
                using (NetworkStream stream = client.GetStream())
                {
                    stream.ReadTimeout = 30000; // 30 second timeout
                    byte[] buffer = new byte[4096];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    string[] parts = request.Split('|');
                    if (parts.Length == 3 && parts[0] == "REQUEST")
                    {
                        string clientId = parts[1];
                        string clientPublicKey = parts[2];

                        Log($"Certificate Request from {clientId} [{clientEndPoint}]");
                        AddClientToList(clientId, clientEndPoint ?? "Unknown", "Certificate Requested");

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

                        Interlocked.Increment(ref certificatesIssued);
                        UpdateStats();
                        Log($"✓ Certificate issued to {clientId} (Serial: {cert.SerialNumber.Substring(0, 8)}...)");
                    }
                    else if (parts.Length > 0 && parts[0] == "GET_CA_KEY")
                    {
                        byte[] responseBytes = Encoding.UTF8.GetBytes(caPublicKey);
                        stream.Write(responseBytes, 0, responseBytes.Length);
                        Log($"Sent CA Public Key to [{clientEndPoint}]");
                    }
                    else
                    {
                        Log($"Invalid request from [{clientEndPoint}]");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"✗ Client Handling Error [{clientEndPoint}]: {ex.Message}");
            }
            finally
            {
                Interlocked.Decrement(ref activeConnections);
                UpdateStats();
            }
        }
    }
}
