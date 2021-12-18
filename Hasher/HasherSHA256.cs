using System.Security.Cryptography;
using System.Text;
using veeam.Hasher;

namespace veeam.Converter
{
    public class HasherSHA256 : IHasher, IDisposable
    {
        private SHA256 _sha256;

        public HasherSHA256()
        {
            _sha256 = SHA256.Create();
        }

        public string ToHash(byte[] block)
        {
            byte[] hashBlock = _sha256.ComputeHash(block);
            
            var stringBuilder = new StringBuilder();

            for (int i = 0; i < hashBlock.Length; i++)
            {
                stringBuilder.Append(hashBlock[i].ToString("x2"));
            }

            return stringBuilder.ToString();
        }

        public void Dispose()
        {
            _sha256.Dispose();
        }
    }
}