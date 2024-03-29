﻿using System;
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
            bool isItFirstDate = true;
            Dictionary<string, string> logsLvl = new Dictionary<string, string>();
            using (FileStream readFile = new FileStream(inputFile, FileMode.Open))
            using (FileStream writeFile = new FileStream(Path.GetFullPath("123.txt"), FileMode.Create))
            {
                while (readFile.Position < readFile.Length)
                {
                    bytes = readFile.ReadByte();
                    if (Helper.IsByteDigit((byte)bytes) && (byteline.Count == 0))
                    {
                        while ((bytes != 10) && (bytes != -1) && (byteline.Count < 2000))
                        {
                            byteline.Add((byte)bytes);
                            bytes = readFile.ReadByte();
                        }
                        if (byteline.Count >= 2000)
                        {
                            writeFile.WriteByte(33);
                            bytesForWriting = byteline.ToArray();
                            for (int i = 0; i < bytesForWriting.Length; i++)
                            {
                                writeFile.WriteByte(bytesForWriting[i]);
                            }
                            while ((bytes != 10) && (bytes != -1))
                            {
                                writeFile.WriteByte((byte)bytes);
                                bytes = readFile.ReadByte();
                            }
                        }
                        else
                        {
                            if (byteline[byteline.Count - 1] == 13)
                            {
                                byteline.RemoveAt(byteline.Count - 1);
                                bytes = 13;
                            }
                            bytesForWriting = Helper.CreateOptimalByteLine(byteline, dateTime, currentId, logsLvl, isItFirstDate, out dateTime, out currentId, out isItFirstDate);
                            for (int i = 0; i < bytesForWriting.Length; i++)
                            {
                                writeFile.WriteByte(bytesForWriting[i]);
                            }
                        }
                        if (bytes == 13) { writeFile.WriteByte(13); }
                        if (bytes == -1) { break; }
                        writeFile.WriteByte(10);
                        byteline.Clear();

                    }
                    else
                    {
                        writeFile.WriteByte(33);
                        while ((bytes != 10) && (bytes != -1))
                        {
                            writeFile.WriteByte((byte)bytes);
                            bytes = readFile.ReadByte();
                        }                                                
                        if (bytes == -1) { break; }
                        writeFile.WriteByte(10);
                        byteline.Clear();
                    }
                }
                ulong bytesCount = 0;
                string pair = "";
                foreach (var curr in logsLvl)
                {
                    pair = curr.Key + '`' + curr.Value;
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

        public static void WriteOriginalLog(string outputFile)
        {
            int bytes;
            long pos = 0;
            ulong currentId = 0;
            List<byte> byteline = new List<byte>();
            byte[] bytesForWriting;
            Dictionary<string, string> logsLvl = new Dictionary<string, string>();
            DateTime dateTime = new DateTime();
            using (FileStream readFile = new FileStream(Path.GetFullPath("123.txt"), FileMode.Open))
            using (FileStream writeFile = new FileStream(outputFile, FileMode.Create))
            {
                //create dictionary
                readFile.Position = readFile.Length - 8;

                //how many bytes is dictionary
                for (int i = 0; i < 8; i++)
                {
                    bytes = readFile.ReadByte();
                    byteline.Add((byte)bytes);
                }

                pos = BitConverter.ToInt64(byteline.ToArray());
                byteline.Clear();
                readFile.Position = readFile.Length - 8 - pos;

                //read dictionary
                while (readFile.Position < readFile.Length - 8)
                {

                    bytes = readFile.ReadByte();
                    string key;
                    string value;
                    while (bytes != 96)
                    {
                        byteline.Add((byte)bytes);
                        bytes = readFile.ReadByte();
                    }
                    value = Encoding.UTF8.GetString(byteline.ToArray());
                    byteline.Clear();
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

                readFile.Position = 0;
                //read file without dictionary
                pos = readFile.Length - 7 - pos;
                while (readFile.Position < pos)
                {
                    bytes = readFile.ReadByte();
                    if (readFile.Position >= pos) { break; }
                    if (byteline.Count == 0 && bytes != 33)
                    {
                        byteline.Add((byte)bytes);
                        bytes = readFile.ReadByte();
                        while ((bytes) != 10 && (bytes) != 13)
                        {
                            if (readFile.Position >= pos) { break; }
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
                        while ((bytes != 10) && (readFile.Position < pos))
                        {
                            writeFile.WriteByte((byte)bytes);
                            bytes = readFile.ReadByte();
                        }                        
                        if (readFile.Position >= pos) { break; }
                        writeFile.WriteByte(10);
                        byteline.Clear();
                    }
                }
                logsLvl.Clear();
            }
            File.Delete(Path.GetFullPath("123.txt"));
        }

    }
}
