using System;
using System.IO;

namespace Kontur.LogPacker
{
    internal static class EntryPoint
    {
        public static void Main(string[] args)
        {
            //TODO:8 нет обработки в случае иного кол-ва аргументов
            if (args.Length == 2)
            {
                //TODO:9 var - не всегда понятно. Используй когда очевиден тип справа
                var (inputFile, outputFile) = (args[0], args[1]);

                if (File.Exists(inputFile))
                {
                    Compress(inputFile, outputFile);
                    return;
                }
                //TODO:1 else?
                //У тебя нет никаких проверок на устойчивость программы. Нужно рассматривать кейсы, когда у тебя нет файл, или нет доступа к нему
            }

            if (args.Length == 3 && args[0] == "-d")
            {
                var (inputFile, outputFile) = (args[1], args[2]);

                if (File.Exists(inputFile))
                {
                    Decompress(inputFile, outputFile);
                    return;
                }
                //TODO: same as 1
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
            //TODO:3 поехала табуляция. Очень сильно
           inputFile = MyCompressor.CreateHelpFile(inputFile);
           using (var inputStream = File.OpenRead(inputFile))
            using (var outputStream = File.OpenWrite(outputFile))
                //TODO:2 new compressor -  не очень хорошо. Если у объекта нет лайфлайна, то можно просто делать статик методы
                new Compressor().Compress(inputStream, outputStream);
           //TODO:4 А почему он удаляется? Это не совсем адекватное поведение для архиватора, когда ты исходник удаляешь
            File.Delete(inputFile);            
        }

        private static void Decompress(string inputFile, string outputFile)
        {
            string helpFile = Path.GetFullPath("123.txt");
            using (var inputStream = File.OpenRead(inputFile))
            using (var outputStream = File.OpenWrite(helpFile))
                new Compressor().Decompress(inputStream, outputStream);
            //TODO:5 Метод Return ничего не возвращает. Интересно.
            MyCompressor.ReturnOriginalLog(outputFile);

           
            //TODO:6 не стоит оставлять закоменченный код. Если это какой-то дебаг, то лучше вынеси его в отдельный класс/метод.
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