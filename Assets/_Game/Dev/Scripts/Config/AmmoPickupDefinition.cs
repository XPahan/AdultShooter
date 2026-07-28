using SexShot.Dev.Weapons;
using UnityEngine;

namespace SexShot.Dev.Config
{
    [System.Serializable]
    public struct AmmoGrant
    {
        public WeaponId WeaponId;
        public int Amount;
    }

    [CreateAssetMenu(fileName = "AmmoPickupDefinition", menuName = "SexShot/Dev/Ammo Pickup Definition")]
    public class AmmoPickupDefinition : ScriptableObject
    {
        [SerializeField] private AmmoGrant[] _grants = new[]
        {
            new AmmoGrant { WeaponId = WeaponId.Pistol, Amount = 5 },
            new AmmoGrant { WeaponId = WeaponId.Shotgun, Amount = 2 },
            new AmmoGrant { WeaponId = WeaponId.Rifle, Amount = 10 }
        };

        public AmmoGrant[] Grants => _grants;
    }
}
