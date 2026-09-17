using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class BalloonFightPrototype : MonoBehaviour
{
    public const float BottomY = -4.55f;
    public const float TopY = 4.85f;

    private Camera _camera;
    private BalloonPlayer _player;
    private readonly List<BalloonEnemy> _enemies = new List<BalloonEnemy>();

    private int _score;
    private int _lives;
    private int _phase;
    private bool _gameOver;
    private bool _cleared;
    private bool _transitioning;
    private bool _resetting;

    public BalloonPlayer Player => _player;
    public bool IsPlaying => !_gameOver && !_cleared;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (FindFirstObjectByType<BalloonFightPrototype>() != null)
        {
            return;
        }

        GameObject root = new GameObject("Balloon Fight Prototype");
        root.AddComponent<BalloonFightPrototype>();
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        Physics2D.gravity = new Vector2(0f, -9.81f);

        SetupCamera();
        CreateBackdrop();
        CreatePlatforms();
        ResetGame();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if ((_gameOver || _cleared) && keyboard.rKey.wasPressedThisFrame && !_resetting)
        {
            StartCoroutine(RestartRoutine());
        }
    }

    private void SetupCamera()
    {
        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera sceneCamera in cameras)
        {
            sceneCamera.enabled = false;
        }

        GameObject cameraObject = new GameObject("BalloonFight Camera");
        cameraObject.transform.SetParent(transform);
        cameraObject.tag = "MainCamera";

        _camera = cameraObject.AddComponent<Camera>();
        _camera.orthographic = true;
        _camera.orthographicSize = 5.5f;
        _camera.transform.position = new Vector3(0f, 0f, -10f);
        _camera.clearFlags = CameraClearFlags.SolidColor;
        _camera.backgroundColor = new Color32(9, 20, 46, 255);
    }

    private void CreateBackdrop()
    {
        GameObject backdrop = new GameObject("Backdrop");
        backdrop.transform.SetParent(transform);

        System.Random random = new System.Random(1984);
        for (int i = 0; i < 52; i++)
        {
            float x = (float)(random.NextDouble() * 14.0 - 7.0);
            float y = (float)(random.NextDouble() * 9.0 - 4.0);
            float size = i % 5 == 0 ? 0.09f : 0.045f;

            RetroArt.CreateBlock(
                "Star",
                backdrop.transform,
                new Vector3(x, y, 3f),
                new Vector2(size, size),
                i % 4 == 0 ? new Color32(255, 228, 138, 255) : new Color32(202, 225, 255, 255),
                -20);
        }

        CreateCloud(backdrop.transform, new Vector2(-4.9f, 3.45f), 0.85f);
        CreateCloud(backdrop.transform, new Vector2(4.4f, 2.7f), 0.65f);

        RetroArt.CreateBlock(
            "Water",
            backdrop.transform,
            new Vector3(0f, -5.05f, 1f),
            new Vector2(16f, 1.1f),
            new Color32(20, 75, 140, 255),
            -15);

        for (int i = 0; i < 8; i++)
        {
            RetroArt.CreateBlock(
                "Wave",
                backdrop.transform,
                new Vector3(-6.5f + i * 1.85f, -4.68f, 0.9f),
                new Vector2(0.7f, 0.08f),
                new Color32(107, 197, 255, 255),
                -14);
        }
    }

    private void CreateCloud(Transform parent, Vector2 position, float scale)
    {
        Color cloudColor = new Color32(146, 164, 194, 170);
        RetroArt.CreateBlock("Cloud", parent, new Vector3(position.x, position.y, 2f), new Vector2(1.4f, 0.35f) * scale, cloudColor, -18);
        RetroArt.CreateBlock("Cloud", parent, new Vector3(position.x - 0.4f * scale, position.y + 0.2f * scale, 2f), new Vector2(0.55f, 0.45f) * scale, cloudColor, -18);
        RetroArt.CreateBlock("Cloud", parent, new Vector3(position.x + 0.25f * scale, position.y + 0.24f * scale, 2f), new Vector2(0.7f, 0.55f) * scale, cloudColor, -18);
    }

    private void CreatePlatforms()
    {
        GameObject platforms = new GameObject("Platforms");
        platforms.transform.SetParent(transform);

        CreatePlatform(platforms.transform, new Vector2(0f, BottomY), new Vector2(14.5f, 0.45f));
        CreatePlatform(platforms.transform, new Vector2(-4.55f, -2.5f), new Vector2(2.75f, 0.34f));
        CreatePlatform(platforms.transform, new Vector2(0f, -1.35f), new Vector2(2.8f, 0.34f));
        CreatePlatform(platforms.transform, new Vector2(4.55f, -2.35f), new Vector2(2.65f, 0.34f));
        CreatePlatform(platforms.transform, new Vector2(-2.9f, 1.05f), new Vector2(2.15f, 0.34f));
        CreatePlatform(platforms.transform, new Vector2(2.75f, 1.35f), new Vector2(2.35f, 0.34f));
    }

    private void CreatePlatform(Transform parent, Vector2 position, Vector2 size)
    {
        GameObject platform = RetroArt.CreateBlock(
            "Platform",
            parent,
            new Vector3(position.x, position.y, 0f),
            size,
            new Color32(75, 144, 93, 255),
            -2);

        BoxCollider2D collider = platform.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;

        GameObject top = RetroArt.CreateBlock(
            "Platform Top",
            platform.transform,
            new Vector3(0f, 0.42f, -0.1f),
            new Vector2(1f, 0.18f),
            new Color32(156, 211, 120, 255),
            -1);
        top.transform.localPosition = new Vector3(0f, 0.42f, -0.1f);
    }

    private void ResetGame()
    {
        _score = 0;
        _lives = 3;
        _phase = 1;
        _gameOver = false;
        _cleared = false;
        _transitioning = false;

        SpawnPlayer();
        SpawnWave();
    }

    private IEnumerator RestartRoutine()
    {
        _resetting = true;

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

        ResetGame();
        _resetting = false;
    }

    private void SpawnPlayer()
    {
        GameObject playerObject = new GameObject("Player");
        playerObject.transform.SetParent(transform);
        playerObject.transform.position = new Vector3(0f, -3.45f, 0f);

        Rigidbody2D rb = playerObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0.78f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.linearDamping = 0.15f;

        CapsuleCollider2D bodyCollider = playerObject.AddComponent<CapsuleCollider2D>();
        bodyCollider.size = new Vector2(0.58f, 0.78f);
        bodyCollider.offset = new Vector2(0f, -0.02f);

        BalloonBodyHitbox bodyHitbox = playerObject.AddComponent<BalloonBodyHitbox>();

        RetroArt.CreateFighterVisual(playerObject.transform, true, 2);

        _player = playerObject.AddComponent<BalloonPlayer>();
        bodyHitbox.Owner = _player;
        _player.Initialize(this, 2, bodyCollider);
    }

    private void SpawnWave()
    {
        int enemyCount = Mathf.Min(2 + _phase, 5);
        Vector2[] spawnPoints =
        {
            new Vector2(-4.9f, 0.2f),
            new Vector2(4.7f, 0.45f),
            new Vector2(-1.8f, 2.8f),
            new Vector2(1.7f, 3.1f),
            new Vector2(0f, 0.6f)
        };

        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy(spawnPoints[i], i);
        }
    }

    private void SpawnEnemy(Vector2 position, int index)
    {
        GameObject enemyObject = new GameObject($"Enemy {index + 1}");
        enemyObject.transform.SetParent(transform);
        enemyObject.transform.position = position;

        Rigidbody2D rb = enemyObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0.63f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.linearDamping = 0.1f;

        CapsuleCollider2D bodyCollider = enemyObject.AddComponent<CapsuleCollider2D>();
        bodyCollider.size = new Vector2(0.58f, 0.78f);
        bodyCollider.offset = new Vector2(0f, -0.02f);

        BalloonBodyHitbox bodyHitbox = enemyObject.AddComponent<BalloonBodyHitbox>();

        RetroArt.CreateFighterVisual(enemyObject.transform, false, 1, index);

        BalloonEnemy enemy = enemyObject.AddComponent<BalloonEnemy>();
        bodyHitbox.Owner = enemy;
        enemy.Initialize(this, 1, bodyCollider);
        _enemies.Add(enemy);
    }

    public void EnemyDefeated(BalloonEnemy enemy)
    {
        if (enemy == null)
        {
            return;
        }

        _enemies.Remove(enemy);
        _score += 500;

        if (_enemies.Count == 0 && !_transitioning && !_gameOver && !_cleared)
        {
            if (_phase >= 3)
            {
                _cleared = true;
            }
            else
            {
                StartCoroutine(NextWaveRoutine());
            }
        }
    }

    private IEnumerator NextWaveRoutine()
    {
        _transitioning = true;
        yield return new WaitForSeconds(1.25f);

        if (_gameOver || _cleared)
        {
            _transitioning = false;
            yield break;
        }

        _phase++;
        SpawnWave();
        _transitioning = false;
    }

    public void PlayerDefeated()
    {
        if (_gameOver || _cleared)
        {
            return;
        }

        _lives--;

        if (_lives <= 0)
        {
            _gameOver = true;
            return;
        }

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(1.35f);

        if (!_gameOver && !_cleared)
        {
            SpawnPlayer();
            _player.SetInvincible(1.6f);
        }
    }

    public void AddScore(int amount)
    {
        _score += amount;
    }

    public void Wrap(Transform target)
    {
        if (_camera == null || target == null)
        {
            return;
        }

        float halfWidth = _camera.orthographicSize * _camera.aspect + 0.45f;
        Vector3 position = target.position;

        if (position.x > halfWidth)
        {
            position.x = -halfWidth;
        }
        else if (position.x < -halfWidth)
        {
            position.x = halfWidth;
        }

        target.position = position;
    }

    private void OnGUI()
    {
        GUIStyle hudStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 22,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };

        GUIStyle helpStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            normal = { textColor = new Color(0.8f, 0.88f, 1f) }
        };

        GUI.Label(new Rect(20, 15, 250, 40), $"SCORE {_score:000000}", hudStyle);
        GUI.Label(new Rect(Screen.width - 210, 15, 190, 40), $"LIVES {_lives}", hudStyle);
        GUI.Label(new Rect(Screen.width / 2f - 75f, 15, 160, 40), $"PHASE {_phase}", hudStyle);

        int enemyCount = _enemies.Count;
        GUI.Label(new Rect(20, 48, 250, 30), $"ENEMIES {enemyCount}", helpStyle);
        GUI.Label(new Rect(20, Screen.height - 58, 720, 30), "MOVE: A/D or ←/→    FLAP: SPACE / Z / ↑    Pop the enemy balloons from below.", helpStyle);

        if (_transitioning)
        {
            DrawCenterMessage("PHASE CLEAR!", "Get ready for the next phase");
        }
        else if (_gameOver)
        {
            DrawCenterMessage("GAME OVER", "Press R to restart");
        }
        else if (_cleared)
        {
            DrawCenterMessage("ALL PHASES CLEAR!", "Press R to play again");
        }
    }

    private void DrawCenterMessage(string title, string subtitle)
    {
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 40,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color32(255, 232, 105, 255) }
        };

        GUIStyle subtitleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };

        float width = 430f;
        float height = 145f;
        Rect panel = new Rect(Screen.width / 2f - width / 2f, Screen.height / 2f - height / 2f, width, height);

        GUI.Box(panel, GUIContent.none);
        GUI.Label(new Rect(panel.x, panel.y + 18f, width, 55f), title, titleStyle);
        GUI.Label(new Rect(panel.x, panel.y + 82f, width, 35f), subtitle, subtitleStyle);
    }
}

internal abstract class BalloonFighter : MonoBehaviour
{
    protected BalloonFightPrototype Game;
    protected Rigidbody2D Body;
    protected Collider2D BodyCollider;

    private readonly List<BalloonHitbox> _balloons = new List<BalloonHitbox>();
    private float _invincibleUntil;
    private bool _dead;

    protected int BalloonCount { get; private set; }
    protected bool IsDead => _dead;

    public virtual void Initialize(BalloonFightPrototype game, int balloons, Collider2D bodyCollider)
    {
        Game = game;
        Body = GetComponent<Rigidbody2D>();
        BodyCollider = bodyCollider;

        BalloonHitbox[] balloonHitboxes = GetComponentsInChildren<BalloonHitbox>(true);
        _balloons.Clear();

        foreach (BalloonHitbox hitbox in balloonHitboxes)
        {
            hitbox.Owner = this;
            _balloons.Add(hitbox);
        }

        BalloonCount = Mathf.Min(balloons, _balloons.Count);
        for (int i = 0; i < _balloons.Count; i++)
        {
            _balloons[i].gameObject.SetActive(i < BalloonCount);
        }
    }

    public void TryPopBalloon(BalloonHitbox hitbox, BalloonFighter attacker)
    {
        if (_dead || hitbox == null || Time.time < _invincibleUntil || !hitbox.gameObject.activeSelf)
        {
            return;
        }

        hitbox.gameObject.SetActive(false);
        BalloonCount = Mathf.Max(0, BalloonCount - 1);
        _invincibleUntil = Time.time + GetHitProtectionTime();

        OnBalloonLost(attacker);
    }

    public void SetInvincible(float duration)
    {
        _invincibleUntil = Mathf.Max(_invincibleUntil, Time.time + duration);
    }

    protected void MarkDead()
    {
        _dead = true;
    }

    protected void LimitVelocity(float maxX, float maxUp, float maxDown)
    {
        Vector2 velocity = Body.linearVelocity;
        velocity.x = Mathf.Clamp(velocity.x, -maxX, maxX);
        velocity.y = Mathf.Clamp(velocity.y, -maxDown, maxUp);
        Body.linearVelocity = velocity;
    }

    protected void KeepBelowCeiling()
    {
        if (transform.position.y <= BalloonFightPrototype.TopY)
        {
            return;
        }

        Vector3 position = transform.position;
        position.y = BalloonFightPrototype.TopY;
        transform.position = position;

        Vector2 velocity = Body.linearVelocity;
        if (velocity.y > 0f)
        {
            velocity.y *= -0.25f;
            Body.linearVelocity = velocity;
        }
    }

    protected virtual float GetHitProtectionTime()
    {
        return 0.2f;
    }

    protected abstract void OnBalloonLost(BalloonFighter attacker);
}

internal sealed class BalloonPlayer : BalloonFighter
{
    private const float GroundAcceleration = 24f;
    private const float AirAcceleration = 9f;
    private const float MaxHorizontalSpeed = 5.8f;
    private const float FlapImpulse = 4.65f;

    private bool _grounded;
    private Transform _visual;

    public override void Initialize(BalloonFightPrototype game, int balloons, Collider2D bodyCollider)
    {
        base.Initialize(game, balloons, bodyCollider);
        _visual = transform.Find("Visual");
    }

    private void Update()
    {
        if (IsDead || Game == null || !Game.IsPlaying)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        bool flapPressed =
            keyboard.spaceKey.wasPressedThisFrame ||
            keyboard.zKey.wasPressedThisFrame ||
            keyboard.upArrowKey.wasPressedThisFrame;

        if (flapPressed)
        {
            float balloonPower = BalloonCount >= 2 ? 1f : 0.78f;
            Body.AddForce(Vector2.up * FlapImpulse * balloonPower, ForceMode2D.Impulse);
            _grounded = false;
        }
    }

    private void FixedUpdate()
    {
        if (IsDead || Game == null || !Game.IsPlaying)
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

        float acceleration = _grounded ? GroundAcceleration : AirAcceleration;
        Body.AddForce(Vector2.right * horizontal * acceleration, ForceMode2D.Force);
        Body.linearDamping = _grounded ? 2.2f : 0.12f;

        LimitVelocity(MaxHorizontalSpeed, 6.4f, 6.2f);
        KeepBelowCeiling();
        Game.Wrap(transform);

        if (_visual != null)
        {
            float lean = Mathf.Clamp(-Body.linearVelocity.x * 2.8f, -14f, 14f);
            _visual.localRotation = Quaternion.Euler(0f, 0f, lean);

            if (Mathf.Abs(horizontal) > 0.01f)
            {
                Vector3 scale = _visual.localScale;
                scale.x = Mathf.Abs(scale.x) * Mathf.Sign(horizontal);
                _visual.localScale = scale;
            }
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        BalloonBodyHitbox otherBody = collision.collider.GetComponent<BalloonBodyHitbox>();
        if (otherBody == null || otherBody.Owner == null || otherBody.Owner == this)
        {
            return;
        }

        float direction = Mathf.Sign(transform.position.x - otherBody.transform.position.x);
        if (Mathf.Approximately(direction, 0f))
        {
            direction = UnityEngine.Random.value > 0.5f ? 1f : -1f;
        }

        Body.AddForce(new Vector2(direction * 1.7f, 1.1f), ForceMode2D.Impulse);
    }

    protected override float GetHitProtectionTime()
    {
        return 0.65f;
    }

    protected override void OnBalloonLost(BalloonFighter attacker)
    {
        Body.AddForce(Vector2.down * 0.9f, ForceMode2D.Impulse);

        if (BalloonCount > 0)
        {
            return;
        }

        MarkDead();
        Body.gravityScale = 2.1f;
        Body.freezeRotation = false;
        Body.angularVelocity = 220f;

        if (BodyCollider != null)
        {
            BodyCollider.enabled = false;
        }

        Game.PlayerDefeated();
        Destroy(gameObject, 0.9f);
    }
}

internal sealed class BalloonEnemy : BalloonFighter
{
    private float _nextDecisionTime;
    private float _moveDirection;
    private float _nextFlapTime;
    private Transform _visual;

    public override void Initialize(BalloonFightPrototype game, int balloons, Collider2D bodyCollider)
    {
        base.Initialize(game, balloons, bodyCollider);
        _visual = transform.Find("Visual");
        ChooseDecision();
    }

    private void FixedUpdate()
    {
        if (IsDead || Game == null || !Game.IsPlaying)
        {
            if (!IsDead && Game != null)
            {
                Game.Wrap(transform);
            }

            return;
        }

        if (Time.time >= _nextDecisionTime)
        {
            ChooseDecision();
        }

        BalloonPlayer player = Game.Player;
        if (player != null && Time.time >= _nextFlapTime)
        {
            float heightDifference = player.transform.position.y - transform.position.y;
            bool shouldFlap = heightDifference > -0.3f || UnityEngine.Random.value < 0.23f;

            if (shouldFlap && Body.linearVelocity.y < 3f)
            {
                Body.AddForce(Vector2.up * 3.45f, ForceMode2D.Impulse);
                _nextFlapTime = Time.time + UnityEngine.Random.Range(0.55f, 0.95f);
            }
        }

        Body.AddForce(Vector2.right * _moveDirection * 5.2f, ForceMode2D.Force);
        Body.linearDamping = 0.08f;

        LimitVelocity(4.2f, 5.1f, 5.8f);
        KeepBelowCeiling();
        Game.Wrap(transform);

        if (_visual != null && Mathf.Abs(_moveDirection) > 0.01f)
        {
            Vector3 scale = _visual.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(_moveDirection);
            _visual.localScale = scale;
        }
    }

    private void ChooseDecision()
    {
        BalloonPlayer player = Game != null ? Game.Player : null;
        float directionToPlayer = 0f;

        if (player != null)
        {
            float delta = player.transform.position.x - transform.position.x;
            directionToPlayer = Mathf.Abs(delta) > 0.4f ? Mathf.Sign(delta) : 0f;
        }

        if (UnityEngine.Random.value < 0.72f && !Mathf.Approximately(directionToPlayer, 0f))
        {
            _moveDirection = directionToPlayer;
        }
        else
        {
            _moveDirection = UnityEngine.Random.value > 0.5f ? 1f : -1f;
        }

        _nextDecisionTime = Time.time + UnityEngine.Random.Range(0.65f, 1.35f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsDead)
        {
            return;
        }

        BalloonBodyHitbox otherBody = collision.collider.GetComponent<BalloonBodyHitbox>();
        if (otherBody == null || otherBody.Owner == null || otherBody.Owner == this)
        {
            return;
        }

        float direction = Mathf.Sign(transform.position.x - otherBody.transform.position.x);
        if (Mathf.Approximately(direction, 0f))
        {
            direction = 1f;
        }

        Body.AddForce(new Vector2(direction * 1.3f, 0.8f), ForceMode2D.Impulse);
    }

    protected override void OnBalloonLost(BalloonFighter attacker)
    {
        if (BalloonCount > 0)
        {
            return;
        }

        MarkDead();

        if (BodyCollider != null)
        {
            BodyCollider.enabled = false;
        }

        Body.gravityScale = 1.8f;
        Body.freezeRotation = false;
        Body.angularVelocity = UnityEngine.Random.Range(-240f, 240f);
        Body.AddForce(Vector2.up * 1.2f, ForceMode2D.Impulse);

        Game.EnemyDefeated(this);
        Destroy(gameObject, 1.65f);
    }
}

internal sealed class BalloonHitbox : MonoBehaviour
{
    public BalloonFighter Owner { get; set; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Owner == null)
        {
            return;
        }

        BalloonBodyHitbox attackingBody = other.GetComponent<BalloonBodyHitbox>();
        if (attackingBody == null || attackingBody.Owner == null || attackingBody.Owner == Owner)
        {
            return;
        }

        Owner.TryPopBalloon(this, attackingBody.Owner);
    }
}

internal sealed class BalloonBodyHitbox : MonoBehaviour
{
    public BalloonFighter Owner { get; set; }
}

internal static class RetroArt
{
    private static Sprite _squareSprite;
    private static Sprite _playerSprite;
    private static Sprite _enemySpriteA;
    private static Sprite _enemySpriteB;
    private static Sprite _balloonRed;
    private static Sprite _balloonPink;
    private static Sprite _balloonYellow;

    public static GameObject CreateBlock(
        string name,
        Transform parent,
        Vector3 position,
        Vector2 size,
        Color color,
        int sortingOrder)
    {
        GameObject block = new GameObject(name);
        block.transform.SetParent(parent);
        block.transform.position = position;
        block.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer renderer = block.AddComponent<SpriteRenderer>();
        renderer.sprite = GetSquareSprite();
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;

        return block;
    }

    public static void CreateFighterVisual(Transform root, bool player, int balloons, int variation = 0)
    {
        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(root);
        visual.transform.localPosition = Vector3.zero;

        SpriteRenderer fighterRenderer = visual.AddComponent<SpriteRenderer>();
        fighterRenderer.sprite = player ? GetPlayerSprite() : GetEnemySprite(variation);
        fighterRenderer.sortingOrder = 10;

        for (int i = 0; i < balloons; i++)
        {
            GameObject balloon = new GameObject($"Balloon {i + 1}");
            balloon.transform.SetParent(root);
            float x = balloons == 1 ? 0f : (i == 0 ? -0.27f : 0.27f);
            balloon.transform.localPosition = new Vector3(x, 1.0f, 0f);

            SpriteRenderer balloonRenderer = balloon.AddComponent<SpriteRenderer>();
            balloonRenderer.sprite = player
                ? (i == 0 ? GetBalloonRed() : GetBalloonYellow())
                : GetBalloonPink();
            balloonRenderer.sortingOrder = 9;

            CircleCollider2D balloonCollider = balloon.AddComponent<CircleCollider2D>();
            balloonCollider.isTrigger = true;
            balloonCollider.radius = 0.35f;
            balloonCollider.offset = new Vector2(0f, 0.3f);

            balloon.AddComponent<BalloonHitbox>();

            GameObject rope = CreateBlock(
                "Rope",
                root,
                Vector3.zero,
                new Vector2(0.035f, 0.78f),
                new Color32(230, 226, 193, 255),
                8);

            rope.transform.localPosition = new Vector3(x * 0.55f, 0.55f, 0.1f);
            rope.transform.localRotation = Quaternion.Euler(0f, 0f, -x * 32f);
        }
    }

    private static Sprite GetSquareSprite()
    {
        if (_squareSprite != null)
        {
            return _squareSprite;
        }

        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.name = "Runtime Square";
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        _squareSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        _squareSprite.name = "Runtime Square Sprite";
        return _squareSprite;
    }

    private static Sprite GetPlayerSprite()
    {
        if (_playerSprite == null)
        {
            _playerSprite = CreateFighterSprite(
                new Color32(42, 117, 215, 255),
                new Color32(231, 57, 54, 255),
                new Color32(249, 207, 153, 255));
        }

        return _playerSprite;
    }

    private static Sprite GetEnemySprite(int variation)
    {
        if (variation % 2 == 0)
        {
            if (_enemySpriteA == null)
            {
                _enemySpriteA = CreateFighterSprite(
                    new Color32(134, 68, 179, 255),
                    new Color32(240, 188, 65, 255),
                    new Color32(166, 211, 102, 255));
            }

            return _enemySpriteA;
        }

        if (_enemySpriteB == null)
        {
            _enemySpriteB = CreateFighterSprite(
                new Color32(63, 153, 129, 255),
                new Color32(215, 75, 126, 255),
                new Color32(248, 194, 137, 255));
        }

        return _enemySpriteB;
    }

    private static Sprite GetBalloonRed()
    {
        if (_balloonRed == null)
        {
            _balloonRed = CreateBalloonSprite(new Color32(237, 55, 72, 255));
        }

        return _balloonRed;
    }

    private static Sprite GetBalloonPink()
    {
        if (_balloonPink == null)
        {
            _balloonPink = CreateBalloonSprite(new Color32(214, 82, 177, 255));
        }

        return _balloonPink;
    }

    private static Sprite GetBalloonYellow()
    {
        if (_balloonYellow == null)
        {
            _balloonYellow = CreateBalloonSprite(new Color32(250, 190, 54, 255));
        }

        return _balloonYellow;
    }

    private static Sprite CreateFighterSprite(Color body, Color helmet, Color skin)
    {
        const int size = 16;
        Texture2D texture = CreateTexture(size, size, "Runtime Fighter");

        Color outline = new Color32(19, 25, 39, 255);
        Color white = new Color32(245, 245, 230, 255);

        FillRect(texture, 5, 11, 6, 3, helmet);
        FillRect(texture, 4, 10, 8, 2, outline);
        FillRect(texture, 5, 9, 6, 2, skin);
        FillRect(texture, 6, 9, 1, 1, outline);
        FillRect(texture, 9, 9, 1, 1, outline);
        FillRect(texture, 6, 8, 4, 1, skin);

        FillRect(texture, 5, 4, 6, 5, body);
        FillRect(texture, 3, 5, 2, 3, white);
        FillRect(texture, 11, 5, 2, 3, white);
        FillRect(texture, 4, 2, 3, 2, outline);
        FillRect(texture, 9, 2, 3, 2, outline);
        FillRect(texture, 6, 4, 4, 1, white);

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.42f), 16f);
    }

    private static Sprite CreateBalloonSprite(Color mainColor)
    {
        const int width = 12;
        const int height = 16;
        Texture2D texture = CreateTexture(width, height, "Runtime Balloon");

        Color outline = new Color32(23, 27, 42, 255);
        Color highlight = Color.Lerp(mainColor, Color.white, 0.45f);

        for (int y = 2; y < 15; y++)
        {
            for (int x = 1; x < 11; x++)
            {
                float nx = (x - 5.5f) / 4.5f;
                float ny = (y - 8.5f) / 6.2f;
                float ellipse = nx * nx + ny * ny;

                if (ellipse <= 1f)
                {
                    bool edge = ellipse > 0.72f;
                    texture.SetPixel(x, y, edge ? outline : mainColor);
                }
            }
        }

        FillRect(texture, 3, 3, 2, 4, highlight);
        FillRect(texture, 5, 0, 2, 2, outline);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.12f), 16f);
    }

    private static Texture2D CreateTexture(int width, int height, string name)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.name = name;
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }

        texture.SetPixels(pixels);
        return texture;
    }

    private static void FillRect(Texture2D texture, int x, int y, int width, int height, Color color)
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
