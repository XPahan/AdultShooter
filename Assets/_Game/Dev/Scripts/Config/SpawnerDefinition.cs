using UnityEngine;

namespace SexShot.Dev.Config
{
    [CreateAssetMenu(fileName = "SpawnerDefinition", menuName = "SexShot/Dev/Spawner Definition")]
    public class SpawnerDefinition : ScriptableObject
    {
        [SerializeField] private GameObject _enemyPrefab;
        [SerializeField] private int _initialCount = 5;
        [SerializeField] private int _maxCount = 15;
        [SerializeField] private float _spawnInterval = 3f;

        public GameObject EnemyPrefab => _enemyPrefab;
        public int InitialCount => _initialCount;
        public int MaxCount => _maxCount;
        public float SpawnInterval => _spawnInterval;
    }
}
