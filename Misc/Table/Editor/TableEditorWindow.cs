using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Table.Editor
{
    [InitializeOnLoad]
    public class TableEditorWindow : EditorWindow
    {
        private static TableEditorWindow window;
        private TableSettings settings = null;
        private const string COMPILING_CHECK_KEY = "TableToolGenerateCompiling";

        [UnityEditor.Callbacks.DidReloadScripts]
        static void AllScriptsReloaded()
        {
            if (EditorPrefs.GetBool(COMPILING_CHECK_KEY, false) && window != null)
            {
                window.GeneAllAssets();
                EditorPrefs.SetBool(COMPILING_CHECK_KEY, false);
            }
        }

        [MenuItem("Tools/配置表工具", priority = 10000)]
        static void OpenTableTool()
        {
            var window = GetWindow(typeof(TableEditorWindow)) as TableEditorWindow;
            window.titleContent = new GUIContent("配置表工具");
        }

        private void OnEnable()
        {
            window = this;

            var guids = AssetDatabase.FindAssets($"{nameof(TableSettings)} t:ScriptableObject");
            if (guids.Length <= 0)
            {
                var obj = ScriptableObject.CreateInstance(typeof(TableSettings));
                AssetDatabase.CreateAsset(obj, $"Assets/{nameof(TableSettings)}.asset");
                AssetDatabase.SaveAssets();
                settings = (TableSettings)obj;
                return;
            }

            var root = Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(guids[0]));
            settings = AssetDatabase.LoadAssetAtPath<TableSettings>($"{root}/TableToolSettings.asset");
        }

        private Vector2 scrollPos;
        private void OnGUI()
        {
            try
            {
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("全部表格：");
                EditorGUILayout.BeginVertical("box");
                if (!Directory.Exists(settings.excelFolderPath))
                {
                    EditorGUILayout.LabelField($"Excel表格路径不存在：{settings.excelFolderPath}");
                }
                else
                {
                    scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                    foreach (var fi in TableHelper.GetExcelList(settings.excelFolderPath))
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(Path.GetFileName(fi));
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndScrollView();
                }
                EditorGUILayout.EndVertical();

            }
            catch (Exception e)
            {
                Debug.LogError("读取xml失败: " + e.Message + "\n" + e.StackTrace);
            }
            try
            {
                EditorGUILayout.Space();
                if (GUILayout.Button("一键生成"))
                {
                    GeneAllClasses();
                    EditorPrefs.SetBool(COMPILING_CHECK_KEY, EditorApplication.isCompiling);
                    if (EditorApplication.isCompiling)
                    {
                        window.ShowNotification(new GUIContent("编译中，请稍后..."));
                    }
                    else
                    {
                        GeneAllAssets();
                    }
                }
                EditorGUILayout.Space();
            }
            catch (Exception e)
            {
                Debug.LogError("生成失败: " + e.Message + "\n" + e.StackTrace);
                EditorUtility.ClearProgressBar();
            }
        }

        public void GeneAllClasses()
        {
            var files = TableHelper.GetExcelList(settings.excelFolderPath);
            foreach (var genSettins in settings.generateSettings)
            {
                int i = 0;
                foreach (var excelPath in files)
                {
                    var excelName = Path.GetFileName(excelPath);
                    bool cancel = EditorUtility.DisplayCancelableProgressBar($"生成Class文件中...", excelName, 1f * i / files.Count);
                    if (cancel)
                    {
                        EditorUtility.ClearProgressBar();
                        break;
                    }

                    GenerateClassText(excelPath, genSettins);
                    i++;
                }
                ClearRedundance(genSettins.classFolder);
            }

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
            Debug.Log("生成数据类完成");
        }

        private static string FixedTableClassName(string name)
        {
            name = Path.GetFileNameWithoutExtension(name);
            if (!name.StartsWith("Table"))
            {
                name = "Table" + name;
            }
            return name;
        }

        private void ClearRedundance(string folderPath)
        {
            var files = TableHelper.GetExcelList(settings.excelFolderPath);
            foreach (var fi in new DirectoryInfo(folderPath).GetFiles())
            {
                var fileName = Path.GetFileNameWithoutExtension(fi.Name);
                fileName = Path.GetFileNameWithoutExtension(fileName); // for .meta
                bool exist = false;
                foreach (var excel in files)
                {
                    var excelName = FixedTableClassName(excel);
                    if (excelName == fileName)
                    {
                        exist = true;
                        break;
                    }
                }
                if (!exist)
                {
                    File.Delete(fi.FullName);
                    Debug.Log("清除冗余文件：" + fi.FullName);
                }
            }
        }

        private void GenerateClassText(string excelPath, TableGenerateSettings genSettings)
        {
            // 第一行属性名
            // 第二行属性类型
            // 第三行属性描述
            // 第四行以及之后数据航

            string className = FixedTableClassName(excelPath);
            string outputFolder = genSettings.classFolder;
            if (!Directory.Exists(outputFolder))
            {
                Debug.LogError("未能找到Class生成目录路径：" + outputFolder);
                return;
            }
            string _namespace = genSettings.@namespace;
            var properties = TableHelper.ReadTableProperties(excelPath);
            var idType = properties[0].type;
            var propertiesStr = "";
            for (int i = 0; i < properties.Count; i++)
            {
                var p = properties[i];
                propertiesStr += TableHelper.GenProperty(p.type, p.name);
            }
            var classStr = TableHelper.GenClass(_namespace, className, idType, propertiesStr);
            File.WriteAllText(Path.Combine(outputFolder, $"{className}.cs"), classStr);
        }

        public void GeneAllAssets()
        {
            try
            {
                var files = TableHelper.GetExcelList(settings.excelFolderPath);
                foreach (var genSettins in settings.generateSettings)
                {
                    int i = 0;
                    foreach (var excelPath in files)
                    {
                        var excelName = Path.GetFileName(excelPath);
                        bool cancel = EditorUtility.DisplayCancelableProgressBar($"生成数据文件中...", excelName, 1f * i / files.Count);
                        if (cancel)
                        {
                            EditorUtility.ClearProgressBar();
                            break;
                        }
                        GenerateAssetFile(excelPath, genSettins);
                        i++;
                    }
                    ClearRedundance(genSettins.assetFolder);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message + "\n" + e.StackTrace);
            }
            finally
            {
                AssetDatabase.Refresh();
                EditorUtility.ClearProgressBar();
            }
            Debug.Log("生成数据文件完成");
        }

        private void GenerateAssetFile(string excelPath, TableGenerateSettings genSettings)
        {
            string className = FixedTableClassName(excelPath);
            string outputFolder = genSettings.assetFolder;
            if (!Directory.Exists(outputFolder))
            {
                Debug.LogError("未能找到Asset生成目录路径：" + outputFolder);
                return;
            }

            var properties = TableHelper.ReadTableProperties(excelPath);
            var datas = TableHelper.ReadTable(excelPath);

            // type<key,stype>
            var types = new Dictionary<string, string>();
            foreach (var p in properties)
            {
                types.Add(p.name, p.type);
            }

            // data<key,val>

            var tableType = Assembly.Load("Assembly-CSharp").GetType(className);
            var idType = Deserializer.GetDataType(properties[0].type);
            var containerType = typeof(TableItemContainer<,>).MakeGenericType(idType, tableType);
            var container = Activator.CreateInstance(containerType);

            for (int i = 3; i < datas.Count; i++)
            {
                var cols = datas[i];
                var colDict = new Dictionary<string, string>();
                for (int j = 0; j < cols.Count; j++)
                {
                    colDict.Add(properties[j].name, cols[j]);
                }
                var itemObj = Deserializer.Deserialze(tableType, types, colDict);
                container.
            }

            var savePath = Path.Combine(outputFolder, $"{className}.bytes");
            var text = LitJson.JsonMapper.ToJson(jsonDict);
            File.WriteAllText(savePath, text);
        }
    }
}
