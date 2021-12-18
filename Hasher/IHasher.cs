namespace veeam.Hasher
{
    public interface IHasher
    {
         string ToHash(byte[] block);
    }
}