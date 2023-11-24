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

        public string root;
        public int actualIdOfUser; //текущий id пользователя

        [JsonIgnore]
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
