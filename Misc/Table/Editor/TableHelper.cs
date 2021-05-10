using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace Table.Editor
{
    internal static class TableHelper
    {
        private const string TRANSPOSED_TAG = "(transposed)";
        internal static string GenProperty(string type, string name)
        {
            var template = @"public {type} {name} { get; private set; }
";
            return template.Replace("{type}", type).Replace("{name}", name);
        }

        internal static string GenClass(string _namespace, string className, string idType, string properties)
        {
            if (string.IsNullOrEmpty(_namespace))
            {
                var template = @"using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class {className} : BaseTable<{idType}, {className}>
{
    {properties}
}";
                properties = properties.Replace("\n", "\n    ");
                return template.Replace("{className}", className).Replace("{properties}", properties).Replace("{idType}", idType);
            }
            else
            {

                var template = @"using System;
using System.Collections.Generic;
using UnityEngine;

namespace {namespace}
{
    [Serializable]
    public class {className} : BaseTable<{idType}, {className}>
    {
        {properties}
    }
}";
                properties = properties.Replace("\n", "\n        ");
                return template.Replace("{className}", className).Replace("{properties}", properties).Replace("{idType}", idType).Replace("{namespace}", _namespace);
            }

        }

        private static bool IsIgnoreRow(List<List<string>> datas, int row)
        {
            return datas[row].Count <= 0 || string.IsNullOrEmpty(datas[row][0]);
        }

        private static bool IsIgnoreColumn(List<List<string>> datas, int column)
        {
            return string.IsNullOrEmpty(datas[0][column])
                || string.IsNullOrEmpty(datas[1][column]);
        }

        // 第一行属性名
        // 第二行属性类型
        // 第三行属性描述
        // 第四行之后是数据
        internal static List<List<string>> ReadTable(string excelPath)
        {
            var datas = new List<List<string>>();
            Action<int, int, string> add = (i, j, d) =>
            {
                if (datas.Count <= i)
                    datas.Add(new List<string>());
                if (datas[i].Count <= j)
                    datas[i].Add(d);
                else
                    datas[i][j] = d;
            };
            using (var stream = File.Open(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet();
                    var table = result.Tables[0];
                    var flip = table.Rows[0].ItemArray[0].ToString().EndsWith(TRANSPOSED_TAG, StringComparison.OrdinalIgnoreCase);
                    int _row = 0;
                    int _col = 0;
                    foreach (DataRow row in table.Rows)
                    {
                        _col = 0;
                        foreach (var item in row.ItemArray)
                        {
                            var _val = item.ToString();
                            if (flip)
                            {
                                if (_row == 0 && _col == 0)
                                {
                                    _val = _val.Substring(0, _val.Length - TRANSPOSED_TAG.Length);
                                }
                                add(_col, _row, _val);
                            }
                            else
                            {
                                add(_row, _col, _val);
                            }
                            _col++;
                        }
                        _row++;
                    }
                }
            }

            // Remove Ignore Rows
            for (int i = datas.Count - 1; i >= 3; i--)
            {
                if (IsIgnoreRow(datas, i))
                {
                    datas.RemoveAt(i);
                }
            }

            // Remove Ignore Columns
            for (int i = datas[0].Count - 1; i >= 0; i--)
            {
                if (IsIgnoreColumn(datas, i))
                {
                    foreach (var row in datas)
                    {
                        row.RemoveAt(i);
                    }
                }
            }

            //TODO: 检查数据合法性

            return datas;
        }

        internal static bool IsValidClassName(string val)
        {
            if (string.IsNullOrEmpty(val))
                return false;
            var index = 0;
            foreach (var c in val)
            {
                if (index == 0)
                {
                    if (!(c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' || c == '_'))
                        return false;
                }
                else
                {
                    if (!(c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' || c == '_' || c >= '0' && c <= '9'))
                        return false;
                }
                index++;
            }
            return true;
        }

        internal static List<string> GetExcelList(string path)
        {
            var files = Directory.GetFiles(path)
                .Where(a =>
                {
                    var fileName = Path.GetFileNameWithoutExtension(a);
                    var ext = Path.GetExtension(a);
                    return IsValidClassName(fileName) && (ext == ".xlsx" || ext == ".xls");
                })
                .ToList();
            return files;
        }

        internal static List<TablePropertyData> ReadTableProperties(string excelPath)
        {
            var properties = new List<TablePropertyData>();
            int _headRowColumnCount = -1;
            var datas = ReadTable(excelPath);
            int _row = 0;
            foreach (var row in datas)
            {
                if (_row == 0)
                {
                    _headRowColumnCount = row.Count;
                }
                else if (row.Count < _headRowColumnCount)
                {
                    throw new Exception($"{excelPath} 第{_row}行列数小于首行。");
                }
                if (_row > 3)
                {
                    break;
                }
                int _column = 0;
                foreach (var item in row)
                {
                    if (_column >= _headRowColumnCount)
                        break;
                    switch (_row)
                    {
                        case 0:
                            var p = new TablePropertyData
                            {
                                name = item.ToString()
                            };
                            properties.Add(p);
                            break;
                        case 1:
                            properties[_column].type = item.ToString();
                            break;
                        case 2:
                            properties[_column].description = item.ToString();
                            break;
                    }
                    _column++;
                }
                _row++;
            }

            return properties;
        }
    }
}
