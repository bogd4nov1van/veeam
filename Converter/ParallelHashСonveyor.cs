using System.Collections.Concurrent;
using veeam.Exceptions;

namespace veeam.Converter
{
    public class ParallelHashСonveyor
    {
        private List<HashСonveyor> hashСonveyors { get; }
        private CancellationTokenSource cancellationTokenSource;
        private List<Thread> threads;
        private int currentBlockIndex = -1;
        private int currentHashIndex = -1;
        private int getBlockIndex()
        {
            currentBlockIndex++;

            if (currentBlockIndex >= hashСonveyors.Count)
                currentBlockIndex = 0;

            return currentBlockIndex;
        }
        private int getHashIndex()
        {
            currentHashIndex++;

            if (currentHashIndex >= hashСonveyors.Count)
                currentHashIndex = 0;

            return currentHashIndex;
        }

        public ParallelHashСonveyor(int countThread, CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource is null)
            {
                throw new ArgumentNullException(nameof(cancellationTokenSource));
            }

            this.cancellationTokenSource = cancellationTokenSource;

            threads = new List<Thread>();
            hashСonveyors = new List<HashСonveyor>();

            for (int i = countThread; i > 0; i--)
            {
                var hashСonveyor = new HashСonveyor(new ConcurrentQueue<byte[]?>(), new ConcurrentQueue<string?>());

                var thread = new Thread(() => hashСonveyor.Start(cancellationTokenSource));

                thread.Name = $"HashСonveyor # {i}"; 

                threads.Add(thread);

                hashСonveyors.Add(hashСonveyor);
            }
        }

        public void StartThreads()
        {
            foreach(var thread in threads)
                thread.Start();
        }
        

        public void AddNextBlock(byte[]? block)
        {
            var currIndex = getBlockIndex();

            hashСonveyors[currIndex].Blocks.Enqueue(block);
        }

        /// <summary>
        /// Возвращает следующий хеш, или исключение EndСonveyorException, в случае конца очереди
        /// </summary>
        public string GetNextHash()
        {
            var currIndex = getHashIndex();

            string? hash;

            while (!cancellationTokenSource.IsCancellationRequested)
            {
                if (hashСonveyors[currIndex].BlockHashs.TryDequeue(out hash))
                {
                    // конец очереди
                    if(hash == null)
                    {
                        cancellationTokenSource.Cancel();
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