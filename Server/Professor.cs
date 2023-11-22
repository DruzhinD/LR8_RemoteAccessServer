using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace SerializedCommandInterface
{
    //проблема с id
    //сериализуется, однако при создании нового преподавателя, id начинается с нуля
    //а не с того, какой был в json
    public class Professor
    {
        [JsonIgnore]
        //счетчик
        public static int counter;

        [JsonIgnore]
        //количество преподавателей
        public static int amount;

        //хоть все не помеченные поля должны быть сериализованы
        //для этого поля нужен аттрибут JsonInclude,
        //чтобы десериализовать id
        [JsonInclude]
        //уникальный номер преподавателя
        public int Id { get; private set; }

        //фамилия
        public string LastName { get; set; }

        //имя преподавателя
        public string FirstName { get; set; }

        //отчество
        public string SecondName { get; set; }

        //дисциплина преподавателя
        public string Subject { get; set; }

        //дата трудоустройства
        public DateTime Employment { get; set; }
        
        [JsonIgnore]
        //количество месяцев, проведенных в университете
        public int PeriodEmployment
        {
            get
            { 
                int years = DateOnly.FromDateTime(DateTime.Now).Year - Employment.Year;
                int months = DateOnly.FromDateTime(DateTime.Now).Month - Employment.Month;
                return years * 12 + months;
            }
        }

        public Professor() { }

        //конструктор с вводом всех полей
        public Professor(string lastName, string firstName, string secondName, string subject, DateTime employment)
        {
            LastName = lastName;
            FirstName = firstName;
            SecondName = secondName;
            Subject = subject;
            Employment = employment;

            Id = counter;
            counter++;
            amount++;
        }

        //Конструктор с датой трудоустройства по умолчанию
        public Professor(string lastName, string firstName, string secondName, string subject)
        {
            LastName = lastName;
            FirstName = firstName;
            SecondName = secondName;
            Subject = subject;

            //значение по умолчанию, т.е. сегодняшний день
            Employment = DateTime.Now; 

            Id = counter;
            counter++;
            amount++;
        }

        public override string ToString()
        {
            return $"{Id,-3} {LastName,-15} {FirstName,-15} {SecondName,-15} {Subject,-25} {Employment,-15:d}";
        }
    }

    public class University
    {
        public University() { }

        [JsonInclude]
        public List<Professor> professorsList { get; set; } = new();
    }
}
