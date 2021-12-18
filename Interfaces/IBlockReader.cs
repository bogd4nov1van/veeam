namespace veeam.Interfaces
{
    public interface IBlockReader
    {
         byte[] ReadBytes(int count);
    }
}