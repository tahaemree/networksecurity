namespace Client2
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelTop = new System.Windows.Forms.Panel();
            this.lblClientId = new System.Windows.Forms.Label();
            this.txtClientId = new System.Windows.Forms.TextBox();
            this.lblCAServer = new System.Windows.Forms.Label();
            this.txtCAServer = new System.Windows.Forms.TextBox();
            this.lblCAPort = new System.Windows.Forms.Label();
            this.txtCAPort = new System.Windows.Forms.TextBox();
            this.lblPeerServer = new System.Windows.Forms.Label();
            this.txtPeerServer = new System.Windows.Forms.TextBox();
            this.lblPeerPort = new System.Windows.Forms.Label();
            this.txtPeerPort = new System.Windows.Forms.TextBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnDisconnect = new System.Windows.Forms.Button();
            this.panelStatus = new System.Windows.Forms.Panel();
            this.lblConnectionStatus = new System.Windows.Forms.Label();
            this.lblCertificateStatus = new System.Windows.Forms.Label();
            this.lblKeyStatus = new System.Windows.Forms.Label();
            this.groupChat = new System.Windows.Forms.GroupBox();
            this.richTextChat = new System.Windows.Forms.RichTextBox();
            this.panelSendMessage = new System.Windows.Forms.Panel();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.groupLog = new System.Windows.Forms.GroupBox();
            this.txtLog = new System.Windows.Forms.RichTextBox();
            this.panelTop.SuspendLayout();
            this.panelStatus.SuspendLayout();
            this.groupChat.SuspendLayout();
            this.panelSendMessage.SuspendLayout();
            this.groupLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.btnDisconnect);
            this.panelTop.Controls.Add(this.btnConnect);
            this.panelTop.Controls.Add(this.txtPeerPort);
            this.panelTop.Controls.Add(this.lblPeerPort);
            this.panelTop.Controls.Add(this.txtPeerServer);
            this.panelTop.Controls.Add(this.lblPeerServer);
            this.panelTop.Controls.Add(this.txtCAPort);
            this.panelTop.Controls.Add(this.lblCAPort);
            this.panelTop.Controls.Add(this.txtCAServer);
            this.panelTop.Controls.Add(this.lblCAServer);
            this.panelTop.Controls.Add(this.txtClientId);
            this.panelTop.Controls.Add(this.lblClientId);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Padding = new System.Windows.Forms.Padding(10);
            this.panelTop.Size = new System.Drawing.Size(1000, 100);
            this.panelTop.TabIndex = 0;
            // 
            // lblClientId
            // 
            this.lblClientId.AutoSize = true;
            this.lblClientId.Location = new System.Drawing.Point(13, 20);
            this.lblClientId.Name = "lblClientId";
            this.lblClientId.Size = new System.Drawing.Size(54, 15);
            this.lblClientId.TabIndex = 0;
            this.lblClientId.Text = "Client ID:";
            // 
            // txtClientId
            // 
            this.txtClientId.Location = new System.Drawing.Point(73, 17);
            this.txtClientId.Name = "txtClientId";
            this.txtClientId.Size = new System.Drawing.Size(150, 23);
            this.txtClientId.TabIndex = 1;
            this.txtClientId.Text = "Client2";
            // 
            // lblCAServer
            // 
            this.lblCAServer.AutoSize = true;
            this.lblCAServer.Location = new System.Drawing.Point(13, 49);
            this.lblCAServer.Name = "lblCAServer";
            this.lblCAServer.Size = new System.Drawing.Size(63, 15);
            this.lblCAServer.TabIndex = 2;
            this.lblCAServer.Text = "CA Server:";
            // 
            // txtCAServer
            // 
            this.txtCAServer.Location = new System.Drawing.Point(82, 46);
            this.txtCAServer.Name = "txtCAServer";
            this.txtCAServer.Size = new System.Drawing.Size(150, 23);
            this.txtCAServer.TabIndex = 3;
            this.txtCAServer.Text = "localhost";
            // 
            // lblCAPort
            // 
            this.lblCAPort.AutoSize = true;
            this.lblCAPort.Location = new System.Drawing.Point(238, 49);
            this.lblCAPort.Name = "lblCAPort";
            this.lblCAPort.Size = new System.Drawing.Size(51, 15);
            this.lblCAPort.TabIndex = 4;
            this.lblCAPort.Text = "CA Port:";
            // 
            // txtCAPort
            // 
            this.txtCAPort.Location = new System.Drawing.Point(295, 46);
            this.txtCAPort.Name = "txtCAPort";
            this.txtCAPort.Size = new System.Drawing.Size(80, 23);
            this.txtCAPort.TabIndex = 5;
            this.txtCAPort.Text = "5000";
            // 
            // lblPeerServer
            // 
            this.lblPeerServer.AutoSize = true;
            this.lblPeerServer.Location = new System.Drawing.Point(238, 20);
            this.lblPeerServer.Name = "lblPeerServer";
            this.lblPeerServer.Size = new System.Drawing.Size(70, 15);
            this.lblPeerServer.TabIndex = 6;
            this.lblPeerServer.Text = "Peer Server:";
            // 
            // txtPeerServer
            // 
            this.txtPeerServer.Location = new System.Drawing.Point(314, 17);
            this.txtPeerServer.Name = "txtPeerServer";
            this.txtPeerServer.Size = new System.Drawing.Size(100, 23);
            this.txtPeerServer.TabIndex = 7;
            this.txtPeerServer.Text = "localhost";
            // 
            // lblPeerPort
            // 
            this.lblPeerPort.AutoSize = true;
            this.lblPeerPort.Location = new System.Drawing.Point(420, 20);
            this.lblPeerPort.Name = "lblPeerPort";
            this.lblPeerPort.Size = new System.Drawing.Size(58, 15);
            this.lblPeerPort.TabIndex = 8;
            this.lblPeerPort.Text = "Peer Port:";
            // 
            // txtPeerPort
            // 
            this.txtPeerPort.Location = new System.Drawing.Point(484, 17);
            this.txtPeerPort.Name = "txtPeerPort";
            this.txtPeerPort.Size = new System.Drawing.Size(80, 23);
            this.txtPeerPort.TabIndex = 9;
            this.txtPeerPort.Text = "6001";
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(580, 17);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(120, 52);
            this.btnConnect.TabIndex = 10;
            this.btnConnect.Text = "Connect to Peer";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnDisconnect
            // 
            this.btnDisconnect.Enabled = false;
            this.btnDisconnect.Location = new System.Drawing.Point(710, 17);
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new System.Drawing.Size(100, 52);
            this.btnDisconnect.TabIndex = 11;
            this.btnDisconnect.Text = "Disconnect";
            this.btnDisconnect.UseVisualStyleBackColor = true;
            this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);
            // 
            // panelStatus
            // 
            this.panelStatus.Controls.Add(this.lblKeyStatus);
            this.panelStatus.Controls.Add(this.lblCertificateStatus);
            this.panelStatus.Controls.Add(this.lblConnectionStatus);
            this.panelStatus.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelStatus.Location = new System.Drawing.Point(0, 100);
            this.panelStatus.Name = "panelStatus";
            this.panelStatus.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);
            this.panelStatus.Size = new System.Drawing.Size(1000, 40);
            this.panelStatus.TabIndex = 1;
            // 
            // lblConnectionStatus
            // 
            this.lblConnectionStatus.AutoSize = true;
            this.lblConnectionStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblConnectionStatus.ForeColor = System.Drawing.Color.Red;
            this.lblConnectionStatus.Location = new System.Drawing.Point(13, 12);
            this.lblConnectionStatus.Name = "lblConnectionStatus";
            this.lblConnectionStatus.Size = new System.Drawing.Size(89, 15);
            this.lblConnectionStatus.TabIndex = 0;
            this.lblConnectionStatus.Text = "⚫ Disconnected";
            // 
            // lblCertificateStatus
            // 
            this.lblCertificateStatus.AutoSize = true;
            this.lblCertificateStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblCertificateStatus.ForeColor = System.Drawing.Color.Gray;
            this.lblCertificateStatus.Location = new System.Drawing.Point(200, 12);
            this.lblCertificateStatus.Name = "lblCertificateStatus";
            this.lblCertificateStatus.Size = new System.Drawing.Size(107, 15);
            this.lblCertificateStatus.TabIndex = 1;
            this.lblCertificateStatus.Text = "Certificate: Pending";
            // 
            // lblKeyStatus
            // 
            this.lblKeyStatus.AutoSize = true;
            this.lblKeyStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblKeyStatus.ForeColor = System.Drawing.Color.Gray;
            this.lblKeyStatus.Location = new System.Drawing.Point(400, 12);
            this.lblKeyStatus.Name = "lblKeyStatus";
            this.lblKeyStatus.Size = new System.Drawing.Size(92, 15);
            this.lblKeyStatus.TabIndex = 2;
            this.lblKeyStatus.Text = "Session: Pending";
            // 
            // groupChat
            // 
            this.groupChat.Controls.Add(this.richTextChat);
            this.groupChat.Controls.Add(this.panelSendMessage);
            this.groupChat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupChat.Location = new System.Drawing.Point(0, 140);
            this.groupChat.Name = "groupChat";
            this.groupChat.Padding = new System.Windows.Forms.Padding(10);
            this.groupChat.Size = new System.Drawing.Size(1000, 260);
            this.groupChat.TabIndex = 2;
            this.groupChat.TabStop = false;
            this.groupChat.Text = "Secure Communication";
            // 
            // richTextChat
            // 
            this.richTextChat.BackColor = System.Drawing.Color.White;
            this.richTextChat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextChat.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.richTextChat.Location = new System.Drawing.Point(10, 26);
            this.richTextChat.Name = "richTextChat";
            this.richTextChat.ReadOnly = true;
            this.richTextChat.Size = new System.Drawing.Size(980, 174);
            this.richTextChat.TabIndex = 0;
            this.richTextChat.Text = "";
            // 
            // panelSendMessage
            // 
            this.panelSendMessage.Controls.Add(this.btnSend);
            this.panelSendMessage.Controls.Add(this.txtMessage);
            this.panelSendMessage.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelSendMessage.Location = new System.Drawing.Point(10, 200);
            this.panelSendMessage.Name = "panelSendMessage";
            this.panelSendMessage.Size = new System.Drawing.Size(980, 50);
            this.panelSendMessage.TabIndex = 1;
            // 
            // txtMessage
            // 
            this.txtMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMessage.Enabled = false;
            this.txtMessage.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtMessage.Location = new System.Drawing.Point(0, 0);
            this.txtMessage.Multiline = true;
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.PlaceholderText = "Type your message here...";
            this.txtMessage.Size = new System.Drawing.Size(880, 50);
            this.txtMessage.TabIndex = 0;
            this.txtMessage.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtMessage_KeyDown);
            // 
            // btnSend
            // 
            this.btnSend.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnSend.Enabled = false;
            this.btnSend.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSend.Location = new System.Drawing.Point(880, 0);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(100, 50);
            this.btnSend.TabIndex = 1;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // groupLog
            // 
            this.groupLog.Controls.Add(this.txtLog);
            this.groupLog.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupLog.Location = new System.Drawing.Point(0, 400);
            this.groupLog.Name = "groupLog";
            this.groupLog.Padding = new System.Windows.Forms.Padding(10);
            this.groupLog.Size = new System.Drawing.Size(1000, 200);
            this.groupLog.TabIndex = 3;
            this.groupLog.TabStop = false;
            this.groupLog.Text = "System Log";
            // 
            // txtLog
            // 
            this.txtLog.BackColor = System.Drawing.Color.Black;
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Font = new System.Drawing.Font("Consolas", 8F);
            this.txtLog.ForeColor = System.Drawing.Color.Lime;
            this.txtLog.Location = new System.Drawing.Point(10, 26);
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.Size = new System.Drawing.Size(980, 164);
            this.txtLog.TabIndex = 0;
            this.txtLog.Text = "";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 600);
            this.Controls.Add(this.groupChat);
            this.Controls.Add(this.groupLog);
            this.Controls.Add(this.panelStatus);
            this.Controls.Add(this.panelTop);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Client 2 - Secure Messenger";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelStatus.ResumeLayout(false);
            this.panelStatus.PerformLayout();
            this.groupChat.ResumeLayout(false);
            this.panelSendMessage.ResumeLayout(false);
            this.panelSendMessage.PerformLayout();
            this.groupLog.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label lblClientId;
        private System.Windows.Forms.TextBox txtClientId;
        private System.Windows.Forms.Label lblCAServer;
        private System.Windows.Forms.TextBox txtCAServer;
        private System.Windows.Forms.Label lblCAPort;
        private System.Windows.Forms.TextBox txtCAPort;
        private System.Windows.Forms.Label lblPeerServer;
        private System.Windows.Forms.TextBox txtPeerServer;
        private System.Windows.Forms.Label lblPeerPort;
        private System.Windows.Forms.TextBox txtPeerPort;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnDisconnect;
        private System.Windows.Forms.Panel panelStatus;
        private System.Windows.Forms.Label lblConnectionStatus;
        private System.Windows.Forms.Label lblCertificateStatus;
        private System.Windows.Forms.Label lblKeyStatus;
        private System.Windows.Forms.GroupBox groupChat;
        private System.Windows.Forms.RichTextBox richTextChat;
        private System.Windows.Forms.Panel panelSendMessage;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.GroupBox groupLog;
        private System.Windows.Forms.RichTextBox txtLog;
    }
}
