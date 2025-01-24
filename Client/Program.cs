using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            const string serverIP = "127.0.0.1";
            const int port = 1234;

            using Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                clientSocket.Connect(new IPEndPoint(IPAddress.Parse(serverIP), port));
                Console.WriteLine("Developed by Kaustubh");
                Console.WriteLine("Connected to the server.");

                Console.Write("Enter a string (e.g., SetA-One): ");

                string input = Console.ReadLine() ?? string.Empty;
                string encryptedMessage = EncryptMessage(input);

                //Console.WriteLine();
                Console.WriteLine($"Encrypted Message Sent: {encryptedMessage}");

                clientSocket.Send(Encoding.UTF8.GetBytes(encryptedMessage));
                Console.WriteLine("Message sent. Waiting for response...");

                while (true)
                {
                    byte[] buffer = new byte[1024];
                    int received = clientSocket.Receive(buffer);

                    if (received == 0)
                    {
                        Console.WriteLine("Server closed the connection.");
                        break;
                    }

                    string encryptedResponse = Encoding.UTF8.GetString(buffer, 0, received);

                    Console.WriteLine("");
                    Console.WriteLine($"Encrypted Server Response: {encryptedResponse}");

                    string response = DecryptMessage(encryptedResponse);

                    Console.WriteLine($"Decrypted Server Response: {response}");

                    if (response == "EMPTY") break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            Console.WriteLine("");
            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }

        static string EncryptMessage(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            return Convert.ToBase64String(data);
        }

        static string DecryptMessage(string encryptedMessage)
        {
            byte[] data = Convert.FromBase64String(encryptedMessage);
            return Encoding.UTF8.GetString(data);
        }
    }
}
