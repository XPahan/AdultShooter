using SexShot.Dev.Config;
using UnityEngine;

namespace SexShot.Dev.Combat
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private ProjectileDefinition _definition;

        private float _damage;
        private bool _hasHit;
        private float _speed;
        private DamageTeam _team;
        private Vector3 _direction;
        private Rigidbody _rigidbody;
        private SphereCollider _sphereCollider;
        private Vector3 _previousPosition;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _sphereCollider = GetComponent<SphereCollider>();
        }

        public void Launch(Vector3 direction, float speed, float damage, DamageTeam team)
        {
            _direction = direction.normalized;
            _speed = speed;
            _damage = damage;
            _team = team;
            _hasHit = false;
            _previousPosition = transform.position;
            if (_direction.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(_direction);
            }

            var lifetime = _definition != null ? _definition.Lifetime : 5f;
            Destroy(gameObject, lifetime);
        }

        private void FixedUpdate()
        {
            if (_hasHit || _direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            var step = _direction * (_speed * Time.fixedDeltaTime);
            var distance = step.magnitude;
            if (distance <= 0f)
            {
                return;
            }

            var radius = GetWorldRadius();
            if (Physics.SphereCast(
                    _previousPosition,
                    radius,
                    _direction,
                    out var hit,
                    distance,
                    ~0,
                    QueryTriggerInteraction.Collide)
                && TryHit(hit.collider))
            {
                return;
            }

            var nextPosition = _rigidbody.position + step;
            _rigidbody.MovePosition(nextPosition);
            _previousPosition = nextPosition;
        }

        private void OnTriggerEnter(Collider other)
        {
            TryHit(other);
        }

        private bool TryHit(Collider other)
        {
            if (_hasHit || other == null)
            {
                return false;
            }

            var damageable = DamageableUtility.Find(other);
            if (damageable != null)
            {
                if (damageable.Team == _team || !damageable.IsAlive)
                {
                    return false;
                }

                damageable.TakeDamage(_damage, _team);
                DestroyProjectile();
                return true;
            }

            if (!other.isTrigger)
            {
                DestroyProjectile();
                return true;
            }

            return false;
        }

        private float GetWorldRadius()
        {
            if (_sphereCollider == null)
            {
                return 0.1f;
            }

            var scale = transform.lossyScale;
            var maxScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
            return _sphereCollider.radius * maxScale;
        }

        private void DestroyProjectile()
        {
            _hasHit = true;
            Destroy(gameObject);
        }
    }
}
