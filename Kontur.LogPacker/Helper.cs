using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Kontur.LogPacker
{
    public static class Helper
    {
        private static bool IsLineCorrect(string line)
        {
            string pattern = @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2},\d{3} \d{1}[\d| ][\d| ][\d| ][\d| ][\d| ]";
            if (line.Length < 33) return false;
            return Regex.IsMatch(line, pattern, RegexOptions.IgnoreCase);            
        }

        private static string PartOfString(string source, int startindex, int count)
        {
            if (source.Length < startindex + count - 1) { return "Error"; }
            else
            {
                string result = "";
                for (int i = startindex; i < startindex + count; i++)
                {
                    result += source[i];
                }
                return result;
            }

        }

        private static string DateDifference(DateTime inputDateTime, DateTime outputDateTime)
        {
            TimeSpan diff = inputDateTime - outputDateTime;
            int days, min, ms;
            days = (int)Math.Floor(diff.TotalDays);
            min = diff.Hours * 24 + diff.Minutes;
            ms = diff.Seconds * 1000 + diff.Milliseconds;
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

            outputId = ulong.Parse(newline);

            if (newline.Length < 6)
            {
                newline.PadRight(6 - newline.Length);
                position = 6;
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

        private static string ChangeStringToCompact(string line, DateTime inputDateTime, ulong inputID, Dictionary<string, string> lvlDict, 
            out DateTime outputDateTime, out ulong outputID)
        {
            bool isReallyCorrect;
            string myID = "";
            ulong outIdDiff = 0;            
            int pos = 0;
            string strToDict = "";
            string ChangeId;
            string newline;
            int lineLength;
            string dictValue;

            //check line length
            if (line.Length < 37)
            {
                outputDateTime = inputDateTime;
                outputID = inputID;
                return null;
            }

            //parse Data            
            string changedDate = DateDifference(DateTime.ParseExact(PartOfString(line, 0, 23), "yyyy-MM-dd HH:mm:ss,fff", null), inputDateTime);
            outputDateTime = DateTime.ParseExact(Helper.PartOfString(line, 0, 23), "yyyy-MM-dd HH:mm:ss,fff", null);
            line = line.Remove(0, 24);

            //check line length after delete date
            if (line.Length < 6)
            {
                outputDateTime = inputDateTime;
                outputID = inputID;
                return null;
            }
            if (line.Length > 20) { lineLength = 20; } else { lineLength = line.Length; }

            myID += line[0];
            //parse ID
            for (int i = 1; i < lineLength; i++)
            {
                myID += line[i];
                if ((line[i] == ' ') && (line[i - 1] != ' '))
                {
                    pos = i;
                    break;
                }
            }
            isReallyCorrect = ulong.TryParse(myID, out outIdDiff);

            //check parsed id
            if (!isReallyCorrect)
            {
                outputDateTime = inputDateTime;
                outputID = inputID;                
                return null;
            }

            //set new id
            outputID = outIdDiff;
            outIdDiff = outIdDiff - inputID;
            ChangeId = outIdDiff.ToString();

            if (pos < 6)
            {
                pos = 6;
            }            
            line = line.Remove(0, pos);

            //check is line correct
            if (line[0] != ' ')
            {
                outputDateTime = inputDateTime;
                outputID = inputID;
                return null;
            }

            line = line.Remove(0, 1);

            //check line length after delete id
            if (line.Length < 5)
            {
                outputDateTime = inputDateTime;
                outputID = inputID;
                return null;
            }

            //check is line correct
            if (line[0] == ' ')
            {
                outputDateTime = inputDateTime;
                outputID = inputID;
                return null;
            }
            else
            {
                strToDict += line[0];
            }

            //parse loglvl
            for (int i = 1; i < line.Length; i++)
            {
                strToDict += line[i];
                if ((line[i] == ' ') && (line[i - 1] != ' '))
                {
                    pos = i;
                    break;
                }
            }
            
            if (pos < 5)
            {
                for (int i = pos + 1; i < 6; i++)
                {
                    if (line[i] != ' ') { isReallyCorrect = false; }
                    strToDict += line[i];
                }
                pos = 5;
            }

            //check log lvl is correct
            if ((!isReallyCorrect) ||(pos == 0))
            {
                outputDateTime = inputDateTime;
                outputID = inputID;
                return null;
            }


            if (!lvlDict.TryGetValue(strToDict, out dictValue))
            {
                lvlDict.Add(strToDict, ("@" + lvlDict.Count));
                dictValue = "@" + (lvlDict.Count - 1);
            }            
            line = line.Remove(0, pos);

            //check is line correct
            if (line[0] != ' ')
            {
                outputDateTime = inputDateTime;
                outputID = inputID;
                return null;
            }

            //create newline
            newline = changedDate + " " + ChangeId + dictValue + line;           

            return newline;

        }

        private static string ChangeStringFromCompact(string line, DateTime inputDateTime, ulong inputId, Dictionary<string, string> lvlDict,
            out DateTime outputDateTime, out ulong outputId)
        {
            string date, id, logLvl;
            int needToDelete = 0;
            date = ReturnDateFromCompact(line, inputDateTime, out int pos, out inputDateTime);
            outputDateTime = inputDateTime;
            needToDelete += pos;
            id = ReturnIdFromCompact(line, inputId, out pos, out inputId);
            outputId = inputId;
            needToDelete += pos;            
            logLvl = ReturnLogLvlFromCompact(line, lvlDict, out pos);
            needToDelete += pos;
            return date + " " + id + " " + logLvl + " " + line.Remove(0, needToDelete);
        }

        public static byte[] CreateOptimalByteLine(List<byte> byteList, DateTime inputDateTime, ulong currId, Dictionary<string, string> loglvl, 
            out DateTime outputDateTime, out ulong outID)
        {
            string newline;
            
            string line = System.Text.Encoding.UTF8.GetString(byteList.ToArray());
            if (Helper.IsLineCorrect(line))
            {
                if (inputDateTime > DateTime.MinValue)
                {                    
                    newline = ChangeStringToCompact(line, inputDateTime, currId, loglvl, out outputDateTime, out outID);
                    if (string.IsNullOrEmpty(newline))
                    {
                        byteList.Insert(0, 33);
                        outputDateTime = inputDateTime;
                        outID = currId;
                        return byteList.ToArray();
                    }
                    else
                    {
                        return Encoding.UTF8.GetBytes(newline);
                    }
                }
                else
                {                    
                    outputDateTime = DateTime.ParseExact(Helper.PartOfString(line, 0, 23), "yyyy-MM-dd HH:mm:ss,fff", null);
                    outID = currId;
                    return byteList.ToArray();
                }                
            }
            else
            {
                byteList.Insert(0, 33);
                outputDateTime = inputDateTime;
                outID = currId;
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
                    outputDateTime = DateTime.ParseExact(PartOfString(line, 0, 23), "yyyy-MM-dd HH:mm:ss,fff", null);
                    outputId = inputId;
                    newline = line;
                }  
            return Encoding.UTF8.GetBytes(newline);

        }
                
    }
}
