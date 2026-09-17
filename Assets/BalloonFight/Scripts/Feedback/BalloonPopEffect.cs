using BalloonFight.Config;
using BalloonFight.Pooling;
using UnityEngine;

namespace BalloonFight.Feedback
{
    public sealed class BalloonPopEffect : MonoBehaviour, IPoolable
    {
        private BalloonPopPool _pool;
        private BalloonFeedbackConfig _config;
        private SpriteRenderer[] _fragments;
        private Vector3[] _velocities;
        private float _elapsed;

        internal void Initialize(BalloonPopPool pool, BalloonFeedbackConfig config, Sprite sprite)
        {
            _pool = pool;
            _config = config;
            int count = Mathf.Max(1, config.FragmentCount);
            _fragments = new SpriteRenderer[count];
            _velocities = new Vector3[count];
            for (int index = 0; index < count; index++)
            {
                GameObject fragment = new(nameof(BalloonPopEffect));
                fragment.transform.SetParent(transform, false);
                SpriteRenderer renderer = fragment.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sortingOrder = config.SortingOrder;
                _fragments[index] = renderer;
            }
        }

        public void OnSpawned()
        {
            _elapsed = 0f;
            for (int index = 0; index < _fragments.Length; index++)
            {
                float angle = index * (Mathf.PI * 2f / _fragments.Length);
                _velocities[index] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * _config.Speed;
                Transform fragment = _fragments[index].transform;
                fragment.localPosition = Vector3.zero;
                fragment.localScale = Vector3.one * _config.FragmentSize;
                _fragments[index].color = _config.Color;
            }
            gameObject.SetActive(true);
        }

        public void OnDespawned()
        {
            gameObject.SetActive(false);
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            if (_elapsed >= Mathf.Max(Mathf.Epsilon, _config.Duration))
            {
                _pool.Release(this);
                return;
            }

            Color color = _config.Color;
            color.a *= 1f - _elapsed / Mathf.Max(Mathf.Epsilon, _config.Duration);
            for (int index = 0; index < _fragments.Length; index++)
            {
                _velocities[index] += Vector3.down * (_config.Gravity * Time.deltaTime);
                _fragments[index].transform.localPosition += _velocities[index] * Time.deltaTime;
                _fragments[index].color = color;
            }
        }
    }
}
