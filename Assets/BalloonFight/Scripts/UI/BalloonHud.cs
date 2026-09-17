using UnityEngine;

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

    internal void Draw(int score, int phase, int lives, int enemyCount,
        bool isChangingPhase, bool isGameOver, bool isAllClear)
    {
        EnsureStyles();
        Matrix4x4 previous = GUI.matrix;
        Vector2 reference = _config.ReferenceSize;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
            new Vector3(Screen.width / Mathf.Max(1f, reference.x),
                Screen.height / Mathf.Max(1f, reference.y), 1f));
        try
        {
            GUI.Label(_config.ScoreRect, string.Format(_config.ScoreFormat, score), _hudStyle);
            GUI.Label(_config.PhaseRect, string.Format(_config.PhaseFormat, phase), _hudStyle);
            GUI.Label(_config.LivesRect, string.Format(_config.LivesFormat, lives), _hudStyle);
            GUI.Label(_config.EnemiesRect, string.Format(_config.EnemiesFormat, enemyCount), _smallStyle);
            GUI.Label(_config.ControlsRect, string.Format(_config.ControlsFormat,
                _input.LeftLabel, _input.RightLabel, _input.FlapLabel), _smallStyle);
            if (isGameOver || isAllClear)
            {
                DrawCenter(isGameOver ? _config.GameOver : _config.AllClear,
                    string.Format(_config.RestartFormat, _input.RestartLabel));
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

    private void DrawCenter(string title, string subtitle)
    {
        GUI.Box(_config.PanelRect, GUIContent.none);
        GUI.Label(_config.TitleRect, title, _titleStyle);
        GUI.Label(_config.SubtitleRect, subtitle, _subtitleStyle);
    }
}
