using UnityEngine;

namespace SexShot.Dev.Weapons
{
    [CreateAssetMenu(fileName = "WeaponDefinition", menuName = "SexShot/Dev/Weapon Definition")]
    public class WeaponDefinition : ScriptableObject
    {
        [SerializeField] private WeaponId _weaponId;
        [SerializeField] private string _displayName;
        [SerializeField] private float _damage = 1f;
        [SerializeField] private float _fireCooldown = 0.25f;
        [SerializeField] private bool _automatic;
        [SerializeField] private int _pelletsPerShot = 1;
        [SerializeField] private float _spreadDegrees;
        [SerializeField] private float _projectileSpeed = 40f;
        [SerializeField] private int _startingAmmo = 20;
        [SerializeField] private int _ammoPerPickup = 5;
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private GameObject _worldModelPrefab;

        public WeaponId WeaponId => _weaponId;
        public string DisplayName => string.IsNullOrWhiteSpace(_displayName) ? _weaponId.ToString() : _displayName;
        public float Damage => _damage;
        public float FireCooldown => _fireCooldown;
        public bool Automatic => _automatic;
        public int PelletsPerShot => Mathf.Max(1, _pelletsPerShot);
        public float SpreadDegrees => _spreadDegrees;
        public float ProjectileSpeed => _projectileSpeed;
        public int StartingAmmo => Mathf.Max(0, _startingAmmo);
        public int AmmoPerPickup => Mathf.Max(0, _ammoPerPickup);
        public GameObject ProjectilePrefab => _projectilePrefab;
        public GameObject WorldModelPrefab => _worldModelPrefab;
    }
}
