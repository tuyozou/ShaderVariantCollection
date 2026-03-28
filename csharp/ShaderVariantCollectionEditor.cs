using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Shader变体收集 - Unity编辑器工具（非运行时）
/// 菜单位置: Tools/Shader Variant Collection
/// </summary>
public static class ShaderVariantCollectionEditor
{
    private const string ServerUrlKey = "ShaderVariantCollection_ServerUrl";
    private const string DefaultServerUrl = "http://localhost:8880";
    
    private static readonly HttpClient _httpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    #region 数据结构

    [Serializable]
    public class VariantData
    {
        public string message;
        public string time;
    }

    [Serializable]
    public class VariantDataArray
    {
        public VariantData[] items;
    }

    #endregion

    #region 菜单项

    [MenuItem("Tools/Shader Variant Collection/获取所有数据", false, 1)]
    public static async void GetAllData()
    {
        string url = GetServerUrl();
        Debug.Log($"[ShaderVariant] 正在从 {url}/data 获取数据...");

        try
        {
            var response = await _httpClient.GetAsync($"{url}/data");
            
            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                
                // 解析JSON数组
                string wrappedJson = $"{{\"items\":{json}}}";
                var dataArray = JsonUtility.FromJson<VariantDataArray>(wrappedJson);
                
                if (dataArray.items == null || dataArray.items.Length == 0)
                {
                    Debug.Log("[ShaderVariant] 服务器没有数据");
                }
                else
                {
                    Debug.Log($"[ShaderVariant] ========== 共 {dataArray.items.Length} 条数据 ==========");
                    for (int i = 0; i < dataArray.items.Length; i++)
                    {
                        var item = dataArray.items[i];
                        string time = FormatTime(item.time);
                        Debug.Log($"[{i + 1}] {item.message}  ({time})");
                    }
                    Debug.Log($"[ShaderVariant] ========== 数据获取完成 ==========");
                }
            }
            else
            {
                Debug.LogError($"[ShaderVariant] 请求失败: HTTP {(int)response.StatusCode} {response.ReasonPhrase}");
            }
        }
        catch (HttpRequestException e)
        {
            Debug.LogError($"[ShaderVariant] 连接失败: 无法连接到服务器 {url}\n请检查:\n1. 服务器是否已启动 (npm start)\n2. 网址是否正确\n3. 防火墙是否阻止连接\n\n详细错误: {e.Message}");
        }
        catch (TaskCanceledException)
        {
            Debug.LogError($"[ShaderVariant] 请求超时: 服务器 {url} 无响应");
        }
        catch (Exception e)
        {
            Debug.LogError($"[ShaderVariant] 错误: {e.Message}");
        }
    }

    [MenuItem("Tools/Shader Variant Collection/测试连接", false, 2)]
    public static async void TestConnection()
    {
        string url = GetServerUrl();
        Debug.Log($"[ShaderVariant] 正在测试连接 {url} ...");

        try
        {
            var response = await _httpClient.GetAsync($"{url}/data");
            
            if (response.IsSuccessStatusCode)
            {
                Debug.Log($"[ShaderVariant] 连接成功! 服务器地址: {url}");
            }
            else
            {
                Debug.LogError($"[ShaderVariant] 服务器响应异常: HTTP {(int)response.StatusCode}");
            }
        }
        catch (HttpRequestException e)
        {
            Debug.LogError($"[ShaderVariant] 连接失败!\n服务器地址: {url}\n\n请检查:\n1. 服务器是否已启动 (npm start)\n2. 网址是否正确\n3. 防火墙是否阻止连接\n\n错误: {e.Message}");
        }
        catch (TaskCanceledException)
        {
            Debug.LogError($"[ShaderVariant] 连接超时! 服务器无响应: {url}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[ShaderVariant] 错误: {e.Message}");
        }
    }

    [MenuItem("Tools/Shader Variant Collection/清空所有数据", false, 20)]
    public static async void ClearAllData()
    {
        if (!EditorUtility.DisplayDialog("确认清空", "确定要清空服务器上的所有数据吗？\n\n此操作不可恢复！", "确定清空", "取消"))
        {
            return;
        }

        string url = GetServerUrl();
        Debug.Log($"[ShaderVariant] 正在清空数据...");

        try
        {
            var response = await _httpClient.PostAsync($"{url}/clear", null);
            
            if (response.IsSuccessStatusCode)
            {
                Debug.Log("[ShaderVariant] 数据已清空!");
            }
            else
            {
                Debug.LogError($"[ShaderVariant] 清空失败: HTTP {(int)response.StatusCode}");
            }
        }
        catch (HttpRequestException e)
        {
            Debug.LogError($"[ShaderVariant] 连接失败: {e.Message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[ShaderVariant] 错误: {e.Message}");
        }
    }

    [MenuItem("Tools/Shader Variant Collection/设置服务器地址", false, 40)]
    public static void SetServerUrl()
    {
        string currentUrl = GetServerUrl();
        string newUrl = EditorInputDialog.Show("设置服务器地址", "请输入服务器地址:", currentUrl);
        
        if (!string.IsNullOrEmpty(newUrl) && newUrl != currentUrl)
        {
            EditorPrefs.SetString(ServerUrlKey, newUrl);
            Debug.Log($"[ShaderVariant] 服务器地址已更新: {newUrl}");
        }
    }

    [MenuItem("Tools/Shader Variant Collection/显示当前配置", false, 41)]
    public static void ShowConfig()
    {
        string url = GetServerUrl();
        Debug.Log($"[ShaderVariant] 当前服务器地址: {url}");
    }

    #endregion

    #region 公共API

    /// <summary>
    /// 获取服务器地址
    /// </summary>
    public static string GetServerUrl()
    {
        return EditorPrefs.GetString(ServerUrlKey, DefaultServerUrl);
    }

    /// <summary>
    /// 设置服务器地址
    /// </summary>
    public static void SetServerUrl(string url)
    {
        EditorPrefs.SetString(ServerUrlKey, url);
    }

    /// <summary>
    /// 获取所有数据（异步）
    /// </summary>
    public static async Task<(bool success, VariantData[] data, string error)> GetAllDataAsync()
    {
        string url = GetServerUrl();

        try
        {
            var response = await _httpClient.GetAsync($"{url}/data");

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                string wrappedJson = $"{{\"items\":{json}}}";
                var dataArray = JsonUtility.FromJson<VariantDataArray>(wrappedJson);
                return (true, dataArray.items ?? new VariantData[0], null);
            }
            else
            {
                return (false, null, $"HTTP {(int)response.StatusCode}");
            }
        }
        catch (HttpRequestException e)
        {
            return (false, null, $"连接失败: {e.Message}");
        }
        catch (Exception e)
        {
            return (false, null, $"错误: {e.Message}");
        }
    }

    #endregion

    #region 辅助方法

    private static string FormatTime(string isoTime)
    {
        if (DateTime.TryParse(isoTime, out DateTime dt))
        {
            return dt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
        }
        return isoTime;
    }

    #endregion
}

/// <summary>
/// 简单的输入对话框
/// </summary>
public class EditorInputDialog : EditorWindow
{
    private string _inputText;
    private string _message;
    private bool _shouldClose;

    private static string _result;

    public static string Show(string title, string message, string defaultValue = "")
    {
        _result = null;
        
        var window = CreateInstance<EditorInputDialog>();
        window.titleContent = new GUIContent(title);
        window._message = message;
        window._inputText = defaultValue;
        window.minSize = new Vector2(400, 100);
        window.maxSize = new Vector2(400, 100);
        window.ShowModalUtility();

        return _result;
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField(_message);
        EditorGUILayout.Space(5);
        
        _inputText = EditorGUILayout.TextField(_inputText);
        
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("确定", GUILayout.Width(80)))
        {
            _result = _inputText;
            _shouldClose = true;
        }
        
        if (GUILayout.Button("取消", GUILayout.Width(80)))
        {
            _shouldClose = true;
        }
        
        EditorGUILayout.EndHorizontal();

        if (_shouldClose)
        {
            Close();
        }
    }
}
