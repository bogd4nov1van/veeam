using System.Collections.Concurrent;
using veeam.Interfaces;

namespace veeam.Converter
{
    public class HashСonveyor
    {
        private readonly IHasher _hasher;
        public readonly ConcurrentQueue<byte[]?> _blocks;
        public readonly ConcurrentQueue<string?> _blockHashs;

        public HashСonveyor(IHasher hasher)
        {
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

        public void Start(CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource is null)
            {
                throw new ArgumentNullException(nameof(cancellationTokenSource));
            }
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                byte[]? block;

                if (_blocks.TryDequeue(out block))
                {
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
    }
}