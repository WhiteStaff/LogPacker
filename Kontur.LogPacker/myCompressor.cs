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
            ulong currentId = 0;
            Dictionary<string, string> logsLvl = new Dictionary<string, string>();
            using (FileStream readFile = new FileStream(inputFile, FileMode.Open))
            using (FileStream writeFile = new FileStream(Path.GetFullPath("123.txt"), FileMode.Create))
            {                
                while (readFile.Position < readFile.Length)
                {
                    bytes = readFile.ReadByte();
                    if ((bytes >= 48) && (bytes <= 57) && (byteline.Count == 0))
                    {
                        while ((bytes != 10) && (bytes != -1))
                        {
                            byteline.Add((byte)bytes);
                            bytes = readFile.ReadByte();
                        }
                        if (byteline[byteline.Count - 1] == 13)
                        {
                            byteline.RemoveAt(byteline.Count - 1);
                            bytes = 13;
                        }
                        bytesForWriting = Helper.CreateOptimalByteLine(byteline, dateTime, currentId, logsLvl, out dateTime, out currentId);
                        for (int i = 0; i < bytesForWriting.Length; i++)
                        {
                            writeFile.WriteByte(bytesForWriting[i]);
                        }
                        if (bytes == 13) { writeFile.WriteByte(13); }
                        if (bytes == -1) { break; }
                        writeFile.WriteByte(10);
                        byteline.Clear();

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
                ulong bytesCount = 2;
                string pair;
                foreach(var curr in logsLvl)
                {
                    pair = curr.Key + "`" + curr.Value;
                    bytesForWriting = Encoding.UTF8.GetBytes(pair);
                    for (int i = 0; i < bytesForWriting.Length; i++)
                    {
                        writeFile.WriteByte(bytesForWriting[i]);
                        bytesCount++;
                    }
                    writeFile.WriteByte(10);
                    bytesCount++;
                }
                bytesForWriting = BitConverter.GetBytes(bytesCount);
                for (int i = 0; i < bytesForWriting.Length; i++)
                {
                    writeFile.WriteByte(bytesForWriting[i]);                    
                }

            }
            
            inputFile = Path.GetFullPath("123.txt");
            return inputFile;
        }

        public static void ReturnOriginalLog(string outputFile)
        {
            int bytes;
            long pos = 0;
            ulong currentId = 0;
            List<byte> byteline = new List<byte>();
            byte[] bytesForWriting;
            Dictionary<string, string> logsLvl = new Dictionary<string, string>();
            DateTime dateTime = new DateTime();
            using (FileStream readFile = new FileStream(Path.GetFullPath("123.txt"), FileMode.Open))
            using (FileStream writeFile = new FileStream(outputFile, FileMode.OpenOrCreate))
            {
                //create dictionary
                readFile.Position = readFile.Length - 8;
                
                for (int i = 0; i < 8; i++)
                {
                    bytes = readFile.ReadByte();
                    byteline.Add((byte)bytes);
                }
                bytes = readFile.ReadByte();
                pos = BitConverter.ToInt64(byteline.ToArray());
                while ((bytes = readFile.ReadByte()) != 92)
                {
                    string key;
                    string value;
                    while (bytes != 96)
                    {
                        byteline.Add((byte)bytes);
                        bytes = readFile.ReadByte();
                    }
                    byteline.Clear();
                    value = Encoding.UTF8.GetString(byteline.ToArray());
                    bytes = readFile.ReadByte();
                    while (bytes != 10)
                    {
                        byteline.Add((byte)bytes);
                        bytes = readFile.ReadByte();
                    }
                    key = Encoding.UTF8.GetString(byteline.ToArray());
                    logsLvl.Add(key, value);
                    byteline.Clear();
                }

                while ((bytes = readFile.ReadByte()) != -1)
                {
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
                        bytesForWriting = Helper.ReturnOriginalCorrectLine(byteline, dateTime, currentId, logsLvl, out dateTime, out currentId);
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
            }
            File.Delete(Path.GetFullPath("123.txt"));
        }

    }
}
