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
    /// </summary>
    internal class ChatService
    {
        private const string ServerAddress = "127.0.0.1";
        private const int ServerPort = 1234;
        private const int TimeoutMs = 5000; // 5 seconds — prevents indefinite blocking
        private const int BufferSize = 1024 * 1024; // 1 MB buffer (matches server)

        /// <summary>
        /// Sends a message to the server asynchronously and returns the server's response.
        /// Runs the TCP operation on a background thread to avoid blocking the UI.
        /// </summary>
        /// <param name="senderName">The sender's display name (prepended to the message).</param>
        /// <param name="message">The message text to send.</param>
        /// <returns>The decoded response string from the server.</returns>
        /// <exception cref="SocketException">Thrown when the server cannot be reached.</exception>
        /// <exception cref="TimeoutException">Thrown when the server does not respond within the timeout.</exception>
        public async Task<string> SendMessageAsync(string senderName, string message)
        {
            string fullMessage = $"{senderName}: {message}";
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
