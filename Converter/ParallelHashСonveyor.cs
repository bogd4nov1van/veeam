using veeam.Exceptions;

namespace veeam.Converter
{
    public class ParallelHashСonveyor
    {
        private List<HashСonveyor> _hashСonveyors { get; }
        private CancellationTokenSource _cancellationTokenSource;
        private List<Thread> _threads;
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

        public ParallelHashСonveyor(int countThread, CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource is null)
            {
                throw new ArgumentNullException(nameof(cancellationTokenSource));
            }

            _cancellationTokenSource = cancellationTokenSource;

            _threads = new List<Thread>();
            _hashСonveyors = new List<HashСonveyor>();

            for (int i = countThread; i > 0; i--)
            {
                var hashСonveyor = new HashСonveyor();

                var thread = new Thread(() => hashСonveyor.Start(cancellationTokenSource));

                thread.Name = $"HashСonveyor # {i}"; 

                _threads.Add(thread);

                _hashСonveyors.Add(hashСonveyor);
            }
        }

        public void StartThreads()
        {
            foreach(var thread in _threads)
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
        /// Возвращает следующий хеш, или исключение EndСonveyorException, в случае конца очереди
        /// </summary>
        public string GetNextHash()
        {
            var currHashIndex = getHashIndex();

            string? hash;

            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                if (_hashСonveyors[currHashIndex].TryDequeue(out hash))
                {
                    // конец очереди
                    if(hash == null)
                    {
                        _cancellationTokenSource.Cancel();
                        break;
                    }

                    return hash;
                }
                else
                {
                    Thread.Sleep(0);
                }
            }

            throw new EndСonveyorException();
        }
    }
}