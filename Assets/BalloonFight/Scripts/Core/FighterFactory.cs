using UnityEngine;

internal static class FighterFactory
{
    internal static BalloonPlayer CreatePlayer(Transform parent, BalloonGameConfig config)
    {
        BalloonPrefabConfig prefabs = Resources.Load<BalloonPrefabConfig>(nameof(BalloonPrefabConfig));
        if (prefabs != null && prefabs.Player != null)
        {
            GameObject instance = Object.Instantiate(prefabs.Player, parent);
            BalloonPlayer actor = instance.GetComponent<BalloonPlayer>();
            instance.GetComponent<BalloonBody>().SetOwner(actor);
            return actor;
        }
        GameObject fighter = CreateBody(parent, "Player", config.PlayerSpawn, config.PlayerGravity, config);
        RetroFactory.CreateFighter(fighter.transform, true, config.PlayerBalloonCount, 0);

        BalloonPlayer player = fighter.AddComponent<BalloonPlayer>();
        fighter.GetComponent<BalloonBody>().SetOwner(player);
        return player;
    }

    internal static BalloonEnemy CreateEnemy(Transform parent, BalloonGameConfig config, int variation)
    {
        BalloonPrefabConfig prefabs = Resources.Load<BalloonPrefabConfig>(nameof(BalloonPrefabConfig));
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

    private static GameObject CreateBody(Transform parent, string objectName, Vector2 position, float gravityScale, BalloonGameConfig config)
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
