using SerializedCommandInterface;
using System.Text.Json;

namespace Server
{
    internal class User
    {
        public string login;
        public string password;
        public string root;
        public int actualIdOfUser; //айди текущего пользователя

        public bool isInSystem = false;

        public User() { }

        public User(string login, string password)
        {
            this.login = login;
            this.password = password;
        }

        /// <summary>Десериализация из json-документа</summary>
        internal static List<User> DeserializeJson(string fileName)
        {
            try
            {
                JsonSerializerOptions options = new()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                };

                using (Stream fileStream = File.Open(fileName, FileMode.Open))
                {
                    List<User> users = JsonSerializer.Deserialize<List<User>>(fileStream, options);
                    return users;
                }
            }
            catch(Exception e)
            {
                return null;
            }
            { }
        }

    }
}
