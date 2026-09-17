using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "BalloonInputConfig", menuName = "Balloon Fight/Input Config")]
public sealed class BalloonInputConfig : ScriptableObject
{
    [SerializeField] private Key[] _left = { Key.A, Key.LeftArrow };
    [SerializeField] private Key[] _right = { Key.D, Key.RightArrow };
    [SerializeField] private Key[] _flap = { Key.Space, Key.Z, Key.UpArrow };
    [SerializeField] private Key _restart = Key.R;

    internal string LeftLabel => string.Join("/", _left);
    internal string RightLabel => string.Join("/", _right);
    internal string FlapLabel => string.Join("/", _flap);
    internal string RestartLabel => _restart.ToString();

    internal float Horizontal => (IsHeld(_right) ? 1f : 0f) - (IsHeld(_left) ? 1f : 0f);
    internal bool FlapPressed => WasPressed(_flap);
    internal bool RestartPressed => Keyboard.current != null && Keyboard.current[_restart].wasPressedThisFrame;

    private static bool IsHeld(Key[] keys)
    {
        if (Keyboard.current == null || keys == null) return false;
        foreach (Key key in keys)
        {
            if (Keyboard.current[key].isPressed) return true;
        }
        return false;
    }

    private static bool WasPressed(Key[] keys)
    {
        if (Keyboard.current == null || keys == null) return false;
        foreach (Key key in keys)
        {
            if (Keyboard.current[key].wasPressedThisFrame) return true;
        }
        return false;
    }
}
