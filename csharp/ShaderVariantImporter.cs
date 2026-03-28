using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Text.RegularExpressions;

/// <summary>
/// 从变体收集网获取缺失变体并添加到 ShaderVariantCollection
/// </summary>
public class ShaderVariantImporter : EditorWindow
{
    private ShaderVariantCollection targetSVC;
    private string variantText = "";
    private Vector2 scrollPos;

    [MenuItem("Tools/Shader Variant Collection/导入变体到SVC", false, 100)]
    public static void ShowWindow()
    {
        GetWindow<ShaderVariantImporter>("导入变体到SVC");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        
        targetSVC = (ShaderVariantCollection)EditorGUILayout.ObjectField(
            "目标 SVC 文件", 
            targetSVC, 
            typeof(ShaderVariantCollection), 
            false
        );

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("粘贴变体信息（每行一个）：");
        
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
        variantText = EditorGUILayout.TextArea(variantText, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(10);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("解析并添加", GUILayout.Height(30)))
        {
            ParseAndAdd();
        }
        
        if (GUILayout.Button("从服务器获取", GUILayout.Height(30)))
        {
            FetchFromServer();
        }
        
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "支持格式：\n" +
            "Shader Unlit/Shader2, subshader 0, pass 0, stage all: variant _GREEN_ON not found.",
            MessageType.Info
        );
    }

    private void ParseAndAdd()
    {
        if (targetSVC == null)
        {
            EditorUtility.DisplayDialog("错误", "请先选择目标 SVC 文件", "确定");
            return;
        }

        if (string.IsNullOrWhiteSpace(variantText))
        {
            EditorUtility.DisplayDialog("错误", "请输入变体信息", "确定");
            return;
        }

        int addedCount = 0;
        int skipCount = 0;
        int failedCount = 0;
        var lines = variantText.Split('\n');

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;
            if (!trimmed.StartsWith("Shader ") || !trimmed.EndsWith("not found.")) continue;

            var result = ParseLine(trimmed);
            if (result.HasValue)
            {
                var (shaderName, keywords) = result.Value;
                
                var shader = Shader.Find(shaderName);
                if (shader == null)
                {
                    Debug.LogWarning($"[ShaderVariantImporter] 找不到 Shader: {shaderName}");
                    failedCount++;
                    continue;
                }

                var variant = new ShaderVariantCollection.ShaderVariant
                {
                    shader = shader,
                    passType = PassType.Normal,
                    keywords = keywords
                };

                if (targetSVC.Contains(variant))
                {
                    skipCount++;
                }
                else
                {
                    targetSVC.Add(variant);
                    addedCount++;
                    string kw = keywords.Length > 0 ? string.Join(" ", keywords) : "<no keywords>";
                    Debug.Log($"[ShaderVariantImporter] 添加: {shaderName} - {kw}");
                }
            }
            else
            {
                Debug.LogWarning($"[ShaderVariantImporter] 无法解析: {trimmed}");
                failedCount++;
            }
        }

        EditorUtility.SetDirty(targetSVC);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("完成", $"添加: {addedCount}，跳过: {skipCount}，失败: {failedCount}", "确定");
    }

    /// <summary>
    /// 解析格式: Shader xxx, subshader x, pass x, stage all: variant xxx not found.
    /// </summary>
    private (string shaderName, string[] keywords)? ParseLine(string line)
    {
        try
        {
            // 提取 shader 名称: "Shader " 到第一个 ","
            int shaderStart = 7; // "Shader " 长度
            int shaderEnd = line.IndexOf(",");
            if (shaderEnd <= shaderStart) return null;

            string shaderName = line.Substring(shaderStart, shaderEnd - shaderStart).Trim();

            // 处理 "(real shader xxx)" 格式
            if (shaderName.Contains("(real shader"))
            {
                int realStart = shaderName.IndexOf("(real shader") + 12;
                int realEnd = shaderName.IndexOf(")", realStart);
                if (realEnd > realStart)
                {
                    shaderName = shaderName.Substring(realStart, realEnd - realStart).Trim();
                }
            }

            // 提取 keywords: "variant " 到 " not found."
            int variantStart = line.IndexOf("variant ") + 8;
            int variantEnd = line.IndexOf(" not found.");
            if (variantEnd <= variantStart) return null;

            string keywordStr = line.Substring(variantStart, variantEnd - variantStart).Trim();

            string[] keywords;
            if (keywordStr == "<no keywords>")
            {
                keywords = new string[0];
            }
            else
            {
                keywords = keywordStr.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            }

            return (shaderName, keywords);
        }
        catch
        {
            return null;
        }
    }

    private async void FetchFromServer()
    {
        if (targetSVC == null)
        {
            EditorUtility.DisplayDialog("错误", "请先选择目标 SVC 文件", "确定");
            return;
        }

        var (success, data, error) = await ShaderVariantCollectionEditor.GetAllDataAsync();
        
        if (success && data != null)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var item in data)
            {
                sb.AppendLine(item.message);
            }
            variantText = sb.ToString();
            Debug.Log($"[ShaderVariantImporter] 从服务器获取了 {data.Length} 条数据");
        }
        else
        {
            EditorUtility.DisplayDialog("错误", $"获取数据失败: {error}", "确定");
        }
    }
}
