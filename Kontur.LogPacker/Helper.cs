using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Kontur.LogPacker
{
    public static class Helper
    {
        private static bool IsBeginLineCorrect(string line)
        {
            string pattern = @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2},\d{3} \d{1}[\d| ][\d| ][\d| ][\d| ][\d| ]";
            if (line.Length < 38) return false;
            return Regex.IsMatch(line, pattern, RegexOptions.IgnoreCase);
        }

        private static bool ChangeStringToCompact(string line, DateTime inputDateTime, ulong inputID, Dictionary<string, string> lvlDict, bool isDateFirstinput,
            out DateTime outputDateTime, out ulong outputID, out bool isDateFirstoutput, out string newline)
        {
            string changedDate = "";
            if (!isDateFirstinput)
            {
                changedDate = DateDifference(DateTime.ParseExact(line.Substring(0, 23), "yyyy-MM-dd HH:mm:ss,fff", null).Subtract(inputDateTime));
                outputDateTime = DateTime.ParseExact(line.Substring(0, 23), "yyyy-MM-dd HH:mm:ss,fff", null);
            }
            else
            {
                outputDateTime = DateTime.ParseExact(line.Substring(0, 23), "yyyy-MM-dd HH:mm:ss,fff", null);
            }
            line = line.Remove(0, 24);

            newline = "";
            int pos = 0; //number of digits                        
            newline += line[0];

            //check is id correct
            for (int i = 1; i < line.Length; i++)
            {
                if ((line[i] == ' ') && (line[i - 1] != ' '))
                {
                    pos = i;
                    break;
                }
                newline += line[i];
            }
            if (pos == 0)
            {
                outputDateTime = inputDateTime;
                outputID = inputID;
                isDateFirstoutput = isDateFirstinput;
                return false;
            } //this situation maybe only if line incorrect

            if (pos < 6)
            {
                for (int i = pos; i < 7; i++)
                {
                    newline += line[i];
                }
                pos = 7;
            }
            else
            {
                pos++;
            }

            if (!ulong.TryParse(newline, out outputID))
            {
                outputDateTime = inputDateTime;
                outputID = inputID;
                isDateFirstoutput = isDateFirstinput;
                return false;
            }//if id is incorrect

            

            if (outputID < inputID)
            {
                outputDateTime = inputDateTime;
                outputID = inputID;
                isDateFirstoutput = isDateFirstinput;
                return false;
            }//id must be greater than prev id


            //add correct string

            line = line.Remove(0, pos);

            if (line[0] == ' ')
            {
                outputDateTime = inputDateTime;
                outputID = inputID;
                isDateFirstoutput = isDateFirstinput;
                return false;
            }//line[0] can't be ' '

            

            pos = 0;
            newline = line[0].ToString();
            for (int i = 1; i < line.Length; i++)
            {
                if ((line[i] == ' ') && (line[i - 1] != ' '))
                {
                    pos = i;
                    break;
                }
                newline += line[i];
            }

            if (pos == 0)
            {
                outputDateTime = inputDateTime;
                outputID = inputID;
                isDateFirstoutput = isDateFirstinput;
                return false;
            }

            if (pos < 5)
            {
                for (int i = pos; i < 5; i++)
                {
                    if (line[i] != ' ')
                    {
                        outputDateTime = inputDateTime;
                        outputID = inputID;
                        isDateFirstoutput = isDateFirstinput;
                        return false;
                    }
                    newline += line[i];
                }
                pos = 6;
            }
            else
            {
                pos++;
            }

            //check dictionary
            if (!lvlDict.TryGetValue(newline, out string dictValue))
            {
                lvlDict.Add(newline, ("@" + lvlDict.Count));
                dictValue = "@" + (lvlDict.Count - 1);
            }

            line = line.Remove(0, pos);
            if (line.Length < 1)
            {
                outputDateTime = inputDateTime;
                outputID = inputID;
                isDateFirstoutput = isDateFirstinput;
                return false;
            }

            if (isDateFirstinput)
            {
                isDateFirstoutput = false;
                return true;
            }

            newline = changedDate + " " + (outputID - inputID).ToString() + dictValue + " " + line;
            isDateFirstoutput = false;
            return true;
        }

        private static string DateDifference(TimeSpan diff)
        {

            string days = ((int)Math.Floor(diff.TotalDays)).ToString();
            if (days == "0") { days = ""; }
            string min = (diff.Hours * 24 + diff.Minutes).ToString();
            if (min == "0") { min = ""; }
            int ms = diff.Seconds * 1000 + diff.Milliseconds;
            string result = days + " " + min + " " + ms;
            return result;
        }

        private static string ReturnDateFromCompact(string line, DateTime dateTime, out int position, out DateTime dateTime1)
        {
            int counter = 0;
            int i = 0;
            string day = "", min = "", ms = "";            
            while (counter < 3)
            {
                if ((counter == 0) && (line[i] != ' '))
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
            if (day == "") { day = "0"; }
            if (min == "") { min = "0"; }
            position = i;
            DateTime curr = new DateTime();

            TimeSpan timeSpan = new TimeSpan(int.Parse(day), int.Parse(min) / 60, int.Parse(min) - (int.Parse(min) / 60) * 60, int.Parse(ms) / 1000, int.Parse(ms) - (int.Parse(ms) / 1000) * 1000);
            curr = dateTime.Add(timeSpan);
            dateTime1 = curr;
            return curr.ToString("yyyy-MM-dd HH:mm:ss,fff");
        }

        private static string ReturnIdFromCompact(string line, ulong inputId, out int position, out ulong outputId)
        {
            string newline;
            position = 0;
            newline = line[0].ToString();
            for (int i = 1; i < 21; i++)
            {
                if (line[i] == '@')
                {
                    position = i;
                    break;
                }
                newline += line[i];
            }

            outputId = ulong.Parse(newline) + inputId;
            newline = outputId.ToString();

            if (newline.Length < 6)
            {
                newline = newline.PadRight(6);
            }

            return newline;
        }

        private static string ReturnLogLvlFromCompact(string line, Dictionary<string, string> logLvl, out int position)
        {
            string newline;
            position = 0;
            newline = line[0].ToString();
            for (int i = 1; i < line.Length; i++)
            {
                if (line[i] == ' ')
                {
                    position = i;
                    break;
                }
                newline += line[i];
            }

            logLvl.TryGetValue(newline, out newline);

            if (newline.Length < 5)
            {
                newline.PadRight(5 - newline.Length);
                position = 6;
            }
            return newline;
        }

        private static string ChangeStringFromCompact(string line, DateTime inputDateTime, ulong inputId, Dictionary<string, string> lvlDict,
            out DateTime outputDateTime, out ulong outputId)
        {
            string date, id, logLvl;
            date = ReturnDateFromCompact(line, inputDateTime, out int pos, out inputDateTime);
            line = line.Remove(0, pos);
            outputDateTime = inputDateTime;

            id = ReturnIdFromCompact(line, inputId, out pos, out inputId);
            outputId = inputId;
            line = line.Remove(0, pos);

            logLvl = ReturnLogLvlFromCompact(line, lvlDict, out pos);
            line = line.Remove(0, pos + 1);

            return date + " " + id + " " + logLvl + " " + line;
        }

        private static ulong ReadFirstId(string line)
        {
            string newline = line[0].ToString();
            for (int i = 1; i < line.Length; i++)
            {
                if ((line[i] == ' ') && (line[i - 1] != ' '))
                {
                    break;
                }
                newline += line[i];
            }

            return ulong.Parse(newline);
        }

        public static bool IsByteDigit(byte bytes)
        {
            return ((bytes >= 48) && (bytes <= 57));
        }

        public static byte[] CreateOptimalByteLine(List<byte> byteList, DateTime inputDateTime, ulong currId, Dictionary<string, string> loglvl, bool isItFirstDateInput,
            out DateTime outputDateTime, out ulong outID, out bool isItFirstDateOutput)
        {
            string newline;            
            string line = System.Text.Encoding.UTF8.GetString(byteList.ToArray());
            if (IsBeginLineCorrect(line))
            {
                if (!isItFirstDateInput)
                {                    
                    if (ChangeStringToCompact(line, inputDateTime, currId, loglvl, isItFirstDateInput, out outputDateTime, out outID, out isItFirstDateOutput, out newline))
                    {
                        return Encoding.UTF8.GetBytes(newline);
                    }
                    else
                    {
                        outputDateTime = inputDateTime;
                        outID = currId;
                        isItFirstDateOutput = isItFirstDateInput;
                        byteList.Insert(0, 33);
                        return byteList.ToArray();
                    }
                }
                else
                {
                    if (ChangeStringToCompact(line, inputDateTime, currId, loglvl, isItFirstDateInput, out outputDateTime, out outID, out isItFirstDateOutput, out newline))
                    {
                        return byteList.ToArray();
                    }
                    else
                    {
                        outputDateTime = inputDateTime;
                        outID = currId;
                        isItFirstDateOutput = isItFirstDateInput;
                        byteList.Insert(0, 33);
                        return byteList.ToArray();
                    }
                    
                    
                }
            }
            else
            {
                byteList.Insert(0, 33);
                outputDateTime = inputDateTime;
                outID = currId;
                isItFirstDateOutput = isItFirstDateInput;
                return byteList.ToArray();
            }
        }

        public static byte[] ReturnOriginalCorrectLine(List<byte> byteList, DateTime inputDateTime, ulong inputId, Dictionary<string, string> logsLvl,
            out DateTime outputDateTime, out ulong outputId)
        {
            string newline;
            string line = Encoding.UTF8.GetString(byteList.ToArray());
            if (inputDateTime > DateTime.MinValue)
            {
                //add work with id and log lvl
                newline = ChangeStringFromCompact(line, inputDateTime, inputId, logsLvl, out outputDateTime, out outputId);
            }
            else
            {
                outputDateTime = DateTime.ParseExact(line.Substring(0, 23), "yyyy-MM-dd HH:mm:ss,fff", null);
                outputId = ReadFirstId(line.Remove(0, 24));
                newline = line;
            }
            return Encoding.UTF8.GetBytes(newline);

        }

    }
}
