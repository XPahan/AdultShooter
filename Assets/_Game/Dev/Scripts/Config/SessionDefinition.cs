using UnityEngine;

namespace SexShot.Dev.Config
{
    [CreateAssetMenu(fileName = "SessionDefinition", menuName = "SexShot/Dev/Session Definition")]
    public class SessionDefinition : ScriptableObject
    {
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private bool _pauseOnPlayerDeath = true;
        [SerializeField] private AudioClip _backgroundMusic;
        [SerializeField] private float _backgroundMusicVolume = 0.35f;

        public GameObject PlayerPrefab => _playerPrefab;
        public bool PauseOnPlayerDeath => _pauseOnPlayerDeath;
        public AudioClip BackgroundMusic => _backgroundMusic;
        public float BackgroundMusicVolume => Mathf.Clamp01(_backgroundMusicVolume);
    }
}
