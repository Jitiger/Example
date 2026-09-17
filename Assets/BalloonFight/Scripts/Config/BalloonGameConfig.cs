using BalloonFight.Actors;
using UnityEngine;

namespace BalloonFight.Config
{
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

    [CreateAssetMenu(fileName = "BalloonGameConfig", menuName = "Balloon Fight/Game Config")]
    public sealed class BalloonGameConfig : ScriptableObject
    {
        [Header("Game")]
        [SerializeField] private int _targetFrameRate = 60;
        [SerializeField] private int _startingLives = 3;
        [SerializeField] private int _maximumPhase = 3;
        [SerializeField] private int _enemyScore = 500;
        [SerializeField] private float _phaseDelay = 1.2f;
        [SerializeField] private float _respawnDelay = 1.1f;
        [SerializeField] private float _respawnInvincibility = 1.4f;
        [SerializeField] private bool _friendlyFire;

        [Header("World")]
        [SerializeField] private Vector2 _bodySize = new(0.58f, 0.78f);
        [SerializeField] private Vector2 _bodyOffset = new(0f, -0.05f);
        [SerializeField] private float _initialDamping = 0.1f;
        [SerializeField] private int _phaseEnemyOffset = 2;
        [SerializeField] private float _cameraSize = 5.5f;
        [SerializeField] private float _cameraDepth = -10f;
        [SerializeField] private float _topLimit = 4.8f;
        [SerializeField] private float _wrapPadding = 0.45f;
        [SerializeField] private float _ceilingBounce = -0.25f;
        [SerializeField] private Vector2[] _playerSpawns =
        {
            new(-1f, -3.35f),
            new(1f, -3.35f)
        };
        [SerializeField] private Vector2[] _enemySpawns =
        {
            new(-4.7f, 0.2f),
            new(4.7f, 0.45f),
            new(-1.8f, 2.7f),
            new(1.9f, 3f),
            new(0f, 0.65f)
        };
        [SerializeField] private PlatformDefinition[] _platforms =
        {
            new(new Vector2(0f, -4.55f), new Vector2(14.6f, 0.45f)),
            new(new Vector2(-4.65f, -2.45f), new Vector2(2.7f, 0.32f)),
            new(new Vector2(0f, -1.25f), new Vector2(2.9f, 0.32f)),
            new(new Vector2(4.55f, -2.25f), new Vector2(2.7f, 0.32f)),
            new(new Vector2(-2.8f, 1.1f), new Vector2(2.2f, 0.32f)),
            new(new Vector2(2.8f, 1.35f), new Vector2(2.3f, 0.32f))
        };

        [Header("Player")]
        [SerializeField] private int _playerBalloonCount = 2;
        [SerializeField] private float _playerGravity = 0.78f;
        [SerializeField] private float _normalFlapForce = 4.7f;
        [SerializeField] private float _damagedFlapForce = 3.7f;
        [SerializeField] private float _groundAcceleration = 24f;
        [SerializeField] private float _airAcceleration = 9f;
        [SerializeField] private float _groundDamping = 2.2f;
        [SerializeField] private float _airDamping = 0.12f;
        [SerializeField] private Vector3 _playerVelocityLimits = new(5.8f, 6.4f, 6.2f);
        [SerializeField] private float _playerHitProtection = 0.65f;
        [SerializeField] private float _playerDeathDelay = 0.9f;
        [SerializeField] private float _groundNormalThreshold = 0.45f;
        [SerializeField] private float _playerBalloonLossImpulse = 0.9f;
        [SerializeField] private float _playerDeathGravity = 2.1f;
        [SerializeField] private float _playerDeathSpin = 220f;
        [SerializeField] private float _visualTiltMultiplier = 2.5f;
        [SerializeField] private float _maximumVisualTilt = 14f;

        [Header("Enemy")]
        [SerializeField] private int _enemyBalloonCount = 1;
        [SerializeField] private float _enemyGravity = 0.63f;
        [SerializeField] private float _enemyFlapForce = 3.45f;
        [SerializeField] private float _enemyAcceleration = 5.2f;
        [SerializeField] private float _enemyHitProtection = 0.2f;
        [SerializeField] private Vector3 _enemyVelocityLimits = new(4.2f, 5.1f, 5.8f);
        [SerializeField] private Vector2 _enemyFlapInterval = new(0.55f, 0.95f);
        [SerializeField] private Vector2 _enemyDecisionInterval = new(0.65f, 1.35f);
        [SerializeField, Range(0f, 1f)] private float _enemyTrackingChance = 0.72f;
        [SerializeField, Range(0f, 1f)] private float _enemyRandomFlapChance = 0.22f;
        [SerializeField] private float _enemyDeathDelay = 1.4f;
        [SerializeField] private int _enemyPoolCapacity = 5;
        [SerializeField] private float _enemyDeathGravity = 1.8f;
        [SerializeField] private float _enemyDeathSpin = 220f;
        [SerializeField] private float _enemyDeathImpulse = 1.2f;
        [SerializeField] private float _enemyTrackingDeadZone = 0.4f;
        [SerializeField] private float _enemyFlapHeightThreshold = -0.35f;

        internal int TargetFrameRate => _targetFrameRate;
        internal Vector2 BodySize => _bodySize;
        internal Vector2 BodyOffset => _bodyOffset;
        internal float InitialDamping => _initialDamping;
        internal int PhaseEnemyOffset => _phaseEnemyOffset;
        internal int StartingLives => _startingLives;
        internal int MaximumPhase => _maximumPhase;
        internal int EnemyScore => _enemyScore;
        internal float PhaseDelay => _phaseDelay;
        internal float RespawnDelay => _respawnDelay;
        internal float RespawnInvincibility => _respawnInvincibility;
        internal bool FriendlyFire => _friendlyFire;
        internal float CameraSize => _cameraSize;
        internal float CameraDepth => _cameraDepth;
        internal float TopLimit => _topLimit;
        internal float WrapPadding => _wrapPadding;
        internal float CeilingBounce => _ceilingBounce;
        internal Vector2[] EnemySpawns => _enemySpawns;
        internal PlatformDefinition[] Platforms => _platforms;
        internal int PlayerBalloonCount => _playerBalloonCount;
        internal float PlayerGravity => _playerGravity;
        internal float NormalFlapForce => _normalFlapForce;
        internal float DamagedFlapForce => _damagedFlapForce;
        internal float GroundAcceleration => _groundAcceleration;
        internal float AirAcceleration => _airAcceleration;
        internal float GroundDamping => _groundDamping;
        internal float AirDamping => _airDamping;
        internal Vector3 PlayerVelocityLimits => _playerVelocityLimits;
        internal float PlayerHitProtection => _playerHitProtection;
        internal float PlayerDeathDelay => _playerDeathDelay;
        internal float GroundNormalThreshold => _groundNormalThreshold;
        internal float PlayerBalloonLossImpulse => _playerBalloonLossImpulse;
        internal float PlayerDeathGravity => _playerDeathGravity;
        internal float PlayerDeathSpin => _playerDeathSpin;
        internal float VisualTiltMultiplier => _visualTiltMultiplier;
        internal float MaximumVisualTilt => _maximumVisualTilt;
        internal int EnemyBalloonCount => _enemyBalloonCount;
        internal float EnemyGravity => _enemyGravity;
        internal float EnemyFlapForce => _enemyFlapForce;
        internal float EnemyAcceleration => _enemyAcceleration;
        internal float EnemyHitProtection => _enemyHitProtection;
        internal Vector3 EnemyVelocityLimits => _enemyVelocityLimits;
        internal Vector2 EnemyFlapInterval => _enemyFlapInterval;
        internal Vector2 EnemyDecisionInterval => _enemyDecisionInterval;
        internal float EnemyTrackingChance => _enemyTrackingChance;
        internal float EnemyRandomFlapChance => _enemyRandomFlapChance;
        internal float EnemyDeathDelay => _enemyDeathDelay;
        internal int EnemyPoolCapacity => _enemyPoolCapacity;
        internal float EnemyDeathGravity => _enemyDeathGravity;
        internal float EnemyDeathSpin => _enemyDeathSpin;
        internal float EnemyDeathImpulse => _enemyDeathImpulse;
        internal float EnemyTrackingDeadZone => _enemyTrackingDeadZone;
        internal float EnemyFlapHeightThreshold => _enemyFlapHeightThreshold;

        internal Vector2 GetPlayerSpawn(PlayerNumber player)
        {
            int index = (int)player;
            return _playerSpawns != null && index < _playerSpawns.Length
                ? _playerSpawns[index]
                : Vector2.zero;
        }

        private void OnValidate()
        {
            _startingLives = Mathf.Max(1, _startingLives);
            _maximumPhase = Mathf.Max(1, _maximumPhase);
            _enemyPoolCapacity = Mathf.Max(1, _enemyPoolCapacity);
            if (_playerSpawns == null || _playerSpawns.Length < PlayerRoster.Count)
            {
                _playerSpawns = new[] { new Vector2(-1f, -3.35f), new Vector2(1f, -3.35f) };
            }
            _playerBalloonCount = Mathf.Clamp(_playerBalloonCount, 1, 2);
            _enemyBalloonCount = Mathf.Clamp(_enemyBalloonCount, 1, 2);
            _cameraSize = Mathf.Max(0.01f, _cameraSize);
            _phaseDelay = Mathf.Max(0f, _phaseDelay);
            _respawnDelay = Mathf.Max(_playerDeathDelay, _respawnDelay);
            _enemyFlapInterval.x = Mathf.Max(0.01f, _enemyFlapInterval.x);
            _enemyFlapInterval.y = Mathf.Max(_enemyFlapInterval.x, _enemyFlapInterval.y);
            _enemyDecisionInterval.x = Mathf.Max(0.01f, _enemyDecisionInterval.x);
            _enemyDecisionInterval.y = Mathf.Max(_enemyDecisionInterval.x, _enemyDecisionInterval.y);
        }
    }
}
