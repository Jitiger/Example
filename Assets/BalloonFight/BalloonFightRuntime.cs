using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

internal sealed class BalloonFightRuntime : MonoBehaviour
{
    private const float TopLimit = 4.8f;
    private const float SpawnY = -3.35f;

    private Camera _camera;
    private BalloonPlayer _player;
    private readonly List<BalloonEnemy> _enemies = new();

    private int _score;
    private int _lives = 3;
    private int _phase = 1;
    private bool _gameOver;
    private bool _allClear;
    private bool _changingPhase;

    internal BalloonPlayer Player => _player;
    internal bool Playing => !_gameOver && !_allClear;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (FindFirstObjectByType<BalloonFightRuntime>() != null)
        {
            return;
        }

        new GameObject("Balloon Fight Runtime").AddComponent<BalloonFightRuntime>();
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        BuildCamera();
        BuildStage();
        SpawnPlayer();
        SpawnPhase();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if ((_gameOver || _allClear) && keyboard.rKey.wasPressedThisFrame)
        {
            StartCoroutine(Restart());
        }
    }

    private void BuildCamera()
    {
        foreach (Camera sceneCamera in FindObjectsByType<Camera>(FindObjectsSortMode.None))
        {
            sceneCamera.enabled = false;
        }

        GameObject cameraObject = new("Balloon Fight Camera");
        cameraObject.transform.SetParent(transform);
        cameraObject.tag = "MainCamera";

        _camera = cameraObject.AddComponent<Camera>();
        _camera.orthographic = true;
        _camera.orthographicSize = 5.5f;
        _camera.transform.position = new Vector3(0f, 0f, -10f);
        _camera.clearFlags = CameraClearFlags.SolidColor;
        _camera.backgroundColor = new Color32(9, 20, 46, 255);
    }

    private void BuildStage()
    {
        Transform stage = new GameObject("Stage").transform;
        stage.SetParent(transform);

        System.Random random = new(1984);
        for (int i = 0; i < 46; i++)
        {
            float x = (float)(random.NextDouble() * 14f - 7f);
            float y = (float)(random.NextDouble() * 9f - 4f);
            float size = i % 5 == 0 ? 0.09f : 0.045f;
            RetroFactory.CreateBlock(stage, "Star", new Vector2(x, y), new Vector2(size, size), new Color32(205, 226, 255, 255), -20);
        }

        RetroFactory.CreateBlock(stage, "Water", new Vector2(0f, -5.05f), new Vector2(16f, 1.1f), new Color32(22, 77, 145, 255), -15);

        CreatePlatform(stage, new Vector2(0f, -4.55f), new Vector2(14.6f, 0.45f));
        CreatePlatform(stage, new Vector2(-4.65f, -2.45f), new Vector2(2.7f, 0.32f));
        CreatePlatform(stage, new Vector2(0f, -1.25f), new Vector2(2.9f, 0.32f));
        CreatePlatform(stage, new Vector2(4.55f, -2.25f), new Vector2(2.7f, 0.32f));
        CreatePlatform(stage, new Vector2(-2.8f, 1.1f), new Vector2(2.2f, 0.32f));
        CreatePlatform(stage, new Vector2(2.8f, 1.35f), new Vector2(2.3f, 0.32f));
    }

    private static void CreatePlatform(Transform parent, Vector2 position, Vector2 size)
    {
        GameObject platform = RetroFactory.CreateBlock(parent, "Platform", position, size, new Color32(72, 143, 90, 255), -2);
        platform.AddComponent<BoxCollider2D>();

        GameObject top = RetroFactory.CreateBlock(platform.transform, "Top", Vector2.zero, new Vector2(1f, 0.2f), new Color32(161, 214, 119, 255), -1);
        top.transform.localPosition = new Vector3(0f, 0.38f, 0f);
    }

    private void SpawnPlayer()
    {
        GameObject playerObject = BuildFighter("Player", new Vector2(0f, SpawnY), true, 2, 0);
        _player = playerObject.AddComponent<BalloonPlayer>();
        playerObject.GetComponent<BalloonBody>().Owner = _player;
        _player.Initialize(this, 2);
    }

    private void SpawnPhase()
    {
        Vector2[] points =
        {
            new(-4.7f, 0.2f),
            new(4.7f, 0.45f),
            new(-1.8f, 2.7f),
            new(1.9f, 3.0f),
            new(0f, 0.65f)
        };

        int count = Mathf.Min(_phase + 2, points.Length);
        for (int i = 0; i < count; i++)
        {
            GameObject enemyObject = BuildFighter($"Enemy {i + 1}", points[i], false, 1, i);
            BalloonEnemy enemy = enemyObject.AddComponent<BalloonEnemy>();
            enemyObject.GetComponent<BalloonBody>().Owner = enemy;
            enemy.Initialize(this, 1);
            _enemies.Add(enemy);
        }
    }

    private GameObject BuildFighter(string objectName, Vector2 position, bool player, int balloonCount, int variation)
    {
        GameObject fighter = new(objectName);
        fighter.transform.SetParent(transform);
        fighter.transform.position = position;

        Rigidbody2D body = fighter.AddComponent<Rigidbody2D>();
        body.gravityScale = player ? 0.78f : 0.63f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.linearDamping = 0.1f;

        CapsuleCollider2D collider = fighter.AddComponent<CapsuleCollider2D>();
        collider.size = new Vector2(0.58f, 0.78f);
        collider.offset = new Vector2(0f, -0.05f);

        fighter.AddComponent<BalloonBody>();
        RetroFactory.CreateFighter(fighter.transform, player, balloonCount, variation);
        return fighter;
    }

    internal void EnemyDefeated(BalloonEnemy enemy)
    {
        if (enemy == null)
        {
            return;
        }

        _enemies.Remove(enemy);
        _score += 500;

        if (_enemies.Count == 0 && !_changingPhase && !_gameOver && !_allClear)
        {
            if (_phase >= 3)
            {
                _allClear = true;
                return;
            }

            StartCoroutine(NextPhase());
        }
    }

    private IEnumerator NextPhase()
    {
        _changingPhase = true;
        yield return new WaitForSeconds(1.2f);
        _phase++;
        SpawnPhase();
        _changingPhase = false;
    }

    internal void PlayerDefeated()
    {
        if (_gameOver || _allClear)
        {
            return;
        }

        _lives--;
        if (_lives <= 0)
        {
            _gameOver = true;
            return;
        }

        StartCoroutine(RespawnPlayer());
    }

    private IEnumerator RespawnPlayer()
    {
        yield return new WaitForSeconds(1.1f);
        if (!Playing)
        {
            yield break;
        }

        SpawnPlayer();
        _player.SetInvincible(1.4f);
    }

    private IEnumerator Restart()
    {
        if (_player != null)
        {
            Destroy(_player.gameObject);
        }

        foreach (BalloonEnemy enemy in _enemies)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }

        _enemies.Clear();
        yield return null;

        _score = 0;
        _lives = 3;
        _phase = 1;
        _gameOver = false;
        _allClear = false;
        _changingPhase = false;
        SpawnPlayer();
        SpawnPhase();
    }

    internal void Wrap(Transform target)
    {
        float edge = _camera.orthographicSize * _camera.aspect + 0.45f;
        Vector3 position = target.position;

        if (position.x > edge)
        {
            position.x = -edge;
        }
        else if (position.x < -edge)
        {
            position.x = edge;
        }

        target.position = position;
    }

    internal static void ClampVertical(Transform target, Rigidbody2D body)
    {
        if (target.position.y <= TopLimit)
        {
            return;
        }

        Vector3 position = target.position;
        position.y = TopLimit;
        target.position = position;

        Vector2 velocity = body.linearVelocity;
        if (velocity.y > 0f)
        {
            velocity.y *= -0.25f;
            body.linearVelocity = velocity;
        }
    }

    private void OnGUI()
    {
        GUIStyle hud = new(GUI.skin.label)
        {
            fontSize = 22,
            fontStyle = FontStyle.Bold
        };
        hud.normal.textColor = Color.white;

        GUIStyle small = new(GUI.skin.label)
        {
            fontSize = 15
        };
        small.normal.textColor = new Color(0.82f, 0.9f, 1f);

        GUI.Label(new Rect(20, 15, 280, 35), $"SCORE {_score:000000}", hud);
        GUI.Label(new Rect(Screen.width / 2f - 70f, 15, 150, 35), $"PHASE {_phase}", hud);
        GUI.Label(new Rect(Screen.width - 170, 15, 150, 35), $"LIVES {_lives}", hud);
        GUI.Label(new Rect(20, 48, 230, 28), $"ENEMIES {_enemies.Count}", small);
        GUI.Label(new Rect(20, Screen.height - 52, 760, 30), "A/D or ←/→ : move    SPACE / Z / ↑ : flap    Hit enemy balloons from below", small);

        if (_changingPhase)
        {
            DrawCenter("PHASE CLEAR", "Next phase incoming");
        }
        else if (_gameOver)
        {
            DrawCenter("GAME OVER", "Press R to restart");
        }
        else if (_allClear)
        {
            DrawCenter("ALL CLEAR", "Press R to play again");
        }
    }

    private static void DrawCenter(string title, string subtitle)
    {
        GUIStyle titleStyle = new(GUI.skin.label)
        {
            fontSize = 40,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        titleStyle.normal.textColor = new Color32(255, 229, 102, 255);

        GUIStyle subStyle = new(GUI.skin.label)
        {
            fontSize = 20,
            alignment = TextAnchor.MiddleCenter
        };
        subStyle.normal.textColor = Color.white;

        Rect panel = new(Screen.width / 2f - 215f, Screen.height / 2f - 70f, 430f, 140f);
        GUI.Box(panel, GUIContent.none);
        GUI.Label(new Rect(panel.x, panel.y + 15f, panel.width, 60f), title, titleStyle);
        GUI.Label(new Rect(panel.x, panel.y + 80f, panel.width, 35f), subtitle, subStyle);
    }
}

internal abstract class BalloonActor : MonoBehaviour
{
    protected BalloonFightRuntime Game;
    protected Rigidbody2D Body;
    protected Collider2D BodyCollider;

    private readonly List<BalloonTarget> _balloons = new();
    private float _invincibleUntil;
    private bool _dead;

    protected int Balloons { get; private set; }
    protected bool Dead => _dead;

    internal virtual void Initialize(BalloonFightRuntime game, int balloonCount)
    {
        Game = game;
        Body = GetComponent<Rigidbody2D>();
        BodyCollider = GetComponent<Collider2D>();

        _balloons.Clear();
        _balloons.AddRange(GetComponentsInChildren<BalloonTarget>(true));
        Balloons = Mathf.Min(balloonCount, _balloons.Count);

        for (int i = 0; i < _balloons.Count; i++)
        {
            _balloons[i].Owner = this;
            _balloons[i].gameObject.SetActive(i < Balloons);
        }
    }

    internal void PopBalloon(BalloonTarget target)
    {
        if (_dead || target == null || Time.time < _invincibleUntil || !target.gameObject.activeSelf)
        {
            return;
        }

        target.gameObject.SetActive(false);
        Balloons = Mathf.Max(0, Balloons - 1);
        _invincibleUntil = Time.time + HitProtection;
        BalloonLost();
    }

    internal void SetInvincible(float seconds)
    {
        _invincibleUntil = Mathf.Max(_invincibleUntil, Time.time + seconds);
    }

    protected void KillActor()
    {
        _dead = true;
    }

    protected void ClampVelocity(float maxX, float maxUp, float maxDown)
    {
        Vector2 velocity = Body.linearVelocity;
        velocity.x = Mathf.Clamp(velocity.x, -maxX, maxX);
        velocity.y = Mathf.Clamp(velocity.y, -maxDown, maxUp);
        Body.linearVelocity = velocity;
    }

    protected virtual float HitProtection => 0.2f;
    protected abstract void BalloonLost();
}

internal sealed class BalloonPlayer : BalloonActor
{
    private bool _grounded;
    private Transform _visual;

    internal override void Initialize(BalloonFightRuntime game, int balloonCount)
    {
        base.Initialize(game, balloonCount);
        _visual = transform.Find("Visual");
    }

    private void Update()
    {
        if (Dead || Game == null || !Game.Playing)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        bool flap = keyboard.spaceKey.wasPressedThisFrame || keyboard.zKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame;
        if (!flap)
        {
            return;
        }

        float power = Balloons >= 2 ? 4.7f : 3.7f;
        Body.AddForce(Vector2.up * power, ForceMode2D.Impulse);
        _grounded = false;
    }

    private void FixedUpdate()
    {
        if (Dead || Game == null || !Game.Playing)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;
        float horizontal = 0f;

        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                horizontal -= 1f;
            }

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                horizontal += 1f;
            }
        }

        float acceleration = _grounded ? 24f : 9f;
        Body.AddForce(Vector2.right * horizontal * acceleration);
        Body.linearDamping = _grounded ? 2.2f : 0.12f;
        ClampVelocity(5.8f, 6.4f, 6.2f);
        BalloonFightRuntime.ClampVertical(transform, Body);
        Game.Wrap(transform);

        if (_visual != null)
        {
            _visual.localRotation = Quaternion.Euler(0f, 0f, Mathf.Clamp(-Body.linearVelocity.x * 2.5f, -14f, 14f));
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y > 0.45f)
            {
                _grounded = true;
                return;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        _grounded = false;
    }

    protected override float HitProtection => 0.65f;

    protected override void BalloonLost()
    {
        Body.AddForce(Vector2.down * 0.9f, ForceMode2D.Impulse);
        if (Balloons > 0)
        {
            return;
        }

        KillActor();
        BodyCollider.enabled = false;
        Body.gravityScale = 2.1f;
        Body.freezeRotation = false;
        Body.angularVelocity = 220f;
        Game.PlayerDefeated();
        Destroy(gameObject, 0.9f);
    }
}

internal sealed class BalloonEnemy : BalloonActor
{
    private float _moveDirection = 1f;
    private float _nextDecision;
    private float _nextFlap;

    internal override void Initialize(BalloonFightRuntime game, int balloonCount)
    {
        base.Initialize(game, balloonCount);
        Decide();
    }

    private void FixedUpdate()
    {
        if (Dead || Game == null || !Game.Playing)
        {
            return;
        }

        if (Time.time >= _nextDecision)
        {
            Decide();
        }

        BalloonPlayer player = Game.Player;
        if (player != null && Time.time >= _nextFlap)
        {
            float playerDeltaY = player.transform.position.y - transform.position.y;
            if (playerDeltaY > -0.35f || Random.value < 0.22f)
            {
                Body.AddForce(Vector2.up * 3.45f, ForceMode2D.Impulse);
            }

            _nextFlap = Time.time + Random.Range(0.55f, 0.95f);
        }

        Body.AddForce(Vector2.right * _moveDirection * 5.2f);
        ClampVelocity(4.2f, 5.1f, 5.8f);
        BalloonFightRuntime.ClampVertical(transform, Body);
        Game.Wrap(transform);
    }

    private void Decide()
    {
        BalloonPlayer player = Game != null ? Game.Player : null;
        if (player != null && Random.value < 0.72f)
        {
            float delta = player.transform.position.x - transform.position.x;
            _moveDirection = Mathf.Abs(delta) < 0.4f ? _moveDirection : Mathf.Sign(delta);
        }
        else
        {
            _moveDirection = Random.value > 0.5f ? 1f : -1f;
        }

        _nextDecision = Time.time + Random.Range(0.65f, 1.35f);
    }

    protected override void BalloonLost()
    {
        if (Balloons > 0)
        {
            return;
        }

        KillActor();
        BodyCollider.enabled = false;
        Body.gravityScale = 1.8f;
        Body.freezeRotation = false;
        Body.angularVelocity = Random.Range(-220f, 220f);
        Body.AddForce(Vector2.up * 1.2f, ForceMode2D.Impulse);
        Game.EnemyDefeated(this);
        Destroy(gameObject, 1.4f);
    }
}

internal sealed class BalloonTarget : MonoBehaviour
{
    internal BalloonActor Owner { get; set; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        BalloonBody attackingBody = other.GetComponent<BalloonBody>();
        if (Owner == null || attackingBody == null || attackingBody.Owner == null || attackingBody.Owner == Owner)
        {
            return;
        }

        Owner.PopBalloon(this);
    }
}

internal sealed class BalloonBody : MonoBehaviour
{
    internal BalloonActor Owner { get; set; }
}

internal static class RetroFactory
{
    private static Sprite _square;
    private static Sprite _player;
    private static Sprite _enemyA;
    private static Sprite _enemyB;
    private static Sprite _redBalloon;
    private static Sprite _yellowBalloon;
    private static Sprite _pinkBalloon;

    internal static GameObject CreateBlock(Transform parent, string name, Vector2 position, Vector2 size, Color color, int sortingOrder)
    {
        GameObject block = new(name);
        block.transform.SetParent(parent);
        block.transform.position = new Vector3(position.x, position.y, 0f);
        block.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer renderer = block.AddComponent<SpriteRenderer>();
        renderer.sprite = Square();
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return block;
    }

    internal static void CreateFighter(Transform root, bool player, int balloonCount, int variation)
    {
        GameObject visual = new("Visual");
        visual.transform.SetParent(root);
        visual.transform.localPosition = Vector3.zero;

        SpriteRenderer visualRenderer = visual.AddComponent<SpriteRenderer>();
        visualRenderer.sprite = player ? PlayerSprite() : EnemySprite(variation);
        visualRenderer.sortingOrder = 10;

        for (int i = 0; i < balloonCount; i++)
        {
            float x = balloonCount == 1 ? 0f : (i == 0 ? -0.28f : 0.28f);

            GameObject rope = CreateBlock(root, "Rope", Vector2.zero, new Vector2(0.035f, 0.8f), new Color32(231, 226, 193, 255), 8);
            rope.transform.localPosition = new Vector3(x * 0.55f, 0.55f, 0f);
            rope.transform.localRotation = Quaternion.Euler(0f, 0f, -x * 28f);

            GameObject balloon = new($"Balloon {i + 1}");
            balloon.transform.SetParent(root);
            balloon.transform.localPosition = new Vector3(x, 1.02f, 0f);

            SpriteRenderer renderer = balloon.AddComponent<SpriteRenderer>();
            renderer.sprite = player ? (i == 0 ? RedBalloon() : YellowBalloon()) : PinkBalloon();
            renderer.sortingOrder = 9;

            CircleCollider2D collider = balloon.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.34f;
            collider.offset = new Vector2(0f, 0.28f);

            balloon.AddComponent<BalloonTarget>();
        }
    }

    private static Sprite Square()
    {
        if (_square != null)
        {
            return _square;
        }

        Texture2D texture = Texture(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        _square = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return _square;
    }

    private static Sprite PlayerSprite()
    {
        return _player ??= FighterSprite(new Color32(42, 117, 215, 255), new Color32(232, 55, 55, 255), new Color32(249, 207, 153, 255));
    }

    private static Sprite EnemySprite(int variation)
    {
        if (variation % 2 == 0)
        {
            return _enemyA ??= FighterSprite(new Color32(134, 68, 179, 255), new Color32(240, 188, 65, 255), new Color32(166, 211, 102, 255));
        }

        return _enemyB ??= FighterSprite(new Color32(63, 153, 129, 255), new Color32(215, 75, 126, 255), new Color32(248, 194, 137, 255));
    }

    private static Sprite RedBalloon()
    {
        return _redBalloon ??= BalloonSprite(new Color32(237, 55, 72, 255));
    }

    private static Sprite YellowBalloon()
    {
        return _yellowBalloon ??= BalloonSprite(new Color32(250, 190, 54, 255));
    }

    private static Sprite PinkBalloon()
    {
        return _pinkBalloon ??= BalloonSprite(new Color32(214, 82, 177, 255));
    }

    private static Sprite FighterSprite(Color body, Color helmet, Color skin)
    {
        Texture2D texture = Texture(16, 16);
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

    private static Sprite BalloonSprite(Color main)
    {
        Texture2D texture = Texture(12, 16);
        Color outline = new Color32(23, 27, 42, 255);
        Color highlight = Color.Lerp(main, Color.white, 0.45f);

        for (int y = 2; y < 15; y++)
        {
            for (int x = 1; x < 11; x++)
            {
                float nx = (x - 5.5f) / 4.5f;
                float ny = (y - 8.5f) / 6.2f;
                float ellipse = nx * nx + ny * ny;
                if (ellipse <= 1f)
                {
                    texture.SetPixel(x, y, ellipse > 0.72f ? outline : main);
                }
            }
        }

        Fill(texture, 3, 4, 2, 4, highlight);
        Fill(texture, 5, 0, 2, 2, outline);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 12, 16), new Vector2(0.5f, 0.12f), 16f);
    }

    private static Texture2D Texture(int width, int height)
    {
        Texture2D texture = new(width, height, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }

        texture.SetPixels(pixels);
        return texture;
    }

    private static void Fill(Texture2D texture, int x, int y, int width, int height, Color color)
    {
        for (int py = y; py < y + height; py++)
        {
            for (int px = x; px < x + width; px++)
            {
                if (px >= 0 && px < texture.width && py >= 0 && py < texture.height)
                {
                    texture.SetPixel(px, py, color);
                }
            }
        }
    }
}
