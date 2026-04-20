using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class StreamingAssetsExtractor : MonoBehaviour
{
    private const string ManifestFile = "manifest.txt";

    void Start()
    {
        StartCoroutine(ExtractFromManifest(OnDataReady));
    }

    // load all files from streaming assets listed in manifest file
    private IEnumerator ExtractFromManifest(System.Action onComplete)
    {
        // fetch and read the manifest
        string manifestSrc = Path.Combine(Application.streamingAssetsPath, ManifestFile);
        using UnityWebRequest manifestReq = UnityWebRequest.Get(manifestSrc);
        yield return manifestReq.SendWebRequest();

        if (manifestReq.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"STREAMING_ASSETS_EXTRACTOR >>> failed to read manifest: {manifestReq.error}");
            yield break;
        }

        // extract each file listed in the manifest
        string[] files = manifestReq.downloadHandler.text.Split('\n');
        foreach (string file in files)
        {
            string relativePath = file.Trim();
            if (string.IsNullOrEmpty(relativePath)) continue;

            yield return ExtractFile(relativePath);
        }

        onComplete?.Invoke();
    }

    public IEnumerator ExtractFile(string relativePath)
    {
        string src = Path.Combine(Application.streamingAssetsPath, relativePath);
        string dest = Path.Combine(Application.persistentDataPath, relativePath);

        if (File.Exists(dest))
        {
            Debug.Log($"STREAMING_ASSETS_EXTRACTOR >>> already exists, skipping: {dest}");
            yield break;
        }

        using UnityWebRequest req = UnityWebRequest.Get(src);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"STREAMING_ASSETS_EXTRACTOR >>> failed to extract {relativePath}: {req.error}");
            yield break;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(dest));
        File.WriteAllBytes(dest, req.downloadHandler.data);
        Debug.Log($"Extracted: {dest}");
    }

    private void OnDataReady()
    {
        Debug.Log("STREAMING_ASSETS_EXTRACTOR >>> all files ready");
    }
}