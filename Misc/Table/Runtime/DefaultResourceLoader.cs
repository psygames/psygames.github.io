
using UnityEngine;

namespace Table
{
    public class DefaultResourceLoader : IResoureLoader
    {
        public byte[] Load(string name)
        {
            string path = $"Table/{name}";
            return Resources.Load<TextAsset>(path).bytes;
        }
    }
}
