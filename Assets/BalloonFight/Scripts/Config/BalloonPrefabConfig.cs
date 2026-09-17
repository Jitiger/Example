using UnityEngine;

[CreateAssetMenu(fileName = "BalloonPrefabConfig", menuName = "Balloon Fight/Prefabs")]
public sealed class BalloonPrefabConfig : ScriptableObject
{
    [SerializeField] private GameObject _player;
    [SerializeField] private GameObject[] _enemies;
    [SerializeField] private GameObject _stage;

    public GameObject Player => _player;
    public GameObject Stage => _stage;

    public GameObject GetEnemy(int variation)
    {
        return _enemies == null || _enemies.Length == 0
            ? null : _enemies[variation % _enemies.Length];
    }
}

