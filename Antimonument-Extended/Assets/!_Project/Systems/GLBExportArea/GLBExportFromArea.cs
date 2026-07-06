using UnityEngine;
using UnityGLTF;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using FileOperations;
using Ftp;

public class GLBExportFromArea : MonoBehaviour
{
    [SerializeField] private Transform boundingBox;
    [SerializeField] private Transform excludeRoot;
    

    private class ObjectParentPair
    {
        public Transform obj;
        public Transform originalParent;
        public ObjectParentPair(Transform obj, Transform parent)
        {
            this.obj = obj;
            this.originalParent = parent;
        }
    }

    private void Awake()
    {
        if (boundingBox == null)
        {
            boundingBox = this.transform;
            Debug.Log("GLB >>> Bounding box set to self in Awake");
        }
    }

private string GetStatueName(List<ObjectParentPair> objects)
{
    foreach (var pair in objects)
    {
        if (pair.obj.CompareTag("Statue"))
        {
            return pair.obj.name;
        }
    }

    Debug.LogWarning("GLB >>> No object with tag 'Statue' found, falling back to default name");
    return "export_area";
}

public void Export()
{
    Debug.Log("GLB >>> Export() called!");
    if (!ValidateBoundingBox()) return;

    List<ObjectParentPair> objectsToExport = FindObjectsInBoundingBox();

    if (objectsToExport.Count == 0)
    {
        Debug.LogWarning("GLB >>> No objects found in bounding box!");
        return;
    }

    string statueName = GetStatueName(objectsToExport);

    GameObject tempRoot = CreateTemporaryParent(statueName);
    ReparentObjects(objectsToExport, tempRoot.transform);

    Debug.Log($"GLB >>> Exporting {objectsToExport.Count} objects");

    string glbFullPath = ExportToGLB(tempRoot);

    RestoreOriginalParents(objectsToExport);
    CleanupTemporaryParent(tempRoot);

    if (!string.IsNullOrEmpty(glbFullPath))
    {
        StartCoroutine(WaitAndUploadGLB(glbFullPath));
    }
}



    private bool ValidateBoundingBox()
    {
        Debug.Log($"GLB >>> BoundingBox is: {(boundingBox == null ? "NULL" : boundingBox.name)}");
        if (boundingBox == null)
        {
            boundingBox = this.transform;
            Debug.Log("GLB >>> Forced bounding box to self!");
        }
        return true;
    }

    private List<ObjectParentPair> FindObjectsInBoundingBox()
    {
        Vector3 halfExtents = boundingBox.lossyScale * 0.5f;
        Debug.Log($"GLB >>> Searching at position: {boundingBox.position}, halfExtents: {halfExtents}");

        Collider[] hitColliders = Physics.OverlapBox(
            boundingBox.position,
            halfExtents,
            boundingBox.rotation
        );

        Debug.Log($"GLB >>> OverlapBox found: {hitColliders.Length} colliders");

        List<ObjectParentPair> objects = new List<ObjectParentPair>();

        foreach (Collider col in hitColliders)
        {
            if (col.transform != boundingBox &&
                !col.transform.IsChildOf(excludeRoot))
            {
                objects.Add(new ObjectParentPair(col.transform, col.transform.parent));
            }
        }
        return objects;
    }

   private GameObject CreateTemporaryParent(string name)
{
    return new GameObject(name);
}

    private void ReparentObjects(List<ObjectParentPair> objects, Transform newParent)
    {
        foreach (var pair in objects)
        {
            pair.obj.SetParent(newParent, true);
        }
    }

    // Gibt den vollen Pfad zur erzeugten .glb zurück (oder null bei Fehler)
   private string ExportToGLB(GameObject rootObject)
{
    string fullPath = Path.Combine(PersistentDataPaths.Runtime, "Polaroid");
    Directory.CreateDirectory(fullPath);
    GLTFSceneExporter exporter = new GLTFSceneExporter(
        new Transform[] { rootObject.transform },
        new ExportContext()
    );
    string timestamp = System.DateTime.Now.ToString("yyyy.MM.dd_HH.mm");
    string filename = "file_" + rootObject.name + "_" + timestamp;
    exporter.SaveGLB(fullPath, filename);

    string glbFullPath = Path.Combine(fullPath, filename + ".glb");
    Debug.Log("GLB >>> Exported to: " + glbFullPath);

    return glbFullPath;
}

    private void RestoreOriginalParents(List<ObjectParentPair> objects)
    {
        foreach (var pair in objects)
        {
            pair.obj.SetParent(pair.originalParent, true);
        }
    }

    private void CleanupTemporaryParent(GameObject tempParent)
    {
        Destroy(tempParent);
    }

    // --- Upload-Teil, analog zu PhotoHandler ---

    private IEnumerator WaitAndUploadGLB(string glbFullPath)
    {
        Debug.Log("GLB >>> waiting for extractor...");
        yield return new WaitUntil(() => StreamingAssetsExtractor.IsReady);
        Debug.Log("GLB >>> extractor ready, loading credentials...");

        byte[] glbData;
        try
        {
            glbData = File.ReadAllBytes(glbFullPath);
        }
        catch (Exception e)
        {
            Debug.LogError($"GLB >>> could not read exported file: {e.Message}");
            yield break;
        }

        Dictionary<string, string> credentials = null;
        yield return LoadCredentialsCoroutine(result => credentials = result);

        if (credentials == null ||
            !credentials.ContainsKey("username") ||
            !credentials.ContainsKey("password") ||
            !credentials.ContainsKey("url") ||
            !credentials.ContainsKey("remoteDirectory"))
        {
            Debug.LogError("GLB >>> credentials missing or incomplete, aborting upload");
            yield break;
        }

        string filename = Path.GetFileName(glbFullPath);
        Debug.Log($"GLB >>> uploading {filename}...");

        Task uploadTask = FtpHandler.UploadFile(
            credentials["username"],
            credentials["password"],
            credentials["url"],
            credentials["remoteDirectory"],
            filename,
            glbData);

        yield return new WaitUntil(() => uploadTask.IsCompleted);

        if (uploadTask.IsFaulted)
        {
            Debug.LogError($"GLB >>> upload task faulted: {uploadTask.Exception}");
        }
        else
        {
            Debug.Log("GLB >>> upload finished");
        }
    }

    private IEnumerator LoadCredentialsCoroutine(Action<Dictionary<string, string>> onLoaded)
    {
        string pathToCredentials = Path.Combine(StreamingAssetsPaths.Credentials, "Secrets", "FTP.txt");
        string separator = ":";

        using (UnityWebRequest request = UnityWebRequest.Get(pathToCredentials))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"GLB >>> failed to load credentials file: {request.error}");
                onLoaded(null);
                yield break;
            }

            string text = request.downloadHandler.text;
            Dictionary<string, string> credentials = ParseCredentials(text, separator);
            onLoaded(credentials);
        }
    }

    private Dictionary<string, string> ParseCredentials(string text, string separator)
    {
        var dict = new Dictionary<string, string>();
        string[] lines = text.Split('\n');

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            int index = line.IndexOf(separator);
            if (index < 0) continue;

            string key = line.Substring(0, index).Trim();
            string value = line.Substring(index + separator.Length).Trim();
            dict[key] = value;
        }

        return dict;
    }
}