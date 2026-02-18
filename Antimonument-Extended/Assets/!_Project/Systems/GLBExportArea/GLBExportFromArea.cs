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

    public void Export()
    {
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
        if (boundingBox == null)
        {
            Debug.LogError("GLB >>> Bounding box not assigned!");
            return false;
        }
        return true;
    }

    private List<ObjectParentPair> FindObjectsInBoundingBox()
    {
        Vector3 halfExtents = boundingBox.lossyScale * 0.5f;
        Collider[] hitColliders = Physics.OverlapBox(
            boundingBox.position,
            halfExtents,
            boundingBox.rotation
        );

        List<ObjectParentPair> objects = new List<ObjectParentPair>();
        
        foreach (Collider col in hitColliders)
        {
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
        string fullPath = Path.Combine(RuntimePaths.Runtime, relativePath);
        
        Directory.CreateDirectory(fullPath);

        GLTFSceneExporter exporter = new GLTFSceneExporter(
            new Transform[] { rootObject.transform },
            new ExportContext()
        );
        
        exporter.SaveGLB(fullPath, "export_area");
        
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
        DestroyImmediate(tempParent);
    }
}