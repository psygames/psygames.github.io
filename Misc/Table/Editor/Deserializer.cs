using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Table.Editor
{
    internal static class Deserializer
    {
        internal static object Deserialze(Type tableType,
            Dictionary<string, string> types,
            Dictionary<string, string> values)
        {
            try
            {
                var obj = Activator.CreateInstance(tableType);
                foreach (var kv in values)
                {
                    var flag = BindingFlags.Instance | BindingFlags.Public;
                    var property = tableType.GetProperty(kv.Key, flag);
                    if (property == null)
                        continue;
                    var val = ParseData(kv.Value, types[kv.Key]);
                    property.SetValue(obj, val);
                }
                return obj;
            }
            catch (Exception e)
            {
                Debug.LogError($"Deserialze Error: {e.Message}");
            }
            return null;
        }

        private static object ParseData(string rawData, string stype)
        {
            // 字符串不做解析
            if (stype == "string")
                return rawData;

            // 数组解析
            if (stype.EndsWith("[]", StringComparison.Ordinal))
            {
                var data = TempCollectionText(rawData);
                data = data.Trim('[', ']');
                var items = data.Split(',');
                int length = items[0] == "" ? 0 : items.Length;
                var itemType = stype.Substring(0, stype.Length - 2);
                Array array = Array.CreateInstance(GetDataType(itemType), length);
                if (array == null)
                {
                    throw new Exception($"invalid array type : {stype}");
                }
                for (int i = 0; i < length; i++)
                {
                    array.SetValue(ParseData(items[i], itemType), i);
                }
                return array;
            }


            var type = GetDataType(stype);
            if (type == null)
            {
                throw new Exception($"非法的数据类型 {stype}: {rawData}");
            }

            if (rawData == "")
            {
                return type.IsValueType ? Activator.CreateInstance(type) : null;
            }

            // 字典解析
            if (stype.StartsWith("Dictionary", StringComparison.Ordinal))
            {
                var data = TempCollectionText(rawData);
                data = data.Trim('[', ']');
                var items = data.Split(',');
                int length = items[0] == "" ? 0 : items.Length;
                var stypes = GetGenericTypes(stype);
                var dict = Activator.CreateInstance(type);
                var addMethod = type.GetMethod("Add");
                for (int i = 0; i < length; i++)
                {
                    var ii = items[i].Trim('(', ')').Split('_');
                    var key = ParseData(ii[0], stypes[0]);
                    var value = ParseData(ii[1], stypes[1]);
                    addMethod.Invoke(dict, new object[] { key, value });
                }
                return dict;
            }

            // 元组解析
            if (stype.StartsWith("Tuple", StringComparison.Ordinal))
            {
                var sps = rawData.Trim('(', ')').Split(',');
                var stypes = GetGenericTypes(stype);
                object[] paras = new object[stypes.Count];
                for (int i = 0; i < stypes.Count; i++)
                {
                    paras[i] = ParseData(sps[i], stypes[i]);
                }
                var tuple = Activator.CreateInstance(type, paras);
                return tuple;
            }

            // Unity 内置类型解析
            if (stype == "Vector2")
            {
                var sps = rawData.Trim('(', ')').Split(',');
                return new Vector2()
                {
                    x = float.Parse(sps[0]),
                    y = float.Parse(sps[1]),
                };
            }

            if (stype == "Vector3")
            {
                var sps = rawData.Trim('(', ')').Split(',');
                return new Vector3()
                {
                    x = float.Parse(sps[0]),
                    y = float.Parse(sps[1]),
                    z = float.Parse(sps[2]),
                };
            }

            if (stype == "Vector4")
            {
                var sps = rawData.Trim('(', ')').Split(',');
                return new Vector4()
                {
                    x = float.Parse(sps[0]),
                    y = float.Parse(sps[1]),
                    z = float.Parse(sps[2]),
                    w = float.Parse(sps[3]),
                };
            }

            if (stype == "RangeInt")
            {
                var sps = rawData.Trim('(', ')').Split(',');
                var start = int.Parse(sps[0]);
                var end = int.Parse(sps[1]);
                return new RangeInt()
                {
                    start = start,
                    length = end - start,
                };
            }

            // 自定义类型 or 基础类型解析
            var flag = BindingFlags.Static | BindingFlags.Public;
            var parseMethod = type.GetMethod("Parse", flag, null, new Type[] { typeof(string) }, null);
            if (parseMethod == null)
            {
                throw new Exception($"类型 {stype} 不存在静态 Parse 方法");
            }

            try
            {
                return parseMethod.Invoke(null, new object[] { rawData });
            }
            catch (Exception e)
            {
                Debug.LogError($"Parse Data Error: type: {type.Name}, parseMethod: {parseMethod}, rawData: {rawData}, error: {e.Message}");
                return null;
            }
        }


        internal static Type GetDataType(string stype)
        {
            if (stype.Contains("<") && stype.Contains(">"))
            {
                var _stype = stype.Substring(0, stype.IndexOf("<", StringComparison.Ordinal)).Trim();
                var stypes = GetGenericTypes(stype);
                Type[] types = new Type[stypes.Count];
                for (int i = 0; i < stypes.Count; i++)
                {
                    types[i] = GetDataType(stypes[i]);
                }
                Type generic = GetDataType($"{_stype}`{stypes.Count}");
                return generic.MakeGenericType(types);
            }

            switch (stype)
            {
                case "string":
                    return typeof(string);
                case "int":
                    return typeof(int);
                case "long":
                    return typeof(long);
                case "float":
                    return typeof(float);
                case "bool":
                    return typeof(bool);
                case "short":
                    return typeof(short);
                case "char":
                    return typeof(char);
                case "double":
                    return typeof(double);
                case "Vector2":
                    return typeof(Vector2);
                case "Vector3":
                    return typeof(Vector3);
                case "Vector4":
                    return typeof(Vector4);
                case "RangeInt":
                    return typeof(RangeInt);
                case "Tuple`1":
                    return typeof(Tuple<>);
                case "Tuple`2":
                    return typeof(Tuple<,>);
                case "Tuple`3":
                    return typeof(Tuple<,,>);
                case "Tuple`4":
                    return typeof(Tuple<,,,>);
                case "Tuple`5":
                    return typeof(Tuple<,,,,>);
                case "Tuple`6":
                    return typeof(Tuple<,,,,,>);
                case "Tuple`7":
                    return typeof(Tuple<,,,,,,>);
                case "Dictionary`2":
                    return typeof(Dictionary<,>);
            }

            var _type = Type.GetType(stype);
            if (_type == null)
                throw new Exception("数据类型不存在: " + stype);
            return _type;
        }

        private static List<string> GetGenericTypes(string stype)
        {
            var _st = stype.IndexOf("<", StringComparison.Ordinal) + 1;
            var _end = stype.IndexOf(">", StringComparison.Ordinal);
            var types = stype.Substring(_st, _end - _st).Split(',');
            var lst = new List<string>();
            foreach (var t in types)
            {
                if (!string.IsNullOrEmpty(t.Trim()))
                {
                    lst.Add(t.Trim());
                }
            }
            return lst;
        }

        private static string TempCollectionText(string rawText)
        {
            int bracketsCount = 0;
            var chars = rawText.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] == '(')
                {
                    bracketsCount++;
                }

                if (chars[i] == ')')
                {
                    bracketsCount--;
                }

                if (bracketsCount > 0 && chars[i] == ',')
                {
                    //TODO: FIX HOLDER
                    chars[i] = '_';
                }
            }
            var data = new string(chars);
            return data;
        }
    }
}
