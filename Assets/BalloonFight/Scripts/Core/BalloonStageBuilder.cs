using UnityEngine;

public static class BalloonStageBuilder
{
    public static void Build(Transform parent, BalloonGameConfig config, BalloonVisualConfig visual)
    {
        Transform stage = new GameObject("Stage").transform;
        stage.SetParent(parent);

        System.Random random = new(visual.StarSeed);
        for (int index = 0; index < visual.StarCount; index++)
        {
            float x = (float)(random.NextDouble() * visual.StarBounds.width + visual.StarBounds.x);
            float y = (float)(random.NextDouble() * visual.StarBounds.height + visual.StarBounds.y);
            float size = index % Mathf.Max(1, visual.LargeStarInterval) == 0 ? visual.LargeStarSize : visual.SmallStarSize;
            RetroFactory.CreateBlock(stage, "Star", new Vector2(x, y), new Vector2(size, size), visual.StarColor, visual.StarOrder);
        }

        RetroFactory.CreateBlock(stage, "Water", visual.WaterPosition, visual.WaterSize, visual.WaterColor, visual.WaterOrder);

        foreach (PlatformDefinition platform in config.Platforms)
        {
            CreatePlatform(stage, platform.Position, platform.Size, visual);
        }
    }

    private static void CreatePlatform(Transform parent, Vector2 position, Vector2 size, BalloonVisualConfig visual)
    {
        GameObject platform = RetroFactory.CreateBlock(parent, "Platform", position, size, visual.PlatformColor, visual.PlatformOrder);
        platform.AddComponent<BoxCollider2D>();

        GameObject top = RetroFactory.CreateBlock(platform.transform, "Top", Vector2.zero, visual.PlatformTopSize, visual.PlatformTopColor, visual.PlatformTopOrder);
        top.transform.localPosition = visual.PlatformTopPosition;
    }
}
