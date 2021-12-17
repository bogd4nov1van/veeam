using System.Collections.Concurrent;
using veeam.Extensions;

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

            while (!cancellationTokenSource.IsCancellationRequested)
            {
                byte[]? block;

                if (Blocks.TryDequeue(out block))
                {
                    //конец очереди
                    if(block == null)
                    {
                        BlockHashs.Enqueue(null);
                        return;
                    }

                    BlockHashs.Enqueue(block.ToHash());
                }
                else
                {
                    Thread.Sleep(0);
                }
            }
        }
    }
}