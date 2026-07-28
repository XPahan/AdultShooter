using SexShot.Dev.Combat;
using SexShot.Dev.Config;
using SexShot.Dev.Weapons;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SexShot.Dev.Player
{
    public class PlayerWeaponController : MonoBehaviour
    {
        [SerializeField] private PlayerDefinition _definition;
        [SerializeField] private AmmoInventory _ammoInventory;
        [SerializeField] private Transform _muzzle;
        [SerializeField] private GameObject[] _weaponModels;

        private bool _fireHeld;
        private bool _inputEnabled = true;
        private float _nextFireTime;
        private int _activeIndex;

        public WeaponDefinition ActiveWeapon
        {
            get
            {
                var weapons = _definition != null ? _definition.Weapons : null;
                return weapons != null && weapons.Length > 0
                    ? weapons[Mathf.Clamp(_activeIndex, 0, weapons.Length - 1)]
                    : null;
            }
        }

        public AmmoInventory AmmoInventory => _ammoInventory;

        public void SetInputEnabled(bool enabled)
        {
            _inputEnabled = enabled;
            _fireHeld = false;
        }

        private void Update()
        {
            if (!_inputEnabled || ActiveWeapon == null)
            {
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.digit1Key.wasPressedThisFrame) SelectWeapon(0);
                if (keyboard.digit2Key.wasPressedThisFrame) SelectWeapon(1);
                if (keyboard.digit3Key.wasPressedThisFrame) SelectWeapon(2);
            }

            var mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            if (mouse.leftButton.wasPressedThisFrame)
            {
                _fireHeld = true;
                TryFire();
            }

            if (mouse.leftButton.wasReleasedThisFrame)
            {
                _fireHeld = false;
            }

            if (_fireHeld && ActiveWeapon.Automatic)
            {
                TryFire();
            }
        }

        private void SelectWeapon(int index)
        {
            var weapons = _definition != null ? _definition.Weapons : null;
            if (weapons == null || index < 0 || index >= weapons.Length || weapons[index] == null)
            {
                return;
            }

            _activeIndex = index;
            if (_weaponModels == null)
            {
                return;
            }

            for (var i = 0; i < _weaponModels.Length; i++)
            {
                if (_weaponModels[i] != null)
                {
                    _weaponModels[i].SetActive(i == _activeIndex);
                }
            }
        }

        private void TryFire()
        {
            var weapon = ActiveWeapon;
            if (weapon == null || Time.time < _nextFireTime)
            {
                return;
            }

            if (!_ammoInventory.TryConsume(weapon.WeaponId))
            {
                return;
            }

            _nextFireTime = Time.time + weapon.FireCooldown;
            FireProjectiles(weapon);
        }

        private void FireProjectiles(WeaponDefinition weapon)
        {
            if (weapon.ProjectilePrefab == null || _muzzle == null)
            {
                return;
            }

            for (var i = 0; i < weapon.PelletsPerShot; i++)
            {
                var direction = ApplySpread(_muzzle.forward, weapon.SpreadDegrees);
                var instance = Instantiate(weapon.ProjectilePrefab, _muzzle.position, Quaternion.LookRotation(direction));
                instance.GetComponent<Projectile>().Launch(
                    direction,
                    weapon.ProjectileSpeed,
                    weapon.Damage,
                    DamageTeam.Player);
            }
        }

        private static Vector3 ApplySpread(Vector3 forward, float spreadDegrees)
        {
            if (spreadDegrees <= 0f)
            {
                return forward;
            }

            return Quaternion.Euler(
                Random.Range(-spreadDegrees, spreadDegrees),
                Random.Range(-spreadDegrees, spreadDegrees),
                0f) * forward;
        }
    }
}
