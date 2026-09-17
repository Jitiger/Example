using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

internal sealed class BalloonPopPool
{
    private readonly Transform _parent;
    private readonly BalloonFeedbackConfig _config;
    private readonly Sprite _sprite;
    private readonly ObjectPool<BalloonPopEffect> _pool;
    private readonly HashSet<BalloonPopEffect> _active = new();
    private readonly List<BalloonPopEffect> _scratch = new();

    internal BalloonPopPool(Transform parent, BalloonFeedbackConfig config, Sprite sprite)
    {
        _parent = parent;
        _config = config;
        _sprite = sprite;
        int capacity = Mathf.Max(1, config.PoolCapacity);
        _pool = new ObjectPool<BalloonPopEffect>(Create, null,
            effect => effect.OnDespawned(),
            effect => { if (effect != null) Object.Destroy(effect.gameObject); },
            true, capacity, capacity);
        for (int index = 0; index < capacity; index++)
        {
            _scratch.Add(_pool.Get());
        }
        foreach (BalloonPopEffect effect in _scratch)
        {
            _pool.Release(effect);
        }
        _scratch.Clear();
    }

    internal void Play(Vector3 position)
    {
        // Saturation drops only a cosmetic effect; no gameplay object is affected.
        if (_active.Count >= Mathf.Max(1, _config.PoolCapacity))
        {
            return;
        }

        BalloonPopEffect effect = _pool.Get();
        _active.Add(effect);
        effect.transform.position = position;
        effect.OnSpawned();
    }

    internal void Release(BalloonPopEffect effect)
    {
        if (effect != null && _active.Remove(effect))
        {
            _pool.Release(effect);
        }
    }

    internal void Reset()
    {
        _scratch.Clear();
        _scratch.AddRange(_active);
        foreach (BalloonPopEffect effect in _scratch)
        {
            Release(effect);
        }
        _scratch.Clear();
    }

    internal void Clear()
    {
        Reset();
        _pool.Clear();
    }

    private BalloonPopEffect Create()
    {
        GameObject instance = new(nameof(BalloonPopEffect));
        instance.transform.SetParent(_parent, false);
        instance.SetActive(false);
        BalloonPopEffect effect = instance.AddComponent<BalloonPopEffect>();
        effect.Initialize(this, _config, _sprite);
        return effect;
    }
}

