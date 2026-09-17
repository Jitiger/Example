using BalloonFight.Actors;
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

    internal Key Restart => _restart;

    internal Key[] GetLeftKeys(PlayerNumber playerNumber)
    {
        return playerNumber == PlayerNumber.One ? _playerOneLeft : _playerTwoLeft;
    }

    internal Key[] GetRightKeys(PlayerNumber playerNumber)
    {
        return playerNumber == PlayerNumber.One ? _playerOneRight : _playerTwoRight;
    }

    internal Key[] GetFlapKeys(PlayerNumber playerNumber)
    {
        return playerNumber == PlayerNumber.One ? _playerOneFlap : _playerTwoFlap;
    }

}
