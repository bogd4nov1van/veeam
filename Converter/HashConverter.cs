using veeam.BlockReader;
using veeam.Hasher;

namespace veeam.Converter
{
    public class HashConverter : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ParallelHashСonveyor _parallelHashСonveyor;
        private readonly IBlockReader _reader;
        private readonly int _sizeBlock;

        public HashConverter(IHasher hasher, IBlockReader reader, int countThread, int sizeBlock)
        {
            if (hasher is null)
            {
                throw new ArgumentNullException(nameof(hasher));
            }

            if (reader is null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (sizeBlock < 1)
            {
                throw new ArgumentException("Размер блока не может быть меньше 1");
            }

            if (countThread < 1)
            {
                throw new ArgumentException("Количество потоков не может быть меньше 1");
            }

            _cancellationTokenSource = new CancellationTokenSource();

            // 1 тред на чтение из IBlockReader
            var countParallelHashing = countThread - 1;

            _parallelHashСonveyor = new ParallelHashСonveyor(hasher, countParallelHashing, _cancellationTokenSource);

            _reader = reader;
            _sizeBlock = sizeBlock;
        }

        public IEnumerable<string> Convert()
        {
            try
            {
                _parallelHashСonveyor.StartAsync();

                readBlockAsync();

                return getHashs();
            }
            catch (System.Exception)
            {
                _cancellationTokenSource.Cancel();
                throw;
            }
        }

        private IEnumerable<string> getHashs()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                var hashResult = _parallelHashСonveyor.GetNextHash();

                if (!hashResult.IsEnd)
                {
                    yield return hashResult.Hash;
                }
                else
                {
                    break;
                }
            }
        }

        private void readBlockAsync()
        {
            var readBlockThread = new Thread(readBlocks);
            readBlockThread.Name = "Read blocks";
            readBlockThread.Start();
        }

        private void readBlocks()
        {
            try
            {
                var block = _reader.ReadBytes(_sizeBlock);

                while (block.Length > 0)
                {
                    if (_cancellationTokenSource.IsCancellationRequested)
                        return;

                    _parallelHashСonveyor.AddNextBlock(block);

                    block = _reader.ReadBytes(_sizeBlock);
                }

                // конец очереди
                _parallelHashСonveyor.SetEnding();
            }
            catch (System.Exception)
            {
                _cancellationTokenSource.Cancel();
                throw;
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Dispose();
        }
    }
}