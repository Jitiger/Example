using UnityEngine;

namespace BalloonFight.Actors
{
    internal sealed class BalloonTarget : MonoBehaviour
    {
        private BalloonActor _owner;

        internal BalloonActor Owner => _owner;

        internal void SetOwner(BalloonActor owner)
        {
            _owner = owner;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            BalloonBody attackingBody = other.GetComponent<BalloonBody>();
            if (_owner == null || attackingBody == null || attackingBody.Owner == null
                || attackingBody.Owner == _owner)
            {
                return;
            }

            _owner.PopBalloon(this);
        }
    }
}
