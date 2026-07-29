using System.Collections.Generic;
using SexShot.Dev.Config;
using SexShot.Dev.WorldMarkers;
using UnityEngine;

namespace SexShot.Dev.Spawn
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private SpawnerDefinition _definition;
        [SerializeField] private Transform _enemiesRoot;
        [SerializeField] private float _clearanceRadius = 1f;
        [SerializeField] private float _capsuleHeight = 2f;
        [SerializeField] private float _minDistanceFromPlayer = 8f;
        [SerializeField] private float _minSeparation = 4f;

        private int _aliveCount;
        private Transform _player;
        private EnemySpawnPoint[] _spawnPoints;
        private MapSpawnArea _spawnArea;
        private readonly List<Vector3> _occupiedPositions = new();
        private float _spawnTimer;
        private bool _running;

        public int AliveCount => _aliveCount;

        public void StartSpawning(Transform player)
        {
            if (_definition == null)
            {
                Debug.LogError("EnemySpawner requires SpawnerDefinition.", this);
                return;
            }

            _player = player;
            _spawnArea = MapSpawnArea.GetOrCreate();
            _spawnPoints = FindObjectsByType<EnemySpawnPoint>(FindObjectsSortMode.None);
            _occupiedPositions.Clear();
            _aliveCount = 0;
            _spawnTimer = 0f;
            _running = true;

            var toSpawn = Mathf.Min(_definition.InitialCount, _definition.MaxCount);
            for (var i = 0; i < toSpawn; i++)
            {
                SpawnOne();
            }
        }

        public void StopSpawning()
        {
            _running = false;
        }

        private void Update()
        {
            if (!_running || _definition == null || _aliveCount >= _definition.MaxCount)
            {
                return;
            }

            _spawnTimer += Time.deltaTime;
            if (_spawnTimer < _definition.SpawnInterval)
            {
                return;
            }

            _spawnTimer = 0f;
            SpawnOne();
        }

        private void SpawnOne()
        {
            if (_definition.EnemyPrefab == null || _player == null)
            {
                return;
            }

            if (_aliveCount >= _definition.MaxCount)
            {
                return;
            }

            if (!TryGetSpawnPose(out var position, out var rotation))
            {
                return;
            }

            var parent = _enemiesRoot != null ? _enemiesRoot : transform;
            var enemy = Instantiate(_definition.EnemyPrefab, position, rotation, parent);
            PlaySpawnEffects(position);
            var brain = enemy.GetComponent<Enemies.EnemyBrain>();
            var avatar = enemy.GetComponent<Enemies.EnemyAvatar>();
            brain?.SetPlayer(_player);

            if (avatar != null)
            {
                _aliveCount++;
                _occupiedPositions.Add(position);
                avatar.Died += () =>
                {
                    _aliveCount = Mathf.Max(0, _aliveCount - 1);
                    _occupiedPositions.Remove(position);
                };
            }
        }

        private void PlaySpawnEffects(Vector3 position)
        {
            if (_definition.SpawnVfxPrefab != null)
            {
                var vfx = Instantiate(_definition.SpawnVfxPrefab, position, Quaternion.identity);
                vfx.transform.localScale = Vector3.one * _definition.SpawnVfxScale;
                Destroy(vfx, 5f);
            }

            if (_definition.SpawnSound != null)
            {
                AudioSource.PlayClipAtPoint(_definition.SpawnSound, position, _definition.SpawnSoundVolume);
            }
        }

        private bool TryGetSpawnPose(out Vector3 position, out Quaternion rotation)
        {
            position = default;
            rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            if (_spawnArea != null
                && _spawnArea.TryGetSpawnPosition(
                    _clearanceRadius,
                    _capsuleHeight,
                    _player.position,
                    _minDistanceFromPlayer,
                    _occupiedPositions,
                    _minSeparation,
                    out position))
            {
                return true;
            }

            if (_spawnPoints == null || _spawnPoints.Length == 0)
            {
                return false;
            }

            var point = _spawnPoints[Random.Range(0, _spawnPoints.Length)];
            position = point.transform.position;
            rotation = point.transform.rotation;
            return true;
        }
    }
}
