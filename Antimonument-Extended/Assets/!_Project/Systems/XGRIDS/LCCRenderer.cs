using UnityEngine;
using LCCCore;
using System.IO;
using UnityEngine.UIElements;

public class LCCRenderer : MonoBehaviour
{
    public LCCManager m_manager;
    public string filename;
    private string m_FilePath;
    private LCCCore.Renderer m_renderer;
    [SerializeField] private float scale = 1f;

    void Start()
    {
        // reset transform first
        //transform.localPosition = Vector3.zero;
        //transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one * scale; // Scale down to 1% size

        // build path using Path.Combine for proper separator handling
        //string relativePath = Path.Combine("!_Project", "Data", "GaussianSplats", filename);
        //m_FilePath = Path.Combine(Application.dataPath, relativePath);
        
        // normalize path separators to forward slashes (Unity/LCC might expect this)
        //m_FilePath = m_FilePath.Replace('\\', '/');

        m_FilePath = "C:\\Users\\hwb01\\Desktop\\Antimonument-Extended\\Antimonument-Extended\\Assets\\!_Project\\Data\\GaussianSplats\\Landschaftspark_LCC2\\lcc2-result\\Landschaftspark2.lcc2";

        // Comprehensive debugging
        Debug.Log("=== LCC Loading Debug Info ===");
        Debug.Log("Application.dataPath: " + Application.dataPath);
        //Debug.Log("Filename: " + filename);
        Debug.Log("Full path: " + m_FilePath);
        Debug.Log("File exists: " + File.Exists(m_FilePath));
        Debug.Log("File extension: " + Path.GetExtension(m_FilePath));
        Debug.Log("Directory exists: " + Directory.Exists(Path.GetDirectoryName(m_FilePath)));
        
        if (!File.Exists(m_FilePath))
        {
            Debug.LogError("File not found at path: " + m_FilePath);
            return;
        }

        try
        {
            m_renderer = m_manager.GetRender(this.transform);
            m_renderer.Load(m_FilePath, PlatformType.PC, onLoadCallback);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to load LCC file: " + e.Message);
            Debug.LogError("Stack trace: " + e.StackTrace);
            
            // try alternative path format (relative from project root)
            TryAlternativePath();
        }
    }

    private void TryAlternativePath()
    {
        // try path relative to project root instead of Assets
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string altPath = Path.Combine(projectRoot, "Assets", "!_Project", "Data", "GaussianSplats", filename);
        altPath = altPath.Replace('\\', '/');
        
        Debug.Log("Trying alternative path: " + altPath);
        
        if (File.Exists(altPath))
        {
            try
            {
                m_renderer.Load(altPath, PlatformType.PC, onLoadCallback);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Alternative path also failed: " + e.Message);
            }
        }
    }

    private void onLoadCallback()
    {
        Debug.Log("=== LCC Data loaded successfully! ===");
    }
}