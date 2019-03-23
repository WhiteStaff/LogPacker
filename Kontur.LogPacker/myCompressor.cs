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
            using (FileStream readFile = new FileStream(inputFile, FileMode.Open) )
            using (FileStream writeFile = new FileStream(Path.GetFullPath("123.txt"), FileMode.OpenOrCreate))
                while ((bytes = readFile.ReadByte()) != -1)
                {
                    if ((byte)bytes == 10 || (byte)bytes == 13)
                    {
                        bytesForWriting = Helper.CreateOptimalByteLine(byteline, dateTime, out dateTime);
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
                        byteline.Add((byte)bytes);
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
                while ((bytes = readFile.ReadByte()) != -1)
                {

                    if (byteline.Count == 0 && bytes != 33)
                    {
                        while ((bytes = readFile.ReadByte()) != 10 && (bytes = readFile.ReadByte()) != 13)
                        {
                            byteline.Add((byte)bytes);
                        }
                        //парс строки лога
                        byteline.Clear();
                        if (counter == 0)
                        {
                            dateTime = DateTime.ParseExact(Helper.PartOfString(line, 0, 23), "yyyy-MM-dd HH:mm:ss,fff", null);
                            newline = line;
                        }
                        else
                        {

                            newline = Helper.DateAsString(line, dateTime, out int pos, out dateTime) + line.Remove(0, pos - 1);

                        }
                        
                    }
                    else
                    {
                        while ((bytes = readFile.ReadByte()) != 10 && (bytes = readFile.ReadByte()) != 13)
                        {
                            byteline.Add((byte)bytes);
                        }
                        //парс некорректной строки
                        byteline.Clear();
                        
                    }                   
                }            
            File.Delete(Path.GetFullPath("123.txt"));
        }

    }
}
