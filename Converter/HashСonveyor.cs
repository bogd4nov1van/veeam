using System.Collections.Concurrent;
using veeam.Hasher;

namespace veeam.Converter
{
    public class HashСonveyor
    {
        private readonly IHasher _hasher;
        public readonly ConcurrentQueue<byte[]?> _blocks;
        public readonly ConcurrentQueue<string?> _blockHashs;
        private CancellationTokenSource _cancellationTokenSource;

        public HashСonveyor(IHasher hasher, CancellationTokenSource cancellationTokenSource)
        {
            _cancellationTokenSource = cancellationTokenSource ?? throw new ArgumentNullException(nameof(cancellationTokenSource));
            _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
            _blocks = new ConcurrentQueue<byte[]?>();
            _blockHashs = new ConcurrentQueue<string?>();
        }

        public void Enqueue(byte[]? block)
        {
            _blocks.Enqueue(block);
        }

        public bool TryDequeue(out string hash)
        {
            return _blockHashs.TryDequeue(out hash);
        }

        public void Start()
        {
            try
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    byte[]? block;

                    if (_blocks.TryDequeue(out block))
                    {
                        // if(Thread.CurrentThread.Name == "3")
                        //     throw new Exception("test");
                        
                        //конец очереди
                        if (block == null)
                        {
                            _blockHashs.Enqueue(null);
                            return;
                        }

                        var hash = _hasher.ToHash(block);

                        _blockHashs.Enqueue(hash);
                    }
                    else
                    {
                        Thread.Sleep(0);
                    }
                }
            }
            catch (Exception)
            {
                _cancellationTokenSource.Cancel();
                throw;
            }
        }
    }
}