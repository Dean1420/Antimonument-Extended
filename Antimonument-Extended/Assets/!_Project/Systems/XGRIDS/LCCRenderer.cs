using System.IO;
using UnityEngine;
using LCCCore;
using FileOperations;
using UnityEngine.Networking;
using System.Collections;

// lcc/lcc-result/Virtuelles Studio.lcc
// lcc2/lcc2-result/Landschaftspark2.lcc2
// ply/point_cloud/iteration_100/point_cloud_3.ply
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
    
    // Check Android FIRST, before file existence
    if (useStreamingAssetsOnAndroid)
    {
        StartCoroutine(LoadFromStreamingAssetsOnAndroid());
        return;
    }
    
    // Now do the normal file path logic for PC/Linux
    m_FilePath = BuildFilePath();
    
    if (!File.Exists(m_FilePath))
    {
        Debug.LogError("LCC_RENDERER >>> File not found: " + m_FilePath);
        return;
    }
    
    LoadFile();
}

    private string BuildFilePath()
{
    string combinedPath = Path.Combine(StreamingAssetsPaths.GaussianSplats, filename);
    
    Debug.Log("LCC_RENDERER >>> StreamingAssets base: " + StreamingAssetsPaths.GaussianSplats);
    Debug.Log("LCC_RENDERER >>> Filename: " + filename);
    Debug.Log("LCC_RENDERER >>> Combined path: " + combinedPath);
    
    string windowsPath = combinedPath.Replace('/', '\\');
    string unixPath = combinedPath.Replace('\\', '/');
    
    Debug.Log("LCC_RENDERER >>> Windows path: " + windowsPath);
    Debug.Log("LCC_RENDERER >>> Unix path: " + unixPath);
    Debug.Log("LCC_RENDERER >>> Windows exists: " + File.Exists(windowsPath));
    Debug.Log("LCC_RENDERER >>> Unix exists: " + File.Exists(unixPath));
    
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
        Debug.Log("LCC_RENDERER >>> m_manager is null? " + (m_manager == null));
        
        m_renderer = m_manager.GetRender(this.transform);
        
        Debug.Log("LCC_RENDERER >>> m_renderer is null? " + (m_renderer == null));

        PlatformType platform = PlatformType.PC;
        if (useStreamingAssetsOnAndroid)
        {
            platform = PlatformType.Quest;
        }
        


        Debug.Log("LCC_RENDERER >>> About to load: " + m_FilePath);
        m_renderer.Load(m_FilePath, platform, onLoadCallback);
    
}

    private void onLoadCallback()
    {
    Debug.Log("LCC_RENDERER >>> Loaded successfully!");
    
    // Check renderer state
    if (m_renderer != null)
    {
        Debug.Log("LCC_RENDERER >>> Renderer exists: true");
        // Check if it exposes any properties (adjust based on LCCCore API)
        Debug.Log("LCC_RENDERER >>> Renderer type: " + m_renderer.GetType().Name);
    }
    
    // Check GameObject hierarchy
    Debug.Log("LCC_RENDERER >>> Transform position: " + transform.position);
    Debug.Log("LCC_RENDERER >>> Transform scale: " + transform.localScale);
    Debug.Log("LCC_RENDERER >>> Active: " + gameObject.activeInHierarchy);
    Debug.Log("LCC_RENDERER >>> Layer: " + LayerMask.LayerToName(gameObject.layer));
    
    // Check for child objects (splats often create meshes as children)
    Debug.Log("LCC_RENDERER >>> Child count: " + transform.childCount);
    for (int i = 0; i < transform.childCount; i++)
    {
        var child = transform.GetChild(i);
        Debug.Log($"LCC_RENDERER >>> Child {i}: {child.name}, active: {child.gameObject.activeSelf}");
        
        var meshRenderer = child.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            Debug.Log($"LCC_RENDERER >>> Child {i} has MeshRenderer, enabled: {meshRenderer.enabled}");
            Debug.Log($"LCC_RENDERER >>> Material: {meshRenderer.material?.shader?.name}");
        }
    }
    
    // Graphics info
    Debug.Log("LCC_RENDERER >>> Graphics API: " + SystemInfo.graphicsDeviceType);
    Debug.Log("LCC_RENDERER >>> Compute shaders: " + SystemInfo.supportsComputeShaders);

    }
}

