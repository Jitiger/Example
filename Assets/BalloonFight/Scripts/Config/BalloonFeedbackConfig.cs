using UnityEngine;

[CreateAssetMenu(fileName = "BalloonFeedbackConfig", menuName = "Balloon Fight/BalloonFeedbackConfig")]
public sealed class BalloonFeedbackConfig : ScriptableObject
{
    [SerializeField] private int _poolCapacity = 16;
    [SerializeField] private float _duration = 0.35f;
    [SerializeField] private int _fragmentCount = 12;
    [SerializeField] private float _speed = 2.5f;
    [SerializeField] private float _fragmentSize = 0.08f;
    [SerializeField] private float _gravity = 3f;
    [SerializeField] private int _sortingOrder = 30;
    [SerializeField] private Color _color = new Color32(255, 225, 120, 255);

    public int PoolCapacity => _poolCapacity;
    public float Duration => _duration;
    public int FragmentCount => _fragmentCount;
    public float Speed => _speed;
    public float FragmentSize => _fragmentSize;
    public float Gravity => _gravity;
    public int SortingOrder => _sortingOrder;
    public Color Color => _color;
}
