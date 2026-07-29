using SexShot.Dev.Config;
using SexShot.Dev.Player;
using SexShot.Dev.Spawn;
using SexShot.Dev.WorldMarkers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SexShot.Dev.Session
{
    public class GameSession : MonoBehaviour
    {
        [SerializeField] private SessionDefinition _definition;
        [SerializeField] private EnemySpawner _enemySpawner;
        [SerializeField] private AmmoSpawner _ammoSpawner;
        [SerializeField] private Transform _runtimeRoot;
        [SerializeField] private Camera _sceneCameraToDisable;

        private PlayerAvatar _player;
        private bool _sessionActive;
        private AudioSource _musicSource;

        public PlayerAvatar Player => _player;
        public bool IsSessionActive => _sessionActive;

        private void Start()
        {
            BeginSession();
        }

        public void BeginSession()
        {
            if (_sessionActive || _definition == null)
            {
                return;
            }

            var spawn = FindFirstObjectByType<PlayerSpawnPoint>();
            var spawnPosition = spawn != null ? spawn.transform.position : Vector3.up;
            var spawnRotation = spawn != null ? spawn.transform.rotation : Quaternion.identity;

            if (_sceneCameraToDisable == null)
            {
                _sceneCameraToDisable = Camera.main;
            }

            if (_sceneCameraToDisable != null)
            {
                _sceneCameraToDisable.gameObject.SetActive(false);
            }

            var parent = _runtimeRoot != null ? _runtimeRoot : transform;
            var playerGo = Instantiate(_definition.PlayerPrefab, spawnPosition, spawnRotation, parent);
            _player = playerGo.GetComponent<PlayerAvatar>();
            _player.Died += EndSession;

            if (_enemySpawner != null)
            {
                _enemySpawner.StartSpawning(playerGo.transform);
            }

            if (_ammoSpawner != null)
            {
                _ammoSpawner.SpawnPickups(spawnPosition);
            }

            StartBackgroundMusic();
            _sessionActive = true;
        }

        public void EndSession()
        {
            if (!_sessionActive)
            {
                return;
            }

            _sessionActive = false;
            _enemySpawner?.StopSpawning();

            _player?.Weapons?.SetInputEnabled(false);

            if (_definition != null && _definition.PauseOnPlayerDeath)
            {
                Time.timeScale = 0f;
            }

            Debug.Log("GameSession ended — player died.");
        }

        public void RestartSession()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void QuitGame()
        {
            Time.timeScale = 1f;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void StartBackgroundMusic()
        {
            if (_definition.BackgroundMusic == null)
            {
                return;
            }

            if (_musicSource == null)
            {
                _musicSource = gameObject.AddComponent<AudioSource>();
                _musicSource.playOnAwake = false;
                _musicSource.loop = true;
                _musicSource.spatialBlend = 0f;
            }

            _musicSource.clip = _definition.BackgroundMusic;
            _musicSource.volume = _definition.BackgroundMusicVolume;
            if (!_musicSource.isPlaying)
            {
                _musicSource.Play();
            }
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
        }
    }
}
