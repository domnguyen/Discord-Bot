using System;
using System.Windows.Forms;
using WhalesFargo.Helpers;

namespace WhalesFargo.UI
{
    public partial class Window : Form
    {
        private DiscordBot m_DiscordBot = null; // Reference to the bot.
        private Timer m_AudioTextTimer = new Timer(); // Text timer to scroll the audio's title.
        private const int m_AudioTextInterval = 600; // Interval for scroll speed (in milliseconds).

        public Window(DiscordBot bot)
        {
            InitializeComponent();
            m_DiscordBot = bot;
        }

        private void Window_Load(object sender, EventArgs e)
        {
            SetToken(ConnectionToken.Text);
            m_AudioTextTimer.Interval = m_AudioTextInterval;
            m_AudioTextTimer.Tick += new System.EventHandler(AudioText_Scroll);
        }

        private void Window_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                SystemTray.Visible = true;
            }
        }

        private void SystemTray_DoubleClick(object sender, EventArgs e)
        {
            Show();
            SystemTray.Visible = false;
            WindowState = FormWindowState.Normal;
        }

        private void AudioText_Scroll(object Sender, EventArgs e)
        {
            if (AudioText.Text.Length > 0)
                AudioText.Text = AudioText.Text.Substring(1, AudioText.Text.Length - 1) + AudioText.Text.Substring(0,1);
        }

        private void ConnectionToken_TextChanged(object sender, EventArgs e)
        {
            SetToken(ConnectionToken.Text);
        }

        private void SetToken(string s)
        {
            if (m_DiscordBot != null) m_DiscordBot.SetBotToken(s);
        }

        private void ConnectionButton_Click(object sender, EventArgs e)
        {
            if (DiscordBot.ConnectionStatus.Equals(Strings.Connecting))
            {
                Program.Cancel();
                ConnectionButton.Text = Strings.ConnectButton;
            }
            else
            {
                Program.Run();
                ConnectionButton.Text = Strings.CancelButton;
            }
        }

        public void DisableConnectionToken()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(DisableConnectionToken));
                return;
            }
            ConnectionToken.Enabled = false;
            ConnectionButton.Enabled = false;
        }

        public void SetConnectionStatus(string s)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(SetConnectionStatus), new object[] { s });
                return;
            }
            ConnectionStatus.Text = s;
            if (s.Equals(Strings.Disconnected)) ConnectionStatus.BackColor = System.Drawing.Color.Red;
            if (s.Equals(Strings.Connecting)) ConnectionStatus.BackColor = System.Drawing.Color.Yellow;
            if (s.Equals(Strings.Connected)) ConnectionStatus.BackColor = System.Drawing.Color.Green;
        }

        public void SetConsoleText(string s)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(SetConsoleText), new object[] { s });
                return;
            }
            ConsoleText.AppendText($"{s}\r\n");
        }

        public void SetAudioText(string s)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(SetAudioText), new object[] { s });
                return;
            }

            AudioText.Text = s.PadRight(s.Length + 20);

            // Turn off scroll if we're playing nothing.
            if (s.Equals(Strings.NotPlaying))
            {
                m_AudioTextTimer.Enabled = false;
                return;
            }
            else if (m_AudioTextTimer.Enabled == false) m_AudioTextTimer.Enabled = true;

            // If desktop notifications and we didn't return, show balloon text.
            if (m_DiscordBot != null && m_DiscordBot.GetDesktopNotifications() && SystemTray.Visible)
            {
                SystemTray.BalloonTipText = s;
                SystemTray.ShowBalloonTip(1000);
            }
        }
    }
}
