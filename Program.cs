using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using veeam.Converter;

namespace veeam
{
    public class Program
    {
        public static int Main(string[] args)
        {
            Thread.CurrentThread.Name = "Main";
            // return runFromConsole(args);
            return runManual();
        }

        private static int runManual()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            
            var sizeBlock = 1024 * 1024 * 1024;
            var path = "/Users/ivanbogdanov/videos/test.txt";

            var result = Convert(path, sizeBlock);

            stopWatch.Stop();
            
            Console.WriteLine($"Время выполнения: {stopWatch.Elapsed}");

            return result;
        }

        private static int runFromConsole(string[] args)
        {
            var pathOption = new Option<string>("--path", description: "Путь до файла");
            pathOption.IsRequired = true;

            var sizeOptions = new Option<int>("--size", description: "Размер блока");
            sizeOptions.IsRequired = true;

            var rootCommand = new RootCommand
            {
                pathOption,
                sizeOptions
            };

            rootCommand.Description = "Конвертирует файл в Hash256 блоки";

            rootCommand.Handler = CommandHandler.Create<string, int>(Convert);

            return rootCommand.Invoke(args);
        }

        private static int Convert(string path, int size)
        {
            // 1 под текущий поток, для вывода
            var countThread = Environment.ProcessorCount - 1;

            try
            {   
                using (var hasher = new HasherSHA256())
                using (var reader = new FileBlockReader(path))
                {
                    var hashConverter = new HashConverter(hasher, reader, countThread, size);

                    var hashNumber = 0;

                    foreach(var hash in hashConverter.Convert())
                    {
                        {
                            var hashWithNumner = $"{++hashNumber}: {hash}";
                        
                            Console.WriteLine(hashWithNumner);
                        }
                    }

                    return 0;
                }
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return -1;
            }
        }
    }
}