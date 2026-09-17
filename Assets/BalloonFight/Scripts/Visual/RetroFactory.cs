using BalloonFight.Actors;
using BalloonFight.Config;
using UnityEngine;

internal static class RetroFactory
{
    private static BalloonVisualConfig _config;
    public static void Configure(BalloonVisualConfig config)
    {
        _config = config;
        _player = null;
        _playerTwo = null;
        _enemyA = null;
        _enemyB = null;
        _redBalloon = null;
        _yellowBalloon = null;
        _playerTwoBalloonA = null;
        _playerTwoBalloonB = null;
        _pinkBalloon = null;
    }

    private static Sprite _square;
    private static Sprite _player;
    private static Sprite _playerTwo;
    private static Sprite _enemyA;
    private static Sprite _enemyB;
    private static Sprite _redBalloon;
    private static Sprite _yellowBalloon;
    private static Sprite _playerTwoBalloonA;
    private static Sprite _playerTwoBalloonB;
    private static Sprite _pinkBalloon;

    internal static GameObject CreateBlock(
        Transform parent,
        string objectName,
        Vector2 position,
        Vector2 size,
        Color color,
        int sortingOrder)
    {
        GameObject block = new(objectName);
        block.transform.SetParent(parent);
        block.transform.position = new Vector3(position.x, position.y, 0f);
        block.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer renderer = block.AddComponent<SpriteRenderer>();
        renderer.sprite = GetSquare();
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return block;
    }

    internal static void CreateFighter(Transform root, bool isPlayer, int balloonCount, int variation)
    {
        GameObject visual = new("Visual");
        visual.transform.SetParent(root);
        visual.transform.localPosition = Vector3.zero;

        SpriteRenderer visualRenderer = visual.AddComponent<SpriteRenderer>();
        visualRenderer.sprite = isPlayer ? GetPlayerSprite(variation) : GetEnemySprite(variation);
        visualRenderer.sortingOrder = _config.ActorOrder;

        for (int index = 0; index < balloonCount; index++)
        {
            float x = (index - (balloonCount - 1) / 2f) * _config.BalloonSpacing;
            CreateRope(root, x);
            CreateBalloon(root, isPlayer, index, x, variation);
        }
    }

    private static void CreateRope(Transform root, float x)
    {
        GameObject rope = CreateBlock(root, "Rope", Vector2.zero, _config.RopeSize, _config.RopeColor, _config.RopeOrder);
        rope.transform.localPosition = new Vector3(x * _config.RopeHorizontalFactor, _config.RopeHeight, 0f);
        rope.transform.localRotation = Quaternion.Euler(0f, 0f, -x * _config.RopeAngleFactor);
    }

    private static void CreateBalloon(Transform root, bool isPlayer, int index, float x, int variation)
    {
        GameObject balloon = new($"Balloon {index + 1}");
        balloon.transform.SetParent(root);
        balloon.transform.localPosition = new Vector3(x, _config.BalloonHeight, 0f);

        SpriteRenderer renderer = balloon.AddComponent<SpriteRenderer>();
        renderer.sprite = isPlayer
            ? GetPlayerBalloon(index, variation)
            : GetPinkBalloon();
        renderer.sortingOrder = _config.BalloonOrder;

        CircleCollider2D collider = balloon.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = _config.BalloonRadius;
        collider.offset = _config.BalloonColliderOffset;
        balloon.AddComponent<BalloonHitTarget>();
    }

    internal static Sprite GetSquare()
    {
        if (_square != null)
        {
            return _square;
        }

        Texture2D texture = CreateTexture(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        _square = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return _square;
    }

    private static Sprite GetPlayerSprite(int variation)
    {
        if (variation == 0)
        {
            if (_config.PlayerSprite != null) return _config.PlayerSprite;
            return _player ??= CreateFighterSprite(_config.PlayerBody, _config.PlayerHelmet, _config.PlayerSkin);
        }

        if (_config.PlayerTwoSprite != null) return _config.PlayerTwoSprite;
        return _playerTwo ??= CreateFighterSprite(
            _config.PlayerTwoBody,
            _config.PlayerTwoHelmet,
            _config.PlayerTwoSkin);
    }

    private static Sprite GetPlayerBalloon(int balloonIndex, int variation)
    {
        if (variation == 0)
        {
            return balloonIndex == 0 ? GetRedBalloon() : GetYellowBalloon();
        }

        if (balloonIndex == 0)
        {
            if (_config.PlayerTwoBalloonASprite != null) return _config.PlayerTwoBalloonASprite;
            return _playerTwoBalloonA ??= CreateBalloonSprite(_config.PlayerTwoBalloonA);
        }

        if (_config.PlayerTwoBalloonBSprite != null) return _config.PlayerTwoBalloonBSprite;
        return _playerTwoBalloonB ??= CreateBalloonSprite(_config.PlayerTwoBalloonB);
    }

    private static Sprite GetEnemySprite(int variation)
    {
        Sprite supplied = variation % 2 == 0 ? _config.EnemyASprite : _config.EnemyBSprite;
        if (supplied != null) return supplied;
        if (variation % 2 == 0)
        {
            return _enemyA ??= CreateFighterSprite(_config.EnemyABody, _config.EnemyAHelmet, _config.EnemyASkin);
        }
        return _enemyB ??= CreateFighterSprite(_config.EnemyBBody, _config.EnemyBHelmet, _config.EnemyBSkin);
    }

    private static Sprite GetRedBalloon()
    {
        if (_config.PlayerBalloonASprite != null) return _config.PlayerBalloonASprite;
        return _redBalloon ??= CreateBalloonSprite(_config.PlayerBalloonA);
    }

    private static Sprite GetYellowBalloon()
    {
        if (_config.PlayerBalloonBSprite != null) return _config.PlayerBalloonBSprite;
        return _yellowBalloon ??= CreateBalloonSprite(_config.PlayerBalloonB);
    }

    private static Sprite GetPinkBalloon()
    {
        if (_config.EnemyBalloonSprite != null) return _config.EnemyBalloonSprite;
        return _pinkBalloon ??= CreateBalloonSprite(_config.EnemyBalloon);
    }

    private static Sprite CreateFighterSprite(Color body, Color helmet, Color skin)
    {
        Texture2D texture = CreateTexture(16, 16);
        Color outline = new Color32(18, 25, 39, 255);
        Color white = new Color32(245, 245, 230, 255);

        Fill(texture, 5, 11, 6, 3, helmet);
        Fill(texture, 4, 10, 8, 2, outline);
        Fill(texture, 5, 9, 6, 2, skin);
        Fill(texture, 6, 9, 1, 1, outline);
        Fill(texture, 9, 9, 1, 1, outline);
        Fill(texture, 5, 4, 6, 5, body);
        Fill(texture, 3, 5, 2, 3, white);
        Fill(texture, 11, 5, 2, 3, white);
        Fill(texture, 4, 2, 3, 2, outline);
        Fill(texture, 9, 2, 3, 2, outline);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.42f), 16f);
    }

    private static Sprite CreateBalloonSprite(Color mainColor)
    {
        Texture2D texture = CreateTexture(12, 16);
        Color outline = new Color32(23, 27, 42, 255);
        Color highlight = Color.Lerp(mainColor, Color.white, 0.45f);

        for (int y = 2; y < 15; y++)
        {
            for (int x = 1; x < 11; x++)
            {
                float normalizedX = (x - 5.5f) / 4.5f;
                float normalizedY = (y - 8.5f) / 6.2f;
                float ellipse = normalizedX * normalizedX + normalizedY * normalizedY;
                if (ellipse <= 1f)
                {
                    texture.SetPixel(x, y, ellipse > 0.72f ? outline : mainColor);
                }
            }
        }

        Fill(texture, 3, 4, 2, 4, highlight);
        Fill(texture, 5, 0, 2, 2, outline);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 12, 16), new Vector2(0.5f, 0.12f), 16f);
    }

    private static Texture2D CreateTexture(int width, int height)
    {
        Texture2D texture = new(width, height, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        Color[] pixels = new Color[width * height];
        for (int index = 0; index < pixels.Length; index++)
        {
            pixels[index] = Color.clear;
        }

        texture.SetPixels(pixels);
        return texture;
    }

    private static void Fill(Texture2D texture, int x, int y, int width, int height, Color color)
    {
        for (int pixelY = y; pixelY < y + height; pixelY++)
        {
            for (int pixelX = x; pixelX < x + width; pixelX++)
            {
                if (pixelX >= 0 && pixelX < texture.width && pixelY >= 0 && pixelY < texture.height)
                {
                    texture.SetPixel(pixelX, pixelY, color);
                }
            }
        }
    }
}
