using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Server
{
    internal class User
    {
        [JsonInclude]
        public string login;
        [JsonInclude]
        public string password;

        [JsonInclude]
        public string root = "user";

        public User() { }

        public User(string login, string password, string root)
        {
            this.login = login;
            this.password = password;
            this.root = root;
        }

        public User(User user)
        {
            this.login = user.login;
            this.password = user.password;
            this.root = user.root;
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
        }

        /// <summary>Сериализация в json-документ</summary>
        internal static void SerializeJson(string fileName, List<User> usersList)
        {
            JsonSerializerOptions options = new()
            {
                IncludeFields = true, //включает в себя все поля
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                //для сохранения не escape-последовательностей, а привычных RU букв
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };

            using (Stream fileStream = File.Create(fileName))
            {
                JsonSerializer.Serialize(utf8Json: fileStream, value: usersList, options);
            }
        }

    }
}
