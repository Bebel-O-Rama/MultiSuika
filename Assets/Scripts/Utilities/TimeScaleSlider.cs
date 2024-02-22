using UnityEngine;

namespace MultiSuika.Utilities
{
    public class TimeScaleSlider : MonoBehaviour
    {
        [SerializeField] [Range (0f, 2f)] public float timeScaleSlider = 1f;

        private float _fixedDeltaTime;

        private void Awake()
        {
            _fixedDeltaTime = Time.fixedDeltaTime;
            timeScaleSlider = Time.timeScale;
        }

        private void Update()
        {
            Time.timeScale = timeScaleSlider;
            Time.fixedDeltaTime = _fixedDeltaTime * Time.timeScale;
        }
    }
}