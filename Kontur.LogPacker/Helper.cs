using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Kontur.LogPacker
{
    //TODO:7 лучше не терять модификаторы досутпа
    static class Helper
    {
        public static bool IsLineCorrect(string line)
        {
            string pattern = @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2},\d{3} \d{1}[\d| ][\d| ][\d| ][\d| ][\d| ]";
            //TODO:10 неочевидная и недокументированная логика. Не могу даже пресдавить зачем это строчка.
            if (line.Length < 33) return false;
            //TODO:11 можно возвращать результат метода, а не делать по нему if
            if (Regex.IsMatch(line, pattern, RegexOptions.IgnoreCase))
            {
                return true;
            }
            return false;
        }

        //TODO:12 startindex
        //TODO:21 Это похоже на велосипед. Подумай как не делать это руками
        public static string PartOfString(string source, int startindex, int count)
        {
            //TODO:18 Скорее всего, тут нужно бросать exception, а не левую строку возвращать
            if (source.Length < startindex + count - 1) { return "Error"; }
            else
            {
                //TODO:19 Очень плохое решение потому что неэффективно работаешь со 1)стороками 2)коллекциями
                string result = "";
                for (int i = startindex; i < startindex + count; i++)
                {
                    result += source[i];
                }
                return result;
            }

        }

        //TODO:13 Лучше не использовать цифры в названиях переменных
        public static string DateDifference(DateTime dateTime1, DateTime dateTime2)
        {
            //TODO:15 это весь метод - один большой велосипед. Читай про TimeSpan и DateTime. Пока ты не поймешь, что этот метод нужно удалить - продолжай читать
            TimeSpan diff = dateTime1 - dateTime2;
            //TODO:14 можно было написать сразу int days = value... и т.д.
            int days, min, ms;
            days = (int)Math.Floor(diff.TotalDays);
            min = diff.Hours * 24 + diff.Minutes;
            ms = diff.Seconds * 1000 + diff.Milliseconds;
            string result = days + " " + min + " " + ms;
            return result;
        }//TODO:16 между методами стоит делать отсутпы
        public static string DateAsString(string line, DateTime dateTime, out int position, out DateTime dateTime1)
        {
            //TODO:17 Еще один большой костыль. Смотреть todo-15
            int counter = 0;
            int i = 0;
            string day = "", min = "", ms = "";
            while (counter < 3)
            {
                if ((counter == 0) && (line[i] != ' ' ))
                {
                    day += line[i];
                    i++;
                }
                else if ((counter == 1) && (line[i] != ' '))
                {
                    min += line[i];
                    i++;
                }
                else if ((counter == 2) && (line[i] != ' '))
                {
                    ms += line[i];
                    i++;
                }
                else if (line[i] == ' ')
                {
                    counter++;
                    i++;
                }
                        
            }
            int.Parse(day);
            int.Parse(min);
            int.Parse(ms);
            position = i;
            DateTime curr = new DateTime();
            
            TimeSpan timeSpan = new TimeSpan(int.Parse(day), int.Parse(min) / 60, int.Parse(min) - (int.Parse(min) / 60) * 60, int.Parse(ms)/1000, int.Parse(ms) - (int.Parse(ms) / 1000) * 1000);
            curr = dateTime.Add(timeSpan);
            dateTime1 = curr;
            return curr.ToString("yyyy-MM-dd HH:mm:ss,fff");
        }

        public static byte[] CreateOptimalByteLine(List<byte> byteList, DateTime dateTime, out DateTime dateTime1)
        {
            string newline;
            
            string line = System.Text.Encoding.UTF8.GetString(byteList.ToArray());
            if (Helper.IsLineCorrect(line))
            {
                if (dateTime.Year != 1)
                {
                    newline = Helper.DateDifference(DateTime.ParseExact(Helper.PartOfString(line, 0, 23), "yyyy-MM-dd HH:mm:ss,fff", null), dateTime) + line.Remove(0, 23);
                }
                else
                {
                    newline = line;
                }

                dateTime1 = DateTime.ParseExact(Helper.PartOfString(line, 0, 23), "yyyy-MM-dd HH:mm:ss,fff", null);
                
            }
            else
            {
                byteList.Insert(0, 33);
                dateTime1 = dateTime;
                return byteList.ToArray();
            }



            return Encoding.UTF8.GetBytes(newline);

        }

        public static byte[] ReturnOriginalCorrectLine(List<byte> byteList, DateTime dateTime, out DateTime dateTime1)
        {
            string newline;
            string line = System.Text.Encoding.UTF8.GetString(byteList.ToArray());            
            //TODO:20 Табуляция очень сильно поехала. Опять
                if (dateTime.Year != 1)
                {
                    newline = Helper.DateAsString(line, dateTime, out int pos, out dateTime) + line.Remove(0, pos - 1);
                    dateTime1 = dateTime;
                }
                else
                {
                    dateTime1 = DateTime.ParseExact(Helper.PartOfString(line, 0, 23), "yyyy-MM-dd HH:mm:ss,fff", null);
                    newline = line;
                }  
            return Encoding.UTF8.GetBytes(newline);

        }
                
    }
}
