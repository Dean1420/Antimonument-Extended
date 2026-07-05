using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "MyMenu/Statue", fileName = "StatueValues")]
public class StatueValues : ScriptableObject
{
    [Header("Socket(Miniatur)")]
    public string Title;
    public Vector3 SocketSpawnPoint;
    public Vector3 SocketSpawnRotation;
    public Statue GrabbableStatuePrefab;


    [Space(20)] [Header("Instantiating Statue")]
    public AudioClip SoundClip;
    public AudioClip ExplanationClip;
    public Statue LifeSizeStatuePrefab;
    public Vector3 StatueSpawnPoint;
    public Vector3 StatueSpawnRotation;
    public float ScaleFactor = 1f;
    public Vector3 ExportVector = new Vector3(0, 0, 180);

    
    [Space(20)] [Header("Textures")] public Material Skybox;
    public Texture2D Tex;
    public Texture2D TempTex;
    // public GameObject StatuePrefab;
}
