using System;
using System.Collections.Generic;
using UnityEngine;

namespace SexShot.Dev.Weapons
{
    public class AmmoInventory : MonoBehaviour
    {
        [Serializable]
        private struct StartingAmmoEntry
        {
            public WeaponId WeaponId;
            public int Amount;
        }

        [SerializeField] private StartingAmmoEntry[] _startingLoadout;

        private readonly Dictionary<WeaponId, int> _ammoByWeapon = new();

        public event Action<WeaponId, int> AmmoChanged;

        private void Awake()
        {
            if (_startingLoadout == null)
            {
                return;
            }

            foreach (var entry in _startingLoadout)
            {
                SetAmmo(entry.WeaponId, entry.Amount);
            }
        }

        public int GetAmmo(WeaponId weaponId)
        {
            return _ammoByWeapon.TryGetValue(weaponId, out var ammo) ? ammo : 0;
        }

        public void SetAmmo(WeaponId weaponId, int amount)
        {
            _ammoByWeapon[weaponId] = Mathf.Max(0, amount);
            AmmoChanged?.Invoke(weaponId, _ammoByWeapon[weaponId]);
        }

        public void AddAmmo(WeaponId weaponId, int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            SetAmmo(weaponId, GetAmmo(weaponId) + amount);
        }

        public bool TryConsume(WeaponId weaponId, int amount = 1)
        {
            var current = GetAmmo(weaponId);
            if (current < amount)
            {
                return false;
            }

            SetAmmo(weaponId, current - amount);
            return true;
        }
    }
}
