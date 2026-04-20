// Assets/Editor/StreamingAssetsManifestBuilder.cs
using System.IO;
using UnityEditor;
using UnityEngine;

public class StreamingAssetsManifestBuilder
{
    // runs automatically every time you build the project
    // [InitializeOnLoadMethod]
    static void RegisterBuildCallback()
    {
        BuildPlayerWindow.RegisterBuildPlayerHandler(build =>
        {
            GenerateManifest();
            BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(build);
        });
    }

    // can also be triggered manually via the Unity menu
    [MenuItem("Tools/Regenerate StreamingAssets Manifest")]
    public static void GenerateManifest()
    {
        string streamingAssetsPath = Application.streamingAssetsPath;
        string manifestPath = Path.Combine(streamingAssetsPath, "manifest.txt");

        if (!Directory.Exists(streamingAssetsPath))
        {
            Debug.LogWarning("StreamingAssets folder does not exist.");
            return;
        }

        // get all files recursively, excluding any existing manifest and .meta files
        string[] allFiles = Directory.GetFiles(streamingAssetsPath, "*", SearchOption.AllDirectories);

        using StreamWriter writer = new StreamWriter(manifestPath);
        foreach (string file in allFiles)
        {
            if (file.EndsWith(".meta") || file == manifestPath) continue;

            // store paths relative to StreamingAssets so they're platform-agnostic
            string relativePath = file.Replace(streamingAssetsPath, "").TrimStart('/', '\\');
            writer.WriteLine(relativePath);
        }

        AssetDatabase.Refresh();
        Debug.Log($"Manifest written to: {manifestPath}");
    }
}