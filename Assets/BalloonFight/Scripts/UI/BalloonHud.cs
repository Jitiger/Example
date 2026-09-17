using BalloonFight.Actors;
using BalloonFight.Config;
using UnityEngine;

namespace BalloonFight.UI;

internal sealed class BalloonHud
{
    private readonly BalloonUiConfig _config;
    private readonly BalloonInputConfig _input;
    private GUIStyle _hudStyle;
    private GUIStyle _smallStyle;
    private GUIStyle _titleStyle;
    private GUIStyle _subtitleStyle;

    internal BalloonHud(BalloonUiConfig config, BalloonInputConfig input)
    {
        _config = config;
        _input = input;
    }

    internal void Draw(
        int score,
        int phase,
        int playerOneLives,
        int playerTwoLives,
        int enemyCount,
        bool isChangingPhase,
        bool isGameOver,
        bool isAllClear)
    {
        EnsureStyles();
        Matrix4x4 previous = GUI.matrix;
        Vector2 reference = _config.ReferenceSize;
        Vector3 scale = new(
            Screen.width / Mathf.Max(1f, reference.x),
            Screen.height / Mathf.Max(1f, reference.y),
            1f);
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
        try
        {
            GUI.Label(_config.ScoreRect, string.Format(_config.ScoreFormat, score), _hudStyle);
            GUI.Label(_config.PhaseRect, string.Format(_config.PhaseFormat, phase), _hudStyle);
            GUI.Label(_config.PlayerOneLivesRect, string.Format(_config.PlayerOneLivesFormat, playerOneLives), _hudStyle);
            GUI.Label(_config.PlayerTwoLivesRect, string.Format(_config.PlayerTwoLivesFormat, playerTwoLives), _hudStyle);
            GUI.Label(_config.EnemiesRect, string.Format(_config.EnemiesFormat, enemyCount), _smallStyle);
            string playerOneControls = string.Format(
                _config.PlayerOneControlsFormat,
                GetControlsLabel(PlayerNumber.One));
            string playerTwoControls = string.Format(
                _config.PlayerTwoControlsFormat,
                GetControlsLabel(PlayerNumber.Two));
            GUI.Label(_config.PlayerOneControlsRect, playerOneControls, _smallStyle);
            GUI.Label(_config.PlayerTwoControlsRect, playerTwoControls, _smallStyle);
            if (isGameOver || isAllClear)
            {
                DrawCenter(isGameOver ? _config.GameOver : _config.AllClear,
                    string.Format(_config.RestartFormat, _input.Restart));
            }
            else if (isChangingPhase)
            {
                DrawCenter(_config.PhaseClear, _config.NextPhase);
            }
        }
        finally
        {
            GUI.matrix = previous;
        }
    }

    private void EnsureStyles()
    {
        if (_hudStyle != null)
        {
            return;
        }

        _hudStyle = CreateStyle(_config.HudFontSize, _config.TextColor, true, false);
        _smallStyle = CreateStyle(_config.SmallFontSize, _config.SmallColor, false, false);
        _titleStyle = CreateStyle(_config.TitleFontSize, _config.TitleColor, true, true);
        _subtitleStyle = CreateStyle(_config.SubtitleFontSize, _config.TextColor, false, true);
    }

    private static GUIStyle CreateStyle(int size, Color color, bool bold, bool centered)
    {
        GUIStyle style = new(GUI.skin.label);
        style.fontSize = size;
        style.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        style.alignment = centered ? TextAnchor.MiddleCenter : TextAnchor.UpperLeft;
        style.normal.textColor = color;
        return style;
    }

    private string GetControlsLabel(PlayerNumber playerNumber)
    {
        string left = string.Join("/", _input.GetLeftKeys(playerNumber));
        string right = string.Join("/", _input.GetRightKeys(playerNumber));
        string flap = string.Join("/", _input.GetFlapKeys(playerNumber));
        return $"{left} / {right} : move    {flap} : flap";
    }

    private void DrawCenter(string title, string subtitle)
    {
        GUI.Box(_config.PanelRect, GUIContent.none);
        GUI.Label(_config.TitleRect, title, _titleStyle);
        GUI.Label(_config.SubtitleRect, subtitle, _subtitleStyle);
    }
}
