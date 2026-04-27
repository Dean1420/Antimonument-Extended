using UnityEngine;
using System.IO;


namespace FileOperations
{

    // data you want so store or edit during runtime
    public static class PersistentDataPaths
    {
        public static readonly string Root = Application.persistentDataPath;

        public static readonly string Runtime = Path.Combine(Root, "Runtime");

    }

    // data you want to
    public static class StreamingAssetsPaths
    {
        public static readonly string Root = Application.streamingAssetsPath;

        public static readonly string Credentials = Path.Combine(Root, "Credentials");

        public static readonly string GaussianSplats = Path.Combine(Root, "GaussianSplats");
    }
}
