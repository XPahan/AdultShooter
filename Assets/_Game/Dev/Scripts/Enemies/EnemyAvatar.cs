using System;
using SexShot.Dev.Combat;
using SexShot.Dev.Config;
using UnityEngine;

namespace SexShot.Dev.Enemies
{
    public class EnemyAvatar : MonoBehaviour, IDamageable
    {
        [SerializeField] private EnemyDefinition _definition;
        [SerializeField] private Health _health;
        [SerializeField] private EnemyBrain _brain;

        private bool _isAlive = true;

        public event Action Died;

        public DamageTeam Team => DamageTeam.Enemy;
        public bool IsAlive => _isAlive && _health != null && !_health.IsDead;
        public Health Health => _health;
        public bool IsStaggered { get; private set; }
        public EnemyDefinition Definition => _definition;

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
            IsStaggered = false;
            if (_health == null)
            {
                return;
            }

            _health.Died += HandleDeath;
            _health.HealthChanged += HandleHealthChanged;
        }

        private void OnDisable()
        {
            if (_health == null)
            {
                return;
            }

            _health.Died -= HandleDeath;
            _health.HealthChanged -= HandleHealthChanged;
        }

        private void HandleHealthChanged(float current, float max)
        {
            if (current <= 0f || current >= max)
            {
                return;
            }

            BeginStagger();
        }

        private void BeginStagger()
        {
            if (!IsAlive || _definition == null)
            {
                return;
            }

            IsStaggered = true;
            _brain?.NotifyStagger();
            CancelInvoke(nameof(EndStagger));
            Invoke(nameof(EndStagger), _definition.StaggerDuration);
        }

        private void EndStagger()
        {
            IsStaggered = false;
        }

        private void HandleDeath()
        {
            if (!_isAlive)
            {
                return;
            }

            _isAlive = false;
            IsStaggered = false;
            CancelInvoke(nameof(EndStagger));
            _brain?.NotifyDeath();
            Died?.Invoke();
        }
    }
}
