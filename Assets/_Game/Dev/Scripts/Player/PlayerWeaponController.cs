using SexShot.Dev.Vfx;
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
        [SerializeField] private PlayerLook _look;
        [SerializeField] private GameObject[] _weaponModels;

        private bool _fireHeld;
        private bool _inputEnabled = true;
        private float _nextFireTime;
        private int _activeIndex;
        private Transform _muzzle;
        private Transform _muzzleFlashPoint;
        private Transform _shellEjectPoint;
        private Transform _activeWeaponTransform;
        private Vector3 _weaponBaseLocalPosition;
        private Quaternion _weaponBaseLocalRotation;
        private Vector3 _weaponRecoilPositionOffset;
        private Vector3 _weaponRecoilRotationOffset;

        private const float ProjectileSpawnOffset = 0.45f;
        private const float WeaponRecoilReturnSpeed = 18f;
        private const float WeaponRecoilRotationReturnSpeed = 20f;
        private const float CameraRecoilPitchMultiplier = 0.45f;
        private const float CameraRecoilYawMultiplier = 0.45f;

        // Visual recoil for weapon mesh.
        private const float WeaponMeshKickStrengthMultiplier = 1.6f;
        private const float WeaponMeshBackMultiplier = 1.9f;
        private const float WeaponMeshRotationMultiplier = 1.35f;

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

        private void Awake()
        {
            if (_look == null)
            {
                _look = GetComponent<PlayerLook>();
            }

            UpdateWeaponFirePoints();
        }

        public void SetInputEnabled(bool enabled)
        {
            _inputEnabled = enabled;
            _fireHeld = false;
        }

        private void Update()
        {
            UpdateWeaponModelRecoil();

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

            UpdateWeaponFirePoints();
        }

        private void UpdateWeaponFirePoints()
        {
            _muzzle = null;
            _muzzleFlashPoint = null;
            _shellEjectPoint = null;
            _activeWeaponTransform = null;

            var model = GetActiveWeaponModel();
            if (model == null)
            {
                return;
            }

            _activeWeaponTransform = model.transform;
            _weaponBaseLocalPosition = _activeWeaponTransform.localPosition;
            _weaponBaseLocalRotation = _activeWeaponTransform.localRotation;
            _weaponRecoilPositionOffset = Vector3.zero;
            _weaponRecoilRotationOffset = Vector3.zero;

            _muzzle = model.transform.Find("Muzzle");
            _muzzleFlashPoint = model.transform.Find("MuzzleFlash");
            _shellEjectPoint = model.transform.Find("ShellEject");

            if (_muzzleFlashPoint == null)
            {
                _muzzleFlashPoint = _muzzle;
            }

            if (_shellEjectPoint == null)
            {
                _shellEjectPoint = _muzzle;
            }
        }

        private GameObject GetActiveWeaponModel()
        {
            if (_weaponModels == null || _activeIndex < 0 || _activeIndex >= _weaponModels.Length)
            {
                return null;
            }

            return _weaponModels[_activeIndex];
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

            SpawnMuzzleFlash(weapon);
            SpawnShell(weapon);
            ApplyRecoil(weapon);

            for (var i = 0; i < weapon.PelletsPerShot; i++)
            {
                var direction = ApplySpread(_muzzle.forward, weapon.SpreadDegrees);
                var spawnPosition = _muzzle.position + direction * ProjectileSpawnOffset;
                var instance = Instantiate(weapon.ProjectilePrefab, spawnPosition, Quaternion.LookRotation(direction));
                var projectile = instance.GetComponent<Projectile>();
                if (projectile == null)
                {
                    continue;
                }

                projectile.Launch(
                    direction,
                    weapon.ProjectileSpeed,
                    weapon.Damage,
                    DamageTeam.Player,
                    weapon.ImpactPrefab);
            }
        }

        private void SpawnMuzzleFlash(WeaponDefinition weapon)
        {
            if (weapon.MuzzleFlashPrefab == null)
            {
                return;
            }

            if (_muzzleFlashPoint == null)
            {
                return;
            }

            // Parent it to the fire point so it stays visually locked while the weapon/model moves.
            var flash = Instantiate(
                weapon.MuzzleFlashPrefab,
                _muzzleFlashPoint.position,
                _muzzleFlashPoint.rotation,
                _muzzleFlashPoint);

            flash.transform.localPosition = Vector3.zero;
            flash.transform.localRotation = Quaternion.identity;
            flash.transform.localScale = Vector3.one * weapon.MuzzleFlashScale;
        }

        private void SpawnShell(WeaponDefinition weapon)
        {
            if (!weapon.EjectShells || weapon.ShellPrefab == null)
            {
                return;
            }

            if (_shellEjectPoint == null)
            {
                return;
            }

            var shell = Instantiate(weapon.ShellPrefab, _shellEjectPoint.position, _shellEjectPoint.rotation);
            if (shell.TryGetComponent<EjectedShell>(out var ejectedShell))
            {
                ejectedShell.Launch(_shellEjectPoint.right);
                return;
            }

            Destroy(shell, 5f);
        }

        private void ApplyRecoil(WeaponDefinition weapon)
        {
            if (_look == null || weapon == null)
            {
                return;
            }

            var yawKick = weapon.RecoilYaw > 0f
                ? Random.Range(-weapon.RecoilYaw, weapon.RecoilYaw)
                : 0f;
            _look.ApplyRecoil(weapon.RecoilPitch * CameraRecoilPitchMultiplier, yawKick * CameraRecoilYawMultiplier);
            ApplyWeaponModelRecoil(weapon, yawKick);
        }

        private void ApplyWeaponModelRecoil(WeaponDefinition weapon, float yawKick)
        {
            if (_activeWeaponTransform == null || weapon == null)
            {
                return;
            }

            var kickStrength = Mathf.Max(0.02f, weapon.RecoilPitch * 0.012f * WeaponMeshKickStrengthMultiplier);
            _weaponRecoilPositionOffset += new Vector3(
                yawKick * 0.0025f,
                -kickStrength * 0.25f,
                -kickStrength * WeaponMeshBackMultiplier);
            _weaponRecoilRotationOffset += new Vector3(
                weapon.RecoilPitch * 0.9f * WeaponMeshRotationMultiplier,
                yawKick * 2.5f * WeaponMeshRotationMultiplier,
                -yawKick * 4f * WeaponMeshRotationMultiplier);
        }

        private void UpdateWeaponModelRecoil()
        {
            if (_activeWeaponTransform == null)
            {
                return;
            }

            _weaponRecoilPositionOffset = Vector3.Lerp(
                _weaponRecoilPositionOffset,
                Vector3.zero,
                Time.deltaTime * WeaponRecoilReturnSpeed);
            _weaponRecoilRotationOffset = Vector3.Lerp(
                _weaponRecoilRotationOffset,
                Vector3.zero,
                Time.deltaTime * WeaponRecoilRotationReturnSpeed);

            _activeWeaponTransform.localPosition = _weaponBaseLocalPosition + _weaponRecoilPositionOffset;
            _activeWeaponTransform.localRotation = _weaponBaseLocalRotation * Quaternion.Euler(_weaponRecoilRotationOffset);
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
