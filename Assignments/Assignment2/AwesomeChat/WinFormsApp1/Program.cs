using System.Net.Sockets;
using WinFormsApp1.Forms;

// Allow the test project to access internal classes (e.g. UserService, ChatService)
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("AwesomeChat.Tests")]

namespace WinFormsApp1
{
    internal static class Program
    {
        /// <summary>
        /// Application entry point. Launches the Login form.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new LoginForm());
        }

        /// <summary>
        /// Sends a byte array message to the server and returns the server's response.
        /// This interface is provided by the backend team — do not modify the signature.
        /// </summary>
        public static byte[] sendMessage(byte[] messageBytes)
        {
            const int bytesize = 1024 * 1024;
            try
            {
                System.Net.Sockets.TcpClient client = new System.Net.Sockets.TcpClient("127.0.0.1", 1234);
                NetworkStream stream = client.GetStream();

                stream.Write(messageBytes, 0, messageBytes.Length);
                Console.WriteLine("================================");
                Console.WriteLine("=   Connected to the server    =");
                Console.WriteLine("================================");
                Console.WriteLine("Waiting for response...");

                messageBytes = new byte[bytesize];
                stream.Read(messageBytes, 0, messageBytes.Length);

                stream.Dispose();
                client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return messageBytes;
        }
    }
}
