using SexShot.Dev.Config;
using SexShot.Dev.Weapons;
using UnityEngine;

namespace SexShot.Dev.Ammo
{
    [RequireComponent(typeof(Collider))]
    public class AmmoPickup : MonoBehaviour
    {
        [SerializeField] private AmmoPickupDefinition _definition;
        [SerializeField] private GameObject _vfxRoot;

        private bool _collected;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_collected || _definition == null)
            {
                return;
            }

            var inventory = other.GetComponentInParent<AmmoInventory>();
            if (inventory == null)
            {
                return;
            }

            Collect(inventory);
        }

        private void Collect(AmmoInventory inventory)
        {
            _collected = true;
            foreach (var grant in _definition.Grants)
            {
                inventory.AddAmmo(grant.WeaponId, grant.Amount);
            }

            if (_vfxRoot != null)
            {
                _vfxRoot.SetActive(false);
            }

            gameObject.SetActive(false);
        }
    }
}
