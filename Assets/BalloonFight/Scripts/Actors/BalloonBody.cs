using UnityEngine;

namespace BalloonFight.Actors
{
    internal sealed class BalloonBody : MonoBehaviour
    {
        private BalloonActor _owner;

        internal BalloonActor Owner => _owner;

        internal void SetOwner(BalloonActor owner)
        {
            _owner = owner;
        }
    }
}
