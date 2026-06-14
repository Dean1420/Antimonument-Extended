using UnityEngine;
using UnityGLTF;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;
using FileOperations;

public class GLBExportFromArea : MonoBehaviour
{
    [SerializeField] private Transform boundingBox;

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

        GameObject tempRoot = CreateTemporaryParent();
        ReparentObjects(objectsToExport, tempRoot.transform);
        
        Debug.Log($"GLB >>> Exporting {objectsToExport.Count} objects");
        
        ExportToGLB(tempRoot);
        RestoreOriginalParents(objectsToExport);
        CleanupTemporaryParent(tempRoot);
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
        Debug.Log($"GLB >>> Found collider on: {col.gameObject.name}");
        if (col.transform != boundingBox)
        {
            objects.Add(new ObjectParentPair(col.transform, col.transform.parent));
        }
    }
    return objects;
}

    private GameObject CreateTemporaryParent()
    {
        return new GameObject("TempExportRoot");
    }

    private void ReparentObjects(List<ObjectParentPair> objects, Transform newParent)
    {
        foreach (var pair in objects)
        {
            pair.obj.SetParent(newParent, true);
        }
    }

    private void ExportToGLB(GameObject rootObject)
    {
        string relativePath = "GLBExport/";
        string fullPath = Path.Combine(PersistentDataPaths.Runtime, relativePath);
        
        Directory.CreateDirectory(fullPath);

        GLTFSceneExporter exporter = new GLTFSceneExporter(
            new Transform[] { rootObject.transform },
            new ExportContext()
        );
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        exporter.SaveGLB(fullPath, "export_area_" + timestamp);
        
        Debug.Log("GLB >>> Exported to: " + fullPath + "/export_area.glb");
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
}