using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "BalloonInputConfig", menuName = "Balloon Fight/Input Config")]
public sealed class BalloonInputConfig : ScriptableObject
{
    [Header("Player 1")]
    [SerializeField] private Key[] _playerOneLeft = { Key.A };
    [SerializeField] private Key[] _playerOneRight = { Key.D };
    [SerializeField] private Key[] _playerOneFlap = { Key.Space, Key.W };

    [Header("Player 2")]
    [SerializeField] private Key[] _playerTwoLeft = { Key.LeftArrow };
    [SerializeField] private Key[] _playerTwoRight = { Key.RightArrow };
    [SerializeField] private Key[] _playerTwoFlap = { Key.UpArrow, Key.Enter };

    [Header("Shared")]
    [SerializeField] private Key _restart = Key.R;

    internal string RestartLabel => _restart.ToString();
    internal bool RestartPressed => IsPressed(_restart);

    internal float GetHorizontal(PlayerNumber player)
    {
        Key[] left = player == PlayerNumber.One ? _playerOneLeft : _playerTwoLeft;
        Key[] right = player == PlayerNumber.One ? _playerOneRight : _playerTwoRight;
        return (IsHeld(right) ? 1f : 0f) - (IsHeld(left) ? 1f : 0f);
    }

    internal bool IsFlapPressed(PlayerNumber player)
    {
        return WasPressed(player == PlayerNumber.One ? _playerOneFlap : _playerTwoFlap);
    }

    internal string GetControlsLabel(PlayerNumber player)
    {
        Key[] left = player == PlayerNumber.One ? _playerOneLeft : _playerTwoLeft;
        Key[] right = player == PlayerNumber.One ? _playerOneRight : _playerTwoRight;
        Key[] flap = player == PlayerNumber.One ? _playerOneFlap : _playerTwoFlap;
        return $"{string.Join("/", left)} / {string.Join("/", right)} : move    {string.Join("/", flap)} : flap";
    }

    private static bool IsPressed(Key key)
    {
        return Keyboard.current != null && Keyboard.current[key].wasPressedThisFrame;
    }

    private static bool IsHeld(Key[] keys)
    {
        if (Keyboard.current == null || keys == null)
        {
            return false;
        }

        foreach (Key key in keys)
        {
            if (Keyboard.current[key].isPressed)
            {
                return true;
            }
        }

        return false;
    }

    private static bool WasPressed(Key[] keys)
    {
        if (Keyboard.current == null || keys == null)
        {
            return false;
        }

        foreach (Key key in keys)
        {
            if (Keyboard.current[key].wasPressedThisFrame)
            {
                return true;
            }
        }

        return false;
    }
}
