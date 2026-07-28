using UnityEngine;

namespace SexShot.Dev.Combat
{
    public static class DamageableUtility
    {
        public static IDamageable Find(Collider collider)
        {
            if (collider == null)
            {
                return null;
            }

            var current = collider.transform;
            while (current != null)
            {
                var components = current.GetComponents<MonoBehaviour>();
                for (var i = 0; i < components.Length; i++)
                {
                    if (components[i] is IDamageable damageable)
                    {
                        return damageable;
                    }
                }

                current = current.parent;
            }

            return null;
        }
    }
}
