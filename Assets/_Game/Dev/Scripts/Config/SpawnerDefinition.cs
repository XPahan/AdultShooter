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
        [SerializeField] private GameObject _spawnVfxPrefab;
        [SerializeField] private float _spawnVfxScale = 1f;
        [SerializeField] private AudioClip _spawnSound;
        [SerializeField] private float _spawnSoundVolume = 1f;

        public GameObject EnemyPrefab => _enemyPrefab;
        public int InitialCount => _initialCount;
        public int MaxCount => _maxCount;
        public float SpawnInterval => _spawnInterval;
        public GameObject SpawnVfxPrefab => _spawnVfxPrefab;
        public float SpawnVfxScale => Mathf.Max(0.01f, _spawnVfxScale);
        public AudioClip SpawnSound => _spawnSound;
        public float SpawnSoundVolume => Mathf.Clamp01(_spawnSoundVolume);
    }
}
