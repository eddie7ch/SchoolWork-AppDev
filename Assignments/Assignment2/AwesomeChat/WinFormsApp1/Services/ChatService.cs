using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp1.Services
{
    /// <summary>
    /// Handles sending and receiving chat messages over TCP.
    /// Design: Single Responsibility — networking only; UI concerns stay in ChatForm.
    /// </summary>
    internal class ChatService
    {
        private const string ServerAddress = "127.0.0.1";
        private const int ServerPort = 1234;
        private const int TimeoutMs = 5000; // 5 seconds — prevents indefinite blocking
        private const int BufferSize = 1024 * 1024; // 1 MB buffer (matches server)

        /// <summary>Maximum allowed message length (characters).</summary>
        internal const int MaxMessageLength = 2000;

        // Pure helpers — no I/O, fully unit-testable without a server

        internal static string FormatMessage(string senderName, string message)
            => $"{senderName}: {message}";

        // Returns null if valid, or an error string if not
        internal static string? ValidateMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return "Message cannot be empty.";

            if (message.Length > MaxMessageLength)
                return $"Message is too long ({message.Length} characters). Maximum allowed is {MaxMessageLength}.";

            return null;
        }
        public async Task<string> SendMessageAsync(string senderName, string message)
        {
            string? validationError = ValidateMessage(message);
            if (validationError != null)
                throw new ArgumentException(validationError, nameof(message));

            byte[] messageBytes = Encoding.Unicode.GetBytes(FormatMessage(senderName, message));
            byte[] responseBytes = await Task.Run(() => SendOverTcp(messageBytes));
            return Encoding.Unicode.GetString(responseBytes).TrimEnd('\0');
        }

        private byte[] SendOverTcp(byte[] messageBytes)
        {
            using var client = new TcpClient();
            client.SendTimeout = TimeoutMs;
            client.ReceiveTimeout = TimeoutMs;
            client.Connect(ServerAddress, ServerPort);

            using var stream = client.GetStream();
            stream.Write(messageBytes, 0, messageBytes.Length);

            // Loop to handle fragmented TCP reads
            byte[] buffer = new byte[BufferSize];
            int totalRead = 0;
            int bytesRead;

            while ((bytesRead = stream.Read(buffer, totalRead, BufferSize - totalRead)) > 0)
            {
                totalRead += bytesRead;
                if (!stream.DataAvailable)
                    break;
            }

            return buffer;
        }
    }
}
