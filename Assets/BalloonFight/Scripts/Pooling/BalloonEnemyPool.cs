using System.Collections.Generic;
using BalloonFight.Actors;
using BalloonFight.Config;
using BalloonFight.Core;
using UnityEngine;
using UnityEngine.Pool;

namespace BalloonFight.Pooling
{
    internal sealed class BalloonEnemyPool
    {
        private readonly BalloonGameConfig _config;
        private readonly BalloonPrefabConfig _prefabs;
        private readonly Transform _parent;
        private readonly ObjectPool<EnemyController> _pool;
        private int _variation;
        private readonly HashSet<EnemyController> _leased = new();

        internal BalloonEnemyPool(Transform parent, BalloonGameConfig config, BalloonPrefabConfig prefabs)
        {
            _parent = parent;
            _config = config;
            _prefabs = prefabs;
            _pool = new ObjectPool<EnemyController>(
                CreateEnemy,
                OnGet,
                OnRelease,
                OnDestroyEnemy,
                true,
                Mathf.Max(1, config.EnemyPoolCapacity),
                Mathf.Max(1, config.EnemyPoolCapacity));

            Prewarm();
        }

        internal EnemyController Get(Vector2 position)
        {
            EnemyController enemy = _pool.Get();
            _leased.Add(enemy);
            enemy.transform.position = position;
            return enemy;
        }

        internal void Release(EnemyController enemy)
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

        internal void ReleaseAll(IEnumerable<EnemyController> enemies)
        {
            foreach (EnemyController enemy in enemies)
            {
                Release(enemy);
            }
        }

        private EnemyController CreateEnemy()
        {
            EnemyController enemy = FighterFactory.CreateEnemy(_parent, _config, _prefabs, _variation++);
            enemy.ConfigurePool(this);
            enemy.gameObject.SetActive(false);
            return enemy;
        }

        private static void OnGet(EnemyController enemy)
        {
            enemy.OnSpawned();
        }

        private static void OnRelease(EnemyController enemy)
        {
            enemy.OnDespawned();
        }

        private static void OnDestroyEnemy(EnemyController enemy)
        {
            if (enemy != null)
            {
                Object.Destroy(enemy.gameObject);
            }
        }

        private void Prewarm()
        {
            List<EnemyController> enemies = new(_config.EnemyPoolCapacity);
            for (int index = 0; index < _config.EnemyPoolCapacity; index++)
            {
                enemies.Add(_pool.Get());
            }

            foreach (EnemyController enemy in enemies)
            {
                _pool.Release(enemy);
            }
        }
    }
}
