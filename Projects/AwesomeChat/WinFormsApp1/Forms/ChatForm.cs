using System.Net.Sockets;
using WinFormsApp1.Services;

namespace WinFormsApp1.Forms
{
    /// <summary>
    /// Chat form — allows the user to send and receive messages.
    /// Responsibilities:
    ///   - Display incoming/outgoing messages in the read-only History field (FR-07)
    ///   - Send messages via ChatService on button click or Enter key (FR-06)
    ///   - Show user-facing error dialogs on network failures (FR-10)
    ///   - Keep UI responsive by using async/await for all network calls (NFR-05)
    /// Design: Separation of Concerns — UI only; networking delegated to ChatService.
    /// </summary>
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

        // Wire Send button to async send
        private async void btnSend_Click(object sender, EventArgs e)
        {
            await SendMessageAsync();
        }

        // Allow Enter key to send message (convenience UX)
        private async void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Suppress the ding sound on Enter
                await SendMessageAsync();
            }
        }

        /// <summary>
        /// Sends the current message asynchronously.
        /// Disables the Send button during transmission to prevent double-sends.
        /// Shows specific error dialogs for each failure type (FR-10).
        /// </summary>
        private async Task SendMessageAsync()
        {
            string message = txtMessage.Text.Trim();
            if (string.IsNullOrEmpty(message))
                return;

            // Disable UI during send — prevents duplicate messages (NFR-05)
            SetSendingState(true);

            try
            {
                string response = await _chatService.SendMessageAsync(_username, message);

                // Append outgoing message to history in blue (FR-07)
                AppendToHistory($"[You] {_username}: {message}", Color.DarkBlue);

                // Append server echo/response in green
                if (!string.IsNullOrWhiteSpace(response))
                    AppendToHistory($"[Server]: {response}", Color.DarkGreen);

                txtMessage.Clear();
                SetStatus("Message sent.", Color.Gray);
            }
            catch (SocketException)
            {
                // Server is not running or connection was refused (FR-10)
                SetStatus("Error: Cannot connect to server.", Color.Red);
                MessageBox.Show(
                    "Could not connect to the chat server.\nPlease ensure the server is running and try again.",
                    "Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (TimeoutException)
            {
                // Server accepted connection but did not respond in time (FR-10)
                SetStatus("Error: Server not responding.", Color.Red);
                MessageBox.Show(
                    "The server is not responding.\nPlease check your connection and try again.",
                    "Timeout",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                // Unexpected error — log message, show generic dialog (FR-10)
                SetStatus("Unexpected error.", Color.Red);
                MessageBox.Show(
                    $"An unexpected error occurred:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                // Always re-enable UI so user can retry (NFR-06)
                SetSendingState(false);
            }
        }

        /// <summary>
        /// Appends a coloured message to the read-only History field and scrolls to bottom.
        /// </summary>
        private void AppendToHistory(string message, Color color)
        {
            rtbHistory.SelectionStart = rtbHistory.TextLength;
            rtbHistory.SelectionLength = 0;
            rtbHistory.SelectionColor = color;
            rtbHistory.AppendText(message + Environment.NewLine);
            rtbHistory.SelectionColor = rtbHistory.ForeColor;
            rtbHistory.ScrollToCaret();
        }

        /// <summary>Enables or disables the Send button and message field during a send operation.</summary>
        private void SetSendingState(bool sending)
        {
            btnSend.Enabled = !sending;
            txtMessage.Enabled = !sending;
            if (sending)
                SetStatus("Sending...", Color.Blue);
            else
                txtMessage.Focus();
        }

        /// <summary>Updates the status bar label with a message and colour.</summary>
        private void SetStatus(string message, Color color)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = color;
        }
    }
}
