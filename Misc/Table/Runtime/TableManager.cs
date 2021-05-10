using System;
using System.Collections.Generic;

namespace Table
{
    public static class TableManager
    {
        public static IResoureLoader resoureLoader = null;
        public static ISerializer serializer = null;

        private static Dictionary<Type, object> tables = new Dictionary<Type, object>();

        private static Dictionary<ID, T> Load<ID, T>() where T : TableBaseClass<ID, T>
        {
            if (resoureLoader == null) resoureLoader = new DefaultResourceLoader();
            if (serializer == null) serializer = new DefaultSerializer();
            var bytes = resoureLoader.Load(typeof(T).Name);
            var obj = serializer.Deserialize(bytes);
            var container = obj as TableItemContainer<ID, T>;
            return container.items;
        }

        internal static Dictionary<ID, T> GetDict<ID, T>() where T : TableBaseClass<ID, T>
        {
            var key = typeof(T);
            if (!tables.ContainsKey(key))
            {
                tables.Add(key, Load<ID, T>());
            }
            return tables[key] as Dictionary<ID, T>;
        }
    }
}
