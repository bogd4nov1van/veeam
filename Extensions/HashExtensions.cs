using System.Security.Cryptography;
using System.Text;

namespace veeam.Extensions
{
    public static class HashExtensions
    {
        public static string ToHash(this byte[] block)
        { 
            using (var sha256Hash = SHA256.Create())  
            {
                byte[] hashBlock = sha256Hash.ComputeHash(block);

                var builder = new StringBuilder();  

                for (int i = 0; i < hashBlock.Length; i++)  
                {  
                    builder.Append(hashBlock[i].ToString("x2"));  
                } 

                return builder.ToString();  
            } 
        }
    }
}