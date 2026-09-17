using System.Collections;
using UnityEngine;

internal sealed class BalloonEnemy : BalloonActor, IPoolable
{
    private BalloonEnemyPool _pool;
    private float _moveDirection;
    private float _nextDecisionTime;
    private float _nextFlapTime;

    internal void ConfigurePool(BalloonEnemyPool pool)
    {
        _pool = pool;
    }

    internal override void Initialize(BalloonFightRuntime game, BalloonGameConfig config, int balloonCount)
    {
        base.Initialize(game, config, balloonCount);
        Body.gravityScale = config.EnemyGravity;
        Body.linearDamping = config.AirDamping;
        _moveDirection = 1f;
        _nextFlapTime = 0f;
        DecideDirection();
    }

    public void OnSpawned()
    {
        gameObject.SetActive(true);
    }

    public void OnDespawned()
    {
        StopAllCoroutines();
        Rigidbody2D body = GetComponent<Rigidbody2D>();
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
        gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (IsDead || Game == null || !Game.IsPlaying)
        {
            return;
        }

        if (Time.time >= _nextDecisionTime)
        {
            DecideDirection();
        }

        TryFlap();
        Body.AddForce(Vector2.right * _moveDirection * Config.EnemyAcceleration);
        ClampVelocity(Config.EnemyVelocityLimits);
        Game.ClampVertical(transform, Body);
        Game.Wrap(transform);
    }

    protected override void OnBalloonLost()
    {
        if (BalloonCount > 0)
        {
            return;
        }

        MarkDead();
        BodyCollider.enabled = false;
        Body.gravityScale = Config.EnemyDeathGravity;
        Body.freezeRotation = false;
        Body.angularVelocity = Random.Range(-Config.EnemyDeathSpin, Config.EnemyDeathSpin);
        Body.AddForce(Vector2.up * Config.EnemyDeathImpulse, ForceMode2D.Impulse);
        Game.EnemyDefeated(this);
        StartCoroutine(ReturnAfterDelay());
    }

    protected override float GetHitProtection()
    {
        return Config.EnemyHitProtection;
    }

    private void TryFlap()
    {
        BalloonPlayer player = Game.Player;
        if (player == null || Time.time < _nextFlapTime)
        {
            return;
        }

        float playerDeltaY = player.transform.position.y - transform.position.y;
        if (playerDeltaY > Config.EnemyFlapHeightThreshold || Random.value < Config.EnemyRandomFlapChance)
        {
            Body.AddForce(Vector2.up * Config.EnemyFlapForce, ForceMode2D.Impulse);
        }

        _nextFlapTime = Time.time + Random.Range(Config.EnemyFlapInterval.x, Config.EnemyFlapInterval.y);
    }

    private void DecideDirection()
    {
        BalloonPlayer player = Game == null ? null : Game.Player;
        if (player != null && Random.value < Config.EnemyTrackingChance)
        {
            float delta = player.transform.position.x - transform.position.x;
            _moveDirection = Mathf.Abs(delta) < Config.EnemyTrackingDeadZone ? _moveDirection : Mathf.Sign(delta);
        }
        else
        {
            _moveDirection = Random.value > 0.5f ? 1f : -1f;
        }

        _nextDecisionTime = Time.time + Random.Range(Config.EnemyDecisionInterval.x, Config.EnemyDecisionInterval.y);
    }

    private IEnumerator ReturnAfterDelay()
    {
        yield return new WaitForSeconds(Config.EnemyDeathDelay);
        _pool.Release(this);
    }
}
