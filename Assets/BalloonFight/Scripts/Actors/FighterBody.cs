using UnityEngine;

internal sealed class FighterBody : MonoBehaviour
{
    private Fighter _owner;

    internal Fighter Owner => _owner;

    internal void SetOwner(Fighter owner)
    {
        _owner = owner;
    }
}
