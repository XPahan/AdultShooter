using System;
using SexShot.Dev.Combat;
using SexShot.Dev.Config;
using UnityEngine;

namespace SexShot.Dev.Player
{
    public class PlayerAvatar : MonoBehaviour, IDamageable
    {
        [SerializeField] private PlayerDefinition _definition;
        [SerializeField] private Health _health;
        [SerializeField] private PlayerMotor _motor;
        [SerializeField] private PlayerLook _look;
        [SerializeField] private PlayerWeaponController _weapons;
        [SerializeField] private PlayerDeathView _deathView;

        private bool _isAlive = true;

        public event Action Died;

        public DamageTeam Team => DamageTeam.Player;
        public bool IsAlive => _isAlive && _health != null && !_health.IsDead;
        public Health Health => _health;
        public PlayerDefinition Definition => _definition;
        public PlayerWeaponController Weapons => _weapons;

        public void SetGameplayInputEnabled(bool enabled)
        {
            if (!IsAlive && enabled)
            {
                return;
            }

            _motor?.SetInputEnabled(enabled);
            _look?.SetInputEnabled(enabled);
            _weapons?.SetInputEnabled(enabled);
        }

        public void TakeDamage(float amount, DamageTeam sourceTeam)
        {
            if (!IsAlive || sourceTeam == Team)
            {
                return;
            }

            _health.TakeDamage(amount);
        }

        private void OnEnable()
        {
            _isAlive = true;
            if (_health == null)
            {
                return;
            }

            _health.Died += HandleDeath;
        }

        private void OnDisable()
        {
            if (_health == null)
            {
                return;
            }

            _health.Died -= HandleDeath;
        }

        private void HandleDeath()
        {
            if (!_isAlive)
            {
                return;
            }

            _isAlive = false;
            _motor?.SetInputEnabled(false);
            _look?.SetInputEnabled(false);
            _weapons?.SetInputEnabled(false);
            _deathView?.PlayDeath();
            Died?.Invoke();
        }
    }
}
