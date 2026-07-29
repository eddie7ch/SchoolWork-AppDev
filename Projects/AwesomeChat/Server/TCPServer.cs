using System;
using System.Net.Sockets;
using System.Net;

namespace TcpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Loopback, 1234);
            TcpListener listener = new TcpListener(ep);
            listener.Start();

            Console.WriteLine(@"  
            ===================================================  
                   Started listening requests at: {0}:{1}  
            ===================================================",
            ep.Address, ep.Port);

            // Run the loop continuously; this is the server.  
            while (true)
            {
                const int bytesize = 1024 * 1024;

                byte[] buffer = new byte[bytesize];

                var sender = listener.AcceptTcpClient();
                using var stream = sender.GetStream();

                int totalRead = 0;
                int bytesRead;
                while ((bytesRead = stream.Read(buffer, totalRead, bytesize - totalRead)) > 0)
                {
                    totalRead += bytesRead;
                    if (!stream.DataAvailable)
                        break;
                }

                // Read the message and perform different actions  
                string message = cleanMessage(buffer);

                byte[] bytes = System.Text.Encoding.Unicode.GetBytes(message);
                Console.WriteLine("your message is: {0}", message);
                stream.Write(bytes, 0, bytes.Length);
            }
        }



        private static string cleanMessage(byte[] bytes)
        {
            string message = System.Text.Encoding.Unicode.GetString(bytes);

            string messageToPrint = string.Empty;
            foreach (var nullChar in message)
            {
                if (nullChar != '\0')
                {
                    messageToPrint += nullChar;
                }
            }
            return messageToPrint;
        }
    }
}