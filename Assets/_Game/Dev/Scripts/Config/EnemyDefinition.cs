using UnityEngine;

namespace SexShot.Dev.Config
{
    [CreateAssetMenu(fileName = "EnemyDefinition", menuName = "SexShot/Dev/Enemy Definition")]
    public class EnemyDefinition : ScriptableObject
    {
        [SerializeField] private string _displayName = "Succubus";
        [SerializeField] private float _maxHealth = 3f;
        [SerializeField] private float _staggerDuration = 0.35f;
        [SerializeField] private float _moveSpeed = 1.8f;
        [SerializeField] private float _turnSpeed = 8f;
        [SerializeField] private float _attackRange = 6f;
        [SerializeField] private float _attackCooldown = 2f;
        [SerializeField] private float _projectileDamage = 2f;
        [SerializeField] private float _projectileSpeed = 10f;
        [SerializeField] private float _aimHeight = 1.2f;
        [SerializeField] private float _deathDespawnDelay = 1.5f;
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private GameObject _muzzleFlashPrefab;
        [SerializeField] private float _muzzleFlashScale = 1f;
        [SerializeField] private GameObject _impactPrefab;

        public string DisplayName => _displayName;
        public float MaxHealth => _maxHealth;
        public float StaggerDuration => _staggerDuration;
        public float MoveSpeed => _moveSpeed;
        public float TurnSpeed => _turnSpeed;
        public float AttackRange => _attackRange;
        public float AttackCooldown => _attackCooldown;
        public float ProjectileDamage => _projectileDamage;
        public float ProjectileSpeed => _projectileSpeed;
        public float AimHeight => _aimHeight;
        public float DeathDespawnDelay => _deathDespawnDelay;
        public GameObject ProjectilePrefab => _projectilePrefab;
        public GameObject MuzzleFlashPrefab => _muzzleFlashPrefab;
        public float MuzzleFlashScale => Mathf.Max(0.01f, _muzzleFlashScale);
        public GameObject ImpactPrefab => _impactPrefab;
    }
}
