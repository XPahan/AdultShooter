using SexShot.Dev.Config;
using SexShot.Dev.Vfx;
using UnityEditor;
using UnityEngine;

namespace SexShot.Dev.Editor
{
    public static class DevGoreVfxSetup
    {
        private const string PrefabPath = "Assets/_Game/Dev/Prefabs/Vfx/EnemyGoreBurst.prefab";
        private const string EnemyConfigPath = "Assets/_Game/Dev/Config/Enemies/Succubus.asset";

        [MenuItem("SexShot/Dev/Create Enemy Gore VFX")]
        public static void CreateEnemyGoreVfx()
        {
            EnsureFolder("Assets/_Game/Dev/Prefabs/Vfx");

            var root = new GameObject("EnemyGoreBurst");
            var effect = root.AddComponent<GoreDeathEffect>();

            var so = new SerializedObject(effect);
            so.FindProperty("_lifetime").floatValue = 5f;
            so.FindProperty("_partForceMin").floatValue = 22f;
            so.FindProperty("_partForceMax").floatValue = 42f;
            so.FindProperty("_partDespawnDelay").floatValue = 1f;
            so.FindProperty("_overallScale").floatValue = 1.1f;
            so.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);

            WireEnemyDefinition(prefab);
            AssetDatabase.SaveAssets();
            Debug.Log("SexShot: Enemy gore VFX created at " + PrefabPath);
        }

        public static void WireEnemyDefinition(GameObject gorePrefab)
        {
            var definition = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(EnemyConfigPath);
            if (definition == null || gorePrefab == null)
            {
                return;
            }

            var so = new SerializedObject(definition);
            so.FindProperty("_deathGorePrefab").objectReferenceValue = gorePrefab;
            so.FindProperty("_deathGoreScale").floatValue = 1f;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
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
