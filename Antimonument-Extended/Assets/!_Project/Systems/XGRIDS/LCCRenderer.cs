using System.IO;
using UnityEngine;
using LCCCore;
using FileOperations;
using UnityEngine.Networking;
using System.Collections;



public class LCCRenderer : MonoBehaviour
{
    public LCCManager m_manager;
    public string filename;
    [SerializeField] private float scale = 1f;
    [SerializeField] private bool useStreamingAssetsOnAndroid = false;


    private string m_FilePath;
    private LCCCore.Renderer m_renderer;

    void Start()
    {
        transform.localScale = Vector3.one * scale;

        m_FilePath = BuildFilePath();

        LogDebugInfo();

        if (!File.Exists(m_FilePath))
        {
            Debug.LogError("LCC_RENDERER >>> File not found: " + m_FilePath);
            return;
        }

        if (useStreamingAssetsOnAndroid)
        {
            StartCoroutine(LoadFromStreamingAssetsOnAndroid());
            return;
        }

        LoadFile();
    }

    private string BuildFilePath()
    {
        string windowsPath = Path.Combine(StreamingAssetsPaths.GaussianSplats, filename).Replace('/', '\\');
        string unixPath = Path.Combine(StreamingAssetsPaths.GaussianSplats, filename).Replace('\\', '/');

        if (File.Exists(windowsPath)) return windowsPath;
        if (File.Exists(unixPath)) return unixPath;

        Debug.LogError("LCC_RENDERER >>> Could not find file on any platform path.");
        return null;
    }

    private IEnumerator LoadFromStreamingAssetsOnAndroid()
    {
        string src = Path.Combine(Application.streamingAssetsPath, "GaussianSplats", filename);

        using UnityWebRequest req = UnityWebRequest.Get(src);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("LCC_RENDERER >>> StreamingAssets fetch failed: " + req.error);
            yield break;
        }

        m_FilePath = Path.Combine(Application.persistentDataPath, "GaussianSplats", filename);
        Directory.CreateDirectory(Path.GetDirectoryName(m_FilePath));
        File.WriteAllBytes(m_FilePath, req.downloadHandler.data);

        LoadFile();
    }

    private void LogDebugInfo()
    {
        Debug.Log("LCC_RENDERER >>> Application.dataPath: " + Application.dataPath);
        Debug.Log("LCC_RENDERER >>> Filename: " + filename);
        Debug.Log("LCC_RENDERER >>> Full path: " + m_FilePath);
        Debug.Log("LCC_RENDERER >>> File exists: " + File.Exists(m_FilePath));
        Debug.Log("LCC_RENDERER >>> File extension: " + Path.GetExtension(m_FilePath));
        Debug.Log("LCC_RENDERER >>> Directory exists: " + Directory.Exists(Path.GetDirectoryName(m_FilePath)));
    }

    private void LoadFile()
    {
        try
        {
            m_renderer = m_manager.GetRender(this.transform);
            m_renderer.Load(m_FilePath, PlatformType.PC, onLoadCallback);
        }
        catch (System.Exception e)
        {
            Debug.LogError("LCC_RENDERER >>> Failed to load: " + e.Message);
            Debug.LogError("LCC_RENDERER >>> Stack trace: " + e.StackTrace);
        }
    }

    private void onLoadCallback()
    {
        Debug.Log("LCC_RENDERER >>> Loaded successfully!");
    }
}