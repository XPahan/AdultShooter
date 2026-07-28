using UnityEngine;

namespace SexShot.Dev.Config
{
    [CreateAssetMenu(fileName = "ProjectileDefinition", menuName = "SexShot/Dev/Projectile Definition")]
    public class ProjectileDefinition : ScriptableObject
    {
        [SerializeField] private float _lifetime = 5f;
        [SerializeField] private GameObject _prefab;

        public float Lifetime => _lifetime;
        public GameObject Prefab => _prefab;
    }
}
