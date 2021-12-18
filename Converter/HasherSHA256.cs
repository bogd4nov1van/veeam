using System.Security.Cryptography;
using System.Text;

namespace veeam.Converter
{
    public class HasherSHA256 : IDisposable
    {
        private SHA256 _sha256;
        private StringBuilder _builder;

        public HasherSHA256()
        {
            _sha256 = SHA256.Create();
            _builder = new StringBuilder();
        }

        public string ToHash(byte[] block)
        {
            byte[] hashBlock = _sha256.ComputeHash(block);
            
            _builder.Clear();

            for (int i = 0; i < hashBlock.Length; i++)
            {
                _builder.Append(hashBlock[i].ToString("x2"));
            }

            return _builder.ToString();
        }

        public void Dispose()
        {
            _sha256.Dispose();
        }
    }
}