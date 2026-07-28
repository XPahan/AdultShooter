using System;
using UnityEngine;

namespace SexShot.Dev.Combat
{
    public class Health : MonoBehaviour
    {
        [SerializeField] private float _maxHealth = 100f;

        private float _currentHealth;

        public event Action<float, float> HealthChanged;
        public event Action Died;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;
        public bool IsDead => _currentHealth <= 0f;

        public void TakeDamage(float amount)
        {
            if (IsDead || amount <= 0f)
            {
                return;
            }

            _currentHealth = Mathf.Max(0f, _currentHealth - amount);
            HealthChanged?.Invoke(_currentHealth, _maxHealth);
            if (IsDead)
            {
                Died?.Invoke();
            }
        }

        private void Awake()
        {
            _currentHealth = _maxHealth;
        }
    }
}
