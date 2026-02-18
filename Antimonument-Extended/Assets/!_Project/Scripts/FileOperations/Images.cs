using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using FileOperations;



namespace FileOperations{

 public static class Images
    {
        public static void SaveTextureAsJpg(Texture2D texture, string relativePath, string filename)
        {

            byte[] bytes = texture.EncodeToJPG();

            // create the directory if it doesn't exist
            Directory.CreateDirectory(RuntimePaths.Runtime + "/" + relativePath);

            string fullPath = Path.Combine(
                RuntimePaths.Runtime,
                relativePath,
                filename
            );

            
            File.WriteAllBytes(fullPath, bytes);
            
            Debug.Log($"LOCAL STORAGE >>> texture stored as jpg at: " + fullPath );
        }
    }
}