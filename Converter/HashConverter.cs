using veeam.Interfaces;

namespace veeam.Converter
{
    public class HashConverter
    {
        private readonly IBlockReader _reader;
        private readonly int sizeBlock;
        private readonly Action<string> outputStream;
        private int numberHash = 0;

        public HashConverter(IBlockReader reader, int sizeBlock, Action<string> outputStream)
        {            
            if (outputStream is null)
            {
                throw new ArgumentNullException(nameof(outputStream));
            }

            if (reader is null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (sizeBlock < 1)
            {
                throw new ArgumentException("Размер блока не может быть меньше 1");
            }

            _reader = reader;
            this.sizeBlock = sizeBlock;
            this.outputStream = outputStream;
        }

        public void Convert()
        {
            using (var hasher = new HasherSHA256())
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                try
                {
                    var countThreads = Environment.ProcessorCount;
                    // 1 под запись, 1 под чтение, остальные под хеширование
                    var countParallelConveyor = countThreads - 3;

                    if (countParallelConveyor < 1)
                        countParallelConveyor = 1;

                    var parallelHashСonveyor = new ParallelHashСonveyor(hasher, countParallelConveyor, cancellationTokenSource);

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
            var block = _reader.ReadBytes(sizeBlock);

            while (block.Length > 0)
            {
                if (cancellationTokenSource.IsCancellationRequested)
                    return;

                parallelHashСonveyor.AddNextBlock(block);

                block = _reader.ReadBytes(sizeBlock);
            }

            // конец очереди
            parallelHashСonveyor.SetEnding();
        }

        private void toOutput(ParallelHashСonveyor parallelHashСonveyor, CancellationTokenSource cancellationTokenSource)
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                var hashResult = parallelHashСonveyor.GetNextHash();

                if (hashResult.IsExists)
                {
                    var line = $"{++numberHash}: {hashResult.Hash}";

                    outputStream(line);
                }
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
                catch (Exception ex)
                {
                    outputStream(ex.ToString());
                }
            });
        }
    }
}