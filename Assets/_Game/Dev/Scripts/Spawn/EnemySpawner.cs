using SexShot.Dev.Config;
using SexShot.Dev.WorldMarkers;
using UnityEngine;

namespace SexShot.Dev.Spawn
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private SpawnerDefinition _definition;
        [SerializeField] private Transform _enemiesRoot;

        private int _aliveCount;
        private Transform _player;
        private EnemySpawnPoint[] _spawnPoints;
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
            _spawnPoints = FindObjectsByType<EnemySpawnPoint>(FindObjectsSortMode.None);
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
            if (_definition.EnemyPrefab == null || _spawnPoints == null || _spawnPoints.Length == 0 || _player == null)
            {
                return;
            }

            if (_aliveCount >= _definition.MaxCount)
            {
                return;
            }

            var point = _spawnPoints[Random.Range(0, _spawnPoints.Length)];
            var parent = _enemiesRoot != null ? _enemiesRoot : transform;
            var enemy = Instantiate(_definition.EnemyPrefab, point.transform.position, point.transform.rotation, parent);
            var brain = enemy.GetComponent<Enemies.EnemyBrain>();
            var avatar = enemy.GetComponent<Enemies.EnemyAvatar>();
            brain?.SetPlayer(_player);

            if (avatar != null)
            {
                _aliveCount++;
                avatar.Died += () => _aliveCount = Mathf.Max(0, _aliveCount - 1);
            }
        }
    }
}
