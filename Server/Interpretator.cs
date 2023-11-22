using SerializedCommandInterface;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using static System.Console;

namespace Server;

//класс, который будет обрабатывать пользовательские команды и возвращать результат
internal class Interpretator
{
    //возвращаемая строка
    public StringBuilder answer = new StringBuilder();

    //путь к директории файлов для сериализации
    private static string path = Path.GetFullPath(Directory.GetCurrentDirectory() + @"\..\..\..\" + "university.json");

    //статическое поле для хранения информации, с которой работают все пользователи
    private static List<Professor> professors = new List<Professor>();

    public string Execute(string messageFromClient)
    {
        string[] command = messageFromClient.Split('|', '_');
        command[0] = command[0].ToLower(); //форматирование ввода в нижнем регистре

        answer.Clear();
        switch (command[0])
        {
            case "help":
                answer.Append(Help(command));
                break;
            case "addprof":
                answer.Append(AddProf(command));
                break;
            case "del":
                answer.Append(Del(command));
                break;
            case "ser":
                answer.Append(Serializing(command));
                break;
            case "deser":
                answer.Append(Deserialization(command));
                break;
            case "list":
                answer.Append(List(command));
                break;
            case "exit":
                WriteLine("Выход из программы...");
                break;
            default:
                WriteLine("Неизвестная команда. Повторите ввод.");
                WriteLine("help - для вызова справки.");
                break;
        }
        return answer.ToString();
    }

    #region поля, необходимые для работы справочника по командам, вызывается через help
    private static Dictionary<string, string> aboutCommands = new()
    {
        { "help", "получить справку по командам" },
        { "addprof",
            "Добавить нового преподавателя. \n (аргументы: Ф_И_О_дисциплина " +
            "ИЛИ Ф_И_О_дисциплина_дата трудоустройства)" },

        { "exit", "Завершить работу программы" },
        { "list", "Вывести список преподавателей. \n (аргументы: id ИЛИ id_'period')" },
        { "del",  "удалить информацию о преподавателе (аргументы: id)"},
        { "ser", "Сериализовать определенное количество преподавателей \n" +
            "с удалением их из памяти (списка в программе) с использованием json \n" +
                "ser_json \n"},
        {"deser", "десериализовать информацию о преподавателях" }
    };

    private static string[] aboutSerArgs =
    {
        "аргументы: 1) индекс преподавателя_количество преподавателей",
        "2) индекс преподавателя_количество преподавателей_'save'",
        "для сохранения сериализованной части списка в списке преподавателей"
    };

    private static string[] aboutDeserArgs =
    {
        "аргументы: 1) тип сериализатора",
        "2) тип сериализатора_'mod'",
        "mod необходим для десериализации файла с несуществующими полями (3 задание ЛР№6)"
    };
    #endregion

    #region Методы обработки команд
    //Вывод доступных команд
    private static string Help(string[] command)
    {
        //возвращаемая строка
        StringBuilder returnString = new();

        //вывод всех команд
        if (command.Length == 1)
        {
            returnString.AppendLine("Доступные команды:");
            foreach (KeyValuePair<string, string> com in aboutCommands)
            {
                returnString.AppendLine($"\t{com.Key}: {com.Value}");
            }
            returnString.AppendLine("Примечание: " +
                "\n 1)команды с аргументами вводятся через _ или |" +
                "\n 2)аргументы в апострофах ('') вводятся именно таким образом.");
        }
        //вывод более подробной информации об определенной команде
        else if (command.Length == 2 && aboutCommands.ContainsKey(command[1]))
        {
            returnString.AppendLine($"\t\"{command[1]}\": {aboutCommands[command[1]]}");
            if (command[1] == "ser")
            {
                foreach (string arg in aboutSerArgs)
                    returnString.AppendLine($"\t {arg}");
            }
            else if (command[1] == "deser")
            {
                foreach (string arg in aboutDeserArgs)
                    returnString.AppendLine($"\t {arg}");
            }
        }

        return returnString.ToString();
    }

        //Команда добавления нового преподавателя в список
        //addProf_Ф_И_О_Дисциплина_дата / addProf_Ф_И_О_Дисциплина
    private static string AddProf(string[] command)
    {
        //проверяет соответствие количества аргументов
        if (command.Length < 5)
        {
            return ($"Ошибка ввода: недостаточно аргументов " +
                $"({command.Length} вместо необходимых 5). \n");
        }
        else if (command.Length == 5)
        {
            professors.Add(new(command[1], command[2], command[3], command[4]));
        }
        else if (command.Length == 6)
        {
            //проверка на соответствие введенного формата даты
            DateTime date;
            if (DateTime.TryParse(command[5], out date))
            {
                professors.Add(new(command[1], command[2], command[3], command[4], date));
            }
            else
            {
                return "Ошибка ввода: Неверный формат даты. \n";
            }
        }
        else if (command.Length > 6)
        {
            return ($"Ошибка ввода: аргументов больше, чем необходимо " +
                $"({command.Length} вместе максимальных 5). \n");
        }
        return "Информация о преподавателе успешно добавлена. \n";
    }

    //вывод списка преподавателей
    private static string List(string[] command)
    {
        //возвращаемая строка
        StringBuilder returnString = new();
        int id; //id преподавателя

        if (command.Length == 1)
        {
            returnString.AppendLine($"{"id",-3}|{"Фамилия",-15}|{"Имя",-15}|{"Отчество",-15}" +
                $"|{"Дисциплина",-25}|{"Дата трудоустройства",-8}");
            returnString.AppendLine(new string('-', 90));
            foreach (Professor professor in professors)
            {
                returnString.AppendLine(professor.ToString());
            }
        }
        else if (command.Length == 2 && int.TryParse(command[1], out id))
        {
            returnString.AppendLine($"Профессор: {FindById(id)}");
        }
        else if (command.Length == 3 && command[2] == "period" && int.TryParse(command[1], out id))
        {
            Professor professor = FindById(id);
            if (professor != null)
            {
                returnString.AppendLine($"Профессор {professor.LastName} {professor.FirstName} {professor.SecondName}" +
                    $" работает в университете уже {professor.PeriodEmployment} месяц(-а)(-ев)");
            }
        }
        else
        {
            returnString.AppendLine($"Неверно задан(-ы) аргумент(-ы).");
        }

        return returnString.ToString();
    }

    //удаление преподавателя по id
    private static string Del(string[] command)
    {
        int id;
        if (command.Length < 2)
        {
            return "Недостаточно аргументов";
        }
        else if (int.TryParse(command[1], out id))
        {
            Professor prof = FindById(id);
            professors.Remove(prof);
            if (prof != null)
                return $"Профессор с id = {id} успешно удален.";
            else
                return $"Преподаватель с id = {id} не найден.";
        }
        else
        {
            return "Аргумент должен быть числом.";
        }
    }

    //поиск преподавателя по ID
    private static Professor FindById(int id)
    {
        foreach (Professor professor in professors)
            if (professor.Id == id)
                return professor;
        return null;
    }

    /// <summary>
    /// Метод для сериализации части списка преподавателей
    /// </summary>
    /// <param name="command">набор аргументов, см. help</param>
    private static string Serializing(string[] command)
    {
        //индекс преподавателя
        int idProf;
        //количество преподавателей
        int amount;

        Regex indexesRegex = new(@"\d{1,3}");
        MatchCollection indexMatches = indexesRegex.Matches(string.Join('_', command));
        if (indexMatches.Count < 2)
        {
            return string.Format("Недостаточно аргументов. Передано {0} из 2 необходимых.",
                indexMatches.Count);
        }
        else if (indexMatches.Count > 2)
        {
            return string.Format("Введено слишком много аргументов. Передано {0} из 2 необходимых.",
                indexMatches.Count);
        }
        else
        {
            //индекс преподавателя
            idProf = int.Parse(indexMatches[0].Value);
            //количество преподавателей
            amount = int.Parse(indexMatches[1].Value);
        }

        //проверка на существование преподавателя с таким id
        Professor isProfExist = FindById(idProf);
        if (isProfExist == null)
        {
            return $"Преподаватель с id = {idProf} не найден.";
        }

        //проверка (относительно конечного индекса) на существование введенного количества преподавателей
        //находящихся после указанного, включая его самого
        //если НЕверно, то НЕсериализуем
        int profListIndex = professors.IndexOf(isProfExist);
        if (profListIndex + amount - 1 > professors.Count - 1)
        {
            return $"Не удалось сериализовать {amount} преподавателей, следующих после {idProf}";
        }

        //выбор сериализатора
        if (command[1].ToLower() == "json")
        {
            SerializeUniversity.SerializeJson(
                path, professors.GetRange(profListIndex, amount));
        }
        else
        {
            return $"Неверно указан сериализатор. {command[1]}, вместо json.";
        }

        //проверка на наличие в команде слова save
        //при отсутствии - удаление сериализованных элементов
        if (command.Length == 5 && command[4].ToLower() != "save")
            professors.RemoveRange(profListIndex, amount);

        return "Информация успешно сериализована.";
    }

    /// <summary>
    /// метод десериализации json
    /// </summary>
    /// <param name="command">набор аргументов, см. help</param>
    private static string Deserialization(string[] command)
    {
        List<Professor> newProfessors = new();
        if (command.Length > 3)
        {
            return "Неверно указан или отсутствует сериализатор.";
        }
        //десериализация json
        else if (command[1].ToLower() == "json")
        {
            newProfessors = SerializeUniversity.DeserializeJson(path);
        }

        if (newProfessors != null)
        {
            professors.AddRange(newProfessors);
            return "Десериализация прошла успешно.";
        }
        else
            return "Объекты для сериализации отсутствуют.";
    }
    #endregion
}
