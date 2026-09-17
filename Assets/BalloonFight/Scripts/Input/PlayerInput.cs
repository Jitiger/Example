using BalloonFight.Actors;
using BalloonFight.Config;
using UnityEngine.InputSystem;

internal sealed class PlayerInput
{
    private readonly BalloonInputConfig _config;

    internal bool RestartPressed => IsPressed(_config.Restart);

    internal PlayerInput(BalloonInputConfig config)
    {
        _config = config;
    }

    internal float GetHorizontal(PlayerNumber playerNumber)
    {
        bool isLeftPressed = IsHeld(_config.GetLeftKeys(playerNumber));
        bool isRightPressed = IsHeld(_config.GetRightKeys(playerNumber));
        return (isRightPressed ? 1f : 0f) - (isLeftPressed ? 1f : 0f);
    }

    internal bool IsFlapPressed(PlayerNumber playerNumber)
    {
        return WasPressed(_config.GetFlapKeys(playerNumber));
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
