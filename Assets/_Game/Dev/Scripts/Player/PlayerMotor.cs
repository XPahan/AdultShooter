using SexShot.Dev.Config;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SexShot.Dev.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotor : MonoBehaviour
    {
        [SerializeField] private PlayerDefinition _definition;
        [SerializeField] private CharacterController _controller;

        private bool _inputEnabled = true;
        private Vector3 _velocity;

        public bool IsGrounded => _controller != null && _controller.isGrounded;
        public float HorizontalSpeed { get; private set; }

        public void SetInputEnabled(bool enabled)
        {
            _inputEnabled = enabled;
            if (!enabled)
            {
                _velocity = Vector3.zero;
            }
        }

        private void Update()
        {
            if (_controller == null || _definition == null)
            {
                return;
            }

            if (_controller.isGrounded && _velocity.y < 0f)
            {
                _velocity.y = -2f;
            }

            var move = Vector3.zero;
            if (_inputEnabled)
            {
                var keyboard = Keyboard.current;
                if (keyboard != null)
                {
                    var input = Vector2.zero;
                    if (keyboard.wKey.isPressed) input.y += 1f;
                    if (keyboard.sKey.isPressed) input.y -= 1f;
                    if (keyboard.aKey.isPressed) input.x -= 1f;
                    if (keyboard.dKey.isPressed) input.x += 1f;
                    input = Vector2.ClampMagnitude(input, 1f);
                    move = transform.right * input.x + transform.forward * input.y;

                    if (keyboard.spaceKey.wasPressedThisFrame && _controller.isGrounded)
                    {
                        _velocity.y = Mathf.Sqrt(_definition.JumpHeight * -2f * _definition.Gravity);
                    }
                }
            }

            _velocity.y += _definition.Gravity * Time.deltaTime;
            var worldMove = move * _definition.MoveSpeed;
            HorizontalSpeed = worldMove.magnitude;
            _controller.Move((worldMove + _velocity) * Time.deltaTime);
        }
    }
}
