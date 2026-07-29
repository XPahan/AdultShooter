using System.Collections.Generic;
using UnityEngine;

namespace SexShot.Dev.Spawn
{
    public class AmmoSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _ammoPrefab;
        [SerializeField] private Transform _pickupsRoot;
        [SerializeField] private int _spawnCount = 12;
        [SerializeField] private float _clearanceRadius = 0.35f;
        [SerializeField] private float _capsuleHeight = 0.6f;
        [SerializeField] private float _minDistanceFromPlayer = 5f;
        [SerializeField] private float _minSeparation = 3f;

        public void SpawnPickups(Vector3 playerPosition)
        {
            if (_ammoPrefab == null || _spawnCount <= 0)
            {
                return;
            }

            var spawnArea = MapSpawnArea.GetOrCreate();

            var parent = _pickupsRoot != null ? _pickupsRoot : transform;
            var occupied = new List<Vector3>(_spawnCount);

            for (var i = 0; i < _spawnCount; i++)
            {
                if (!spawnArea.TryGetSpawnPosition(
                        _clearanceRadius,
                        _capsuleHeight,
                        playerPosition,
                        _minDistanceFromPlayer,
                        occupied,
                        _minSeparation,
                        out var position))
                {
                    continue;
                }

                var pickup = Instantiate(_ammoPrefab, position, Quaternion.identity, parent);
                pickup.name = $"AmmoPickup_{i + 1}";
                occupied.Add(position);
            }
        }
    }
}
