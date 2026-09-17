using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal sealed class BalloonFightRuntime : MonoBehaviour
{
    private const string ConfigResourcePath = "BalloonGameConfig";
    [SerializeField] private BalloonGameConfig _config;

    private readonly List<BalloonEnemy> _enemies = new();
    private readonly BalloonPlayer[] _players = new BalloonPlayer[PlayerRoster.Count];
    private readonly int[] _lives = new int[PlayerRoster.Count];
    private readonly List<ScriptableObject> _ownedSettings = new();
    private BalloonEnemyPool _enemyPool;
    private BalloonPopPool _popPool;
    private BalloonHud _hud;
    private BalloonInputConfig _input;
    private BalloonVisualConfig _visual;
    private BalloonUiConfig _ui;
    private BalloonFeedbackConfig _feedback;
    private Camera _camera;
    private int _score;
    private int _phase;
    private bool _isGameOver;
    private bool _isAllClear;
    private bool _isChangingPhase;

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
        _config = LoadSettings(ConfigResourcePath, _config);
        _input = LoadSettings<BalloonInputConfig>();
        _visual = LoadSettings<BalloonVisualConfig>();
        _ui = LoadSettings<BalloonUiConfig>();
        _feedback = LoadSettings<BalloonFeedbackConfig>();
        RetroFactory.Configure(_visual);
        ResetGameState();
    }

    private void Start()
    {
        Application.targetFrameRate = _config.TargetFrameRate;
        BuildCamera();
        BuildStage();
        _enemyPool = new BalloonEnemyPool(transform, _config);
        _popPool = new BalloonPopPool(transform, _feedback, RetroFactory.GetSquare());
        _hud = new BalloonHud(_ui, _input);
        SpawnAllPlayers();
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
        _hud?.Draw(
            _score,
            _phase,
            _lives[(int)PlayerNumber.One],
            _lives[(int)PlayerNumber.Two],
            _enemies.Count,
            _isChangingPhase,
            _isGameOver,
            _isAllClear);
    }

    private void OnDestroy()
    {
        _popPool?.Clear();
        _enemyPool?.Clear();
        foreach (ScriptableObject settings in _ownedSettings)
        {
            Destroy(settings);
        }
    }

    internal void BalloonPopped(Vector3 position)
    {
        _popPool?.Play(position);
    }

    internal BalloonPlayer GetNearestPlayer(Vector3 position)
    {
        BalloonPlayer nearest = null;
        float nearestDistance = float.MaxValue;
        foreach (BalloonPlayer player in _players)
        {
            if (player == null || !player.IsAvailable)
            {
                continue;
            }

            float distance = (player.transform.position - position).sqrMagnitude;
            if (distance < nearestDistance)
            {
                nearest = player;
                nearestDistance = distance;
            }
        }

        return nearest;
    }

    internal void EnemyDefeated(BalloonEnemy enemy)
    {
        if (enemy == null || !_enemies.Remove(enemy))
        {
            return;
        }

        _score += _config.EnemyScore;
        if (_enemies.Count != 0 || _isChangingPhase || !IsPlaying)
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

    internal void PlayerDefeated(BalloonPlayer player)
    {
        if (player == null || !IsPlaying)
        {
            return;
        }

        int index = (int)player.PlayerNumber;
        _lives[index] = Mathf.Max(0, _lives[index] - 1);
        if (_lives[index] > 0)
        {
            StartCoroutine(RespawnPlayer(player.PlayerNumber));
            return;
        }

        if (AllPlayersEliminated())
        {
            _isGameOver = true;
        }
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

    private T LoadSettings<T>(string resourcePath = null, T assigned = null) where T : ScriptableObject
    {
        T settings = assigned != null
            ? assigned
            : Resources.Load<T>(resourcePath ?? typeof(T).Name);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<T>();
            _ownedSettings.Add(settings);
        }
        return settings;
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

    private void BuildStage()
    {
        BalloonPrefabConfig prefabs = Resources.Load<BalloonPrefabConfig>(nameof(BalloonPrefabConfig));
        if (prefabs != null && prefabs.Stage != null)
        {
            Instantiate(prefabs.Stage, transform);
            return;
        }
        BalloonStageBuilder.Build(transform, _config, _visual);
    }

    private void SpawnAllPlayers()
    {
        SpawnPlayer(PlayerNumber.One);
        SpawnPlayer(PlayerNumber.Two);
    }

    private void SpawnPlayer(PlayerNumber playerNumber)
    {
        int index = (int)playerNumber;
        if (_players[index] == null)
        {
            _players[index] = FighterFactory.CreatePlayer(transform, _config, playerNumber);
        }

        BalloonPlayer player = _players[index];
        player.gameObject.SetActive(true);
        player.transform.position = _config.GetPlayerSpawn(playerNumber);
        player.InitializePlayer(this, _config, playerNumber);
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

    private IEnumerator RespawnPlayer(PlayerNumber playerNumber)
    {
        yield return new WaitForSeconds(_config.RespawnDelay);
        if (!IsPlaying || _lives[(int)playerNumber] <= 0)
        {
            yield break;
        }

        SpawnPlayer(playerNumber);
        _players[(int)playerNumber].SetInvincible(_config.RespawnInvincibility);
    }

    private bool AllPlayersEliminated()
    {
        foreach (int life in _lives)
        {
            if (life > 0)
            {
                return false;
            }
        }
        return true;
    }

    private void Restart()
    {
        StopAllCoroutines();
        _popPool.Reset();
        foreach (BalloonPlayer player in _players)
        {
            player?.gameObject.SetActive(false);
        }
        _enemyPool.ReleaseAll(GetComponentsInChildren<BalloonEnemy>(true));
        _enemies.Clear();
        ResetGameState();
        SpawnAllPlayers();
        SpawnPhase();
    }

    private void ResetGameState()
    {
        _score = 0;
        _phase = 1;
        _isGameOver = false;
        _isAllClear = false;
        _isChangingPhase = false;
        for (int index = 0; index < _lives.Length; index++)
        {
            _lives[index] = _config.StartingLives;
        }
    }
}
