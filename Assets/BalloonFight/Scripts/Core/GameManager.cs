using System.Collections;
using System.Collections.Generic;
using BalloonFight.Actors;
using BalloonFight.Config;
using BalloonFight.Feedback;
using BalloonFight.Input;
using BalloonFight.Pooling;
using BalloonFight.UI;
using BalloonFight.Visual;
using UnityEngine;

internal sealed class GameManager : MonoBehaviour
{
    private const string ConfigResourcePath = "BalloonGameConfig";

    [SerializeField] private BalloonGameConfig _config;
    [SerializeField] private BalloonPrefabConfig _prefabs;

    private readonly List<ScriptableObject> _ownedSettings = new();
    private BalloonFeedbackConfig _feedback;
    private GameStateManager _gameStateManager;
    private BalloonHud _hud;
    private BalloonInputConfig _inputConfig;
    private PlayerInput _input;
    private BalloonPopPool _popPool;
    private FighterSpawner _spawner;
    private BalloonUiConfig _ui;
    private BalloonVisualConfig _visual;
    private MapBoundary _mapBoundary;

    internal PlayerInput Input => _input;
    internal bool IsPlaying => _gameStateManager.IsPlaying;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (FindFirstObjectByType<GameManager>() == null)
        {
            new GameObject("Game Manager").AddComponent<GameManager>();
        }
    }

    private void Awake()
    {
        _config = LoadSettings(ConfigResourcePath, _config);
        _prefabs = LoadSettings(nameof(BalloonPrefabConfig), _prefabs);
        _inputConfig = LoadSettings<BalloonInputConfig>();
        _visual = LoadSettings<BalloonVisualConfig>();
        _ui = LoadSettings<BalloonUiConfig>();
        _feedback = LoadSettings<BalloonFeedbackConfig>();
        _input = new PlayerInput(_inputConfig);
        _gameStateManager = new GameStateManager(_config);
        RetroFactory.Configure(_visual);
    }

    private void Start()
    {
        Application.targetFrameRate = _config.TargetFrameRate;
        Camera gameCamera = GetOrCreateCamera();
        _mapBoundary = new MapBoundary(gameCamera, _config);
        BuildStage();
        _spawner = new FighterSpawner(transform, this, _config, _prefabs);
        _popPool = new BalloonPopPool(transform, _feedback, RetroFactory.GetSquare());
        _hud = new BalloonHud(_ui, _inputConfig);
        _spawner.SpawnAllPlayers();
        _spawner.SpawnPhase(_gameStateManager.Phase);
    }

    private void Update()
    {
        if (!_gameStateManager.IsPlaying && _input.RestartPressed)
        {
            Restart();
        }
    }

    private void OnGUI()
    {
        _hud?.Draw(
            _gameStateManager.Score,
            _gameStateManager.Phase,
            _gameStateManager.GetLives(PlayerNumber.One),
            _gameStateManager.GetLives(PlayerNumber.Two),
            _spawner.ActiveEnemyCount,
            _gameStateManager.IsChangingPhase,
            _gameStateManager.IsGameOver,
            _gameStateManager.IsAllClear);
    }

    private void OnDestroy()
    {
        _popPool?.Clear();
        _spawner?.Clear();
        foreach (ScriptableObject settings in _ownedSettings)
        {
            Destroy(settings);
        }
    }

    internal void BalloonPopped(Vector3 position)
    {
        _popPool?.Play(position);
    }

    internal PlayerController GetNearestPlayer(Vector3 position)
    {
        return _spawner.GetNearestPlayer(position);
    }

    internal void EnemyDefeated(EnemyController enemy)
    {
        if (!_spawner.RemoveEnemy(enemy))
        {
            return;
        }

        if (_gameStateManager.RegisterEnemyDefeat(_spawner.ActiveEnemyCount))
        {
            StartCoroutine(NextPhase());
        }
    }

    internal void PlayerDefeated(PlayerController player)
    {
        if (player == null || !_gameStateManager.IsPlaying)
        {
            return;
        }

        if (_gameStateManager.RegisterPlayerDefeat(player.PlayerNumber))
        {
            StartCoroutine(RespawnPlayer(player.PlayerNumber));
        }
    }

    internal void Wrap(Transform target)
    {
        _mapBoundary.Wrap(target);
    }

    internal void ClampVertical(Transform target, Rigidbody2D body)
    {
        _mapBoundary.ClampVertical(target, body);
    }

    private T LoadSettings<T>(string resourcePath = null, T assigned = null) where T : ScriptableObject
    {
        T settings = assigned != null ? assigned : Resources.Load<T>(resourcePath ?? typeof(T).Name);
        if (settings != null)
        {
            return settings;
        }

        settings = ScriptableObject.CreateInstance<T>();
        _ownedSettings.Add(settings);
        return settings;
    }

    private Camera GetOrCreateCamera()
    {
        Camera gameCamera = Camera.main;
        if (gameCamera == null)
        {
            GameObject cameraObject = new("Balloon Fight Camera");
            cameraObject.transform.SetParent(transform);
            cameraObject.tag = "MainCamera";
            gameCamera = cameraObject.AddComponent<Camera>();
        }

        gameCamera.orthographic = true;
        gameCamera.orthographicSize = _config.CameraSize;
        gameCamera.transform.position = new Vector3(0f, 0f, _config.CameraDepth);
        gameCamera.clearFlags = CameraClearFlags.SolidColor;
        gameCamera.backgroundColor = _visual.Background;
        return gameCamera;
    }

    private void BuildStage()
    {
        if (_prefabs.Stage != null)
        {
            Instantiate(_prefabs.Stage, transform);
            return;
        }

        BalloonStageBuilder.Build(transform, _config, _visual);
    }

    private IEnumerator NextPhase()
    {
        yield return new WaitForSeconds(_config.PhaseDelay);
        _gameStateManager.AdvancePhase();
        _spawner.SpawnPhase(_gameStateManager.Phase);
        _gameStateManager.CompletePhaseChange();
    }

    private IEnumerator RespawnPlayer(PlayerNumber playerNumber)
    {
        yield return new WaitForSeconds(_config.RespawnDelay);
        if (!_gameStateManager.CanRespawn(playerNumber))
        {
            yield break;
        }

        PlayerController player = _spawner.SpawnPlayer(playerNumber);
        player.SetInvincible(_config.RespawnInvincibility);
    }

    private void Restart()
    {
        StopAllCoroutines();
        _popPool.Reset();
        _spawner.Reset();
        _gameStateManager.Reset();
        _spawner.SpawnAllPlayers();
        _spawner.SpawnPhase(_gameStateManager.Phase);
    }
}
