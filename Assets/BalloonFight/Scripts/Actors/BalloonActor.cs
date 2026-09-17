using System.Collections.Generic;
using UnityEngine;

internal abstract class BalloonActor : MonoBehaviour
{
    private readonly List<BalloonTarget> _balloons = new();
    private float _invincibleUntil;
    private int _balloonCount;
    private bool _isDead;
    private BalloonFightRuntime _game;
    private BalloonGameConfig _config;
    private Rigidbody2D _body;
    private Collider2D _bodyCollider;

    protected BalloonFightRuntime Game => _game;
    protected BalloonGameConfig Config => _config;
    protected Rigidbody2D Body => _body;
    protected Collider2D BodyCollider => _bodyCollider;
    protected int BalloonCount => _balloonCount;
    protected bool IsDead => _isDead;

    internal virtual void Initialize(BalloonFightRuntime game, BalloonGameConfig config, int balloonCount)
    {
        StopAllCoroutines();
        _game = game;
        _config = config;
        _body = GetComponent<Rigidbody2D>();
        _bodyCollider = GetComponent<Collider2D>();
        GetComponent<BalloonBody>().SetOwner(this);
        CapsuleCollider2D capsule = _bodyCollider as CapsuleCollider2D;
        if (capsule != null)
        {
            capsule.size = config.BodySize;
            capsule.offset = config.BodyOffset;
        }
        _isDead = false;
        _invincibleUntil = 0f;
        _balloons.Clear();
        _balloons.AddRange(GetComponentsInChildren<BalloonTarget>(true));
        _balloonCount = Mathf.Min(balloonCount, _balloons.Count);

        _bodyCollider.enabled = true;
        _body.freezeRotation = true;
        _body.angularVelocity = 0f;
        _body.linearVelocity = Vector2.zero;
        _body.rotation = 0f;
        transform.rotation = Quaternion.identity;

        for (int index = 0; index < _balloons.Count; index++)
        {
            _balloons[index].SetOwner(this);
            _balloons[index].gameObject.SetActive(index < _balloonCount);
        }
    }

    internal void PopBalloon(BalloonTarget target)
    {
        if (_isDead || target == null || Time.time < _invincibleUntil || !target.gameObject.activeSelf)
        {
            return;
        }

        Game.BalloonPopped(target.transform.position);
        target.gameObject.SetActive(false);
        _balloonCount = Mathf.Max(0, _balloonCount - 1);
        _invincibleUntil = Time.time + GetHitProtection();
        OnBalloonLost();
    }

    internal void SetInvincible(float duration)
    {
        _invincibleUntil = Mathf.Max(_invincibleUntil, Time.time + duration);
    }

    internal bool CanReceiveHitFrom(BalloonActor attacker)
    {
        return Config.FriendlyFire
            || this is not BalloonPlayer
            || attacker is not BalloonPlayer;
    }

    protected void MarkDead()
    {
        _isDead = true;
    }

    protected void ClampVelocity(Vector3 limits)
    {
        Vector2 velocity = Body.linearVelocity;
        velocity.x = Mathf.Clamp(velocity.x, -limits.x, limits.x);
        velocity.y = Mathf.Clamp(velocity.y, -limits.z, limits.y);
        Body.linearVelocity = velocity;
    }

    protected abstract void OnBalloonLost();
    protected abstract float GetHitProtection();
}
