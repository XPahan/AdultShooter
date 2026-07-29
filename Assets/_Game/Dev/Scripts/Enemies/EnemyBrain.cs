using SexShot.Dev.Combat;
using SexShot.Dev.Config;
using UnityEngine;

namespace SexShot.Dev.Enemies
{
    [RequireComponent(typeof(CharacterController))]
    public class EnemyBrain : MonoBehaviour
    {
        [SerializeField] private EnemyDefinition _definition;
        [SerializeField] private EnemyAvatar _avatar;
        [SerializeField] private Transform _muzzle;
        [SerializeField] private Animator _animator;
        [SerializeField] private Collider _hitCollider;
        [SerializeField] private CharacterController _controller;

        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int HitHash = Animator.StringToHash("Hit");
        private static readonly int SpeedHash = Animator.StringToHash("Speed");

        private float _nextAttackTime;
        private Transform _player;
        private Vector3 _velocity;
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
            _velocity = Vector3.zero;

            if (_hitCollider != null)
            {
                _hitCollider.enabled = false;
            }

            if (_controller != null)
            {
                _controller.enabled = false;
            }

            HideModel();
            PlayDeathSound();
            SpawnDeathGore();
            Destroy(gameObject, 0.05f);
        }

        private void HideModel()
        {
            var model = transform.Find("Model");
            if (model == null)
            {
                return;
            }

            foreach (var renderer in model.GetComponentsInChildren<Renderer>(true))
            {
                renderer.enabled = false;
            }
        }

        private void SpawnDeathGore()
        {
            if (_definition.DeathGorePrefab == null)
            {
                return;
            }

            var spawnPosition = transform.position + Vector3.up * _definition.AimHeight;
            var gore = Instantiate(_definition.DeathGorePrefab, spawnPosition, Quaternion.identity);
            gore.transform.localScale = Vector3.one * _definition.DeathGoreScale;
        }

        private void Awake()
        {
            if (_controller == null)
            {
                _controller = GetComponent<CharacterController>();
            }
        }

        private void OnEnable()
        {
            _isDead = false;
            _velocity = Vector3.zero;
            if (_hitCollider != null)
            {
                _hitCollider.enabled = true;
            }

            if (_controller != null)
            {
                _controller.enabled = true;
            }
        }

        private void Update()
        {
            if (_isDead || _avatar == null || !_avatar.IsAlive || _player == null || _definition == null)
            {
                ApplyMovement(Vector3.zero);
                SetSpeed(0f);
                return;
            }

            if (_avatar.IsStaggered)
            {
                ApplyMovement(Vector3.zero);
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
                ApplyMovement(transform.forward * _definition.MoveSpeed);
                SetSpeed(_definition.MoveSpeed);
                return;
            }

            ApplyMovement(Vector3.zero);
            SetSpeed(0f);
            TryAttack();
        }

        private void ApplyMovement(Vector3 horizontalVelocity)
        {
            if (_controller == null || _definition == null)
            {
                return;
            }

            if (_controller.isGrounded && _velocity.y < 0f)
            {
                _velocity.y = -2f;
            }

            _velocity.y += _definition.Gravity * Time.deltaTime;
            _controller.Move((horizontalVelocity + new Vector3(0f, _velocity.y, 0f)) * Time.deltaTime);
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
            PlayAttackSound();

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

        private void PlayAttackSound()
        {
            if (_definition.AttackSound == null)
            {
                return;
            }

            var position = _muzzle != null
                ? _muzzle.position
                : transform.position + Vector3.up * _definition.AimHeight;
            AudioSource.PlayClipAtPoint(
                _definition.AttackSound,
                position,
                _definition.AttackSoundVolume);
        }

        private void PlayDeathSound()
        {
            if (_definition.DeathSound == null)
            {
                return;
            }

            var position = transform.position + Vector3.up * _definition.AimHeight;
            AudioSource.PlayClipAtPoint(
                _definition.DeathSound,
                position,
                _definition.DeathSoundVolume);
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
