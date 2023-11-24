using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Server;

//класс, который будет обрабатывать пользовательские команды и возвращать результат
internal class Interpretator
{
    //возвращаемая строка
    public StringBuilder answer = new StringBuilder();

    //путь к директории файлов для сериализации
    private static string path = Path.GetFullPath(Directory.GetCurrentDirectory() + @"\..\..\..\");

    //статическое поле для хранения информации, с которой работают все пользователи
    private static List<Professor> professors = new List<Professor>();

    //список пользователей
    private static List<User> users = User.DeserializeJson(Path.Combine(path, "userslist.json"));

    public string Execute(string messageFromClient)
    {
        string[] command = messageFromClient.Split('|', '_');
        command[0] = command[0].ToLower(); //форматирование ввода в нижнем регистре

        //бесконечная аутентификация
        //РЕШЕНИЕ: передавать в метод id клиента users[индекс user'а с указанным id]
        if (users != null)
        {
            if (command[0] == "login")
            {
                bool result = Autentification(command);
                if (result == true)
                {
                    return "Вход выполнен. Введите команду";
                }
                else
                {
                    return "Неверный логин или пароль";
                }
                
            }
            else
            {
                return "Пройдите аутентификацию: введите логин и пароль (не содержащий '_' и '|'). \n" +
                    "Пример аутентификации: login_Dimon_12345";
            }
        }

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
            case "adduser":
                answer.Append(AddUser(command));
                break;
            case "exit":
                answer.Append("Выход из программы...");
                //указываем, что клиент закончил работу.
                //опять же нужнен текущий id клиента
                
                break;
            default:
                answer.AppendLine("Неизвестная команда. Повторите ввод.");
                answer.AppendLine("help - для вызова справки.");
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
            "с удалением их из памяти (списка в программе) с использованием json"},
        {"deser", "десериализовать информацию о преподавателях" }
    };

    private static string[] aboutSerArgs =
    {
        "аргументы: 1) индекс преподавателя_количество преподавателей",
        "2) индекс преподавателя_количество преподавателей_'save'",
        "для сохранения сериализованной части списка в списке преподавателей"
    };

    #endregion

    #region Методы обработки команд
    //аутентификация
    private static bool Autentification(string[] command)
    {
        foreach (User user in users)
        {
            //возврат true в случае совпадения логина и пароля
            if (command[1] == user.login && command[2] == user.password)
            {
                //указываем, что пользователь находится в системе
                user.isInSystem = true;
                return true;
            }
        }
        return false;
    }

    //Добавить пользователя
    private static string AddUser(string[] command)
    {
        //прерываем функцию, если отстутсвуют логин, пароль или аргументов слишком много
        if (command.Length != 3)
        {
            return $"Количество аргументов отличается от ожидаемого: " +
                $"введено - {command.Length}, ожидалось - 3";
        }

        //инициализируем список пользователей если он пуст
        if (users == null)
            users = new();
        //добавляем нового пользователя сначала в поле программы, а затем сериализуем все поле целиком
        users.Add(new User(command[1], command[2]));
        User.SerializeJson(Path.Combine(path, "userslist.json"), users);
        //выводим ответ клиенту
        return $"Новый пользователь с логином: {command[1]} и паролем {command[2]} успешно добавлен!";
    }

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
            else
            {
                returnString.AppendLine($"Нет информации о команде");
            }
        }

        return returnString.ToString();
    }

        //Команда добавления нового преподавателя в список
        //addProf_Ф_И_О_Дисциплина_дата / addProf_Ф_И_О_Дисциплина
    private static string AddProf(string[] command)
    {
        //проверяет соответствие количества аргументов
        int n = command.Length;
        if (command.Length != 5 & command.Length != 6)
        {
            return $"Ошибка ввода: ожидалось 5/6 аргументов, а получено - {command.Length}";
        }
        else if (command.Length == 5)
        {
            Professor.counter = ChangeCounter();
            professors.Add(new(command[1], command[2], command[3], command[4]));
        }
        else if (command.Length == 6)
        {
            //проверка на соответствие введенного формата даты
            DateTime date;
            if (DateTime.TryParse(command[5], out date))
            {
                Professor.counter = ChangeCounter();
                professors.Add(new(command[1], command[2], command[3], command[4], date));
            }
            else
            {
                return "Ошибка ввода: Неверный формат даты. \n";
            }
        }

        return "Информация о преподавателе успешно добавлена. \n";

        //функция, возвращающая текущее состояние счетчика в Professor
        int ChangeCounter()
        {
            List<int> idsMassive = new();
            //добавляем все id из статического списка
            idsMassive.AddRange(from prof in professors select prof.Id);
            //добавляем все id из файла для сериализации на случай,
            //если файл не был сериализован до добавления нового преподавателя
            idsMassive.AddRange(
                from prof in SerializeUniversity.DeserializeJson(Path.Combine(path, "university.json")) select prof.Id);

            if (idsMassive.Count != 0)
                return idsMassive.Max() + 1;
            else
                return 0;
        }
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
        if (indexMatches.Count != 2)
        {
            return string.Format("Передано недостаточно или слишком много аргументов. Передано {0} из 2 необходимых.",
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
        //иначе сериализуем
        else
        {
            SerializeUniversity.SerializeJson(
                Path.Combine(path, "university.json"), professors.GetRange(profListIndex, amount));
        }

        //возможно следует УДАЛИТЬ

        //проверка на наличие в команде слова save
        //при отсутствии - удаление сериализованных элементов
        if (command.Length == 4 && command[3].ToLower() != "save")
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
        if (command.Length == 1)
            newProfessors = SerializeUniversity.DeserializeJson(Path.Combine(path, "university.json"));
        else
            return "Неверно указан или отсутствует сериализатор.";

        if (newProfessors != null)
        {
            professors.AddRange(newProfessors);
            return "Десериализация прошла успешно.";
        }
        else
        {
            return "Объекты для десериализации отсутствуют.";
        }
    }
    #endregion
}
