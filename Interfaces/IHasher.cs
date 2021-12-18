namespace veeam.Interfaces
{
    public interface IHasher
    {
         string ToHash(byte[] block);
    }
}