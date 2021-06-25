using System;
using UnityEngine;

namespace Table.Editor
{
    public class TableSettings : ScriptableObject
    {
        [Header("表格路径")]
        public string excelFolderPath = "Assets/Table/";

        [Header("生成设置")]
        public TableGenerateSettings[] generateSettings = {
            new TableGenerateSettings() {
                name = "Unity",
                @namespace = "Table",
                classFolder = "Assets/Scripts/Table/Generated/",
                assetFolder = "Assets/Resources/Table/",
        } };
    }

    [Serializable]
    public struct TableGenerateSettings
    {
        [Header("名称")]
        public string name;
        [Header("命名空间")]
        public string @namespace;
        [Header("生成描述类路径")]
        public string classFolder;
        [Header("生成数据路径")]
        public string assetFolder;
    }
}
