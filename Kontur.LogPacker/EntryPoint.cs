using System;
using System.IO;

namespace Kontur.LogPacker
{
    internal static class EntryPoint
    {
        public static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                var (inputFile, outputFile) = (args[0], args[1]);

                if (File.Exists(inputFile))
                {
                    Compress(inputFile, outputFile);
                    return;
                }
            }

            if (args.Length == 3 && args[0] == "-d")
            {
                var (inputFile, outputFile) = (args[1], args[2]);

                if (File.Exists(inputFile))
                {
                    Decompress(inputFile, outputFile);
                    return;
                }
            }

            ShowUsage();
        }

        private static void ShowUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine($"{AppDomain.CurrentDomain.FriendlyName} [-d] <inputFile> <outputFile>");
            Console.WriteLine("\t-d flag turns on the decompression mode");
            Console.WriteLine();
        }

        private static void Compress(string inputFile, string outputFile)
        {
            int counter = 0;
            string line;

            DateTime dateTime = new DateTime();
            System.IO.StreamReader file = new System.IO.StreamReader(inputFile);
            while ((line = file.ReadLine()) != null)
            {
                string newline = "";
                if (Helper.IsLineCorrect(line))
                {
                    if (counter != 0)
                    {
                        newline = Helper.DateDifference(DateTime.ParseExact(Helper.PartOfString(line, 0, 23), "yyyy-MM-dd HH:mm:ss,fff", null), dateTime) + line.Remove(0, 23);
                    }
                    else
                    {
                        newline = line;
                    }

                    dateTime = DateTime.ParseExact(Helper.PartOfString(line, 0, 23), "yyyy-MM-dd HH:mm:ss,fff", null);

                }
                else
                {
                    newline = line;
                }
                counter++;
                using (StreamWriter sw = new StreamWriter(outputFile, true, System.Text.Encoding.UTF8))
                {
                    sw.WriteLine(newline);
                }
            }

            file.Close();


            using (var inputStream = File.OpenRead(outputFile))
            using (var outputStream = File.OpenWrite(@"C:\Users\224801\Desktop\123t.txt"))
                new Compressor().Compress(inputStream, outputStream);
            //File.WriteAllBytes(outputFile, File.ReadAllBytes(inputFile));
        }

        private static void Decompress(string inputFile, string outputFile)
        {
            using (var inputStream = File.OpenRead(inputFile))
            using (var outputStream = File.OpenWrite(outputFile))
                new Compressor().Decompress(inputStream, outputStream);
        }
    }
}