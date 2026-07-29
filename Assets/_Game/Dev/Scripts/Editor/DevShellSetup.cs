using SexShot.Dev.Vfx;
using SexShot.Dev.Weapons;
using UnityEditor;
using UnityEngine;

namespace SexShot.Dev.Editor
{
    public static class DevShellSetup
    {
        private const string PrefabPath = "Assets/_Game/Dev/Prefabs/Vfx/BrassShell.prefab";
        private const string ShellMeshPath =
            "Assets/EffectCore/packs/StylizedProjectilePack1/prefabs/Bullet/shells/gunShells.obj";
        private const string ShellMaterialPath =
            "Assets/_Game/Dev/Materials/EffectCore/packs/StylizedProjectilePack1/prefabs/Bullet/shells/pistolShell.mat";

        [MenuItem("SexShot/Dev/Create Brass Shell Prefab")]
        public static void CreateBrassShellPrefab()
        {
            EnsureFolder("Assets/_Game/Dev/Prefabs/Vfx");

            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(ShellMeshPath);
            if (mesh == null)
            {
                var assets = AssetDatabase.LoadAllAssetsAtPath(ShellMeshPath);
                foreach (var asset in assets)
                {
                    if (asset is Mesh shellMesh)
                    {
                        mesh = shellMesh;
                        break;
                    }
                }
            }

            var material = AssetDatabase.LoadAssetAtPath<Material>(ShellMaterialPath)
                ?? AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/EffectCore/packs/StylizedProjectilePack1/prefabs/Bullet/shells/pistolShell.mat");

            var root = new GameObject("BrassShell");
            var meshFilter = root.AddComponent<MeshFilter>();
            var meshRenderer = root.AddComponent<MeshRenderer>();
            meshFilter.sharedMesh = mesh;
            meshRenderer.sharedMaterial = material;
            root.AddComponent<EjectedShell>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);

            WireWeaponShellPrefabs(prefab);
            AssetDatabase.SaveAssets();
            Debug.Log("SexShot: Brass shell prefab created at " + PrefabPath);
        }

        public static void WireWeaponShellPrefabs(GameObject shellPrefab)
        {
            if (shellPrefab == null)
            {
                return;
            }

            WireWeapon("Assets/_Game/Dev/Config/Weapons/Pistol.asset", shellPrefab);
            WireWeapon("Assets/_Game/Dev/Config/Weapons/Rifle.asset", shellPrefab);
            WireWeapon("Assets/_Game/Dev/Config/Weapons/Shotgun.asset", shellPrefab);
        }

        private static void WireWeapon(string assetPath, GameObject shellPrefab)
        {
            var weapon = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(assetPath);
            if (weapon == null)
            {
                return;
            }

            var so = new SerializedObject(weapon);
            so.FindProperty("_shellPrefab").objectReferenceValue = shellPrefab;
            so.FindProperty("_ejectShells").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(weapon);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
            var folderName = System.IO.Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(folderName))
            {
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }
    }
}
