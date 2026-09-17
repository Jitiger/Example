using System.Collections.Generic;
using BalloonFight.Actors;
using BalloonFight.Config;
using BalloonFight.Pooling;
using UnityEngine;

namespace BalloonFight.Core;

internal sealed class BalloonSpawner
{
    private readonly BalloonFightRuntime _game;
    private readonly BalloonGameConfig _config;
    private readonly BalloonPrefabConfig _prefabs;
    private readonly Transform _parent;
    private readonly BalloonEnemyPool _enemyPool;
    private readonly List<BalloonEnemy> _enemies = new();
    private readonly BalloonPlayer[] _players = new BalloonPlayer[PlayerRoster.Count];

    internal int ActiveEnemyCount => _enemies.Count;

    internal BalloonSpawner(
        Transform parent,
        BalloonFightRuntime game,
        BalloonGameConfig config,
        BalloonPrefabConfig prefabs)
    {
        _parent = parent;
        _game = game;
        _config = config;
        _prefabs = prefabs;
        _enemyPool = new BalloonEnemyPool(parent, config, prefabs);
    }

    internal void SpawnAllPlayers()
    {
        SpawnPlayer(PlayerNumber.One);
        SpawnPlayer(PlayerNumber.Two);
    }

    internal BalloonPlayer SpawnPlayer(PlayerNumber playerNumber)
    {
        int index = (int)playerNumber;
        if (_players[index] == null)
        {
            _players[index] = FighterFactory.CreatePlayer(_parent, _config, _prefabs, playerNumber);
        }

        BalloonPlayer player = _players[index];
        player.gameObject.SetActive(true);
        player.transform.position = _config.GetPlayerSpawn(playerNumber);
        player.InitializePlayer(_game, _config, playerNumber);
        return player;
    }

    internal void SpawnPhase(int phase)
    {
        int enemyCount = Mathf.Min(phase + _config.PhaseEnemyOffset, _config.EnemySpawns.Length);
        for (int index = 0; index < enemyCount; index++)
        {
            BalloonEnemy enemy = _enemyPool.Get(_config.EnemySpawns[index]);
            enemy.Initialize(_game, _config, _config.EnemyBalloonCount);
            _enemies.Add(enemy);
        }
    }

    internal bool RemoveEnemy(BalloonEnemy enemy)
    {
        return enemy != null && _enemies.Remove(enemy);
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

    internal void Reset()
    {
        foreach (BalloonPlayer player in _players)
        {
            player?.gameObject.SetActive(false);
        }

        _enemyPool.ReleaseAll(_enemies);
        _enemies.Clear();
    }

    internal void Clear()
    {
        _enemyPool.Clear();
    }
}
