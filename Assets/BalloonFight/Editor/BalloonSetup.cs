using BalloonFight.Config;
using UnityEditor;
using UnityEngine;

namespace BalloonFight.Editor
{
    internal static class BalloonSetup
    {
        private const string ResourcesFolder = "Assets/Resources";

        [MenuItem("Balloon Fight/Create Settings")]
        private static void CreateSettings()
        {
            if (!AssetDatabase.IsValidFolder(ResourcesFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            CreateIfMissing<BalloonGameConfig>();
            CreateIfMissing<BalloonInputConfig>();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateIfMissing<T>() where T : ScriptableObject
        {
            string path = $"{ResourcesFolder}/{typeof(T).Name}.asset";
            if (AssetDatabase.LoadAssetAtPath<T>(path) != null)
            {
                return;
            }

            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<T>(), path);
        }
    }
}
