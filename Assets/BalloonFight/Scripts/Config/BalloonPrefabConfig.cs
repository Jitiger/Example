using BalloonFight.Actors;
using UnityEngine;

namespace BalloonFight.Config
{
    [CreateAssetMenu(fileName = "BalloonPrefabConfig", menuName = "Balloon Fight/Prefabs")]
    public sealed class BalloonPrefabConfig : ScriptableObject
    {
        [SerializeField] private GameObject[] _players;
        [SerializeField] private GameObject[] _enemies;
        [SerializeField] private GameObject _stage;

        public GameObject Stage => _stage;

        public GameObject GetPlayer(PlayerNumber player)
        {
            int index = (int)player;
            return _players == null || index >= _players.Length ? null : _players[index];
        }

        public GameObject GetEnemy(int variation)
        {
            return _enemies == null || _enemies.Length == 0
                ? null : _enemies[variation % _enemies.Length];
        }
    }
}
