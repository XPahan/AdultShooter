using SexShot.Dev.Config;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SexShot.Dev.Player
{
    public class PlayerLook : MonoBehaviour
    {
        [SerializeField] private PlayerDefinition _definition;
        [SerializeField] private Transform _cameraPivot;

        private bool _inputEnabled = true;
        private float _pitch;

        public Transform CameraPivot => _cameraPivot;

        public void ApplyRecoil(float pitchKick, float yawKick)
        {
            if (_definition == null || _cameraPivot == null)
            {
                return;
            }

            _pitch = Mathf.Clamp(
                _pitch - pitchKick,
                _definition.MinPitch,
                _definition.MaxPitch);
            transform.Rotate(0f, yawKick, 0f);
            _cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        public void SetInputEnabled(bool enabled)
        {
            _inputEnabled = enabled;
            if (enabled)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void Start()
        {
            SetInputEnabled(true);
        }

        private void Update()
        {
            if (!_inputEnabled || _cameraPivot == null || _definition == null)
            {
                return;
            }

            var mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            var delta = mouse.delta.ReadValue() * _definition.LookSensitivity;
            transform.Rotate(0f, delta.x, 0f);
            _pitch = Mathf.Clamp(_pitch - delta.y, _definition.MinPitch, _definition.MaxPitch);
            _cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }
    }
}
