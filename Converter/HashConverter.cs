using veeam.Exceptions;

namespace veeam.Converter
{
    public class HashConverter
    {
        private readonly string filePath;
        private readonly int sizeBlock;
        private readonly Action<string> outputStream;
        private int numberHash = 0;

        public HashConverter(string filePath, int sizeBlock, Action<string> outputStream)
        {
            if (filePath is null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (outputStream is null)
            {
                throw new ArgumentNullException(nameof(outputStream));
            }

            if (sizeBlock < 1)
            {
                throw new ArgumentException("Размер блока не может быть меньше 1");
            }

            this.filePath = filePath;
            this.sizeBlock = sizeBlock;
            this.outputStream = outputStream;
        }

        public void Convert()
        {
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                try
                {
                    var countThreads = Environment.ProcessorCount;
                    // 1 под запись, 1 под чтение, остальные под хеширование
                    var countParallelConveyor = countThreads - 3;

                    if (countParallelConveyor < 1)
                        countParallelConveyor = 1;

                    var parallelHashСonveyor = new ParallelHashСonveyor(countParallelConveyor, cancellationTokenSource);

                    var outputThread = createThread(() => toOutput(parallelHashСonveyor, cancellationTokenSource), cancellationTokenSource);

                    var start = DateTime.Now;

                    parallelHashСonveyor.StartThreads();

                    outputThread.Start();

                    writeBlocks(parallelHashСonveyor, cancellationTokenSource);

                    outputThread.Join();

                    var end = DateTime.Now;

                    Console.WriteLine((end - start).TotalSeconds);
                }
                catch (Exception ex)
                {
                    outputStream(ex.ToString());
                }
            }
        }

        private void writeBlocks(ParallelHashСonveyor parallelHashСonveyor, CancellationTokenSource cancellationTokenSource)
        {
            if (!File.Exists(filePath))
            {
                throw new ArgumentException($"Файл {filePath} не существует.");
            }

            using (var stream = new BinaryReader(File.OpenRead(filePath)))
            {
                var block = stream.ReadBytes(sizeBlock);

                while (block.Length > 0)
                {
                    if (cancellationTokenSource.IsCancellationRequested)
                        return;

                    parallelHashСonveyor.AddNextBlock(block);

                    block = stream.ReadBytes(sizeBlock);
                }
            }

            // конец очереди
            parallelHashСonveyor.AddNextBlock(null);
        }

        private void toOutput(ParallelHashСonveyor parallelHashСonveyor, CancellationTokenSource cancellationTokenSource)
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                var hash = parallelHashСonveyor.GetNextHash();
                var line = $"{++numberHash}: {hash}";

                outputStream(line);
            }
        }

        private Thread createThread(Action action, CancellationTokenSource cancellationTokenSource)
        {
            return new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (EndСonveyorException)
                {
                    outputStream("Конец обработки");
                }
                catch (Exception ex)
                {
                    outputStream(ex.ToString());
                }
            });
        }
    }
}