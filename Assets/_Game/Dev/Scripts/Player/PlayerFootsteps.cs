using UnityEngine;

namespace SexShot.Dev.Player
{
    [RequireComponent(typeof(PlayerMotor))]
    public class PlayerFootsteps : MonoBehaviour
    {
        [SerializeField] private PlayerMotor _motor;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip[] _clips;
        [SerializeField] private float _volume = 0.55f;
        [SerializeField] private float _stepInterval = 0.42f;
        [SerializeField] private float _minSpeed = 0.5f;

        private float _stepTimer;

        private void Awake()
        {
            if (_motor == null)
            {
                _motor = GetComponent<PlayerMotor>();
            }

            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
            }

            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
                _audioSource.spatialBlend = 0f;
            }
        }

        private void Update()
        {
            if (_motor == null || _clips == null || _clips.Length == 0)
            {
                return;
            }

            if (!_motor.IsGrounded || _motor.HorizontalSpeed < _minSpeed)
            {
                _stepTimer = 0f;
                return;
            }

            _stepTimer += Time.deltaTime;
            if (_stepTimer < _stepInterval)
            {
                return;
            }

            _stepTimer = 0f;
            PlayStep();
        }

        private void PlayStep()
        {
            var clip = _clips[Random.Range(0, _clips.Length)];
            if (clip == null)
            {
                return;
            }

            _audioSource.PlayOneShot(clip, _volume);
        }
    }
}
