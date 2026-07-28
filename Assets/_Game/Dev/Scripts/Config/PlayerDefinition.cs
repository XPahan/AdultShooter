using SexShot.Dev.Weapons;
using UnityEngine;

namespace SexShot.Dev.Config
{
    [CreateAssetMenu(fileName = "PlayerDefinition", menuName = "SexShot/Dev/Player Definition")]
    public class PlayerDefinition : ScriptableObject
    {
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _moveSpeed = 6f;
        [SerializeField] private float _jumpHeight = 1.4f;
        [SerializeField] private float _gravity = -20f;
        [SerializeField] private float _lookSensitivity = 0.12f;
        [SerializeField] private float _minPitch = -85f;
        [SerializeField] private float _maxPitch = 85f;
        [SerializeField] private WeaponDefinition[] _weapons;

        public float MaxHealth => _maxHealth;
        public float MoveSpeed => _moveSpeed;
        public float JumpHeight => _jumpHeight;
        public float Gravity => _gravity;
        public float LookSensitivity => _lookSensitivity;
        public float MinPitch => _minPitch;
        public float MaxPitch => _maxPitch;
        public WeaponDefinition[] Weapons => _weapons;
    }
}
