namespace veeam.BlockReader
{
    public interface IBlockReader
    {
         byte[] ReadBytes(int count);
    }
}