using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

internal static class BalloonPrefabBuilder
{
    private const string GeneratedFolder = "Assets/BalloonFight/Generated";
    private const string ResourcesFolder = "Assets/Resources";
    private const string PrefabsFolder = GeneratedFolder + "/Prefabs";
    private const string SpritesFolder = GeneratedFolder + "/Sprites";

    [MenuItem("Balloon Fight/Prepare Phase 3")]
    private static void Prepare()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("Exit Play Mode before preparing assets.");
            return;
        }

        EnsureFolder(ResourcesFolder);
        EnsureFolder(PrefabsFolder);
        EnsureFolder(SpritesFolder);
        BalloonGameConfig game = GetOrCreate<BalloonGameConfig>();
        BalloonVisualConfig visual = GetOrCreate<BalloonVisualConfig>();
        GetOrCreate<BalloonUiConfig>();
        GetOrCreate<BalloonInputConfig>();
        GetOrCreate<BalloonFeedbackConfig>();
        BalloonPrefabConfig prefabs = GetOrCreate<BalloonPrefabConfig>();
        RetroFactory.Configure(visual);

        GameObject root = new(nameof(BalloonPrefabBuilder));
        root.SetActive(false);
        try
        {
            GameObject player = LoadPrefab("Player");
            if (player == null)
            {
                player = Save(FighterFactory.CreatePlayer(root.transform, game).gameObject, "Player");
            }

            GameObject[] enemies = new GameObject[2];
            for (int index = 0; index < enemies.Length; index++)
            {
                string assetName = $"Enemy{index}";
                enemies[index] = LoadPrefab(assetName);
                if (enemies[index] == null)
                {
                    enemies[index] = Save(FighterFactory.CreateEnemy(root.transform, game, index).gameObject, assetName);
                }
            }

            GameObject stage = LoadPrefab("Stage");
            if (stage == null)
            {
                GameObject stageRoot = new("StagePrefab");
                stageRoot.transform.SetParent(root.transform, false);
                BalloonStageBuilder.Build(stageRoot.transform, game, visual);
                stage = Save(stageRoot, "Stage");
            }

            SerializedObject serialized = new(prefabs);
            AssignIfEmpty(serialized.FindProperty("_player"), player);
            AssignIfEmpty(serialized.FindProperty("_stage"), stage);
            SerializedProperty enemyArray = serialized.FindProperty("_enemies");
            if (enemyArray.arraySize == 0)
            {
                enemyArray.arraySize = enemies.Length;
                for (int index = 0; index < enemies.Length; index++)
                {
                    enemyArray.GetArrayElementAtIndex(index).objectReferenceValue = enemies[index];
                }
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            Selection.activeObject = prefabs;
            Debug.Log("Phase 3 assets prepared. Existing assets and references were preserved.");
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    private static void AssignIfEmpty(SerializedProperty property, Object value)
    {
        if (property.objectReferenceValue == null)
        {
            property.objectReferenceValue = value;
        }
    }

    private static GameObject LoadPrefab(string assetName)
    {
        return AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsFolder}/{assetName}.prefab");
    }

    private static GameObject Save(GameObject instance, string assetName)
    {
        Dictionary<Sprite, Sprite> saved = new();
        int index = 0;
        foreach (SpriteRenderer renderer in instance.GetComponentsInChildren<SpriteRenderer>(true))
        {
            Sprite sprite = renderer.sprite;
            if (sprite == null || AssetDatabase.Contains(sprite))
            {
                continue;
            }

            if (!saved.TryGetValue(sprite, out Sprite persistent))
            {
                string path = $"{SpritesFolder}/{assetName}_{index++}.asset";
                persistent = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (persistent == null)
                {
                    Texture2D texture = Object.Instantiate(sprite.texture);
                    Vector2 pivot = new(sprite.pivot.x / sprite.rect.width, sprite.pivot.y / sprite.rect.height);
                    persistent = Sprite.Create(texture, sprite.rect, pivot, sprite.pixelsPerUnit);
                    persistent.name = renderer.gameObject.name;
                    AssetDatabase.CreateAsset(persistent, path);
                    AssetDatabase.AddObjectToAsset(texture, persistent);
                }
                saved.Add(sprite, persistent);
            }
            renderer.sprite = persistent;
        }

        instance.SetActive(true);
        GameObject result = PrefabUtility.SaveAsPrefabAsset(instance, $"{PrefabsFolder}/{assetName}.prefab");
        if (result == null)
        {
            throw new System.InvalidOperationException($"Could not save {assetName} prefab.");
        }
        return result;
    }

    private static T GetOrCreate<T>() where T : ScriptableObject
    {
        string path = $"{ResourcesFolder}/{typeof(T).Name}.asset";
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
        }
        return asset;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }
        string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
    }
}
