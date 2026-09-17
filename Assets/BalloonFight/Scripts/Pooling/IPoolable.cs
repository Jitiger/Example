namespace BalloonFight.Pooling;

public interface IPoolable
{
    void OnSpawned();
    void OnDespawned();
}
