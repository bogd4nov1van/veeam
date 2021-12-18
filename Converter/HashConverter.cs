using veeam.Interfaces;

namespace veeam.Converter
{
    public class HashConverter
    {
        private readonly IBlockReader _reader;
        private readonly int _countThread;
        private readonly int _sizeBlock;
        private readonly IHasher _hasher;
        private int _numberHash = 0;

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

            if(countThread < 1)
            {
                throw new ArgumentException("Количество потоков не может быть меньше 1");
            }

            _reader = reader;
            _countThread = countThread;
            _sizeBlock = sizeBlock;
            _hasher = hasher;
        }

        public IEnumerable<string> Convert()
        {
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var parallelHashСonveyor = new ParallelHashСonveyor(_hasher, _countThread, cancellationTokenSource);

                parallelHashСonveyor.StartThreads();

                readBlocksToHashing(parallelHashСonveyor, cancellationTokenSource);
                
                return getHashs(parallelHashСonveyor, cancellationTokenSource);
            }
        }

        private void readBlocksToHashing(ParallelHashСonveyor parallelHashСonveyor, CancellationTokenSource cancellationTokenSource)
        {
            var block = _reader.ReadBytes(_sizeBlock);

            while (block.Length > 0)
            {
                if (cancellationTokenSource.IsCancellationRequested)
                    return;

                parallelHashСonveyor.AddNextBlock(block);

                block = _reader.ReadBytes(_sizeBlock);
            }

            // конец очереди
            parallelHashСonveyor.SetEnding();
        }

        private IEnumerable<string> getHashs(ParallelHashСonveyor parallelHashСonveyor, CancellationTokenSource cancellationTokenSource)
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                var hashResult = parallelHashСonveyor.GetNextHash();

                if (hashResult.IsExists)
                {
                    yield return hashResult.Hash;
                }
            }
        }
    }
}