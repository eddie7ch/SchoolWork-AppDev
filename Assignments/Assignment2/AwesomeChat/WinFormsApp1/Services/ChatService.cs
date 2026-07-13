using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp1.Services
{
    /// <summary>
    /// Handles sending and receiving chat messages over TCP.
    /// Wraps the sendMessage interface provided by the backend team with:
    ///   - Async/await to keep the UI thread responsive
    ///   - Timeouts to prevent the app from hanging
    ///   - Proper resource disposal via using statements
    ///   - Read loop to handle fragmented TCP responses
    /// Design: Single Responsibility — this class only handles network communication.
    ///
    /// DCR (Design Change Request): Two new testable methods were extracted:
    ///   - FormatMessage: builds the "Name: message" string (pure function, no I/O)
    ///   - ValidateMessage: checks length/content rules before sending (pure function, no I/O)
    /// Both methods are internal static, so tests can call them directly without a server.
    /// </summary>
    internal class ChatService
    {
        private const string ServerAddress = "127.0.0.1";
        private const int ServerPort = 1234;
        private const int TimeoutMs = 5000; // 5 seconds — prevents indefinite blocking
        private const int BufferSize = 1024 * 1024; // 1 MB buffer (matches server)

        /// <summary>Maximum allowed message length (characters).</summary>
        internal const int MaxMessageLength = 2000;

        // ── DCR: Pure helper methods — no network, no I/O; fully unit-testable ───

        /// <summary>
        /// Formats a message for transmission: "SenderName: message text".
        /// Extracted so tests can verify the wire format independently of TCP.
        /// </summary>
        internal static string FormatMessage(string senderName, string message)
            => $"{senderName}: {message}";

        /// <summary>
        /// Validates a message before it is sent.
        /// Returns null if the message is valid, or an error description if it is not.
        /// Null-safe — callers can use the return value directly in a conditional.
        /// </summary>
        /// <param name="message">The message text entered by the user.</param>
        /// <returns>Null on success, or a human-readable error string on failure.</returns>
        internal static string? ValidateMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return "Message cannot be empty.";

            if (message.Length > MaxMessageLength)
                return $"Message is too long ({message.Length} characters). Maximum allowed is {MaxMessageLength}.";

            return null; // null → valid
        }

        // ── Public async API ───────────────────────────────────────────────────

        /// <summary>
        /// Sends a message to the server asynchronously and returns the server's response.
        /// Validates the message before sending; throws ArgumentException for invalid input.
        /// Runs the TCP operation on a background thread to avoid blocking the UI.
        /// </summary>
        /// <param name="senderName">The sender's display name (prepended to the message).</param>
        /// <param name="message">The message text to send.</param>
        /// <returns>The decoded response string from the server.</returns>
        /// <exception cref="ArgumentException">Thrown when the message fails validation.</exception>
        /// <exception cref="SocketException">Thrown when the server cannot be reached.</exception>
        /// <exception cref="TimeoutException">Thrown when the server does not respond within the timeout.</exception>
        public async Task<string> SendMessageAsync(string senderName, string message)
        {
            // Validate before committing to a network call
            string? validationError = ValidateMessage(message);
            if (validationError != null)
                throw new ArgumentException(validationError, nameof(message));

            string fullMessage = FormatMessage(senderName, message);
            byte[] messageBytes = Encoding.Unicode.GetBytes(fullMessage);

            // Run blocking TCP call on a thread pool thread to keep UI responsive
            byte[] responseBytes = await Task.Run(() => SendOverTcp(messageBytes));

            // Decode response and strip null characters (server buffer padding)
            return Encoding.Unicode.GetString(responseBytes).TrimEnd('\0');
        }

        /// <summary>
        /// Performs the blocking TCP send/receive operation.
        /// Uses using statements to guarantee resource disposal on any exit path.
        /// </summary>
        private byte[] SendOverTcp(byte[] messageBytes)
        {
            // using ensures TcpClient and NetworkStream are disposed even if an exception occurs
            using var client = new TcpClient();
            client.SendTimeout = TimeoutMs;
            client.ReceiveTimeout = TimeoutMs;

            // Connect — throws SocketException if server is not running
            client.Connect(ServerAddress, ServerPort);

            using var stream = client.GetStream();

            // Send the encoded message bytes
            stream.Write(messageBytes, 0, messageBytes.Length);

            // Read response in a loop to handle fragmented TCP reads
            byte[] buffer = new byte[BufferSize];
            int totalRead = 0;
            int bytesRead;

            while ((bytesRead = stream.Read(buffer, totalRead, BufferSize - totalRead)) > 0)
            {
                totalRead += bytesRead;
                // Stop reading when no more data is immediately available
                if (!stream.DataAvailable)
                    break;
            }

            return buffer;
        }
    }
}
