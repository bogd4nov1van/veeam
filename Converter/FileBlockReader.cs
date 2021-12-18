using veeam.Interfaces;

namespace veeam.Converter
{
    public class FileBlockReader : IBlockReader, IDisposable
    {
        private BinaryReader _reader;

        public FileBlockReader(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new ArgumentException($"Файл {filePath} не существует.");
            }

            _reader = new BinaryReader(File.OpenRead(filePath));
        }

        public byte[] ReadBytes(int count)
        {
            return _reader.ReadBytes(count);
        }
        
        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}