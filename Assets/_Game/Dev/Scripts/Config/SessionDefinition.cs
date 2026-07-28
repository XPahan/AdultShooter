using UnityEngine;

namespace SexShot.Dev.Config
{
    [CreateAssetMenu(fileName = "SessionDefinition", menuName = "SexShot/Dev/Session Definition")]
    public class SessionDefinition : ScriptableObject
    {
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private bool _pauseOnPlayerDeath = true;

        public GameObject PlayerPrefab => _playerPrefab;
        public bool PauseOnPlayerDeath => _pauseOnPlayerDeath;
    }
}
