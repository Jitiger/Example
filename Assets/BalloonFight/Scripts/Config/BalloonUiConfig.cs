using UnityEngine;

[CreateAssetMenu(fileName = "BalloonUiConfig", menuName = "Balloon Fight/BalloonUiConfig")]
public sealed class BalloonUiConfig : ScriptableObject
{
    [SerializeField] private Vector2 _referenceSize = new Vector2(1280f, 720f);
    [SerializeField] private int _hudFontSize = 22;
    [SerializeField] private int _smallFontSize = 15;
    [SerializeField] private int _titleFontSize = 40;
    [SerializeField] private int _subtitleFontSize = 20;
    [SerializeField] private Color _textColor = Color.white;
    [SerializeField] private Color _smallColor = new Color(0.82f, 0.9f, 1f);
    [SerializeField] private Color _titleColor = new Color32(255, 229, 102, 255);
    [SerializeField] private Rect _scoreRect = new Rect(20f, 15f, 280f, 35f);
    [SerializeField] private Rect _phaseRect = new Rect(570f, 15f, 150f, 35f);
    [SerializeField] private Rect _livesRect = new Rect(1110f, 15f, 150f, 35f);
    [SerializeField] private Rect _enemiesRect = new Rect(20f, 48f, 230f, 28f);
    [SerializeField] private Rect _controlsRect = new Rect(20f, 668f, 1240f, 35f);
    [SerializeField] private Rect _panelRect = new Rect(425f, 290f, 430f, 140f);
    [SerializeField] private Rect _titleRect = new Rect(425f, 305f, 430f, 60f);
    [SerializeField] private Rect _subtitleRect = new Rect(425f, 370f, 430f, 35f);
    [SerializeField] private string _scoreFormat = "SCORE {0:000000}";
    [SerializeField] private string _phaseFormat = "PHASE {0}";
    [SerializeField] private string _livesFormat = "LIVES {0}";
    [SerializeField] private string _enemiesFormat = "ENEMIES {0}";
    [SerializeField] private string _controlsFormat = "{0} / {1}: move    {2}: flap";
    [SerializeField] private string _restartFormat = "Press {0} to restart";
    [SerializeField] private string _phaseClear = "PHASE CLEAR";
    [SerializeField] private string _nextPhase = "Next phase incoming";
    [SerializeField] private string _gameOver = "GAME OVER";
    [SerializeField] private string _allClear = "ALL CLEAR";

    public Vector2 ReferenceSize => _referenceSize;
    public int HudFontSize => _hudFontSize;
    public int SmallFontSize => _smallFontSize;
    public int TitleFontSize => _titleFontSize;
    public int SubtitleFontSize => _subtitleFontSize;
    public Color TextColor => _textColor;
    public Color SmallColor => _smallColor;
    public Color TitleColor => _titleColor;
    public Rect ScoreRect => _scoreRect;
    public Rect PhaseRect => _phaseRect;
    public Rect LivesRect => _livesRect;
    public Rect EnemiesRect => _enemiesRect;
    public Rect ControlsRect => _controlsRect;
    public Rect PanelRect => _panelRect;
    public Rect TitleRect => _titleRect;
    public Rect SubtitleRect => _subtitleRect;
    public string ScoreFormat => _scoreFormat;
    public string PhaseFormat => _phaseFormat;
    public string LivesFormat => _livesFormat;
    public string EnemiesFormat => _enemiesFormat;
    public string ControlsFormat => _controlsFormat;
    public string RestartFormat => _restartFormat;
    public string PhaseClear => _phaseClear;
    public string NextPhase => _nextPhase;
    public string GameOver => _gameOver;
    public string AllClear => _allClear;
}

