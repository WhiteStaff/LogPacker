using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Kontur.LogPacker
{
    public static class MyCompressor
    {
        public static string CreateHelpFile(string inputFile)
        {            
            int bytes;
            List<byte> byteline = new List<byte>();
            byte[] bytesForWriting;
            DateTime dateTime = new DateTime();
            using (FileStream readFile = new FileStream(inputFile, FileMode.Open))
                //TODO:22 "123.txt" -скорее всего должно быть переменной, которая приходит в метод
            using (FileStream writeFile = new FileStream(Path.GetFullPath("123.txt"), FileMode.Create))
                while ((bytes = readFile.ReadByte()) != -1)
                {

                    /*if ((byte)bytes == 10 || (byte)bytes == 13)
                    {
                        isLineVeryBad = true;
                        bytesForWriting = Helper.CreateOptimalByteLine(byteline, dateTime, out dateTime);
                        for (int i = 0; i < bytesForWriting.Length; i++)
                        {
                            writeFile.WriteByte(bytesForWriting[i]);
                        }
                        if (bytes == 13)
                        {
                            writeFile.WriteByte(13);
                            bytes = readFile.ReadByte();
                        }
                        if (bytes == 10)
                        {
                            writeFile.WriteByte(10);
                            byteline.Clear();
                            isLineVeryBad = false;
                        }
                        else
                        {
                            writeFile.WriteByte((byte)bytes);
                        }
                        if (isLineVeryBad)
                        {
                            byteline.Clear();
                        }
                    }
                    else
                    {
                        byteline.Add((byte)bytes);
                    }*/
                    //TODO:23 Вообще не понятно что за 48 и 57. Код нечитаем
                    if ((bytes >= 48) && (bytes <= 57) && (byteline.Count == 0))
                    {
                        while ((bytes != 10) && (bytes != -1))
                        {
                            byteline.Add((byte)bytes);
                            bytes = readFile.ReadByte();
                        }
                        //TODO:24 if == 13 делаем какую-то непонятную логику. Подобные кейсы лучше выносить в отдельные методы и хотябы давать имена понятные.
                        if (byteline[byteline.Count - 1] == 13)
                        {
                            byteline.RemoveAt(byteline.Count - 1);
                            bytes = 13;
                        }
                        bytesForWriting = Helper.CreateOptimalByteLine(byteline, dateTime, out dateTime);
                        for (int i = 0; i < bytesForWriting.Length; i++)
                        {
                            writeFile.WriteByte(bytesForWriting[i]);
                        }
                        if (bytes == 13) { writeFile.WriteByte(13); }
                        //TODO:25 Этот брейк очень сильно усложняет понимание того, как должен работать этот while (хотя казалось бы, куда усложнять)
                        if (bytes == -1) { break; }
                        writeFile.WriteByte(10);
                        byteline.Clear();
                        //true method
                    }
                    else
                    {
                        while ((bytes != 10) && (bytes != -1))
                        {
                            byteline.Add((byte)bytes);
                            bytes = readFile.ReadByte();
                        }
                        byteline.Insert(0, 33);
                        bytesForWriting = byteline.ToArray();
                        for (int i = 0; i < bytesForWriting.Length; i++)
                        {
                            writeFile.WriteByte(bytesForWriting[i]);
                        }
                        if (bytes == -1) { break; }
                        writeFile.WriteByte(10);
                        byteline.Clear();
                    }
                }
            inputFile = Path.GetFullPath("123.txt");
            return inputFile;
        }

        public static void ReturnOriginalLog(string outputFile)
        {
            int bytes;
            List<byte> byteline = new List<byte>();
            byte[] bytesForWriting;
            DateTime dateTime = new DateTime();                    
            using (FileStream readFile = new FileStream(Path.GetFullPath("123.txt"), FileMode.Open))
            using (FileStream writeFile = new FileStream(outputFile, FileMode.OpenOrCreate))
                //TODO:26 я бы советовал разделить, насколько это возможно открытие файлов и этот while от логики внутри
                //сделать метод, который будет что-то делать там и возвращать true/false - нужно ли вайлу дальше работать
                while ((bytes = readFile.ReadByte()) != -1)
                {
                    //TODO:27 есть подозрение, что у тебя дублируется логика в этих двух методах. Попытайся ее обобщить
                    if (byteline.Count == 0 && bytes != 33)
                    {
                        byteline.Add((byte)bytes);
                        bytes = readFile.ReadByte();
                        while ((bytes) != 10 && (bytes) != 13)
                        {
                            if (bytes == -1) { break; }
                            byteline.Add((byte)bytes);
                            bytes = readFile.ReadByte();
                        }
                        bytesForWriting = Helper.ReturnOriginalCorrectLine(byteline, dateTime, out dateTime);
                        for (int i = 0; i < bytesForWriting.Length; i++)
                        {
                            writeFile.WriteByte(bytesForWriting[i]);
                        }
                        if (bytes == 13)
                        {
                            writeFile.WriteByte(13);
                            readFile.ReadByte();
                        }
                        writeFile.WriteByte(10);
                        byteline.Clear();
                    }
                    else
                    {
                        bytes = readFile.ReadByte();
                        /*while ((bytes) != 10 && (bytes) != 13)
                        {
                            if (bytes == -1) { break; }
                            byteline.Add((byte)bytes);
                            bytes = readFile.ReadByte();
                            
                        }
                        bytesForWriting = byteline.ToArray();
                        for (int i = 0; i < bytesForWriting.Length; i++)
                        {
                            writeFile.WriteByte(bytesForWriting[i]);
                        }
                        if (bytes == 13)
                        {
                            writeFile.WriteByte(13);
                            while ((bytes = readFile.ReadByte()) == 13)  //для картинок и других жутких наборов байт
                            {
                                writeFile.WriteByte(13);                                
                            }                            
                            if (bytes == 33)
                            {
                                readFile.Position--;
                               
                                byteline.Clear();
                            }
                        }
                        else
                        if (bytes == 10)
                        {
                            writeFile.WriteByte(10);
                            byteline.Clear();
                        }*/
                        while ((bytes != 10) && (bytes != -1))
                        {
                            byteline.Add((byte)bytes);
                            bytes = readFile.ReadByte();
                        }
                        bytesForWriting = byteline.ToArray();
                        for (int i = 0; i < bytesForWriting.Length; i++)
                        {
                            writeFile.WriteByte(bytesForWriting[i]);
                        }
                        if (bytes == -1) { break; }
                        writeFile.WriteByte(10);
                        byteline.Clear();
                    }
                }            
                //TODO:28 Опять же, удаление в данном контексте - странное решение, а табуляции все также плохо. Самое время почитать про хоткеи студии?
           File.Delete(Path.GetFullPath("123.txt"));
        }

    }
}
