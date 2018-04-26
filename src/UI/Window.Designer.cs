namespace WhalesFargo.UI
{
    partial class Window
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Window));
            this.ConsoleText = new System.Windows.Forms.TextBox();
            this.AudioLabel = new System.Windows.Forms.Label();
            this.ConnectionStatus = new System.Windows.Forms.Label();
            this.ConnectionToken = new System.Windows.Forms.TextBox();
            this.ConnectionButton = new System.Windows.Forms.Button();
            this.ConnectionStatusLabel = new System.Windows.Forms.Label();
            this.SystemTray = new System.Windows.Forms.NotifyIcon(this.components);
            this.AudioText = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // ConsoleText
            // 
            this.ConsoleText.BackColor = System.Drawing.SystemColors.Window;
            this.ConsoleText.Cursor = System.Windows.Forms.Cursors.Default;
            this.ConsoleText.ForeColor = System.Drawing.SystemColors.InfoText;
            this.ConsoleText.Location = new System.Drawing.Point(10, 85);
            this.ConsoleText.Margin = new System.Windows.Forms.Padding(2, 5, 2, 5);
            this.ConsoleText.Multiline = true;
            this.ConsoleText.Name = "ConsoleText";
            this.ConsoleText.ReadOnly = true;
            this.ConsoleText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ConsoleText.ShortcutsEnabled = false;
            this.ConsoleText.Size = new System.Drawing.Size(310, 115);
            this.ConsoleText.TabIndex = 1;
            this.ConsoleText.TabStop = false;
            // 
            // AudioLabel
            // 
            this.AudioLabel.Location = new System.Drawing.Point(10, 30);
            this.AudioLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.AudioLabel.Name = "AudioLabel";
            this.AudioLabel.Size = new System.Drawing.Size(85, 15);
            this.AudioLabel.TabIndex = 3;
            this.AudioLabel.Text = "Now Playing :";
            this.AudioLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ConnectionStatus
            // 
            this.ConnectionStatus.BackColor = System.Drawing.Color.Red;
            this.ConnectionStatus.Location = new System.Drawing.Point(130, 10);
            this.ConnectionStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ConnectionStatus.Name = "ConnectionStatus";
            this.ConnectionStatus.Size = new System.Drawing.Size(93, 15);
            this.ConnectionStatus.TabIndex = 5;
            this.ConnectionStatus.Text = "Disconnected";
            this.ConnectionStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ConnectionToken
            // 
            this.ConnectionToken.Location = new System.Drawing.Point(10, 55);
            this.ConnectionToken.Margin = new System.Windows.Forms.Padding(2, 5, 2, 5);
            this.ConnectionToken.Name = "ConnectionToken";
            this.ConnectionToken.Size = new System.Drawing.Size(215, 21);
            this.ConnectionToken.TabIndex = 6;
            this.ConnectionToken.Text = "BotToken.txt";
            this.ConnectionToken.TextChanged += new System.EventHandler(this.ConnectionToken_TextChanged);
            // 
            // ConnectionButton
            // 
            this.ConnectionButton.Location = new System.Drawing.Point(235, 53);
            this.ConnectionButton.Margin = new System.Windows.Forms.Padding(2, 5, 2, 5);
            this.ConnectionButton.Name = "ConnectionButton";
            this.ConnectionButton.Size = new System.Drawing.Size(85, 25);
            this.ConnectionButton.TabIndex = 7;
            this.ConnectionButton.Text = "Connect";
            this.ConnectionButton.UseVisualStyleBackColor = true;
            this.ConnectionButton.Click += new System.EventHandler(this.ConnectionButton_Click);
            // 
            // ConnectionStatusLabel
            // 
            this.ConnectionStatusLabel.Location = new System.Drawing.Point(10, 10);
            this.ConnectionStatusLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ConnectionStatusLabel.Name = "ConnectionStatusLabel";
            this.ConnectionStatusLabel.Size = new System.Drawing.Size(115, 15);
            this.ConnectionStatusLabel.TabIndex = 8;
            this.ConnectionStatusLabel.Text = "Connection Status :";
            // 
            // SystemTray
            // 
            this.SystemTray.BalloonTipText = "\r\n";
            this.SystemTray.Icon = ((System.Drawing.Icon)(resources.GetObject("SystemTray.Icon")));
            this.SystemTray.Text = "DiscordBot";
            this.SystemTray.DoubleClick += new System.EventHandler(this.SystemTray_DoubleClick);
            // 
            // AudioText
            // 
            this.AudioText.Location = new System.Drawing.Point(100, 30);
            this.AudioText.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.AudioText.Name = "AudioText";
            this.AudioText.Size = new System.Drawing.Size(210, 15);
            this.AudioText.TabIndex = 9;
            this.AudioText.Text = "Nothing";
            this.AudioText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Window
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.SlateGray;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(334, 211);
            this.Controls.Add(this.AudioText);
            this.Controls.Add(this.ConnectionStatusLabel);
            this.Controls.Add(this.ConnectionButton);
            this.Controls.Add(this.ConnectionToken);
            this.Controls.Add(this.ConnectionStatus);
            this.Controls.Add(this.AudioLabel);
            this.Controls.Add(this.ConsoleText);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 5, 2, 5);
            this.MaximizeBox = false;
            this.Name = "Window";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Discord Bot";
            this.Load += new System.EventHandler(this.Window_Load);
            this.SizeChanged += new System.EventHandler(this.Window_SizeChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox ConsoleText;
        private System.Windows.Forms.Label AudioLabel;
        private System.Windows.Forms.Label ConnectionStatus;
        private System.Windows.Forms.TextBox ConnectionToken;
        private System.Windows.Forms.Button ConnectionButton;
        private System.Windows.Forms.Label ConnectionStatusLabel;
        private System.Windows.Forms.NotifyIcon SystemTray;
        private System.Windows.Forms.Label AudioText;
    }
}