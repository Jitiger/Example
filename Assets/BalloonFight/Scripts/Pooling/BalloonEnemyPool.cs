using System.Collections.Generic;
using BalloonFight.Actors;
using BalloonFight.Config;
using BalloonFight.Core;
using UnityEngine;
using UnityEngine.Pool;

namespace BalloonFight.Pooling;

internal sealed class BalloonEnemyPool
{
    private readonly BalloonGameConfig _config;
    private readonly BalloonPrefabConfig _prefabs;
    private readonly Transform _parent;
    private readonly ObjectPool<BalloonEnemy> _pool;
    private int _variation;
    private readonly HashSet<BalloonEnemy> _leased = new();

    internal BalloonEnemyPool(Transform parent, BalloonGameConfig config, BalloonPrefabConfig prefabs)
    {
        _parent = parent;
        _config = config;
        _prefabs = prefabs;
        _pool = new ObjectPool<BalloonEnemy>(
            CreateEnemy,
            OnGet,
            OnRelease,
            OnDestroyEnemy,
            true,
            Mathf.Max(1, config.EnemyPoolCapacity),
            Mathf.Max(1, config.EnemyPoolCapacity));

        Prewarm();
    }

    internal BalloonEnemy Get(Vector2 position)
    {
        BalloonEnemy enemy = _pool.Get();
        _leased.Add(enemy);
        enemy.transform.position = position;
        return enemy;
    }

    internal void Release(BalloonEnemy enemy)
    {
        if (enemy != null && _leased.Remove(enemy))
        {
            _pool.Release(enemy);
        }
    }

    internal void Clear()
    {
        _leased.Clear();
        _pool.Clear();
    }

    internal void ReleaseAll(IEnumerable<BalloonEnemy> enemies)
    {
        foreach (BalloonEnemy enemy in enemies)
        {
            Release(enemy);
        }
    }

    private BalloonEnemy CreateEnemy()
    {
        BalloonEnemy enemy = FighterFactory.CreateEnemy(_parent, _config, _prefabs, _variation++);
        enemy.ConfigurePool(this);
        enemy.gameObject.SetActive(false);
        return enemy;
    }

    private static void OnGet(BalloonEnemy enemy)
    {
        enemy.OnSpawned();
    }

    private static void OnRelease(BalloonEnemy enemy)
    {
        enemy.OnDespawned();
    }

    private static void OnDestroyEnemy(BalloonEnemy enemy)
    {
        if (enemy != null)
        {
            Object.Destroy(enemy.gameObject);
        }
    }

    private void Prewarm()
    {
        List<BalloonEnemy> enemies = new(_config.EnemyPoolCapacity);
        for (int index = 0; index < _config.EnemyPoolCapacity; index++)
        {
            enemies.Add(_pool.Get());
        }

        foreach (BalloonEnemy enemy in enemies)
        {
            _pool.Release(enemy);
        }
    }
}
