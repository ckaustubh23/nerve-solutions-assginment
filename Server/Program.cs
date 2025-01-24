using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Server
{
    class Program
    {
        static readonly string dataCollection = @"{
            ""SetA"": [{""One"": 1, ""Two"": 2}],
            ""SetB"": [{""Three"": 3, ""Four"": 4}],
            ""SetC"": [{""Five"": 5, ""Six"": 6}],
            ""SetD"": [{""Seven"": 7, ""Eight"": 8}],
            ""SetE"": [{""Nine"": 9, ""Ten"": 10}]
        }";

        static void Main(string[] args)
        {
            const int port = 1234;
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
                serverSocket.Listen(100);
                Console.WriteLine($"Server started. Listening on port {port}...");

                while (true)
                {
                    Socket clientSocket = serverSocket.Accept();
                    Console.WriteLine("Client connected.");
                    ThreadPool.QueueUserWorkItem(HandleClient, clientSocket);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void HandleClient(object? obj)
        {
            if (obj is Socket clientSocket)
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    int received = clientSocket.Receive(buffer);
                    string encryptedMessage = Encoding.UTF8.GetString(buffer, 0, received);
                    string clientMessage = DecryptMessage(encryptedMessage);

                    Console.WriteLine($"Decrypted Client Message: {clientMessage}");

                    string[] parts = clientMessage.Split('-');
                    string response = "EMPTY";

                    if (parts.Length == 2)
                    {
                        string key = parts[0];
                        string subsetKey = parts[1];

                        var jsonDoc = JsonDocument.Parse(dataCollection);
                        if (jsonDoc.RootElement.TryGetProperty(key, out var subset))
                        {
                            foreach (var item in subset.EnumerateArray())
                            {
                                if (item.TryGetProperty(subsetKey, out var value))
                                {
                                    int n = value.GetInt32();

                                    for (int i = 0; i < n; i++)
                                    {
                                        string timeMessage = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                        string encryptedTime = EncryptMessage(timeMessage);

                                        Console.WriteLine($"Encrypted Time Message: {encryptedTime}");

                                        clientSocket.Send(Encoding.UTF8.GetBytes(encryptedTime));
                                        Thread.Sleep(1000);
                                    }

                                    return;
                                }
                            }
                        }
                    }

                    string encryptedEmpty = EncryptMessage(response);

                    // Display the encrypted empty response
                    Console.WriteLine($"Encrypted Empty Response: {encryptedEmpty}");

                    clientSocket.Send(Encoding.UTF8.GetBytes(encryptedEmpty));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Client handling error: {ex.Message}");
                }
                finally
                {
                    clientSocket.Close();
                }
            }
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
