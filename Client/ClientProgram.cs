using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    internal class ClientProgram
    {
        static void Main(string[] args)
        {
            int port = 2023;
            Console.Write("Введите IP-адрес сервера: ");
            string address = Console.ReadLine();
            try
            {
                IPEndPoint ipPoint = new(IPAddress.Parse(address), port);
                Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(ipPoint);

                string message = string.Empty;
                StringBuilder builder = new();

                do
                {
                    Console.Write("Введите сообщение для отправки на сервер: ");
                    message = Console.ReadLine();
                    //повторный ввод в случае ввода пустой строки
                    if (message == "")
                    {
                        Console.WriteLine("Отсутствует сообщение. Необходим повторный ввод...");
                        continue;
                    }    

                    Sending(socket, message);

                    //получение ответа от сервера
                    builder = Receiving(socket);
                    Console.WriteLine($"Ответ сервера: {builder}");

                } while (message != "exit");
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// метод получения сообщения с клиента
        /// </summary>
        /// <param name="socket">сокет, к которому подключен клиент</param>
        /// <returns>перезаписанный StringBuilder с сообщением с клиента</returns>
        private static StringBuilder Receiving(Socket socket)
        {
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            byte[] data = new byte[256];
            do
            {
                bytes = socket.Receive(data);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            } while (socket.Available > 0);

            return builder;
        }

        /// <summary>
        /// Отправка сообщения на сервер
        /// </summary>
        /// <param name="socket">сокет</param>
        /// <param name="message">сообщение</param>
        private static void Sending(Socket socket, string message)
        {
            byte[] data;
            data = Encoding.Unicode.GetBytes(message);
            socket.Send(data);
        }
    }
}