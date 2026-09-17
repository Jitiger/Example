using BalloonFight.Actors;
using BalloonFight.Config;
using BalloonFight.Visual;
using UnityEngine;

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

    internal static PlayerController CreatePlayer(
        Transform parent,
        BalloonGameConfig config,
        BalloonPrefabConfig prefabs,
        PlayerNumber playerNumber)
    {
        GameObject prefab = prefabs == null ? null : prefabs.GetPlayer(playerNumber);
        if (prefab != null)
        {
            GameObject instance = Object.Instantiate(prefab, parent);
            PlayerController actor = instance.GetComponent<PlayerController>();
            instance.GetComponent<FighterBody>().SetOwner(actor);
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

        PlayerController player = fighter.AddComponent<PlayerController>();
        fighter.GetComponent<FighterBody>().SetOwner(player);
        return player;
    }

    internal static EnemyController CreateEnemy(
        Transform parent,
        BalloonGameConfig config,
        BalloonPrefabConfig prefabs,
        int variation)
    {
        GameObject prefab = prefabs == null ? null : prefabs.GetEnemy(variation);
        if (prefab != null)
        {
            GameObject instance = Object.Instantiate(prefab, parent);
            EnemyController actor = instance.GetComponent<EnemyController>();
            instance.GetComponent<FighterBody>().SetOwner(actor);
            return actor;
        }

        GameObject fighter = CreateBody(parent, $"Enemy {variation + 1}", Vector2.zero, config.EnemyGravity, config);
        RetroFactory.CreateFighter(fighter.transform, false, config.EnemyBalloonCount, variation);

        EnemyController enemy = fighter.AddComponent<EnemyController>();
        fighter.GetComponent<FighterBody>().SetOwner(enemy);
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
        fighter.AddComponent<FighterBody>();
        return fighter;
    }
}
