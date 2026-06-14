


using UnityEngine;
using System.Collections;
using Ftp;
using FileOperations;
using System.Collections.Generic;
using System.IO;
using System;

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
    string fileType = ".jpg";
    string timestamp = DateTime.Now.ToString("yyyy.MM.dd_HH.mm.ss");
    string filename = "polaroid_";
    
    string polaroidFolder = Path.Combine(PersistentDataPaths.Runtime, "Polaroid");
    string fullPath = Path.Combine(polaroidFolder, timestamp + filename + fileType);
    
    Directory.CreateDirectory(polaroidFolder);
    File.WriteAllBytes(fullPath, currentImageJpg);
    Debug.Log($"POLAROID >>> Successfully saved");
    
    try
    {
        Dictionary<string, string> credentials = LoadCredentials();
        Ftp.FtpHandler.uploadFile(
            credentials["username"],
            credentials["password"],
            credentials["url"],
            credentials["remoteDirectory"],
            timestamp + filename + fileType,
            currentImageJpg);
    }
    catch (Exception e)
    {
        Debug.LogWarning($"POLAROID >>> FTP upload failed: {e.Message}");
    }
}

    private Dictionary<string, string> LoadCredentials()
{
    string pathToCredentials = Path.Combine(StreamingAssetsPaths.Credentials, "Secrets", "FTP.txt");
    string separator = ":";
    return Text.LoadLinesByKeyValue(pathToCredentials, separator);
}

}
