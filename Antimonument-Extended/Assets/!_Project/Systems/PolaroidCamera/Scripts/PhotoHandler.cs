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