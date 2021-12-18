using System.Collections.Concurrent;

namespace veeam.Converter
{
    public class HashСonveyor
    {
        public readonly ConcurrentQueue<byte[]?> Blocks;
        public readonly ConcurrentQueue<string?> BlockHashs;

        public HashСonveyor(ConcurrentQueue<byte[]?> blocks,
                            ConcurrentQueue<string?> blockHashs)
        {
            this.Blocks = blocks ?? throw new ArgumentNullException(nameof(blocks));
            this.BlockHashs = blockHashs ?? throw new ArgumentNullException(nameof(blockHashs));
        }

        public void Start(CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource is null)
            {
                throw new ArgumentNullException(nameof(cancellationTokenSource));
            }
            using (var hasher = new HasherSHA256())
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    byte[]? block;

                    if (Blocks.TryDequeue(out block))
                    {
                        //конец очереди
                        if (block == null)
                        {
                            BlockHashs.Enqueue(null);
                            return;
                        }

                        var hash = hasher.ToHash(block);

                        BlockHashs.Enqueue(hash);
                    }
                    else
                    {
                        Thread.Sleep(0);
                    }
                }
            }
        }
    }
}