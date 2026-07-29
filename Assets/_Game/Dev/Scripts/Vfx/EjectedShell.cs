using UnityEngine;

namespace SexShot.Dev.Vfx
{
    public class EjectedShell : MonoBehaviour
    {
        [SerializeField] private float _scale = 0.22f;
        [SerializeField] private float _lifetime = 5f;
        [SerializeField] private float _collisionDespawnDelay = 1.5f;
        [SerializeField] private float _ejectForce = 2.5f;
        [SerializeField] private float _ejectTorque = 6f;

        public void Launch(Vector3 direction)
        {
            transform.localScale = Vector3.one * _scale;

            if (!TryGetComponent<Rigidbody>(out var rb))
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }

            rb.isKinematic = false;
            rb.useGravity = true;
            rb.mass = 0.015f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            EnsureCollider();

            var launchDirection = direction.sqrMagnitude > 0.0001f
                ? direction.normalized
                : transform.right;
            launchDirection = (launchDirection + Random.insideUnitSphere * 0.12f).normalized;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddForce(launchDirection * _ejectForce, ForceMode.VelocityChange);
            rb.AddTorque(Random.insideUnitSphere * _ejectTorque, ForceMode.Impulse);

            if (!TryGetComponent<GorePartDespawnOnCollision>(out var despawn))
            {
                despawn = gameObject.AddComponent<GorePartDespawnOnCollision>();
            }

            despawn.Initialize(_collisionDespawnDelay, _lifetime);
        }

        private void EnsureCollider()
        {
            if (TryGetComponent<Collider>(out _))
            {
                return;
            }

            if (TryGetComponent<MeshFilter>(out var meshFilter) && meshFilter.sharedMesh != null)
            {
                var meshCollider = gameObject.AddComponent<MeshCollider>();
                meshCollider.convex = true;
                meshCollider.sharedMesh = meshFilter.sharedMesh;
                return;
            }

            gameObject.AddComponent<BoxCollider>();
        }
    }
}
