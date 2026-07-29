using UnityEngine;

namespace SexShot.Dev.Spawn
{
    public class MapSpawnArea : MonoBehaviour
    {
        [SerializeField] private Collider _boundsCollider;
        [SerializeField] private float _margin = 3f;
        [SerializeField] private float _raycastStartHeight = 40f;
        [SerializeField] private float _minSurfaceNormalY = 0.55f;
        [SerializeField] private LayerMask _obstacleMask = ~0;
        [SerializeField] private int _maxAttempts = 40;

        private void Awake()
        {
            if (_boundsCollider == null)
            {
                _boundsCollider = ResolveBoundsCollider();
            }
        }

        public static MapSpawnArea GetOrCreate()
        {
            var area = FindFirstObjectByType<MapSpawnArea>();
            if (area != null)
            {
                return area;
            }

            var go = new GameObject("MapSpawnArea");
            return go.AddComponent<MapSpawnArea>();
        }

        public bool TryGetSpawnPosition(
            float clearanceRadius,
            float capsuleHeight,
            Vector3 avoidCenter,
            float avoidRadius,
            System.Collections.Generic.List<Vector3> occupiedPositions,
            float minSeparation,
            out Vector3 position)
        {
            position = default;

            if (_boundsCollider == null)
            {
                _boundsCollider = ResolveBoundsCollider();
            }

            if (_boundsCollider == null)
            {
                return false;
            }

            var bounds = _boundsCollider.bounds;
            var minX = bounds.min.x + _margin;
            var maxX = bounds.max.x - _margin;
            var minZ = bounds.min.z + _margin;
            var maxZ = bounds.max.z - _margin;

            if (minX >= maxX || minZ >= maxZ)
            {
                return false;
            }

            for (var attempt = 0; attempt < _maxAttempts; attempt++)
            {
                var x = Random.Range(minX, maxX);
                var z = Random.Range(minZ, maxZ);
                var rayOrigin = new Vector3(x, bounds.max.y + _raycastStartHeight, z);

                if (!Physics.Raycast(
                        rayOrigin,
                        Vector3.down,
                        out var hit,
                        _raycastStartHeight + 100f,
                        _obstacleMask,
                        QueryTriggerInteraction.Ignore))
                {
                    continue;
                }

                if (hit.normal.y < _minSurfaceNormalY)
                {
                    continue;
                }

                var floorY = hit.point.y;
                var capsuleBottomY = floorY + clearanceRadius + 0.05f;
                var capsuleTopY = capsuleBottomY + Mathf.Max(capsuleHeight, clearanceRadius * 2f);
                var capsuleBottom = new Vector3(x, capsuleBottomY, z);
                var capsuleTop = new Vector3(x, capsuleTopY, z);

                if (Physics.CheckCapsule(
                        capsuleBottom,
                        capsuleTop,
                        clearanceRadius,
                        _obstacleMask,
                        QueryTriggerInteraction.Ignore))
                {
                    continue;
                }

                var candidate = new Vector3(x, floorY, z);

                if (avoidRadius > 0f
                    && HorizontalDistance(candidate, avoidCenter) < avoidRadius)
                {
                    continue;
                }

                if (occupiedPositions != null && minSeparation > 0f)
                {
                    var tooClose = false;
                    foreach (var occupied in occupiedPositions)
                    {
                        if (HorizontalDistance(candidate, occupied) < minSeparation)
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    if (tooClose)
                    {
                        continue;
                    }
                }

                position = candidate;
                return true;
            }

            return false;
        }

        private static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }

        private Collider ResolveBoundsCollider()
        {
            var world = GameObject.Find("World");
            if (world != null)
            {
                foreach (Transform child in world.transform)
                {
                    if (child.name == "Plane")
                    {
                        var planeCollider = child.GetComponent<Collider>();
                        if (planeCollider != null)
                        {
                            return planeCollider;
                        }
                    }
                }
            }

            var colliders = FindObjectsByType<Collider>(FindObjectsSortMode.None);
            Collider best = null;
            var bestArea = 0f;

            foreach (var collider in colliders)
            {
                if (collider.isTrigger)
                {
                    continue;
                }

                var colliderBounds = collider.bounds;
                var area = colliderBounds.size.x * colliderBounds.size.z;
                if (area > bestArea)
                {
                    bestArea = area;
                    best = collider;
                }
            }

            return best;
        }

        private void OnDrawGizmosSelected()
        {
            if (_boundsCollider == null)
            {
                _boundsCollider = ResolveBoundsCollider();
            }

            if (_boundsCollider == null)
            {
                return;
            }

            var bounds = _boundsCollider.bounds;
            Gizmos.color = new Color(0.2f, 1f, 0.45f, 0.25f);
            var size = bounds.size;
            size.x = Mathf.Max(0f, size.x - _margin * 2f);
            size.z = Mathf.Max(0f, size.z - _margin * 2f);
            Gizmos.DrawCube(bounds.center, size);
        }
    }
}
