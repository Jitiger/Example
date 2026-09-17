using UnityEngine;

[System.Serializable]
internal struct PlatformDefinition
{
    [SerializeField] private Vector2 _position;
    [SerializeField] private Vector2 _size;
    internal PlatformDefinition(Vector2 position, Vector2 size)
    {
        _position = position;
        _size = size;
    }

    internal Vector2 Position => _position;
    internal Vector2 Size => _size;
}

public sealed class BalloonStageBuilder : MonoBehaviour
{
    [SerializeField] private Color _starColor = new Color32(205, 226, 255, 255);
    [SerializeField] private int _starCount = 46;
    [SerializeField] private int _starSeed = 1984;
    [SerializeField] private Rect _starBounds = new(-7f, -4f, 14f, 9f);
    [SerializeField] private Color _waterColor = new Color32(22, 77, 145, 255);
    [SerializeField] private Vector2 _waterPosition = new(0f, -5.05f);
    [SerializeField] private Vector2 _waterSize = new(16f, 1.1f);
    [SerializeField] private Color _platformColor = new Color32(72, 143, 90, 255);
    [SerializeField] private PlatformDefinition[] _platforms =
    {
        new(new Vector2(0f, -4.55f), new Vector2(14.6f, 0.45f)),
        new(new Vector2(-4.65f, -2.45f), new Vector2(2.7f, 0.32f)),
        new(new Vector2(0f, -1.25f), new Vector2(2.9f, 0.32f)),
        new(new Vector2(4.55f, -2.25f), new Vector2(2.7f, 0.32f)),
        new(new Vector2(-2.8f, 1.1f), new Vector2(2.2f, 0.32f)),
        new(new Vector2(2.8f, 1.35f), new Vector2(2.3f, 0.32f))
    };

    internal void Build(Transform parent)
    {
        Transform stage = new GameObject("Stage").transform;
        stage.SetParent(parent);
        System.Random random = new(_starSeed);
        for (int index = 0; index < _starCount; index++)
        {
            Vector2 position = new((float)(random.NextDouble() * _starBounds.width + _starBounds.x), (float)(random.NextDouble() * _starBounds.height + _starBounds.y));
            RetroFactory.CreateBlock(stage, "Star", position, Vector2.one * 0.05f, _starColor, -20);
        }

        RetroFactory.CreateBlock(stage, "Water", _waterPosition, _waterSize, _waterColor, -15);
        foreach (PlatformDefinition platform in _platforms)
        {
            GameObject block = RetroFactory.CreateBlock(stage, "Platform", platform.Position, platform.Size, _platformColor, -2);
            block.AddComponent<BoxCollider2D>();
        }
    }
}
