using veeam.Hasher;

namespace veeam.Converter
{
    public class ParallelHashСonveyor
    {
        private List<HashСonveyor> _hashСonveyors { get; }
        private CancellationTokenSource _cancellationTokenSource;
        private List<Thread> _threads;
        private List<Exception> _exceptions = new List<Exception>();
        private int _currentBlockIndex = -1;
        private int _currentHashIndex = -1;
        private int getBlockIndex()
        {
            _currentBlockIndex++;

            if (_currentBlockIndex >= _hashСonveyors.Count)
                _currentBlockIndex = 0;

            return _currentBlockIndex;
        }
        private int getHashIndex()
        {
            _currentHashIndex++;

            if (_currentHashIndex >= _hashСonveyors.Count)
                _currentHashIndex = 0;

            return _currentHashIndex;
        }

        public ParallelHashСonveyor(IHasher hasher, int countThread, CancellationTokenSource cancellationTokenSource)
        {
            if (hasher is null)
            {
                throw new ArgumentNullException(nameof(hasher));
            }

            _cancellationTokenSource = cancellationTokenSource ?? throw new ArgumentNullException(nameof(cancellationTokenSource));

            _threads = new List<Thread>();
            _hashСonveyors = new List<HashСonveyor>();

            for (int i = countThread; i > 0; i--)
            {
                var hashСonveyor = new HashСonveyor(hasher, _cancellationTokenSource);
                _hashСonveyors.Add(hashСonveyor);

                var hashСonveyorThread = new Thread(new ThreadStart(hashСonveyor.Start));
                hashСonveyorThread.Name = i.ToString();
                _threads.Add(hashСonveyorThread);
            }
        }

        public void StartAsync()
        {
            foreach (var thread in _threads)
                thread.Start();
        }

        public void SetEnding()
        {
            AddNextBlock(null);
        }

        public void AddNextBlock(byte[] block)
        {
            var currBlockIndex = getBlockIndex();

            _hashСonveyors[currBlockIndex].Enqueue(block);
        }

        /// <summary>
        /// Возвращает false и хеш, или true и null в случае конца очереди
        /// </summary>
        public (bool IsEnd, string? Hash) GetNextHash()
        {
            var currHashIndex = getHashIndex();

            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                string? hash;

                if (_hashСonveyors[currHashIndex].TryDequeue(out hash))
                {
                    // конец очереди
                    if (hash == null)
                    {
                        _cancellationTokenSource.Cancel();
                        return (true, null);
                    }

                    return (false, hash);
                }
                else
                {
                    Thread.Sleep(0);
                }
            }

            return (true, null);
        }
    }
}