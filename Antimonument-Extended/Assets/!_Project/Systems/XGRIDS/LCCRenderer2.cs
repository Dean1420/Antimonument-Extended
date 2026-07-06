using System.IO;
using UnityEngine;
using LCCCore;
using UnityEngine.Networking;
using System.Collections;

// lcc/lcc-result/Virtuelles Studio.lcc
// lcc2/lcc2-result/Landschaftspark2.lcc2
// ply/point_cloud/iteration_100/point_cloud_3.ply
public class LCCRenderer2 : MonoBehaviour
{
    public LCCManager m_manager;
    public string filename;
    [SerializeField] private float scale = 1f;
    [SerializeField] private bool useStreamingAssetsOnAndroid = false;
    [SerializeField] private bool verboseLogging = false; // controls debug log verbosity


    private string m_FilePath;
    private LCCCore.Renderer m_renderer;

    void Start()
    {
        transform.localScale = Vector3.one * scale;

        // Wenn wir auf Android laufen, automatisch StreamingAssets-Load verwenden.
        if (Application.platform == RuntimePlatform.Android || useStreamingAssetsOnAndroid)
        {
            if (verboseLogging) Debug.Log("LCC_RENDERER >>> Starting LoadFromStreamingAssetsOnAndroid");
            StartCoroutine(LoadFromStreamingAssetsOnAndroid());
            return;
        }

        // Normaler Pfad für PC/Linux
        m_FilePath = BuildFilePath();

        if (string.IsNullOrEmpty(m_FilePath) || !File.Exists(m_FilePath))
        {
            Debug.LogError("LCC_RENDERER >>> File not found: " + m_FilePath);
            if (verboseLogging) LogDebugInfo();
            return;
        }

        LoadFile();
    }

    private string BuildFilePath()
    {
        // Try streamingAssets path first (editor / build unpacked)
        string baseStreaming = Path.Combine(Application.streamingAssetsPath, "GaussianSplats");
        string combinedStreaming = Path.Combine(baseStreaming, filename);

        // Also try persistentDataPath (we may have copied files there on Android)
        string basePersistent = Path.Combine(Application.persistentDataPath, "GaussianSplats");
        string combinedPersistent = Path.Combine(basePersistent, filename);

        if (verboseLogging) Debug.Log("LCC_RENDERER >>> streaming base: " + baseStreaming);
        if (verboseLogging) Debug.Log("LCC_RENDERER >>> persistent base: " + basePersistent);
        if (verboseLogging) Debug.Log("LCC_RENDERER >>> filename: " + filename);

        // Normalize and check common variants
        string windowsStreaming = combinedStreaming.Replace('/', '\\');
        string unixStreaming = combinedStreaming.Replace('\\', '/');

        string windowsPersistent = combinedPersistent.Replace('/', '\\');
        string unixPersistent = combinedPersistent.Replace('\\', '/');

        if (verboseLogging) Debug.Log("LCC_RENDERER >>> Checking paths...");
        if (verboseLogging) Debug.Log("LCC_RENDERER >>> windowsStreaming: " + windowsStreaming + " exists:" + File.Exists(windowsStreaming));
        if (verboseLogging) Debug.Log("LCC_RENDERER >>> unixStreaming: " + unixStreaming + " exists:" + File.Exists(unixStreaming));
        if (verboseLogging) Debug.Log("LCC_RENDERER >>> windowsPersistent: " + windowsPersistent + " exists:" + File.Exists(windowsPersistent));
        if (verboseLogging) Debug.Log("LCC_RENDERER >>> unixPersistent: " + unixPersistent + " exists:" + File.Exists(unixPersistent));

        if (File.Exists(windowsStreaming)) return windowsStreaming;
        if (File.Exists(unixStreaming)) return unixStreaming;
        if (File.Exists(windowsPersistent)) return windowsPersistent;
        if (File.Exists(unixPersistent)) return unixPersistent;

        Debug.LogError("LCC_RENDERER >>> Could not find file on any checked path.");
        return null;
    }

    private IEnumerator LoadFromStreamingAssetsOnAndroid()
    {
        // Verwende saubere URL mit Vorwärtsslashes für UnityWebRequest auf Android
        string src = $"{Application.streamingAssetsPath}/GaussianSplats/{filename}";

        using UnityWebRequest req = UnityWebRequest.Get(src);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("LCC_RENDERER >>> StreamingAssets fetch failed: " + req.error + " | src: " + src);
            yield break;
        }

        m_FilePath = Path.Combine(Application.persistentDataPath, "GaussianSplats", filename);
        var dir = Path.GetDirectoryName(m_FilePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllBytes(m_FilePath, req.downloadHandler.data);

        if (verboseLogging) Debug.Log("LCC_RENDERER >>> Copied to persistentDataPath: " + m_FilePath + " | Exists: " + File.Exists(m_FilePath));
        if (verboseLogging) LogDebugInfo();

        LoadFile();
    }

    private void LogDebugInfo()
    {
        if (!verboseLogging) return;

        Debug.Log("LCC_RENDERER >>> Application.dataPath: " + Application.dataPath);
        Debug.Log("LCC_RENDERER >>> Filename: " + filename);
        Debug.Log("LCC_RENDERER >>> Full path: " + m_FilePath);
        Debug.Log("LCC_RENDERER >>> File exists: " + (m_FilePath != null && File.Exists(m_FilePath)));
        Debug.Log("LCC_RENDERER >>> File extension: " + (m_FilePath != null ? Path.GetExtension(m_FilePath) : "null"));
        Debug.Log("LCC_RENDERER >>> Directory exists: " + (m_FilePath != null && Directory.Exists(Path.GetDirectoryName(m_FilePath))));
    }

    private void LoadFile()
    {
        if (verboseLogging) Debug.Log("LCC_RENDERER >>> m_manager is null? " + (m_manager == null));

        if (m_manager == null)
        {
            Debug.LogError("LCC_RENDERER >>> m_manager ist null. Abbruch.");
            return;
        }

        m_renderer = m_manager.GetRender(this.transform);

        if (verboseLogging) Debug.Log("LCC_RENDERER >>> m_renderer is null? " + (m_renderer == null));
        if (m_renderer == null) return;

        // Plattform automatisch bestimmen
        PlatformType platform = (Application.platform == RuntimePlatform.Android) ? PlatformType.Quest : PlatformType.PC;

        if (verboseLogging) Debug.Log("LCC_RENDERER >>> About to load: " + m_FilePath + " | Platform: " + platform);
        m_renderer.Load(m_FilePath, platform, onLoadCallback);
    }

    private void onLoadCallback()
    {
        Debug.Log("LCC_RENDERER >>> Loaded successfully!");

        if (!verboseLogging) return; // avoid heavy logging on mobile by default

        // Check renderer state
        if (m_renderer != null)
        {
            Debug.Log("LCC_RENDERER >>> Renderer exists: true");
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