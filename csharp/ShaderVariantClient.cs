using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Shader变体收集 - 运行时客户端
/// 仅用于在游戏运行时发送变体数据到服务器
/// </summary>
public class ShaderVariantClient : MonoBehaviour
{
    [Header("服务器配置")]
    [Tooltip("服务器地址，例如: http://192.168.1.100:8880")]
    public string serverUrl = "http://localhost:8880";
    
    [Header("请求配置")]
    [Tooltip("请求超时时间（秒）")]
    public int timeout = 10;
    
    [Tooltip("静默模式：不打印成功日志")]
    public bool silentMode = false;

    #region 单例

    private static ShaderVariantClient _instance;
    public static ShaderVariantClient Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("[ShaderVariantClient]");
                _instance = go.AddComponent<ShaderVariantClient>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    #endregion

    #region 数据结构

    [Serializable]
    private class SubmitRequest
    {
        public string message;
    }

    [Serializable]
    private class SubmitResponse
    {
        public bool ok;
        public bool exists;
        public string error;
    }

    #endregion

    #region 公共方法

    /// <summary>
    /// 发送变体数据到服务器
    /// </summary>
    /// <param name="variantMessage">变体信息</param>
    /// <param name="onComplete">完成回调(成功, 消息)</param>
    public void Submit(string variantMessage, Action<bool, string> onComplete = null)
    {
        StartCoroutine(SubmitCoroutine(variantMessage, onComplete));
    }

    #endregion

    #region 协程

    private IEnumerator SubmitCoroutine(string variantMessage, Action<bool, string> onComplete)
    {
        string url = $"{serverUrl}/submit";
        
        var requestData = new SubmitRequest { message = variantMessage };
        string jsonBody = JsonUtility.ToJson(requestData);
        
        using (var request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = timeout;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var response = JsonUtility.FromJson<SubmitResponse>(request.downloadHandler.text);
                    
                    if (response.ok)
                    {
                        if (!silentMode) Debug.Log($"[ShaderVariant] 提交成功: {variantMessage}");
                        onComplete?.Invoke(true, "提交成功");
                    }
                    else if (response.exists)
                    {
                        if (!silentMode) Debug.Log($"[ShaderVariant] 已存在: {variantMessage}");
                        onComplete?.Invoke(true, "已存在");
                    }
                    else
                    {
                        Debug.LogWarning($"[ShaderVariant] 提交失败: {response.error}");
                        onComplete?.Invoke(false, response.error ?? "未知错误");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ShaderVariant] 解析响应失败: {e.Message}");
                    onComplete?.Invoke(false, e.Message);
                }
            }
            else
            {
                string error = GetErrorMessage(request);
                Debug.LogError($"[ShaderVariant] 请求失败: {error}");
                onComplete?.Invoke(false, error);
            }
        }
    }

    private string GetErrorMessage(UnityWebRequest request)
    {
        switch (request.result)
        {
            case UnityWebRequest.Result.ConnectionError:
                return $"连接失败: 无法连接 {serverUrl}，请检查服务器是否启动";
            case UnityWebRequest.Result.ProtocolError:
                return $"HTTP错误: {request.responseCode}";
            case UnityWebRequest.Result.DataProcessingError:
                return $"数据错误: {request.error}";
            default:
                return request.error ?? "未知错误";
        }
    }

    #endregion
}
