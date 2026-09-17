using System.Collections;
using BalloonFight.Config;
using BalloonFight.Core;
using UnityEngine;

namespace BalloonFight.Actors
{
    internal sealed class BalloonPlayer : BalloonActor
    {
        private bool _isGrounded;
        private Transform _visual;
        private PlayerNumber _playerNumber;

        internal PlayerNumber PlayerNumber => _playerNumber;
        internal bool IsAvailable => gameObject.activeSelf && !IsDead;

        internal void InitializePlayer(BalloonFightRuntime game, BalloonGameConfig config, PlayerNumber playerNumber)
        {
            _playerNumber = playerNumber;
            Initialize(game, config, config.PlayerBalloonCount);
        }

        internal override void Initialize(BalloonFightRuntime game, BalloonGameConfig config, int balloonCount)
        {
            base.Initialize(game, config, balloonCount);
            _visual = transform.Find("Visual");
            _isGrounded = false;
            Body.gravityScale = config.PlayerGravity;
            Body.linearDamping = config.AirDamping;
        }

        private void Update()
        {
            if (IsDead || Game == null || !Game.IsPlaying)
            {
                return;
            }

            if (!Game.Input.IsFlapPressed(_playerNumber))
            {
                return;
            }

            float flapForce = BalloonCount >= Config.PlayerBalloonCount
                ? Config.NormalFlapForce
                : Config.DamagedFlapForce;

            Body.AddForce(Vector2.up * flapForce, ForceMode2D.Impulse);
            _isGrounded = false;
        }

        private void FixedUpdate()
        {
            if (IsDead || Game == null || !Game.IsPlaying)
            {
                return;
            }

            float horizontalInput = Game.Input.GetHorizontal(_playerNumber);
            float acceleration = _isGrounded ? Config.GroundAcceleration : Config.AirAcceleration;
            Body.AddForce(Vector2.right * horizontalInput * acceleration);
            Body.linearDamping = _isGrounded ? Config.GroundDamping : Config.AirDamping;

            ClampVelocity(Config.PlayerVelocityLimits);
            Game.ClampVertical(transform, Body);
            Game.Wrap(transform);
            UpdateVisualTilt();
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            for (int index = 0; index < collision.contactCount; index++)
            {
                if (collision.GetContact(index).normal.y > Config.GroundNormalThreshold)
                {
                    _isGrounded = true;
                    return;
                }
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            _isGrounded = false;
        }

        protected override void OnBalloonLost()
        {
            Body.AddForce(Vector2.down * Config.PlayerBalloonLossImpulse, ForceMode2D.Impulse);
            if (BalloonCount > 0)
            {
                return;
            }

            MarkDead();
            BodyCollider.enabled = false;
            Body.gravityScale = Config.PlayerDeathGravity;
            Body.freezeRotation = false;
            Body.angularVelocity = Config.PlayerDeathSpin;
            Game.PlayerDefeated(this);
            StartCoroutine(DisableAfterDelay());
        }

        protected override float GetHitProtection()
        {
            return Config.PlayerHitProtection;
        }

        private IEnumerator DisableAfterDelay()
        {
            yield return new WaitForSeconds(Config.PlayerDeathDelay);
            gameObject.SetActive(false);
        }

        private void UpdateVisualTilt()
        {
            if (_visual == null)
            {
                return;
            }

            float maximumTilt = Config.MaximumVisualTilt;
            float tilt = Mathf.Clamp(-Body.linearVelocity.x * Config.VisualTiltMultiplier, -maximumTilt, maximumTilt);
            _visual.localRotation = Quaternion.Euler(0f, 0f, tilt);
        }
    }
}
