using System.Net;
using System.Net.Sockets;

namespace Server;

internal class ServerProgram
{
    static int port = 2023;
    static IPEndPoint ipPoint = new(IPAddress.Any, port);
    static Socket listenSocket =
        new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    static void Main(string[] args)
    {
        try
        {
            listenSocket.Bind(ipPoint);
            listenSocket.Listen(10);

            do
            {
                Socket handler = listenSocket.Accept();
                Console.WriteLine($"Входящее подключение от {handler.RemoteEndPoint}");

                // Создание объекта для работы с соединением в отдельном потоке
                WorkWithClient client = new WorkWithClient(handler);
                ThreadStart threadStart = new ThreadStart(client.Run);
                Thread thread = new Thread(threadStart);
                thread.Start();

            } while (true);

            listenSocket.Shutdown(SocketShutdown.Both);
            listenSocket.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}