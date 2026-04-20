/* using UnityEngine;
using LCCCore;

public class LCCRendererMetaQuest : MonoBehaviour
{
    public LCCManager m_manager;
    private LCCCore.Renderer m_renderer;

    void Start()
    {
        // persistentDataPath フォルダ内のLCCフォルダ内に存在する .lcc ファイルを検索する
        // persistentDataPath フォルダは環境ごとに以下のパスを指す
        // Windows : C:\Users\<username>\AppData\LocalLow\<companyname>\<productname>
        // Android : /Android/data/<packagename>/files
        string searchPath = System.IO.Path.Combine(Application.persistentDataPath, "LCC");
        string[] files = Directory.GetFiles(searchPath, "*.lcc2", SearchOption.TopDirectoryOnly);

        // .lcc ファイルがない場合は処理しない
        if (files.Length == 0)
        {
            Debug.LogError($"[LCCRenderer] No .lcc files found in: {searchPath}");
            return;
        }

        // 複数ある場合は最初のファイルを利用する
        string path = files[0];
        Debug.Log($"[LCCRenderer] Loading file: {Path.GetFileName(path)}");

        m_renderer = m_manager.GetRender(this.transform);
        m_renderer.Load(path, PlatformType.Quest, onLoadCallback);
    }

    private void onLoadCallback()
    {
        Debug.Log("data loaded");
    }
} */