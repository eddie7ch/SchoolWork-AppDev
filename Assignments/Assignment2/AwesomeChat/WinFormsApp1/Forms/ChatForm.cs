using System.Net.Sockets;
using System.Diagnostics.CodeAnalysis;
using Serilog;
using WinFormsApp1.Services;

namespace WinFormsApp1.Forms
{
    /// <summary>
    /// Chat form — send/receive messages via ChatService.
    /// Design: Separation of Concerns — UI only; networking delegated to ChatService.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class ChatForm : Form
    {
        private readonly string _username;
        private readonly ChatService _chatService;

        public ChatForm(string username)
        {
            InitializeComponent();
            _username = username;
            _chatService = new ChatService();
            lblUserInfo.Text = $"Logged in as: {username}";
        }

        private async void btnSend_Click(object sender, EventArgs e)
            => await SendMessageAsync();

        private async void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await SendMessageAsync();
            }
        }

        private async Task SendMessageAsync()
        {
            string message = txtMessage.Text.Trim();
            if (string.IsNullOrEmpty(message))
                return;

            SetSendingState(true);

            try
            {
                string response = await _chatService.SendMessageAsync(_username, message);

                AppendToHistory($"[You] {_username}: {message}", Color.DarkBlue);

                if (!string.IsNullOrWhiteSpace(response))
                    AppendToHistory($"[Server]: {response}", Color.DarkGreen);

                txtMessage.Clear();
                SetStatus("Message sent.", Color.Gray);
            }
            catch (ArgumentException ex)
            {
                Log.Warning("Message validation failed: {Error}", ex.Message);
                SetStatus("Message not sent — validation failed.", Color.Red);
                MessageBox.Show(ex.Message, "Invalid Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (SocketException ex)
            {
                Log.Error(ex, "TCP connection failed — server may be offline");
                SetStatus("Error: Cannot connect to server.", Color.Red);
                MessageBox.Show(
                    "Could not connect to the chat server.\nPlease ensure the server is running and try again.",
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (TimeoutException ex)
            {
                Log.Error(ex, "Server did not respond within {Timeout}ms", ChatService.TimeoutMs);
                SetStatus("Error: Server not responding.", Color.Red);
                MessageBox.Show(
                    "The server is not responding.\nPlease check your connection and try again.",
                    "Timeout", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error during message send");
                SetStatus("Unexpected error.", Color.Red);
                MessageBox.Show($"An unexpected error occurred:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetSendingState(false);
            }
        }

        private void AppendToHistory(string message, Color color)
        {
            rtbHistory.SelectionStart = rtbHistory.TextLength;
            rtbHistory.SelectionLength = 0;
            rtbHistory.SelectionColor = color;
            rtbHistory.AppendText(message + Environment.NewLine);
            rtbHistory.SelectionColor = rtbHistory.ForeColor;
            rtbHistory.ScrollToCaret();
        }

        private void SetSendingState(bool sending)
        {
            btnSend.Enabled = !sending;
            txtMessage.Enabled = !sending;
            if (sending)
                SetStatus("Sending...", Color.Blue);
            else
                txtMessage.Focus();
        }

        private void SetStatus(string message, Color color)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = color;
        }
    }
}
