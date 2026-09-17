using BalloonFight.Config;
using UnityEngine;

namespace BalloonFight.Core;

internal sealed class BalloonWorldBounds
{
    private readonly Camera _camera;
    private readonly BalloonGameConfig _config;

    internal BalloonWorldBounds(Camera gameCamera, BalloonGameConfig config)
    {
        _camera = gameCamera;
        _config = config;
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
}
