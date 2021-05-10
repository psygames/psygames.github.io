
namespace Table
{
    public interface ISerializer
    {
        public byte[] Serialize(object obj);
        public object Deserialize(byte[] bytes);
    }
}
