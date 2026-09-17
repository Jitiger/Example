using BalloonFight.Actors;
using BalloonFight.Config;
using BalloonFight.Visual;
using UnityEngine;

namespace BalloonFight.Core;

internal static class FighterFactory
{
    internal static GameObject CreatePlayerPrefabObject(
        Transform parent,
        BalloonGameConfig config,
        PlayerNumber playerNumber)
    {
        return CreatePlayer(parent, config, null, playerNumber).gameObject;
    }

    internal static GameObject CreateEnemyPrefabObject(Transform parent, BalloonGameConfig config, int variation)
    {
        return CreateEnemy(parent, config, null, variation).gameObject;
    }

    internal static BalloonPlayer CreatePlayer(
        Transform parent,
        BalloonGameConfig config,
        BalloonPrefabConfig prefabs,
        PlayerNumber playerNumber)
    {
        GameObject prefab = prefabs == null ? null : prefabs.GetPlayer(playerNumber);
        if (prefab != null)
        {
            GameObject instance = Object.Instantiate(prefab, parent);
            BalloonPlayer actor = instance.GetComponent<BalloonPlayer>();
            instance.GetComponent<BalloonBody>().SetOwner(actor);
            return actor;
        }

        int variation = (int)playerNumber;
        GameObject fighter = CreateBody(
            parent,
            $"Player {variation + 1}",
            config.GetPlayerSpawn(playerNumber),
            config.PlayerGravity,
            config);
        RetroFactory.CreateFighter(fighter.transform, true, config.PlayerBalloonCount, variation);

        BalloonPlayer player = fighter.AddComponent<BalloonPlayer>();
        fighter.GetComponent<BalloonBody>().SetOwner(player);
        return player;
    }

    internal static BalloonEnemy CreateEnemy(
        Transform parent,
        BalloonGameConfig config,
        BalloonPrefabConfig prefabs,
        int variation)
    {
        GameObject prefab = prefabs == null ? null : prefabs.GetEnemy(variation);
        if (prefab != null)
        {
            GameObject instance = Object.Instantiate(prefab, parent);
            BalloonEnemy actor = instance.GetComponent<BalloonEnemy>();
            instance.GetComponent<BalloonBody>().SetOwner(actor);
            return actor;
        }

        GameObject fighter = CreateBody(parent, $"Enemy {variation + 1}", Vector2.zero, config.EnemyGravity, config);
        RetroFactory.CreateFighter(fighter.transform, false, config.EnemyBalloonCount, variation);

        BalloonEnemy enemy = fighter.AddComponent<BalloonEnemy>();
        fighter.GetComponent<BalloonBody>().SetOwner(enemy);
        return enemy;
    }

    private static GameObject CreateBody(
        Transform parent,
        string objectName,
        Vector2 position,
        float gravityScale,
        BalloonGameConfig config)
    {
        GameObject fighter = new(objectName);
        fighter.transform.SetParent(parent);
        fighter.transform.position = position;

        Rigidbody2D body = fighter.AddComponent<Rigidbody2D>();
        body.gravityScale = gravityScale;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.linearDamping = config.InitialDamping;

        CapsuleCollider2D collider = fighter.AddComponent<CapsuleCollider2D>();
        collider.size = config.BodySize;
        collider.offset = config.BodyOffset;
        fighter.AddComponent<BalloonBody>();
        return fighter;
    }
}
