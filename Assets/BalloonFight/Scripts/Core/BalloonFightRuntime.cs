using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

internal sealed class BalloonFightRuntime : MonoBehaviour
{
    private const string ConfigResourcePath = "BalloonGameConfig";

    [SerializeField] private BalloonGameConfig _config;

    private readonly List<BalloonEnemy> _enemies = new();
    private BalloonEnemyPool _enemyPool;
    private BalloonHud _hud;
    private Camera _camera;
    private BalloonPlayer _player;
    private BalloonInputConfig _input;
    private bool _ownsConfig;
    private bool _ownsInput;
    private readonly List<ScriptableObject> _ownedSettings = new();
    private BalloonVisualConfig _visual;
    private BalloonUiConfig _ui;
    private BalloonFeedbackConfig _feedback;
    private BalloonPopPool _popPool;
    private int _score;
    private int _lives;
    private int _phase;
    private bool _isGameOver;
    private bool _isAllClear;
    private bool _isChangingPhase;

    internal BalloonPlayer Player => _player;
    internal BalloonInputConfig Input => _input;
    internal bool IsPlaying => !_isGameOver && !_isAllClear;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (FindFirstObjectByType<BalloonFightRuntime>() == null)
        {
            new GameObject("Balloon Fight Runtime").AddComponent<BalloonFightRuntime>();
        }
    }

    private void Awake()
    {
        if (_config == null)
        {
            _config = Resources.Load<BalloonGameConfig>(ConfigResourcePath);
        }

        if (_config == null)
        {
            _config = ScriptableObject.CreateInstance<BalloonGameConfig>();
            _ownsConfig = true;
        }

        _input = Resources.Load<BalloonInputConfig>(nameof(BalloonInputConfig));
        if (_input == null)
        {
            _input = ScriptableObject.CreateInstance<BalloonInputConfig>();
            _ownsInput = true;
        }

        _lives = _config.StartingLives;
        _visual = LoadSettings<BalloonVisualConfig>();
        _ui = LoadSettings<BalloonUiConfig>();
        _feedback = LoadSettings<BalloonFeedbackConfig>();
        RetroFactory.Configure(_visual);
        _phase = 1;
    }

    private void Start()
    {
        Application.targetFrameRate = _config.TargetFrameRate;
        BuildCamera();
        BalloonPrefabConfig prefabs = Resources.Load<BalloonPrefabConfig>(nameof(BalloonPrefabConfig));
        if (prefabs != null && prefabs.Stage != null)
        {
            Instantiate(prefabs.Stage, transform);
        }
        else
        {
            BalloonStageBuilder.Build(transform, _config, _visual);
        }
        _enemyPool = new BalloonEnemyPool(transform, _config);
        _popPool = new BalloonPopPool(transform, _feedback, RetroFactory.GetSquare());
        _hud = new BalloonHud(_ui, _input);
        SpawnPlayer();
        SpawnPhase();
    }

    private void Update()
    {
        if ((_isGameOver || _isAllClear) && _input.RestartPressed)
        {
            Restart();
        }
    }

    private void OnGUI()
    {
        _hud?.Draw(_score, _phase, _lives, _enemies.Count, _isChangingPhase, _isGameOver, _isAllClear);
    }

    private void OnDestroy()
    {
        _popPool?.Clear();
        _enemyPool?.Clear();
        foreach (ScriptableObject settings in _ownedSettings)
        {
            Destroy(settings);
        }
        if (_ownsConfig) Destroy(_config);
        if (_ownsInput) Destroy(_input);
    }

    private T LoadSettings<T>() where T : ScriptableObject
    {
        T settings = Resources.Load<T>(typeof(T).Name);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<T>();
            _ownedSettings.Add(settings);
        }
        return settings;
    }

    internal void BalloonPopped(Vector3 position)
    {
        _popPool?.Play(position);
    }

    internal void EnemyDefeated(BalloonEnemy enemy)
    {
        if (enemy == null || !_enemies.Remove(enemy))
        {
            return;
        }

        _score += _config.EnemyScore;
        if (_enemies.Count != 0 || _isChangingPhase || _isGameOver || _isAllClear)
        {
            return;
        }

        if (_phase >= _config.MaximumPhase)
        {
            _isAllClear = true;
            return;
        }

        StartCoroutine(NextPhase());
    }

    internal void PlayerDefeated()
    {
        if (_isGameOver || _isAllClear)
        {
            return;
        }

        _lives--;
        if (_lives <= 0)
        {
            _isGameOver = true;
            return;
        }

        StartCoroutine(RespawnPlayer());
    }

    internal void Wrap(Transform target)
    {
        float edge = _camera.orthographicSize * _camera.aspect + _config.WrapPadding;
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

    internal void ClampVertical(Transform target, Rigidbody2D body)
    {
        if (target.position.y <= _config.TopLimit)
        {
            return;
        }

        Vector3 position = target.position;
        position.y = _config.TopLimit;
        target.position = position;

        Vector2 velocity = body.linearVelocity;
        if (velocity.y > 0f)
        {
            velocity.y *= _config.CeilingBounce;
            body.linearVelocity = velocity;
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
        _camera.orthographicSize = _config.CameraSize;
        _camera.transform.position = new Vector3(0f, 0f, _config.CameraDepth);
        _camera.clearFlags = CameraClearFlags.SolidColor;
        _camera.backgroundColor = _visual.Background;
    }

    private void SpawnPlayer()
    {
        if (_player == null)
        {
            _player = FighterFactory.CreatePlayer(transform, _config);
        }

        _player.gameObject.SetActive(true);
        _player.transform.position = _config.PlayerSpawn;
        _player.Initialize(this, _config, _config.PlayerBalloonCount);
    }

    private void SpawnPhase()
    {
        int enemyCount = Mathf.Min(_phase + _config.PhaseEnemyOffset, _config.EnemySpawns.Length);
        for (int index = 0; index < enemyCount; index++)
        {
            BalloonEnemy enemy = _enemyPool.Get(_config.EnemySpawns[index]);
            enemy.Initialize(this, _config, _config.EnemyBalloonCount);
            _enemies.Add(enemy);
        }
    }

    private IEnumerator NextPhase()
    {
        _isChangingPhase = true;
        yield return new WaitForSeconds(_config.PhaseDelay);
        _phase++;
        SpawnPhase();
        _isChangingPhase = false;
    }

    private IEnumerator RespawnPlayer()
    {
        yield return new WaitForSeconds(_config.RespawnDelay);
        if (!IsPlaying)
        {
            yield break;
        }

        SpawnPlayer();
        _player.SetInvincible(_config.RespawnInvincibility);
    }

    private void Restart()
    {
        StopAllCoroutines();
        _popPool.Reset();
        _player?.gameObject.SetActive(false);
        _enemyPool.ReleaseAll(GetComponentsInChildren<BalloonEnemy>(true));
        _enemies.Clear();

        _score = 0;
        _lives = _config.StartingLives;
        _phase = 1;
        _isGameOver = false;
        _isAllClear = false;
        _isChangingPhase = false;
        SpawnPlayer();
        SpawnPhase();
    }
}
