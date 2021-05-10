using System.Collections.Generic;

namespace Table
{
    public class TableItemContainer<ID, T> where T : TableBaseClass<ID, T>
    {
        public Dictionary<ID, T> items { get; private set; } = new Dictionary<ID, T>();
    }
}
