using UnityEngine;

namespace SexShot.Dev.Player
{
    public class PlayerDeathView : MonoBehaviour
    {
        [SerializeField] private Transform _cameraPivot;
        [SerializeField] private CharacterController _controller;

        [SerializeField] private float _fallDuration = 1.25f;
        [SerializeField] private float _targetPitch = -82f;
        [SerializeField] private float _cameraDrop = 1.15f;
        [SerializeField] private float _bodyDrop = 0.85f;
        [SerializeField] private float _rollAngle = 16f;
        [SerializeField] private float _maxOverlayAlpha = 0.72f;
        [SerializeField] private float _maxVignetteAlpha = 0.88f;

        private static Texture2D _solidTexture;
        private static Texture2D _vignetteTexture;

        private bool _isPlaying;
        private float _elapsed;
        private float _startPitch;
        private float _startBodyY;
        private Vector3 _startPivotLocalPosition;
        private float _overlayAlpha;
        private float _vignetteAlpha;

        public bool IsPlaying => _isPlaying;

        public void PlayDeath()
        {
            if (_isPlaying)
            {
                return;
            }

            _isPlaying = true;
            _elapsed = 0f;
            _overlayAlpha = 0f;
            _vignetteAlpha = 0f;

            if (_controller != null)
            {
                _controller.enabled = false;
            }

            if (_cameraPivot != null)
            {
                _startPivotLocalPosition = _cameraPivot.localPosition;
                _startPitch = NormalizePitch(_cameraPivot.localEulerAngles.x);
            }

            _startBodyY = transform.position.y;
        }

        private void Update()
        {
            if (!_isPlaying)
            {
                return;
            }

            _elapsed += Time.unscaledDeltaTime;
            var t = Mathf.Clamp01(_elapsed / _fallDuration);
            var eased = 1f - Mathf.Pow(1f - t, 3f);

            if (_cameraPivot != null)
            {
                var pitch = Mathf.Lerp(_startPitch, _targetPitch, eased);
                var roll = Mathf.Lerp(0f, _rollAngle, eased);
                _cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, roll);
                _cameraPivot.localPosition = Vector3.Lerp(
                    _startPivotLocalPosition,
                    _startPivotLocalPosition + Vector3.down * _cameraDrop,
                    eased);
            }

            if (_bodyDrop > 0f)
            {
                var position = transform.position;
                position.y = Mathf.Lerp(_startBodyY, _startBodyY - _bodyDrop, eased);
                transform.position = position;
            }

            var overlayT = Mathf.Clamp01((t - 0.05f) / 0.55f);
            _overlayAlpha = Mathf.Lerp(0f, _maxOverlayAlpha, overlayT);
            _vignetteAlpha = Mathf.Lerp(0f, _maxVignetteAlpha, overlayT);
        }

        private void OnGUI()
        {
            if (_overlayAlpha <= 0f && _vignetteAlpha <= 0f)
            {
                return;
            }

            EnsureTextures();

            var previousDepth = GUI.depth;
            var previousColor = GUI.color;
            GUI.depth = -1000;

            if (_overlayAlpha > 0f)
            {
                GUI.color = new Color(0.52f, 0.02f, 0.02f, _overlayAlpha);
                GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), _solidTexture);
            }

            if (_vignetteAlpha > 0f)
            {
                GUI.color = new Color(0.28f, 0f, 0f, _vignetteAlpha);
                GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), _vignetteTexture, ScaleMode.StretchToFill);
            }

            GUI.color = previousColor;
            GUI.depth = previousDepth;
        }

        private static float NormalizePitch(float eulerX)
        {
            if (eulerX > 180f)
            {
                eulerX -= 360f;
            }

            return eulerX;
        }

        private static void EnsureTextures()
        {
            if (_solidTexture == null)
            {
                _solidTexture = Texture2D.whiteTexture;
            }

            if (_vignetteTexture != null)
            {
                return;
            }

            const int size = 256;
            _vignetteTexture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            var center = (size - 1) * 0.5f;
            var maxRadius = center;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var dx = (x - center) / maxRadius;
                    var dy = (y - center) / maxRadius;
                    var distance = Mathf.Sqrt(dx * dx + dy * dy);
                    var alpha = Mathf.Clamp01((distance - 0.15f) / 0.85f);
                    alpha = alpha * alpha;
                    _vignetteTexture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            _vignetteTexture.Apply();
        }
    }
}
