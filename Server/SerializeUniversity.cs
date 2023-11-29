using System.Text.Encodings.Web;
using System.Text.Json;

namespace Server
{
    /// <summary>
    /// Класс для сериализации объектов в списке преподавателей
    /// </summary>
    internal static class SerializeUniversity
    {

        /// <summary>Сериализация в json-документ</summary>
        internal static void SerializeJson(string fileName, List<Professor> professorsList)
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
                JsonSerializer.Serialize(
                    utf8Json: fileStream, value: professorsList, options);
            }
        }

        /// <summary>Десериализация из json-документа</summary>
        internal static List<Professor> DeserializeJson(string fileName)
        {
            JsonSerializerOptions options = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            using (Stream fileStream = File.Open(fileName, FileMode.Open))
            {
                List<Professor> professors = JsonSerializer.Deserialize<List<Professor>>(fileStream, options);
                return professors;
            }
        }
    }
}
