using System.Text.Encodings.Web;
using System.Text.Json;
using System.Xml.Serialization;

namespace SerializedCommandInterface
{
    /// <summary>
    /// Класс для сериализации объектов в списке преподавателей
    /// </summary>
    internal static class SerializeUniversity
    {
        /// <summary>Сериализация в xml-документ</summary>
        internal static void SerializeXML(string fileName, List<Professor> professorsList)
        {
            University university = new();
            university.professorsList = professorsList;

            XmlSerializer xml = new XmlSerializer(typeof(University));
            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                xml.Serialize(fs, university);
            }
        }

        /// <summary>Десериализация из xml-документа</summary>
        internal static List<Professor> DeserializeXML(string fileName)
        {
            XmlSerializer xml = new XmlSerializer(typeof(University));
            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                University? university = xml.Deserialize(fs) as University;
                return university.professorsList;
            }
        }

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

            University university = new();
            university.professorsList = professorsList;

            using (Stream fileStream = File.Create(fileName))
            {
                JsonSerializer.Serialize<University>(
                    utf8Json: fileStream, value: university, options);
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
                University? university = JsonSerializer.Deserialize<University>(fileStream, options);
                return university.professorsList;
            }
        }
    }
}
