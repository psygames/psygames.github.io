using System;
using System.Collections.Generic;

namespace Table
{
    public class TableBaseClass<ID, T> where T : TableBaseClass<ID, T>
    {
        public virtual ID id { get; protected set; }

        public static T Get(ID id)
        {
            var dict = TableManager.GetDict<ID, T>();
            dict.TryGetValue(id, out var val);
            return val;
        }

        public static T Get(Func<T, bool> predicate = null)
        {
            var dict = TableManager.GetDict<ID, T>();
            foreach (var item in dict)
            {
                if (predicate(item.Value))
                {
                    return item.Value;
                }
            }
            return null;
        }

        public static ICollection<T> GetAll(Func<T, bool> predicate = null)
        {
            var dict = TableManager.GetDict<ID, T>();
            if (predicate == null)
            {
                return dict.Values;
            }
            var list = new List<T>();
            foreach (var item in dict)
            {
                if (predicate(item.Value))
                {
                    list.Add(item.Value);
                }
            }
            return list;
        }
    }
}
