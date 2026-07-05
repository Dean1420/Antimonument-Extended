//using FileOperations;
//using Ftp;
//using LCCCore;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Threading.Tasks;
//using UnityEngine;
//using UnityEngine.Networking;
//using UnityEngine.UI;
//using Renderer = UnityEngine.Renderer;

//public class PhotoHandler : MonoBehaviour
//{
//    [Header("Photo Data Model")]
//    [SerializeField] private GameObject polaroid;
//    [SerializeField] private RenderTexture cameraView;
//    [SerializeField] private Camera photoCamera;
//    [SerializeField] private GameObject cameraPrefab;
//    [SerializeField] private Transform polaroidSpawnPosition;
//    [SerializeField] private bool spawnNewPolaroidEachShot = false;
//    private Texture2D currentImage;

//    [SerializeField] private Transform boundingBox;
//    [SerializeField] private Transform excludeRoot;

//    [Header("Camera Effects")]
//    [SerializeField] private GameObject cameraFlash;
//    [SerializeField] private float flashTime;
//    [SerializeField] private AudioSource cameraShutter;

//    [Header("Culling Settings")]
//    [SerializeField] private bool hideObjectsOutsideBoundingBox = true;
//    private enum CullingMode { LiveView, OnShutter }
//    [SerializeField] private CullingMode cullingMode = CullingMode.OnShutter;

//    private List<ObjectVisibilityState> objectStates = new List<ObjectVisibilityState>();
//    private int originalCullingMask;
//    private int liveViewCullingMask;
//    private bool isPhotoMode = false;

//    // Layer names (nur noch für Culling)
//    private const string PAINTABLE_LAYER = "Paintable";
//    private const string DEFAULT_PICTURE_LAYER = "DefaultPicture";
//    private const string SLICEABLE_PICTURE_LAYER = "SliceablePicture";

//    // Tag names
//    private const string PAINTABLE_TAG = "Paintable";

//    private class ObjectVisibilityState
//    {
//        public GameObject obj;
//        public int originalLayer;

//        public ObjectVisibilityState(GameObject obj, int originalLayer)
//        {
//            this.obj = obj;
//            this.originalLayer = originalLayer;
//        }
//    }

//    void Start()
//    {
//        if (photoCamera == null)
//        {
//            photoCamera = GetComponent<Camera>();
//            if (photoCamera == null)
//            {
//                Debug.LogError("POLAROID >>> Camera not found!");
//            }
//        }
//        originalCullingMask = photoCamera.cullingMask;
//        SetupLiveViewCullingMask();

//        // Starte mit Live View Culling Mask
//        photoCamera.cullingMask = liveViewCullingMask;
//    }

//    private void SetupLiveViewCullingMask()
//    {
//        // Live View soll nur Paintable und DefaultPicture sehen
//        int paintableLayer = LayerMask.NameToLayer(PAINTABLE_LAYER);
//        int defaultPictureLayer = LayerMask.NameToLayer(DEFAULT_PICTURE_LAYER);

//        liveViewCullingMask = 0;

//        if (paintableLayer != -1)
//        {
//            liveViewCullingMask |= (1 << paintableLayer);
//            Debug.Log($"POLAROID >>> Live View: Adding {PAINTABLE_LAYER} to culling mask");
//        }
//        else
//        {
//            Debug.LogWarning($"POLAROID >>> Layer {PAINTABLE_LAYER} not found!");
//        }

//        if (defaultPictureLayer != -1)
//        {
//            liveViewCullingMask |= (1 << defaultPictureLayer);
//            Debug.Log($"POLAROID >>> Live View: Adding {DEFAULT_PICTURE_LAYER} to culling mask");
//        }
//        else
//        {
//            Debug.LogWarning($"POLAROID >>> Layer {DEFAULT_PICTURE_LAYER} not found!");
//        }

//        Debug.Log($"POLAROID >>> Live View culling mask set to: {liveViewCullingMask}");
//    }

//    public void CreatePolaroid()
//    {
//        Debug.Log("POLAROID >>> Start creating");
//        StartCoroutine(CreatePolaroidCoroutine());
//    }

//    private IEnumerator CreatePolaroidCoroutine()
//    {
//        if (hideObjectsOutsideBoundingBox && cullingMode == CullingMode.OnShutter)
//        {
//            ChangeLayersForPhoto();
//            SetCullingMaskForPhotoMode();
//            Debug.Log("POLAROID >>> Layers changed for photo, camera set to photo mode");
//        }

//        // Warte einen Frame für RenderTexture
//        yield return new WaitForEndOfFrame();

//        UpdateCurrentImage();
//        RenderCurrentImageOnPolaroid();

//        yield return StartCoroutine(SpawnPolaroid());
//        Debug.Log("POLAROID >>> successfully rendered and spawned");

//        // Upload happens, but we need to wait for it to complete
//        yield return StartCoroutine(UploadPolaroidCoroutine());
//        Debug.Log($"POLAROID >>> successfully uploaded");

//        // Now restore - AFTER everything is done
//        if (hideObjectsOutsideBoundingBox)
//        {
//            RestoreLayersAfterPhoto();
//            SetCullingMaskForLiveView();
//            Debug.Log("POLAROID >>> Layers and culling mask restored to live view");
//        }
//    }

//    public void UpdateLiveViewCulling(bool hide)
//    {
//        if (!hideObjectsOutsideBoundingBox || cullingMode != CullingMode.LiveView)
//            return;

//        if (hide)
//        {
//            ChangeLayersForPhoto();
//            SetCullingMaskForPhotoMode();
//            Debug.Log("POLAROID >>> Live View: Photo mode enabled");
//        }
//        else
//        {
//            RestoreLayersAfterPhoto();
//            SetCullingMaskForLiveView();
//            Debug.Log("POLAROID >>> Live View: Photo mode disabled, back to normal");
//        }
//    }

//    public void ChangeLayersForPhoto()
//    {
//        objectStates.Clear();

//        List<Transform> objectsInBox = FindObjectsInBoundingBox();

//        Debug.Log($"POLAROID >>> Found {objectsInBox.Count} objects in bounding box to change layers");

//        foreach (Transform obj in objectsInBox)
//        {
//            int currentLayer = obj.gameObject.layer;
//            int newLayer = currentLayer;
//            string currentLayerName = LayerMask.LayerToName(currentLayer);

//            // Speichere den ursprünglichen Layer
//            objectStates.Add(new ObjectVisibilityState(obj.gameObject, currentLayer));

//            // Ändere den Layer basierend auf dem aktuellen Layer
//            if (currentLayerName == "Default")
//            {
//                newLayer = LayerMask.NameToLayer(DEFAULT_PICTURE_LAYER);
//                Debug.Log($"POLAROID >>> Changing {obj.name} from Default to {DEFAULT_PICTURE_LAYER}");
//            }
//            else if (currentLayerName == PAINTABLE_LAYER)
//            {
//                newLayer = LayerMask.NameToLayer(SLICEABLE_PICTURE_LAYER);
//                Debug.Log($"POLAROID >>> Changing {obj.name} from {PAINTABLE_LAYER} to {SLICEABLE_PICTURE_LAYER}");
//            }

//            obj.gameObject.layer = newLayer;

//            // Ändere auch alle Children
//            foreach (Transform child in obj.GetComponentsInChildren<Transform>())
//            {
//                int childCurrentLayer = child.gameObject.layer;
//                int childNewLayer = childCurrentLayer;
//                string childLayerName = LayerMask.LayerToName(childCurrentLayer);

//                if (childLayerName == "Default")
//                {
//                    childNewLayer = LayerMask.NameToLayer(DEFAULT_PICTURE_LAYER);
//                }
//                else if (childLayerName == PAINTABLE_LAYER)
//                {
//                    childNewLayer = LayerMask.NameToLayer(SLICEABLE_PICTURE_LAYER);
//                }

//                if (childNewLayer != childCurrentLayer)
//                {
//                    child.gameObject.layer = childNewLayer;
//                    objectStates.Add(new ObjectVisibilityState(child.gameObject, childCurrentLayer));
//                    Debug.Log($"POLAROID >>> Changing child {child.name} from {childLayerName} to {LayerMask.LayerToName(childNewLayer)}");
//                }
//            }
//        }
//    }

//    private void RestoreLayersAfterPhoto()
//    {
//        Debug.Log($"POLAROID >>> Restoring {objectStates.Count} objects to their original layers");

//        foreach (ObjectVisibilityState state in objectStates)
//        {
//            if (state.obj != null)
//            {
//                string originalLayerName = LayerMask.LayerToName(state.originalLayer);
//                string currentLayerName = LayerMask.LayerToName(state.obj.layer);
//                state.obj.layer = state.originalLayer;
//                Debug.Log($"POLAROID >>> Restored {state.obj.name} from {currentLayerName} to {originalLayerName}");
//            }
//        }

//        objectStates.Clear();
//        Debug.Log("POLAROID >>> All objects restored to original layers");
//    }

//    private void SetCullingMaskForPhotoMode()
//    {
//        // Setze die Culling Mask nur auf die Photo-Layer
//        int defaultPictureLayer = LayerMask.NameToLayer(DEFAULT_PICTURE_LAYER);
//        int sliceablePictureLayer = LayerMask.NameToLayer(SLICEABLE_PICTURE_LAYER);

//        int newMask = 0;

//        if (defaultPictureLayer != -1)
//        {
//            newMask |= (1 << defaultPictureLayer);
//            Debug.Log($"POLAROID >>> Photo Mode: Adding {DEFAULT_PICTURE_LAYER} to culling mask");
//        }

//        if (sliceablePictureLayer != -1)
//        {
//            newMask |= (1 << sliceablePictureLayer);
//            Debug.Log($"POLAROID >>> Photo Mode: Adding {SLICEABLE_PICTURE_LAYER} to culling mask");
//        }

//        Debug.Log($"POLAROID >>> Photo Mode: Setting culling mask to: {newMask}");
//        photoCamera.cullingMask = newMask;
//        isPhotoMode = true;
//    }

//    private void SetCullingMaskForLiveView()
//    {
//        Debug.Log($"POLAROID >>> Live View: Restoring culling mask to: {liveViewCullingMask}");
//        photoCamera.cullingMask = liveViewCullingMask;
//        isPhotoMode = false;
//    }

//    private void RenderCurrentImageOnPolaroid()
//    {
//        if (spawnNewPolaroidEachShot)
//        {
//            polaroid = Instantiate(polaroid);
//        }

//        Transform quadTransform = polaroid.transform.Find("Quad");
//        MeshRenderer renderer = quadTransform.GetComponent<MeshRenderer>();
//        Material mat = new Material(Shader.Find("Unlit/Texture"));
//        mat.mainTexture = currentImage;
//        renderer.material = mat;

//    }



//    // get current image as texture from render texture
//    private void UpdateCurrentImage()
//    {
//        Debug.Log("POLAROID >>> Capturing current image");  
//        RenderTexture.active = cameraView;
//        currentImage = new Texture2D(
//            cameraView.width,
//            cameraView.height,
//            TextureFormat.ARGB32,
//            false
//        );

//        currentImage.ReadPixels(
//            new Rect(0, 0, cameraView.width, cameraView.height),
//            0,
//            0
//        );

//        currentImage.Apply();
//        RenderTexture.active = null;
//    }



//    private IEnumerator SpawnPolaroid()
//    {
//        yield return SpawnEffects();
//        MovePolaroidToCamera();
//        yield return PolaroidSpawnAnimation();

//    }



//    private IEnumerator PolaroidSpawnAnimation()
//    {
//        polaroid.transform.Rotate(90f, 90f, 90f);
//        Vector3 stepDistance = -1 * polaroidSpawnPosition.right * 0.015f;
//        float stepDelay = 0.1f;
//        for (int i = 0; i < 10; i++)
//        {
//            polaroid.transform.localPosition += stepDistance;
//            yield return new WaitForSeconds(stepDelay);
//        }
//    }



//    private void MovePolaroidToCamera()
//    {
//        polaroid.transform.position = polaroidSpawnPosition.position;
//        polaroid.transform.rotation = polaroidSpawnPosition.rotation;
//    }



//    private IEnumerator SpawnEffects()
//    {
//        cameraShutter.Play();

//        cameraFlash.SetActive(true);
//        yield return new WaitForSeconds(flashTime);
//        cameraFlash.SetActive(false);
//    }

//    IEnumerator UploadPolaroidCoroutine()
//    {
//        byte[] currentImageJpg = currentImage.EncodeToJPG();
//        string timestamp = DateTime.Now.ToString("yyyy.MM.dd_HH.mm");
//        string filename = "file_" + timestamp + ".jpg";
//        yield return StartCoroutine(WaitAndUpload(currentImageJpg, filename));
//    }

//    private IEnumerator WaitAndUpload(byte[] imageData, string filename)
//    {
//        Debug.Log("POLAROID >>> waiting for extractor...");
//        yield return new WaitUntil(() => StreamingAssetsExtractor.IsReady);
//        Debug.Log("POLAROID >>> extractor ready, loading credentials...");

//        Dictionary<string, string> credentials = null;
//        yield return LoadCredentialsCoroutine(result => credentials = result);

//        if (credentials == null ||
//            !credentials.ContainsKey("username") ||
//            !credentials.ContainsKey("password") ||
//            !credentials.ContainsKey("url") ||
//            !credentials.ContainsKey("remoteDirectory"))
//        {
//            Debug.LogError("POLAROID >>> credentials missing or incomplete, aborting upload");
//            yield break;
//        }

//        Debug.Log("POLAROID >>> credentials loaded, starting upload...");

//        Task uploadTask = FtpHandler.UploadFile(
//            credentials["username"],
//            credentials["password"],
//            credentials["url"],
//            credentials["remoteDirectory"],
//            filename,
//            imageData);

//        yield return new WaitUntil(() => uploadTask.IsCompleted);

//        if (uploadTask.IsFaulted)
//        {
//            Debug.LogError($"POLAROID >>> upload task faulted: {uploadTask.Exception}");
//        }
//        else
//        {
//            Debug.Log("POLAROID >>> upload finished");
//        }
//    }

//    private IEnumerator LoadCredentialsCoroutine(Action<Dictionary<string, string>> onLoaded)
//    {
//        string pathToCredentials = Path.Combine(StreamingAssetsPaths.Credentials, "Secrets", "FTP.txt");
//        string separator = ":";

//        using (UnityWebRequest request = UnityWebRequest.Get(pathToCredentials))
//        {
//            yield return request.SendWebRequest();

//            if (request.result != UnityWebRequest.Result.Success)
//            {
//                Debug.LogError($"POLAROID >>> failed to load credentials file: {request.error}");
//                onLoaded(null);
//                yield break;
//            }

//            string text = request.downloadHandler.text;
//            Dictionary<string, string> credentials = ParseCredentials(text, separator);
//            onLoaded(credentials);
//        }
//    }

//    private Dictionary<string, string> ParseCredentials(string text, string separator)
//    {
//        var dict = new Dictionary<string, string>();
//        string[] lines = text.Split('\n');

//        foreach (string rawLine in lines)
//        {
//            string line = rawLine.Trim();
//            if (string.IsNullOrEmpty(line)) continue;

//            int index = line.IndexOf(separator);
//            if (index < 0) continue;

//            string key = line.Substring(0, index).Trim();
//            string value = line.Substring(index + separator.Length).Trim();
//            dict[key] = value;
//        }

//        return dict;
//    }


//    private class ObjectParentPair
//    {
//        public Transform obj;
//        public Transform originalParent;
//        public ObjectParentPair(Transform obj, Transform parent)
//        {
//            this.obj = obj;
//            this.originalParent = parent;
//        }
//    }

//    private List<Transform> FindObjectsInBoundingBox()
//    {
//        if (boundingBox == null)
//        {
//            Debug.LogError("POLAROID >>> Bounding Box not assigned!");
//            return new List<Transform>();
//        }

//        Vector3 halfExtents = boundingBox.lossyScale * 0.5f;
//        Debug.Log($"POLAROID >>> Searching at position: {boundingBox.position}, halfExtents: {halfExtents}");

//        Collider[] hitColliders = Physics.OverlapBox(
//            boundingBox.position,
//            halfExtents,
//            boundingBox.rotation
//        );

//        Debug.Log($"POLAROID >>> OverlapBox found: {hitColliders.Length} colliders");

//        List<Transform> objectsInBox = new List<Transform>();

//        foreach (Collider col in hitColliders)
//        {
//            if (col.transform != boundingBox &&
//                !col.transform.IsChildOf(excludeRoot))
//            {
//                objectsInBox.Add(col.transform);
//            }
//        }

//        return objectsInBox;
//    }

//    private bool IsChildOfAnyObject(Transform target, List<Transform> parents)
//    {
//        foreach (Transform parent in parents)
//        {
//            if (target.IsChildOf(parent))
//                return true;
//        }
//        return false;
//    }

//    public void OnObjectEnterBoundingBox(GameObject obj)
//    {
//        if (obj == null) return;

//        Debug.Log($"POLAROID >>> Object entered bounding box: {obj.name} with tag: {obj.tag}");

//        // Wenn Objekt KEIN Paintable Tag hat, gib ihm das Tag
//        if (obj.CompareTag(PAINTABLE_TAG) == false)
//        {
//            obj.tag = PAINTABLE_TAG;
//            Debug.Log($"POLAROID >>> Added {PAINTABLE_TAG} tag to {obj.name}");

//            // Auch alle Children
//            foreach (Transform child in obj.GetComponentsInChildren<Transform>())
//            {
//                if (child != obj.transform && !child.CompareTag(PAINTABLE_TAG))
//                {
//                    child.tag = PAINTABLE_TAG;
//                    Debug.Log($"POLAROID >>> Added {PAINTABLE_TAG} tag to child {child.name}");
//                }
//            }
//        }
//        else
//        {
//            Debug.Log($"POLAROID >>> {obj.name} already has {PAINTABLE_TAG} tag");
//        }
//    }

//    public void OnObjectExitBoundingBox(GameObject obj)
//    {
//        if (obj == null) return;

//        Debug.Log($"POLAROID >>> Object exited bounding box: {obj.name} with tag: {obj.tag}");

//        // Wenn Objekt Paintable Tag hat, entferne es (setze auf "Untagged")
//        if (obj.CompareTag(PAINTABLE_TAG))
//        {
//            obj.tag = "Untagged";
//            Debug.Log($"POLAROID >>> Removed {PAINTABLE_TAG} tag from {obj.name}");

//            // Auch alle Children
//            foreach (Transform child in obj.GetComponentsInChildren<Transform>())
//            {
//                if (child != obj.transform && child.CompareTag(PAINTABLE_TAG))
//                {
//                    child.tag = "Untagged";
//                    Debug.Log($"POLAROID >>> Removed {PAINTABLE_TAG} tag from child {child.name}");
//                }
//            }
//        }
//        else
//        {
//            Debug.Log($"POLAROID >>> {obj.name} doesn't have {PAINTABLE_TAG} tag");
//        }
//    }
//}


///*
// * using FileOperations;
//using Ftp;
//using LCCCore;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Threading.Tasks;
//using UnityEngine;
//using UnityEngine.Networking;
//using UnityEngine.UI;
//using Renderer = UnityEngine.Renderer;

//public class PhotoHandler : MonoBehaviour
//{
//    [Header("Photo Data Model")]
//    [SerializeField] private GameObject polaroid;
//    [SerializeField] private RenderTexture cameraView;
//    [SerializeField] private Camera photoCamera; 
//    [SerializeField] private GameObject cameraPrefab;
//    [SerializeField] private Transform polaroidSpawnPosition;
//    [SerializeField] private bool spawnNewPolaroidEachShot = false;
//    private Texture2D currentImage;

//    [SerializeField] private Transform boundingBox;
//    [SerializeField] private Transform excludeRoot;

//    [Header("Camera Effects")]
//    [SerializeField] private GameObject cameraFlash;
//    [SerializeField] private float flashTime;
//    [SerializeField] private AudioSource cameraShutter;

//    [Header("Culling Settings")]
//    [SerializeField] private bool hideObjectsOutsideBoundingBox = true;
//    private enum CullingMode { LiveView, OnShutter }
//    [SerializeField] private CullingMode cullingMode = CullingMode.OnShutter;

//    private List<ObjectVisibilityState> objectStates = new List<ObjectVisibilityState>();
//    private int originalCullingMask;
//    private int liveViewCullingMask;
//    private bool isPhotoMode = false;

//    // Layer names
//    private const string DEFAULT_LAYER = "Default";
//    private const string PAINTABLE_LAYER = "Paintable";
//    private const string SLICEABLE_LAYER = "Sliceable";
//    private const string DEFAULT_PICTURE_LAYER = "DefaultPicture";
//    private const string SLICEABLE_PICTURE_LAYER = "SliceablePicture";

//    private class ObjectVisibilityState
//    {
//        public GameObject obj;
//        public int originalLayer;

//        public ObjectVisibilityState(GameObject obj, int originalLayer)
//        {
//            this.obj = obj;
//            this.originalLayer = originalLayer;
//        }
//    }

//    void Start()
//    {
//        if (photoCamera == null)
//        {
//            photoCamera = GetComponent<Camera>();
//            if (photoCamera == null)
//            {
//                Debug.LogError("POLAROID >>> Camera not found!");
//            }
//        }
//        originalCullingMask = photoCamera.cullingMask;
//        SetupLiveViewCullingMask();

//        // Starte mit Live View Culling Mask
//        photoCamera.cullingMask = liveViewCullingMask;
//    }

//    private void SetupLiveViewCullingMask()
//    {
//        // Live View soll nur Sliceable und DefaultPicture sehen
//        int sliceableLayer = LayerMask.NameToLayer(SLICEABLE_LAYER);
//        int defaultPictureLayer = LayerMask.NameToLayer(DEFAULT_PICTURE_LAYER);

//        liveViewCullingMask = 0;

//        if (sliceableLayer != -1)
//        {
//            liveViewCullingMask |= (1 << sliceableLayer);
//            Debug.Log($"POLAROID >>> Live View: Adding {SLICEABLE_LAYER} to culling mask");
//        }
//        else
//        {
//            Debug.LogWarning($"POLAROID >>> Layer {SLICEABLE_LAYER} not found!");
//        }

//        if (defaultPictureLayer != -1)
//        {
//            liveViewCullingMask |= (1 << defaultPictureLayer);
//            Debug.Log($"POLAROID >>> Live View: Adding {DEFAULT_PICTURE_LAYER} to culling mask");
//        }
//        else
//        {
//            Debug.LogWarning($"POLAROID >>> Layer {DEFAULT_PICTURE_LAYER} not found!");
//        }

//        Debug.Log($"POLAROID >>> Live View culling mask set to: {liveViewCullingMask}");
//    }

//    public void CreatePolaroid()
//    {
//        Debug.Log("POLAROID >>> Start creating");
//        StartCoroutine(CreatePolaroidCoroutine());
//    }

//    private IEnumerator CreatePolaroidCoroutine()
//    {
//        if (hideObjectsOutsideBoundingBox && cullingMode == CullingMode.OnShutter)
//        {
//            ChangeLayersForPhoto();
//            SetCullingMaskForPhotoMode();
//            Debug.Log("POLAROID >>> Layers changed for photo, camera set to photo mode");
//        }

//        // Warte einen Frame für RenderTexture
//        yield return new WaitForEndOfFrame();

//        UpdateCurrentImage();
//        RenderCurrentImageOnPolaroid();

//        yield return StartCoroutine(SpawnPolaroid());
//        Debug.Log("POLAROID >>> successfully rendered and spawned");

//        // Upload happens, but we need to wait for it to complete
//        yield return StartCoroutine(UploadPolaroidCoroutine());
//        Debug.Log($"POLAROID >>> successfully uploaded");

//        // Now restore - AFTER everything is done
//        if (hideObjectsOutsideBoundingBox)
//        {
//            RestoreLayersAfterPhoto();
//            SetCullingMaskForLiveView();
//            Debug.Log("POLAROID >>> Layers and culling mask restored to live view");
//        }
//    }

//    public void UpdateLiveViewCulling(bool hide)
//    {
//        if (!hideObjectsOutsideBoundingBox || cullingMode != CullingMode.LiveView)
//            return;

//        if (hide)
//        {
//            ChangeLayersForPhoto();
//            SetCullingMaskForPhotoMode();
//            Debug.Log("POLAROID >>> Live View: Photo mode enabled");
//        }
//        else
//        {
//            RestoreLayersAfterPhoto();
//            SetCullingMaskForLiveView();
//            Debug.Log("POLAROID >>> Live View: Photo mode disabled, back to normal");
//        }
//    }

//    public void ChangeLayersForPhoto()
//    {
//        objectStates.Clear();

//        List<Transform> objectsInBox = FindObjectsInBoundingBox();

//        Debug.Log($"POLAROID >>> Found {objectsInBox.Count} objects in bounding box to change layers");

//        foreach (Transform obj in objectsInBox)
//        {
//            int currentLayer = obj.gameObject.layer;
//            int newLayer = currentLayer;
//            string currentLayerName = LayerMask.LayerToName(currentLayer);

//            // Speichere den ursprünglichen Layer
//            objectStates.Add(new ObjectVisibilityState(obj.gameObject, currentLayer));

//            // Ändere den Layer basierend auf dem aktuellen Layer
//            if (currentLayerName == DEFAULT_LAYER || currentLayerName == PAINTABLE_LAYER)
//            {
//                newLayer = LayerMask.NameToLayer(DEFAULT_PICTURE_LAYER);
//                Debug.Log($"POLAROID >>> Changing {obj.name} from {currentLayerName} to {DEFAULT_PICTURE_LAYER}");
//            }
//            else if (currentLayerName == SLICEABLE_LAYER)
//            {
//                newLayer = LayerMask.NameToLayer(SLICEABLE_PICTURE_LAYER);
//                Debug.Log($"POLAROID >>> Changing {obj.name} from {SLICEABLE_LAYER} to {SLICEABLE_PICTURE_LAYER}");
//            }

//            obj.gameObject.layer = newLayer;

//            // Ändere auch alle Children
//            foreach (Transform child in obj.GetComponentsInChildren<Transform>())
//            {
//                int childCurrentLayer = child.gameObject.layer;
//                int childNewLayer = childCurrentLayer;
//                string childLayerName = LayerMask.LayerToName(childCurrentLayer);

//                if (childLayerName == DEFAULT_LAYER || childLayerName == PAINTABLE_LAYER)
//                {
//                    childNewLayer = LayerMask.NameToLayer(DEFAULT_PICTURE_LAYER);
//                }
//                else if (childLayerName == SLICEABLE_LAYER)
//                {
//                    childNewLayer = LayerMask.NameToLayer(SLICEABLE_PICTURE_LAYER);
//                }

//                if (childNewLayer != childCurrentLayer)
//                {
//                    child.gameObject.layer = childNewLayer;
//                    objectStates.Add(new ObjectVisibilityState(child.gameObject, childCurrentLayer));
//                    Debug.Log($"POLAROID >>> Changing child {child.name} from {childLayerName} to {LayerMask.LayerToName(childNewLayer)}");
//                }
//            }
//        }
//    }

//    private void RestoreLayersAfterPhoto()
//    {
//        Debug.Log($"POLAROID >>> Restoring {objectStates.Count} objects to their original layers");

//        foreach (ObjectVisibilityState state in objectStates)
//        {
//            if (state.obj != null)
//            {
//                string originalLayerName = LayerMask.LayerToName(state.originalLayer);
//                string currentLayerName = LayerMask.LayerToName(state.obj.layer);
//                state.obj.layer = state.originalLayer;
//                Debug.Log($"POLAROID >>> Restored {state.obj.name} from {currentLayerName} to {originalLayerName}");
//            }
//        }

//        objectStates.Clear();
//        Debug.Log("POLAROID >>> All objects restored to original layers");
//    }

//    private void SetCullingMaskForPhotoMode()
//    {
//        // Setze die Culling Mask nur auf die Photo-Layer
//        int defaultPictureLayer = LayerMask.NameToLayer(DEFAULT_PICTURE_LAYER);
//        int sliceableLayer = LayerMask.NameToLayer(SLICEABLE_LAYER);

//        int newMask = 0;

//        if (defaultPictureLayer != -1)
//        {
//            newMask |= (1 << defaultPictureLayer);
//            Debug.Log($"POLAROID >>> Photo Mode: Adding {DEFAULT_PICTURE_LAYER} to culling mask");
//        }
//        else
//        {
//            Debug.LogWarning($"POLAROID >>> Layer {DEFAULT_PICTURE_LAYER} not found!");
//        }

//        if (sliceableLayer != -1)
//        {
//            newMask |= (1 << sliceableLayer);
//            Debug.Log($"POLAROID >>> Photo Mode: Adding {SLICEABLE_LAYER} to culling mask");
//        }
//        else
//        {
//            Debug.LogWarning($"POLAROID >>> Layer {SLICEABLE_LAYER} not found!");
//        }

//        Debug.Log($"POLAROID >>> Photo Mode: Setting culling mask to: {newMask}");
//        photoCamera.cullingMask = newMask;
//        isPhotoMode = true;
//    }

//    private void SetCullingMaskForLiveView()
//    {
//        Debug.Log($"POLAROID >>> Live View: Restoring culling mask to: {liveViewCullingMask}");
//        photoCamera.cullingMask = liveViewCullingMask;
//        isPhotoMode = false;
//    }

//    private void RenderCurrentImageOnPolaroid()
//    {
//        if (spawnNewPolaroidEachShot)
//        {
//            polaroid = Instantiate(polaroid);
//        }

//        Transform quadTransform = polaroid.transform.Find("Quad");
//        MeshRenderer renderer = quadTransform.GetComponent<MeshRenderer>();
//        Material mat = new Material(Shader.Find("Unlit/Texture"));
//        mat.mainTexture = currentImage;
//        renderer.material = mat;

//    }



//    // get current image as texture from render texture
//    private void UpdateCurrentImage()
//    {
//        Debug.Log("POLAROID >>> Capturing current image");  
//        RenderTexture.active = cameraView;
//        currentImage = new Texture2D(
//            cameraView.width,
//            cameraView.height,
//            TextureFormat.ARGB32,
//            false
//        );

//        currentImage.ReadPixels(
//            new Rect(0, 0, cameraView.width, cameraView.height),
//            0,
//            0
//        );

//        currentImage.Apply();
//        RenderTexture.active = null;
//    }



//    private IEnumerator SpawnPolaroid()
//    {
//        yield return SpawnEffects();
//        MovePolaroidToCamera();
//        yield return PolaroidSpawnAnimation();

//    }



//    private IEnumerator PolaroidSpawnAnimation()
//    {
//        polaroid.transform.Rotate(90f, 90f, 90f);
//        Vector3 stepDistance = -1 * polaroidSpawnPosition.right * 0.015f;
//        float stepDelay = 0.1f;
//        for (int i = 0; i < 10; i++)
//        {
//            polaroid.transform.localPosition += stepDistance;
//            yield return new WaitForSeconds(stepDelay);
//        }
//    }



//    private void MovePolaroidToCamera()
//    {
//        polaroid.transform.position = polaroidSpawnPosition.position;
//        polaroid.transform.rotation = polaroidSpawnPosition.rotation;
//    }



//    private IEnumerator SpawnEffects()
//    {
//        cameraShutter.Play();

//        cameraFlash.SetActive(true);
//        yield return new WaitForSeconds(flashTime);
//        cameraFlash.SetActive(false);
//    }

//    IEnumerator UploadPolaroidCoroutine()
//    {
//        byte[] currentImageJpg = currentImage.EncodeToJPG();
//        string timestamp = DateTime.Now.ToString("yyyy.MM.dd_HH.mm");
//        string filename = "file_" + timestamp + ".jpg";
//        yield return StartCoroutine(WaitAndUpload(currentImageJpg, filename));
//    }

//    private IEnumerator WaitAndUpload(byte[] imageData, string filename)
//    {
//        Debug.Log("POLAROID >>> waiting for extractor...");
//        yield return new WaitUntil(() => StreamingAssetsExtractor.IsReady);
//        Debug.Log("POLAROID >>> extractor ready, loading credentials...");

//        Dictionary<string, string> credentials = null;
//        yield return LoadCredentialsCoroutine(result => credentials = result);

//        if (credentials == null ||
//            !credentials.ContainsKey("username") ||
//            !credentials.ContainsKey("password") ||
//            !credentials.ContainsKey("url") ||
//            !credentials.ContainsKey("remoteDirectory"))
//        {
//            Debug.LogError("POLAROID >>> credentials missing or incomplete, aborting upload");
//            yield break;
//        }

//        Debug.Log("POLAROID >>> credentials loaded, starting upload...");

//        Task uploadTask = FtpHandler.UploadFile(
//            credentials["username"],
//            credentials["password"],
//            credentials["url"],
//            credentials["remoteDirectory"],
//            filename,
//            imageData);

//        yield return new WaitUntil(() => uploadTask.IsCompleted);

//        if (uploadTask.IsFaulted)
//        {
//            Debug.LogError($"POLAROID >>> upload task faulted: {uploadTask.Exception}");
//        }
//        else
//        {
//            Debug.Log("POLAROID >>> upload finished");
//        }
//    }

//    private IEnumerator LoadCredentialsCoroutine(Action<Dictionary<string, string>> onLoaded)
//    {
//        string pathToCredentials = Path.Combine(StreamingAssetsPaths.Credentials, "Secrets", "FTP.txt");
//        string separator = ":";

//        using (UnityWebRequest request = UnityWebRequest.Get(pathToCredentials))
//        {
//            yield return request.SendWebRequest();

//            if (request.result != UnityWebRequest.Result.Success)
//            {
//                Debug.LogError($"POLAROID >>> failed to load credentials file: {request.error}");
//                onLoaded(null);
//                yield break;
//            }

//            string text = request.downloadHandler.text;
//            Dictionary<string, string> credentials = ParseCredentials(text, separator);
//            onLoaded(credentials);
//        }
//    }

//    private Dictionary<string, string> ParseCredentials(string text, string separator)
//    {
//        var dict = new Dictionary<string, string>();
//        string[] lines = text.Split('\n');

//        foreach (string rawLine in lines)
//        {
//            string line = rawLine.Trim();
//            if (string.IsNullOrEmpty(line)) continue;

//            int index = line.IndexOf(separator);
//            if (index < 0) continue;

//            string key = line.Substring(0, index).Trim();
//            string value = line.Substring(index + separator.Length).Trim();
//            dict[key] = value;
//        }

//        return dict;
//    }


//    private class ObjectParentPair
//    {
//        public Transform obj;
//        public Transform originalParent;
//        public ObjectParentPair(Transform obj, Transform parent)
//        {
//            this.obj = obj;
//            this.originalParent = parent;
//        }
//    }

//    private List<Transform> FindObjectsInBoundingBox()
//    {
//        if (boundingBox == null)
//        {
//            Debug.LogError("POLAROID >>> Bounding Box not assigned!");
//            return new List<Transform>();
//        }

//        Vector3 halfExtents = boundingBox.lossyScale * 0.5f;
//        Debug.Log($"POLAROID >>> Searching at position: {boundingBox.position}, halfExtents: {halfExtents}");

//        Collider[] hitColliders = Physics.OverlapBox(
//            boundingBox.position,
//            halfExtents,
//            boundingBox.rotation
//        );

//        Debug.Log($"POLAROID >>> OverlapBox found: {hitColliders.Length} colliders");

//        List<Transform> objectsInBox = new List<Transform>();

//        foreach (Collider col in hitColliders)
//        {
//            if (col.transform != boundingBox &&
//                !col.transform.IsChildOf(excludeRoot))
//            {
//                objectsInBox.Add(col.transform);
//            }
//        }

//        return objectsInBox;
//    }

//    private bool IsChildOfAnyObject(Transform target, List<Transform> parents)
//    {
//        foreach (Transform parent in parents)
//        {
//            if (target.IsChildOf(parent))
//                return true;
//        }
//        return false;
//    }

//    public void OnObjectEnterBoundingBox(GameObject obj)
//    {
//        if (obj == null) return;

//        int currentLayer = obj.layer;
//        string currentLayerName = LayerMask.LayerToName(currentLayer);

//        Debug.Log($"POLAROID >>> Object entered bounding box: {obj.name} on layer {currentLayerName}");

//        if (currentLayerName == DEFAULT_LAYER)
//        {
//            int newLayer = LayerMask.NameToLayer(DEFAULT_PICTURE_LAYER);
//            if (newLayer != -1)
//            {
//                obj.layer = newLayer;
//                Debug.Log($"POLAROID >>> Changed {obj.name} from {DEFAULT_LAYER} to {DEFAULT_PICTURE_LAYER}");

//                foreach (Transform child in obj.GetComponentsInChildren<Transform>())
//                {
//                    if (child != obj.transform)
//                    {
//                        string childLayerName = LayerMask.LayerToName(child.gameObject.layer);
//                        if (childLayerName == DEFAULT_LAYER)
//                        {
//                            child.gameObject.layer = newLayer;
//                            Debug.Log($"POLAROID >>> Changed child {child.name} from {DEFAULT_LAYER} to {DEFAULT_PICTURE_LAYER}");
//                        }
//                    }
//                }
//            }
//        }
//    }

//    public void OnObjectExitBoundingBox(GameObject obj)
//    {
//        if (obj == null) return;

//        int currentLayer = obj.layer;
//        string currentLayerName = LayerMask.LayerToName(currentLayer);

//        Debug.Log($"POLAROID >>> Object exited bounding box: {obj.name} on layer {currentLayerName}");

//        if (currentLayerName == DEFAULT_PICTURE_LAYER)
//        {
//            int newLayer = LayerMask.NameToLayer(DEFAULT_LAYER);
//            if (newLayer != -1)
//            {
//                obj.layer = newLayer;
//                Debug.Log($"POLAROID >>> Changed {obj.name} from {DEFAULT_PICTURE_LAYER} to {DEFAULT_LAYER}");

//                foreach (Transform child in obj.GetComponentsInChildren<Transform>())
//                {
//                    if (child != obj.transform)
//                    {
//                        string childLayerName = LayerMask.LayerToName(child.gameObject.layer);
//                        if (childLayerName == DEFAULT_PICTURE_LAYER)
//                        {
//                            child.gameObject.layer = newLayer;
//                            Debug.Log($"POLAROID >>> Changed child {child.name} from {DEFAULT_PICTURE_LAYER} to {DEFAULT_LAYER}");
//                        }
//                    }
//                }
//            }
//        }
//    }
//}
//*/

using UnityEngine;
using System.Collections;
using Ftp;
using FileOperations;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine.Networking;
using System.Threading.Tasks;
public class PhotoHandler : MonoBehaviour
{

    [Header("Photo Data Model")]
    [SerializeField] private GameObject polaroid;
    [SerializeField] private RenderTexture cameraView;
    [SerializeField] private GameObject cameraPrefab;
    [SerializeField] private Transform polaroidSpawnPosition;
    [SerializeField] private bool spawnNewPolaroidEachShot = false;
    private Texture2D currentImage;

    [Header("Camera Effects")]
    [SerializeField] private GameObject cameraFlash;
    [SerializeField] private float flashTime;
    [SerializeField] private AudioSource cameraShutter;



    public void CreatePolaroid()
    {
        UpdateCurrentImage();
        RenderCurrentImageOnPolaroid();
        StartCoroutine(SpawnPolaroid());
        Debug.Log("POLAROID >>> successfully rendered and spawned");

        UploadPolaroid();
        Debug.Log($"POLAROID >>> successfully uploaded");
    }



    private void RenderCurrentImageOnPolaroid()
    {
        if (spawnNewPolaroidEachShot)
        {
            polaroid = Instantiate(polaroid);
        }

        Transform quadTransform = polaroid.transform.Find("Quad");
        MeshRenderer renderer = quadTransform.GetComponent<MeshRenderer>();
        Material mat = new Material(Shader.Find("Unlit/Texture"));
        mat.mainTexture = currentImage;
        renderer.material = mat;

    }



    // get current image as texture from render texture
    private void UpdateCurrentImage()
    {
        RenderTexture.active = cameraView;
        currentImage = new Texture2D(
            cameraView.width,
            cameraView.height,
            TextureFormat.ARGB32,
            false
            );

        currentImage.ReadPixels(
            new Rect(0, 0, cameraView.width, cameraView.height),
            0,
            0
        );

        currentImage.Apply();
        RenderTexture.active = null;
    }



    private IEnumerator SpawnPolaroid()
    {
        yield return SpawnEffects();
        MovePolaroidToCamera();
        yield return PolaroidSpawnAnimation();

    }



    private IEnumerator PolaroidSpawnAnimation()
    {
        polaroid.transform.Rotate(90f, 90f, 90f);
        Vector3 stepDistance = -1 * polaroidSpawnPosition.right * 0.015f;
        float stepDelay = 0.1f;
        for (int i = 0; i < 10; i++)
        {
            polaroid.transform.localPosition += stepDistance;
            yield return new WaitForSeconds(stepDelay);
        }
    }



    private void MovePolaroidToCamera()
    {
        polaroid.transform.position = polaroidSpawnPosition.position;
        polaroid.transform.rotation = polaroidSpawnPosition.rotation;
    }



    private IEnumerator SpawnEffects()
    {
        cameraShutter.Play();

        cameraFlash.SetActive(true);
        yield return new WaitForSeconds(flashTime);
        cameraFlash.SetActive(false);
    }


    void UploadPolaroid()
    {
        byte[] currentImageJpg = currentImage.EncodeToJPG();
        string timestamp = DateTime.Now.ToString("yyyy.MM.dd_HH.mm");
        string filename = "file_" + timestamp + ".jpg";
        StartCoroutine(WaitAndUpload(currentImageJpg, filename));
    }


    private IEnumerator WaitAndUpload(byte[] imageData, string filename)
    {
        Debug.Log("POLAROID >>> waiting for extractor...");
        yield return new WaitUntil(() => StreamingAssetsExtractor.IsReady);
        Debug.Log("POLAROID >>> extractor ready, loading credentials...");

        Dictionary<string, string> credentials = null;
        yield return LoadCredentialsCoroutine(result => credentials = result);

        if (credentials == null ||
            !credentials.ContainsKey("username") ||
            !credentials.ContainsKey("password") ||
            !credentials.ContainsKey("url") ||
            !credentials.ContainsKey("remoteDirectory"))
        {
            Debug.LogError("POLAROID >>> credentials missing or incomplete, aborting upload");
            yield break;
        }

        Debug.Log("POLAROID >>> credentials loaded, starting upload...");

        Task uploadTask = FtpHandler.UploadFile(
            credentials["username"],
            credentials["password"],
            credentials["url"],
            credentials["remoteDirectory"],
            filename,
            imageData);

        yield return new WaitUntil(() => uploadTask.IsCompleted);

        if (uploadTask.IsFaulted)
        {
            Debug.LogError($"POLAROID >>> upload task faulted: {uploadTask.Exception}");
        }
        else
        {
            Debug.Log("POLAROID >>> upload finished");
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
                Debug.LogError($"POLAROID >>> failed to load credentials file: {request.error}");
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