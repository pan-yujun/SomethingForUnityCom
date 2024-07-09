using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CustomUnityEditorTool
{
    public class SearchShader
    {
        public static string outputPath = Application.dataPath + "/SearchMaterialsForShader.txt";
        public static string FilePath = "Assets";
        //搜索固定文件夹中的所有Material的路径
        public static List<string> listMatrials;
        public static List<string> listTargetMaterial;

        public static string selectedShaderName;

        public static StringBuilder sb;

        [MenuItem("Assets/EditorTool/SearchShader", true)]
        private static bool OptionSelectAvailable()
        {
            if (Selection.activeObject == null)
            {
                return false;
            }
            return Selection.activeObject.GetType() == typeof(Shader);
        }

        [MenuItem("Assets/EditorTool/SearchShader")]
        private static void SearchConstantShader()
        {
            Debug.Log("当前选中的Shader名字：" + Selection.activeObject.name);
            sb = new StringBuilder();

            selectedShaderName = Selection.activeObject.name;

            listMatrials = new List<string>();
            listMatrials.Clear();
            listTargetMaterial = new List<string>();
            listTargetMaterial.Clear();

            //项目路径 eg:projectPath = D:Project/Test/Assets
            string projectPath = Application.dataPath;

            //eg:projectPath = D:Project/Test
            projectPath = projectPath.Substring(0, projectPath.IndexOf("Assets"));

            try
            {
                //获取某一文件夹中的所有Matrial的Path信息
                GetMaterialsPath(projectPath, FilePath, "Material", ref listMatrials);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }

            for (int i = 0; i < listMatrials.Count; i++)
            {
                EditorUtility.DisplayProgressBar("Check Materials", "Current Material :"
                    + i + "/" + listMatrials.Count, (float)i / listMatrials.Count);

                try
                {
                    //开始Check
                    BegainCheckMaterials(listMatrials[i]);
                }
                catch (System.Exception e)
                {
                    EditorUtility.ClearProgressBar();
                    Debug.LogError(e);
                }
            }

            PrintToTxt();
            EditorUtility.ClearProgressBar();
            Debug.Log("Check Success");
        }

        //获取某一文件夹中的所有Matrial的Path信息
        public static void GetMaterialsPath(string projectPath, string targetFilePath, string searchType, ref List<string> array)
        {
            if (Directory.Exists(targetFilePath))
            {
                string[] guids;
                //搜索
                guids = AssetDatabase.FindAssets("t:" + searchType, new[] { targetFilePath });
                foreach (string guid in guids)
                {
                    string source = AssetDatabase.GUIDToAssetPath(guid);
                    listMatrials.Add(source);
                }
            }
        }

        //开始检查Material
        public static void BegainCheckMaterials(string materialPath)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (mat.shader.name == selectedShaderName)
            {
                listTargetMaterial.Add(materialPath);
            }
        }

        public static void PrintToTxt()
        {
            //加入shader的名字
            listTargetMaterial.Add(selectedShaderName);

            // 清空文件
            FileStream stream = File.Open(outputPath, FileMode.OpenOrCreate, FileAccess.Write);
            stream.Seek(0, SeekOrigin.Begin);
            stream.SetLength(0);
            stream.Close();

            // 情况临时字浮串buffer
            sb.Clear();
            // 写入文件
            using (FileStream fs = new FileStream(outputPath, FileMode.Open))
            {
                using (BinaryWriter w = new BinaryWriter(fs))
                {
                    sb.Append($"引用了{selectedShaderName}的材质有：\n");
                    for (int i = 0; i < listTargetMaterial.Count - 1; i++)
                    {
                        sb.Append(listTargetMaterial[i] + "\n");
                    }
                    string useNum = string.Format("共有 {0} 个Material用到：{1}", listTargetMaterial.Count - 1, selectedShaderName);
                    sb.Append(useNum + "\n");
                    
                    w.Write(sb.ToString());
                }
            }
            Debug.Log($"引用{selectedShaderName}的文件列表：{outputPath}");
        }
    }
}