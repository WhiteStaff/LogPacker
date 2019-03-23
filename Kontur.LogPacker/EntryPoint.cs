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
           inputFile = MyCompressor.CreateHelpFile(inputFile);
           using (var inputStream = File.OpenRead(inputFile))
            using (var outputStream = File.OpenWrite(outputFile))
                new Compressor().Compress(inputStream, outputStream);
            File.Delete(inputFile);            
        }

        private static void Decompress(string inputFile, string outputFile)
        {
            string helpFile = Path.GetFullPath("123.txt");
            using (var inputStream = File.OpenRead(inputFile))
            using (var outputStream = File.OpenWrite(helpFile))
                new Compressor().Decompress(inputStream, outputStream);

           

           /* System.IO.StreamReader file2 = new System.IO.StreamReader(@"C:\Users\224801\Desktop\dec.txt");
            System.IO.StreamReader file3 = new System.IO.StreamReader(@"C:\Users\224801\Desktop\000.txt");
            int c = 0;
            while ((line = file2.ReadLine()) != null)
            {
                string line2 = file3.ReadLine();
                
                if (line.Equals(line2))
                {
                    c++;
                }
                else
                {
                    Console.WriteLine(c);
                    c++;
                }
            }*/

        }
    }
}