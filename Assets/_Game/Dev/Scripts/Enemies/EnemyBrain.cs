using SexShot.Dev.Combat;
using SexShot.Dev.Config;
using UnityEngine;

namespace SexShot.Dev.Enemies
{
    public class EnemyBrain : MonoBehaviour
    {
        [SerializeField] private EnemyDefinition _definition;
        [SerializeField] private EnemyAvatar _avatar;
        [SerializeField] private Transform _muzzle;
        [SerializeField] private Animator _animator;
        [SerializeField] private Collider _hitCollider;

        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int DieHash = Animator.StringToHash("Die");
        private static readonly int HitHash = Animator.StringToHash("Hit");
        private static readonly int SpeedHash = Animator.StringToHash("Speed");

        private float _nextAttackTime;
        private Transform _player;
        private bool _isDead;

        public void SetPlayer(Transform player)
        {
            _player = player;
        }

        public void NotifyStagger()
        {
            if (_isDead || _animator == null)
            {
                return;
            }

            _animator.SetTrigger(HitHash);
        }

        public void NotifyDeath()
        {
            if (_isDead || _definition == null)
            {
                return;
            }

            _isDead = true;
            if (_animator != null)
            {
                _animator.SetTrigger(DieHash);
            }

            if (_hitCollider != null)
            {
                _hitCollider.enabled = false;
            }

            Destroy(gameObject, _definition.DeathDespawnDelay);
        }

        private void OnEnable()
        {
            _isDead = false;
            if (_hitCollider != null)
            {
                _hitCollider.enabled = true;
            }
        }

        private void Update()
        {
            if (_isDead || _avatar == null || !_avatar.IsAlive || _player == null || _definition == null)
            {
                SetSpeed(0f);
                return;
            }

            if (_avatar.IsStaggered)
            {
                SetSpeed(0f);
                return;
            }

            var toPlayer = _player.position - transform.position;
            toPlayer.y = 0f;
            var distance = toPlayer.magnitude;
            if (distance > 0.01f)
            {
                var look = Quaternion.LookRotation(toPlayer.normalized);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    look,
                    _definition.TurnSpeed * Time.deltaTime);
            }

            if (distance > _definition.AttackRange)
            {
                var step = transform.forward * (_definition.MoveSpeed * Time.deltaTime);
                step.y = 0f;
                transform.position += step;
                SetSpeed(_definition.MoveSpeed);
                return;
            }

            SetSpeed(0f);
            TryAttack();
        }

        private void TryAttack()
        {
            if (_definition.ProjectilePrefab == null || _player == null || Time.time < _nextAttackTime)
            {
                return;
            }

            _nextAttackTime = Time.time + _definition.AttackCooldown;
            if (_animator != null)
            {
                _animator.SetTrigger(AttackHash);
            }

            SpawnMuzzleFlash();

            var targetPoint = _player.position + Vector3.up * _definition.AimHeight;
            var origin = _muzzle != null ? _muzzle.position : transform.position + Vector3.up * _definition.AimHeight;
            var direction = (targetPoint - origin).normalized;
            var instance = Instantiate(_definition.ProjectilePrefab, origin, Quaternion.LookRotation(direction));
            var projectile = instance.GetComponent<Projectile>();
            if (projectile == null)
            {
                return;
            }

            projectile.Launch(
                direction,
                _definition.ProjectileSpeed,
                _definition.ProjectileDamage,
                DamageTeam.Enemy,
                _definition.ImpactPrefab);
        }

        private void SpawnMuzzleFlash()
        {
            if (_definition.MuzzleFlashPrefab == null || _muzzle == null)
            {
                return;
            }

            var flash = Instantiate(_definition.MuzzleFlashPrefab, _muzzle.position, _muzzle.rotation);
            flash.transform.localScale = Vector3.one * _definition.MuzzleFlashScale;
        }

        private void SetSpeed(float speed)
        {
            if (_animator != null)
            {
                _animator.SetFloat(SpeedHash, speed);
            }
        }
    }
}
