using System.IO;
using SexShot.Dev.Ammo;
using SexShot.Dev.Combat;
using SexShot.Dev.Config;
using SexShot.Dev.Enemies;
using SexShot.Dev.Player;
using SexShot.Dev.Session;
using SexShot.Dev.Spawn;
using SexShot.Dev.Weapons;
using SexShot.Dev.WorldMarkers;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SexShot.Dev.Editor
{
    public static class DevPrototypeSetup
    {
        private const string DevRoot = "Assets/_Game/Dev";
        private const string PrefabRoot = DevRoot + "/Prefabs";
        private const string ConfigRoot = DevRoot + "/Config";
        private const string WeaponsConfigRoot = ConfigRoot + "/Weapons";
        private const string MaterialsRoot = DevRoot + "/Materials";
        private const string AnimatorPath = DevRoot + "/Animators/Enemy.controller";
        private const string EffectCoreBulletRoot =
            "Assets/EffectCore/packs/StylizedProjectilePack1/prefabs/Bullet/Bullet_GoldFire_Template/Bullet_Small_Goldfire_Template";
        private const string EffectCoreGoldFireMediumRoot =
            "Assets/EffectCore/packs/StylizedProjectilePack1/prefabs/Bullet/Bullet_GoldFire_Template/Bullet_Medium_GoldFire_Template";
        private const string EffectCoreGoldFireBigRoot =
            "Assets/EffectCore/packs/StylizedProjectilePack1/prefabs/Bullet/Bullet_GoldFire_Template/Bullet_Big_GoldFire_Template";
        private const string EffectCoreBlazingRedMediumRoot =
            "Assets/EffectCore/packs/StylizedProjectilePack1/prefabs/Bullet/Bullet_BlazingRed/Bullet_Medium_BlazingRed";
        private const string EffectCorePlasmaPurpleHazeMediumRoot =
            "Assets/EffectCore/packs/StylizedProjectilePack1/prefabs/Plasma/Plasma_PurpleHaze/Plasma_Medium_PurpleHaze";
        private const string EffectCoreShellPrefab =
            "Assets/EffectCore/packs/StylizedProjectilePack1/prefabs/Bullet/shells/bulletShell_ParticleSystem.prefab";
        private const string EffectCoreShotgunShellPrefab =
            "Assets/EffectCore/packs/StylizedProjectilePack1/prefabs/Bullet/shells/shotgunShell_rigidBody.prefab";
        private const string DevBrassShellPrefab = PrefabRoot + "/Vfx/BrassShell.prefab";

        [MenuItem("SexShot/Dev/Wire Enemy Prefab References")]
        public static void WireEnemyPrefabMenu()
        {
            var definition = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(ConfigRoot + "/Enemies/Succubus.asset");
            if (definition == null)
            {
                Debug.LogError("SexShot: Succubus.asset not found.");
                return;
            }

            WireEnemyPrefabReferences(PrefabRoot + "/Enemies/Enemy.prefab", definition);
            AssetDatabase.SaveAssets();
            Debug.Log("SexShot: Enemy prefab references wired.");
        }

        [MenuItem("SexShot/Dev/Wire Rifle GoldFire VFX")]
        public static void WireRifleGoldFireVfxMenu()
        {
            WireCombatVfx();
            AssetDatabase.SaveAssets();
            Debug.Log("SexShot: Combat VFX wired for all weapons and enemies.");
        }

        [MenuItem("SexShot/Dev/Wire Combat VFX")]
        public static void WireCombatVfxMenu()
        {
            WireCombatVfx();
            AssetDatabase.SaveAssets();
            Debug.Log("SexShot: Combat VFX wired for pistol, shotgun, rifle and enemies.");
        }

        [MenuItem("SexShot/Dev/Ensure Weapon Fire Points")]
        public static void EnsureWeaponFirePointsMenu()
        {
            EnsureAllWeaponPrefabFirePoints();
            CleanupLegacyPlayerFirePoints(PrefabRoot + "/Player/Player.prefab");
            AssetDatabase.SaveAssets();
            Debug.Log("SexShot: Muzzle, MuzzleFlash and ShellEject added to weapon prefabs.");
        }

        [MenuItem("SexShot/Dev/Setup Prototype Prefabs")]
        public static void SetupAll()
        {
            EnsureFolders();
            DevMaterialLibrary.BuildAll();
            var playerProjectile = CreateProjectilePrefab(
                PrefabRoot + "/Projectiles/PlayerProjectile.prefab",
                "PlayerProjectile",
                new Color(1f, 0.85f, 0.2f),
                0.18f);
            var enemyProjectile = CreateProjectilePrefab(
                PrefabRoot + "/Projectiles/EnemyProjectile.prefab",
                "EnemyProjectile",
                new Color(1f, 0.25f, 0.35f),
                0.28f);

            var pistolModel = DevMaterialLibrary.CopyPrefabWithUrpMaterials(
                "Assets/Low Poly Weapons VOL.1/Prefabs/M1911.prefab",
                PrefabRoot + "/Weapons/M1911.prefab");
            var shotgunModel = DevMaterialLibrary.CopyPrefabWithUrpMaterials(
                "Assets/Low Poly Weapons VOL.1/Prefabs/Bennelli_M4.prefab",
                PrefabRoot + "/Weapons/Bennelli_M4.prefab");
            var rifleModel = DevMaterialLibrary.CopyPrefabWithUrpMaterials(
                "Assets/Low Poly Weapons VOL.1/Prefabs/M4_8.prefab",
                PrefabRoot + "/Weapons/M4_8.prefab");

            var playerProjectileDef = CreateProjectileDefinition(
                ConfigRoot + "/Projectiles/PlayerProjectile.asset",
                playerProjectile,
                5f);
            var enemyProjectileDef = CreateProjectileDefinition(
                ConfigRoot + "/Projectiles/EnemyProjectile.asset",
                enemyProjectile,
                5f);
            AssignProjectileDefinition(playerProjectile, playerProjectileDef);
            AssignProjectileDefinition(enemyProjectile, enemyProjectileDef);

            var pistol = CreateWeaponAsset(
                WeaponsConfigRoot + "/Pistol.asset",
                WeaponId.Pistol,
                "Pistol",
                damage: 1f,
                cooldown: 0.3f,
                automatic: false,
                pellets: 1,
                spread: 0.5f,
                speed: 45f,
                startingAmmo: 30,
                ammoPickup: 5,
                playerProjectile,
                pistolModel,
                recoilPitch: 1.5f,
                recoilYaw: 0.5f);
            var shotgun = CreateWeaponAsset(
                WeaponsConfigRoot + "/Shotgun.asset",
                WeaponId.Shotgun,
                "Shotgun",
                damage: 1f,
                cooldown: 0.85f,
                automatic: false,
                pellets: 5,
                spread: 8f,
                speed: 35f,
                startingAmmo: 10,
                ammoPickup: 2,
                playerProjectile,
                shotgunModel,
                recoilPitch: 7.5f,
                recoilYaw: 2.2f);
            var rifleProjectile = CreateRifleGoldFireProjectile(
                PrefabRoot + "/Projectiles/RifleGoldFireProjectile.prefab",
                playerProjectileDef);
            var rifle = CreateWeaponAsset(
                WeaponsConfigRoot + "/Rifle.asset",
                WeaponId.Rifle,
                "Rifle",
                damage: 2f,
                cooldown: 0.1f,
                automatic: true,
                pellets: 1,
                spread: 1.5f,
                speed: 55f,
                startingAmmo: 60,
                ammoPickup: 10,
                rifleProjectile,
                rifleModel,
                LoadEffectCorePrefab(EffectCoreBulletRoot + "/Bullet_GoldFire_Small_MuzzleFlare_Template.prefab"),
                LoadEffectCorePrefab(EffectCoreBulletRoot + "/Bullet_GoldFire_Small_Impact_Template.prefab"),
                LoadEffectCorePrefab(EffectCoreShellPrefab),
                ejectShells: true,
                muzzleFlashScale: 0.25f,
                recoilPitch: 0.45f,
                recoilYaw: 0.2f);

            CreateEnemyAnimator();
            var enemyDefinition = CreateEnemyDefinition(enemyProjectile);
            var playerDefinition = CreatePlayerDefinition(pistol, shotgun, rifle);
            var playerPrefab = CreatePlayerPrefab(playerDefinition, pistolModel, shotgunModel, rifleModel);
            var enemyPrefab = CreateEnemyPrefab(enemyDefinition);
            var spawnerDefinition = CreateSpawnerDefinition(enemyPrefab);
            var sessionDefinition = CreateSessionDefinition(playerPrefab);
            var ammoPickupDefinition = CreateAmmoPickupDefinition();
            CreateAmmoPickupPrefab(ammoPickupDefinition);
            CreateSpawnPointPrefabs();
            var arenaPrefab = CreatePrototypeArena();
            WireDevPrefab(sessionDefinition, spawnerDefinition, arenaPrefab);
            EnsureEmptyWorldPrefab();
            SyncScenePrefabs();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.MarkAllScenesDirty();
            Debug.Log("SexShot Dev prototype setup complete.");
        }

        private static void EnsureFolders()
        {
            EnsureFolder(DevRoot + "/Prefabs");
            EnsureFolder(PrefabRoot + "/Projectiles");
            EnsureFolder(PrefabRoot + "/Player");
            EnsureFolder(PrefabRoot + "/Enemies");
            EnsureFolder(PrefabRoot + "/Ammo");
            EnsureFolder(PrefabRoot + "/Spawn");
            EnsureFolder(PrefabRoot + "/Weapons");
            EnsureFolder(PrefabRoot + "/Vfx");
            EnsureFolder(MaterialsRoot);
            EnsureFolder(ConfigRoot);
            EnsureFolder(WeaponsConfigRoot);
            EnsureFolder(ConfigRoot + "/Enemies");
            EnsureFolder(ConfigRoot + "/Player");
            EnsureFolder(ConfigRoot + "/Spawn");
            EnsureFolder(ConfigRoot + "/Ammo");
            EnsureFolder(ConfigRoot + "/Session");
            EnsureFolder(ConfigRoot + "/Projectiles");
            EnsureFolder(DevRoot + "/Animators");
            EnsureFolder(DevRoot + "/Prototype");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            var name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, name);
        }

        private static GameObject CreateProjectilePrefab(string path, string name, Color color, float scale)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.localScale = Vector3.one * scale;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            var col = go.AddComponent<SphereCollider>();
            col.isTrigger = true;
            var rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            go.AddComponent<Projectile>();

            var matPath = path.Replace(".prefab", "_Mat.mat");
            var mat = DevMaterialLibrary.CreateUrpLitMaterial(matPath, color);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 2f);
            EditorUtility.SetDirty(mat);

            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static void AssignProjectileDefinition(GameObject projectilePrefab, ProjectileDefinition definition)
        {
            if (projectilePrefab == null || definition == null)
            {
                return;
            }

            var contents = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(projectilePrefab));
            var projectile = contents.GetComponent<Projectile>();
            if (projectile != null)
            {
                var so = new SerializedObject(projectile);
                so.FindProperty("_definition").objectReferenceValue = definition;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            PrefabUtility.SaveAsPrefabAsset(contents, AssetDatabase.GetAssetPath(projectilePrefab));
            PrefabUtility.UnloadPrefabContents(contents);
        }

        private static ProjectileDefinition CreateProjectileDefinition(string path, GameObject prefab, float lifetime)
        {
            var asset = AssetDatabase.LoadAssetAtPath<ProjectileDefinition>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<ProjectileDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }

            var so = new SerializedObject(asset);
            so.FindProperty("_lifetime").floatValue = lifetime;
            so.FindProperty("_prefab").objectReferenceValue = prefab;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static EnemyDefinition CreateEnemyDefinition(GameObject projectilePrefab)
        {
            const string path = ConfigRoot + "/Enemies/Succubus.asset";
            var asset = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<EnemyDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }

            var so = new SerializedObject(asset);
            so.FindProperty("_displayName").stringValue = "Succubus";
            so.FindProperty("_maxHealth").floatValue = 3f;
            so.FindProperty("_staggerDuration").floatValue = 0.35f;
            so.FindProperty("_moveSpeed").floatValue = 1.8f;
            so.FindProperty("_turnSpeed").floatValue = 8f;
            so.FindProperty("_attackRange").floatValue = 10f;
            so.FindProperty("_attackCooldown").floatValue = 2f;
            so.FindProperty("_projectileDamage").floatValue = 2f;
            so.FindProperty("_projectileSpeed").floatValue = 10f;
            so.FindProperty("_aimHeight").floatValue = 1.2f;
            so.FindProperty("_deathDespawnDelay").floatValue = 1.5f;
            so.FindProperty("_projectilePrefab").objectReferenceValue = projectilePrefab;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static PlayerDefinition CreatePlayerDefinition(params WeaponDefinition[] weapons)
        {
            const string path = ConfigRoot + "/Player/Player.asset";
            var asset = AssetDatabase.LoadAssetAtPath<PlayerDefinition>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<PlayerDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }

            var so = new SerializedObject(asset);
            so.FindProperty("_maxHealth").floatValue = 100f;
            so.FindProperty("_moveSpeed").floatValue = 6f;
            so.FindProperty("_jumpHeight").floatValue = 1.4f;
            so.FindProperty("_gravity").floatValue = -20f;
            so.FindProperty("_lookSensitivity").floatValue = 0.12f;
            so.FindProperty("_minPitch").floatValue = -85f;
            so.FindProperty("_maxPitch").floatValue = 85f;
            var weaponsProp = so.FindProperty("_weapons");
            weaponsProp.arraySize = weapons.Length;
            for (var i = 0; i < weapons.Length; i++)
            {
                weaponsProp.GetArrayElementAtIndex(i).objectReferenceValue = weapons[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static SpawnerDefinition CreateSpawnerDefinition(GameObject enemyPrefab)
        {
            const string path = ConfigRoot + "/Spawn/EnemySpawner.asset";
            var asset = AssetDatabase.LoadAssetAtPath<SpawnerDefinition>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<SpawnerDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }

            var so = new SerializedObject(asset);
            so.FindProperty("_enemyPrefab").objectReferenceValue = enemyPrefab;
            so.FindProperty("_initialCount").intValue = 5;
            so.FindProperty("_maxCount").intValue = 15;
            so.FindProperty("_spawnInterval").floatValue = 3f;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static SessionDefinition CreateSessionDefinition(GameObject playerPrefab)
        {
            const string path = ConfigRoot + "/Session/GameSession.asset";
            var asset = AssetDatabase.LoadAssetAtPath<SessionDefinition>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<SessionDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }

            var so = new SerializedObject(asset);
            so.FindProperty("_playerPrefab").objectReferenceValue = playerPrefab;
            so.FindProperty("_pauseOnPlayerDeath").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static AmmoPickupDefinition CreateAmmoPickupDefinition()
        {
            const string path = ConfigRoot + "/Ammo/AmmoPickup.asset";
            var asset = AssetDatabase.LoadAssetAtPath<AmmoPickupDefinition>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<AmmoPickupDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }

            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static WeaponDefinition CreateWeaponAsset(
            string path,
            WeaponId id,
            string displayName,
            float damage,
            float cooldown,
            bool automatic,
            int pellets,
            float spread,
            float speed,
            int startingAmmo,
            int ammoPickup,
            GameObject projectile,
            GameObject worldModelPrefab,
            GameObject muzzleFlashPrefab = null,
            GameObject impactPrefab = null,
            GameObject shellPrefab = null,
            bool ejectShells = false,
            float muzzleFlashScale = 1f,
            float recoilPitch = 1f,
            float recoilYaw = 0.35f)
        {
            var asset = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<WeaponDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }

            var so = new SerializedObject(asset);
            so.FindProperty("_weaponId").enumValueIndex = (int)id;
            so.FindProperty("_displayName").stringValue = displayName;
            so.FindProperty("_damage").floatValue = damage;
            so.FindProperty("_fireCooldown").floatValue = cooldown;
            so.FindProperty("_automatic").boolValue = automatic;
            so.FindProperty("_pelletsPerShot").intValue = pellets;
            so.FindProperty("_spreadDegrees").floatValue = spread;
            so.FindProperty("_projectileSpeed").floatValue = speed;
            so.FindProperty("_startingAmmo").intValue = startingAmmo;
            so.FindProperty("_ammoPerPickup").intValue = ammoPickup;
            so.FindProperty("_projectilePrefab").objectReferenceValue = projectile;
            so.FindProperty("_worldModelPrefab").objectReferenceValue = worldModelPrefab;
            so.FindProperty("_muzzleFlashPrefab").objectReferenceValue = muzzleFlashPrefab;
            so.FindProperty("_muzzleFlashScale").floatValue = muzzleFlashScale;
            so.FindProperty("_impactPrefab").objectReferenceValue = impactPrefab;
            so.FindProperty("_shellPrefab").objectReferenceValue = shellPrefab;
            so.FindProperty("_ejectShells").boolValue = ejectShells;
            so.FindProperty("_recoilPitch").floatValue = recoilPitch;
            so.FindProperty("_recoilYaw").floatValue = recoilYaw;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static void CreateEnemyAnimator()
        {
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(AnimatorPath) != null)
            {
                AssetDatabase.DeleteAsset(AnimatorPath);
            }

            var fbx = AssetDatabase.LoadAllAssetsAtPath("Assets/DemonGirlSuccubus/FBX/DemonGirl_Upgrade.fbx");
            AnimationClip idle = null;
            AnimationClip walk = null;
            AnimationClip attack = null;
            foreach (var obj in fbx)
            {
                if (!(obj is AnimationClip clip) || clip.name.StartsWith("__preview__"))
                {
                    continue;
                }

                if (clip.name == "1_Idle") idle = clip;
                if (clip.name == "2_Catwalk") walk = clip;
                if (clip.name == "4_MagicAttack") attack = clip;
            }

            var controller = AnimatorController.CreateAnimatorControllerAtPath(AnimatorPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);

            var root = controller.layers[0].stateMachine;
            var idleState = root.AddState("Idle");
            idleState.motion = idle;
            var walkState = root.AddState("Walk");
            walkState.motion = walk;
            var attackState = root.AddState("Attack");
            attackState.motion = attack;
            var hitState = root.AddState("Hit");
            hitState.motion = idle;
            var dieState = root.AddState("Die");
            dieState.motion = idle;
            root.defaultState = idleState;

            var toWalk = idleState.AddTransition(walkState);
            toWalk.hasExitTime = false;
            toWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

            var toIdle = walkState.AddTransition(idleState);
            toIdle.hasExitTime = false;
            toIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

            var anyAttack = root.AddAnyStateTransition(attackState);
            anyAttack.hasExitTime = false;
            anyAttack.AddCondition(AnimatorConditionMode.If, 0f, "Attack");

            var attackExit = attackState.AddTransition(idleState);
            attackExit.hasExitTime = true;
            attackExit.exitTime = 0.9f;

            var anyHit = root.AddAnyStateTransition(hitState);
            anyHit.hasExitTime = false;
            anyHit.AddCondition(AnimatorConditionMode.If, 0f, "Hit");
            var hitExit = hitState.AddTransition(idleState);
            hitExit.hasExitTime = true;
            hitExit.exitTime = 0.6f;

            var anyDie = root.AddAnyStateTransition(dieState);
            anyDie.hasExitTime = false;
            anyDie.AddCondition(AnimatorConditionMode.If, 0f, "Die");

            EditorUtility.SetDirty(controller);
        }

        private static GameObject CreatePlayerPrefab(
            PlayerDefinition playerDefinition,
            GameObject pistolModelPrefab,
            GameObject shotgunModelPrefab,
            GameObject rifleModelPrefab)
        {
            var path = PrefabRoot + "/Player/Player.prefab";
            var root = new GameObject("Player");
            root.tag = "Player";
            var characterController = root.AddComponent<CharacterController>();
            characterController.height = 1.8f;
            characterController.radius = 0.35f;
            characterController.center = new Vector3(0f, 0.9f, 0f);

            var hitVolume = new GameObject("HitVolume");
            hitVolume.transform.SetParent(root.transform);
            hitVolume.transform.localPosition = Vector3.zero;
            var hitbox = hitVolume.AddComponent<CapsuleCollider>();
            hitbox.height = 1.8f;
            hitbox.radius = 0.4f;
            hitbox.center = new Vector3(0f, 0.9f, 0f);
            hitbox.isTrigger = true;

            var health = root.AddComponent<Health>();
            var ammoInventory = root.AddComponent<AmmoInventory>();
            var motor = root.AddComponent<PlayerMotor>();
            var look = root.AddComponent<PlayerLook>();
            var avatar = root.AddComponent<PlayerAvatar>();
            var weaponController = root.AddComponent<PlayerWeaponController>();
            var deathView = root.AddComponent<PlayerDeathView>();

            var cameraPivot = new GameObject("CameraPivot");
            cameraPivot.transform.SetParent(root.transform);
            cameraPivot.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            var camGo = new GameObject("PlayerCamera");
            camGo.transform.SetParent(cameraPivot.transform);
            camGo.transform.localPosition = Vector3.zero;
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.nearClipPlane = 0.05f;
            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();

            var weaponSocket = new GameObject("WeaponSocket");
            weaponSocket.transform.SetParent(cameraPivot.transform);
            weaponSocket.transform.localPosition = new Vector3(0.25f, -0.2f, 0.45f);
            weaponSocket.transform.localRotation = Quaternion.identity;
            weaponSocket.transform.localScale = Vector3.one * 0.8f;

            var pistolModel = AddWeaponModel(weaponSocket.transform, pistolModelPrefab, "PistolModel", true);
            var shotgunModel = AddWeaponModel(weaponSocket.transform, shotgunModelPrefab, "ShotgunModel", false);
            var rifleModel = AddWeaponModel(weaponSocket.transform, rifleModelPrefab, "RifleModel", false);

            var healthSo = new SerializedObject(health);
            healthSo.FindProperty("_maxHealth").floatValue = playerDefinition.MaxHealth;
            healthSo.ApplyModifiedPropertiesWithoutUndo();

            SetStartingAmmo(ammoInventory, playerDefinition);

            var lookSo = new SerializedObject(look);
            lookSo.FindProperty("_definition").objectReferenceValue = playerDefinition;
            lookSo.FindProperty("_cameraPivot").objectReferenceValue = cameraPivot.transform;
            lookSo.ApplyModifiedPropertiesWithoutUndo();

            var avatarSo = new SerializedObject(avatar);
            avatarSo.FindProperty("_definition").objectReferenceValue = playerDefinition;
            avatarSo.FindProperty("_health").objectReferenceValue = health;
            avatarSo.FindProperty("_motor").objectReferenceValue = motor;
            avatarSo.FindProperty("_look").objectReferenceValue = look;
            avatarSo.FindProperty("_weapons").objectReferenceValue = weaponController;
            avatarSo.FindProperty("_deathView").objectReferenceValue = deathView;
            avatarSo.ApplyModifiedPropertiesWithoutUndo();

            var deathViewSo = new SerializedObject(deathView);
            deathViewSo.FindProperty("_cameraPivot").objectReferenceValue = cameraPivot.transform;
            deathViewSo.FindProperty("_controller").objectReferenceValue = characterController;
            deathViewSo.ApplyModifiedPropertiesWithoutUndo();

            var motorSo = new SerializedObject(motor);
            motorSo.FindProperty("_definition").objectReferenceValue = playerDefinition;
            motorSo.FindProperty("_controller").objectReferenceValue = characterController;
            motorSo.ApplyModifiedPropertiesWithoutUndo();

            var weaponSo = new SerializedObject(weaponController);
            weaponSo.FindProperty("_definition").objectReferenceValue = playerDefinition;
            weaponSo.FindProperty("_ammoInventory").objectReferenceValue = ammoInventory;
            weaponSo.FindProperty("_weaponModels").arraySize = 3;
            weaponSo.FindProperty("_weaponModels").GetArrayElementAtIndex(0).objectReferenceValue = pistolModel;
            weaponSo.FindProperty("_weaponModels").GetArrayElementAtIndex(1).objectReferenceValue = shotgunModel;
            weaponSo.FindProperty("_weaponModels").GetArrayElementAtIndex(2).objectReferenceValue = rifleModel;
            weaponSo.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject CreateEnemyPrefab(EnemyDefinition enemyDefinition)
        {
            var path = PrefabRoot + "/Enemies/Enemy.prefab";
            var demonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/DemonGirlSuccubus/Prefabs/DemonGirl_var1.prefab");
            var root = new GameObject("Enemy");
            var model = (GameObject)PrefabUtility.InstantiatePrefab(demonPrefab);
            model.name = "Model";
            model.transform.SetParent(root.transform, false);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;
            // Nested prefab overrides are lost on SaveAsPrefabAsset — unpack so mats/animator stick.
            PrefabUtility.UnpackPrefabInstance(model, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            DevMaterialLibrary.RemapRendererMaterials(root);

            var capsule = root.AddComponent<CapsuleCollider>();
            capsule.isTrigger = true;
            FitCapsuleToRenderers(root.transform, capsule);

            var controller = root.AddComponent<CharacterController>();
            FitCharacterControllerFromCapsule(capsule, controller);

            root.AddComponent<Health>();
            root.AddComponent<EnemyAvatar>();
            root.AddComponent<EnemyBrain>();

            var muzzle = new GameObject("Muzzle");
            muzzle.transform.SetParent(root.transform);
            muzzle.transform.localPosition = GetMuzzleLocalPosition(root.transform, capsule);

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);

            WireEnemyPrefabReferences(path, enemyDefinition);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static void WireEnemyPrefabReferences(string path, EnemyDefinition enemyDefinition)
        {
            var contents = PrefabUtility.LoadPrefabContents(path);
            var health = contents.GetComponent<Health>();
            var avatar = contents.GetComponent<EnemyAvatar>();
            var brain = contents.GetComponent<EnemyBrain>();
            var capsule = contents.GetComponent<CapsuleCollider>();
            var controller = contents.GetComponent<CharacterController>();
            if (controller == null)
            {
                controller = contents.AddComponent<CharacterController>();
            }

            var muzzle = contents.transform.Find("Muzzle");
            var model = contents.transform.Find("Model");
            var animator = model != null ? model.GetComponent<Animator>() : contents.GetComponentInChildren<Animator>();
            var enemyController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(AnimatorPath);

            if (animator != null && enemyController != null)
            {
                animator.runtimeAnimatorController = enemyController;
                EditorUtility.SetDirty(animator);
            }

            DevMaterialLibrary.RemapRendererMaterials(contents);

            if (capsule != null)
            {
                FitCapsuleToRenderers(contents.transform, capsule);
                if (controller != null)
                {
                    FitCharacterControllerFromCapsule(capsule, controller);
                }

                if (muzzle != null)
                {
                    muzzle.localPosition = GetMuzzleLocalPosition(contents.transform, capsule);
                }
            }

            if (health != null && enemyDefinition != null)
            {
                var healthSo = new SerializedObject(health);
                healthSo.FindProperty("_maxHealth").floatValue = enemyDefinition.MaxHealth;
                healthSo.ApplyModifiedPropertiesWithoutUndo();
            }

            if (avatar != null)
            {
                var avatarSo = new SerializedObject(avatar);
                avatarSo.FindProperty("_definition").objectReferenceValue = enemyDefinition;
                avatarSo.FindProperty("_health").objectReferenceValue = health;
                avatarSo.FindProperty("_brain").objectReferenceValue = brain;
                avatarSo.ApplyModifiedPropertiesWithoutUndo();
            }

            if (brain != null)
            {
                var brainSo = new SerializedObject(brain);
                brainSo.FindProperty("_definition").objectReferenceValue = enemyDefinition;
                brainSo.FindProperty("_avatar").objectReferenceValue = avatar;
                brainSo.FindProperty("_muzzle").objectReferenceValue = muzzle;
                brainSo.FindProperty("_animator").objectReferenceValue = animator;
                brainSo.FindProperty("_hitCollider").objectReferenceValue = capsule;
                brainSo.FindProperty("_controller").objectReferenceValue = controller;
                brainSo.ApplyModifiedPropertiesWithoutUndo();
            }

            PrefabUtility.SaveAsPrefabAsset(contents, path);
            PrefabUtility.UnloadPrefabContents(contents);
        }

        private static void CreateAmmoPickupPrefab(AmmoPickupDefinition definition)
        {
            var path = PrefabRoot + "/Ammo/AmmoPickup.prefab";
            var root = new GameObject("AmmoPickup");
            var col = root.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 0.8f;
            var pickup = root.AddComponent<AmmoPickup>();

            var vfx = new GameObject("VfxLightBeam");
            vfx.transform.SetParent(root.transform);
            vfx.transform.localPosition = Vector3.zero;
            var light = vfx.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.45f, 0.85f, 1f);
            light.intensity = 3.5f;
            light.range = 4f;

            var beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beam.name = "Beam";
            beam.transform.SetParent(vfx.transform);
            beam.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            beam.transform.localScale = new Vector3(0.15f, 1.2f, 0.15f);
            Object.DestroyImmediate(beam.GetComponent<Collider>());
            var mat = DevMaterialLibrary.CreateUrpLitMaterial(
                PrefabRoot + "/Ammo/AmmoBeam_Mat.mat",
                new Color(0.4f, 0.9f, 1f, 1f));
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.4f, 0.9f, 1f) * 2f);
            EditorUtility.SetDirty(mat);
            beam.GetComponent<MeshRenderer>().sharedMaterial = mat;

            var laserPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                PrefabRoot + "/Vfx/LaserBlueMuzzleFlare.prefab");
            if (laserPrefab == null)
            {
                laserPrefab = DevMaterialLibrary.CopyPrefabWithUrpMaterials(
                    "Assets/EffectCore/packs/StylizedProjectilePack1/prefabs/Laser/Laser_Blue/Laser_Medium_Blue/Laser_Blue_Medium_MuzzleFlare.prefab",
                    PrefabRoot + "/Vfx/LaserBlueMuzzleFlare.prefab");
            }

            if (laserPrefab != null)
            {
                var flare = (GameObject)PrefabUtility.InstantiatePrefab(laserPrefab);
                flare.name = "EffectCoreFlare";
                flare.transform.SetParent(vfx.transform);
                flare.transform.localPosition = new Vector3(0f, 0.2f, 0f);
                flare.transform.localScale = Vector3.one * 0.5f;
                foreach (var mb in flare.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (mb != null)
                    {
                        Object.DestroyImmediate(mb);
                    }
                }
            }

            var so = new SerializedObject(pickup);
            so.FindProperty("_definition").objectReferenceValue = definition;
            so.FindProperty("_vfxRoot").objectReferenceValue = vfx;
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        private static void CreateSpawnPointPrefabs()
        {
            var player = new GameObject("PlayerSpawnPoint");
            player.AddComponent<PlayerSpawnPoint>();
            PrefabUtility.SaveAsPrefabAsset(player, PrefabRoot + "/Spawn/PlayerSpawnPoint.prefab");
            Object.DestroyImmediate(player);

            var enemy = new GameObject("EnemySpawnPoint");
            enemy.AddComponent<EnemySpawnPoint>();
            PrefabUtility.SaveAsPrefabAsset(enemy, PrefabRoot + "/Spawn/EnemySpawnPoint.prefab");
            Object.DestroyImmediate(enemy);
        }

        private static void WireDevPrefab(
            SessionDefinition sessionDefinition,
            SpawnerDefinition spawnerDefinition,
            GameObject arenaPrefab)
        {
            var devPath = DevRoot + "/Dev.prefab";
            var contents = PrefabUtility.LoadPrefabContents(devPath);

            foreach (Transform child in contents.transform)
            {
                Object.DestroyImmediate(child.gameObject);
            }

            var components = contents.GetComponents<Component>();
            foreach (var component in components)
            {
                if (!(component is Transform))
                {
                    Object.DestroyImmediate(component);
                }
            }

            var runtime = new GameObject("Runtime");
            runtime.transform.SetParent(contents.transform, false);

            if (arenaPrefab != null)
            {
                var arena = (GameObject)PrefabUtility.InstantiatePrefab(arenaPrefab, contents.transform);
                arena.name = "PrototypeArena";
                arena.transform.localPosition = Vector3.zero;
                arena.transform.localRotation = Quaternion.identity;
                arena.transform.localScale = Vector3.one;
            }

            var session = contents.AddComponent<GameSession>();
            var spawner = contents.AddComponent<EnemySpawner>();
            var ammoSpawner = contents.AddComponent<AmmoSpawner>();
            contents.AddComponent<SessionHud>();

            var ammoPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabRoot + "/Ammo/AmmoPickup.prefab");

            var sessionSo = new SerializedObject(session);
            sessionSo.FindProperty("_definition").objectReferenceValue = sessionDefinition;
            sessionSo.FindProperty("_enemySpawner").objectReferenceValue = spawner;
            sessionSo.FindProperty("_ammoSpawner").objectReferenceValue = ammoSpawner;
            sessionSo.FindProperty("_runtimeRoot").objectReferenceValue = runtime.transform;
            sessionSo.ApplyModifiedPropertiesWithoutUndo();

            var spawnerSo = new SerializedObject(spawner);
            spawnerSo.FindProperty("_definition").objectReferenceValue = spawnerDefinition;
            spawnerSo.FindProperty("_enemiesRoot").objectReferenceValue = runtime.transform;
            spawnerSo.ApplyModifiedPropertiesWithoutUndo();

            var ammoSpawnerSo = new SerializedObject(ammoSpawner);
            ammoSpawnerSo.FindProperty("_ammoPrefab").objectReferenceValue = ammoPrefab;
            ammoSpawnerSo.FindProperty("_pickupsRoot").objectReferenceValue = runtime.transform;
            ammoSpawnerSo.FindProperty("_spawnCount").intValue = 16;
            ammoSpawnerSo.ApplyModifiedPropertiesWithoutUndo();

            var hud = contents.GetComponent<SessionHud>();
            var hudSo = new SerializedObject(hud);
            hudSo.FindProperty("_session").objectReferenceValue = session;
            hudSo.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(contents, devPath);
            PrefabUtility.UnloadPrefabContents(contents);
        }

        private static GameObject CreatePrototypeArena()
        {
            var arenaPath = DevRoot + "/Prototype/PrototypeArena.prefab";
            var root = new GameObject("PrototypeArena");

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(root.transform);
            ground.transform.localScale = new Vector3(3f, 1f, 3f);
            var groundMat = DevMaterialLibrary.CreateUrpLitMaterial(
                DevRoot + "/Prototype/Ground_Mat.mat",
                new Color(0.35f, 0.38f, 0.32f));
            ground.GetComponent<MeshRenderer>().sharedMaterial = groundMat;

            root.AddComponent<MapSpawnArea>();

            var playerSpawnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabRoot + "/Spawn/PlayerSpawnPoint.prefab");

            var playerSpawn = (GameObject)PrefabUtility.InstantiatePrefab(playerSpawnPrefab);
            playerSpawn.transform.SetParent(root.transform);
            playerSpawn.transform.localPosition = new Vector3(0f, 0.1f, 0f);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, arenaPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void EnsureEmptyWorldPrefab()
        {
            const string worldPath = "Assets/_Game/World/World.prefab";
            EnsureFolder("Assets/_Game/World");

            if (AssetDatabase.IsValidFolder("Assets/_Game/World/Materials"))
            {
                AssetDatabase.DeleteAsset("Assets/_Game/World/Materials");
            }

            var contents = PrefabUtility.LoadPrefabContents(worldPath);
            foreach (Transform child in contents.transform)
            {
                Object.DestroyImmediate(child.gameObject);
            }

            var components = contents.GetComponents<Component>();
            foreach (var component in components)
            {
                if (!(component is Transform))
                {
                    Object.DestroyImmediate(component);
                }
            }

            PrefabUtility.SaveAsPrefabAsset(contents, worldPath);
            PrefabUtility.UnloadPrefabContents(contents);
        }

        private static void SyncScenePrefabs()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                return;
            }

            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == "PlayerProjectile" || root.name == "EnemyProjectile")
                {
                    Object.DestroyImmediate(root);
                }
            }

            ReplaceOrCreateScenePrefabInstance("Dev", DevRoot + "/Dev.prefab");
            ReplaceOrCreateScenePrefabInstance("World", "Assets/_Game/World/World.prefab");
            EditorSceneManager.MarkSceneDirty(scene);
        }

        private static void ReplaceOrCreateScenePrefabInstance(string name, string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                return;
            }

            var existing = GameObject.Find(name);
            Vector3 position = Vector3.zero;
            Quaternion rotation = Quaternion.identity;
            if (existing != null)
            {
                position = existing.transform.position;
                rotation = existing.transform.rotation;
                Object.DestroyImmediate(existing);
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = name;
            instance.transform.SetPositionAndRotation(position, rotation);
        }

        private static GameObject AddWeaponModel(Transform socket, GameObject prefab, string name, bool active)
        {
            var model = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            model.name = name;
            model.transform.SetParent(socket);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            model.transform.localScale = Vector3.one;
            foreach (var col in model.GetComponentsInChildren<Collider>(true))
            {
                col.enabled = false;
            }

            model.SetActive(active);
            return model;
        }

        private static void SetStartingAmmo(AmmoInventory inventory, PlayerDefinition playerDefinition)
        {
            var weapons = playerDefinition.Weapons;
            if (weapons == null)
            {
                return;
            }

            var so = new SerializedObject(inventory);
            var loadout = so.FindProperty("_startingLoadout");
            loadout.arraySize = weapons.Length;
            for (var i = 0; i < weapons.Length; i++)
            {
                var weapon = weapons[i];
                if (weapon == null)
                {
                    continue;
                }

                var entry = loadout.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("WeaponId").enumValueIndex = (int)weapon.WeaponId;
                entry.FindPropertyRelative("Amount").intValue = weapon.StartingAmmo;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void FitCapsuleToRenderers(Transform root, CapsuleCollider capsule, float padding = 0.05f)
        {
            var renderers = root.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                capsule.height = 1.8f;
                capsule.radius = 0.4f;
                capsule.center = new Vector3(0f, 0.9f, 0f);
                return;
            }

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            capsule.radius = Mathf.Max(Mathf.Max(bounds.size.x, bounds.size.z) * 0.5f - padding, 0.1f);
            capsule.height = Mathf.Max(bounds.size.y - padding, capsule.radius * 2f);
            capsule.center = root.InverseTransformPoint(bounds.center);
        }

        private static void FitCharacterControllerFromCapsule(CapsuleCollider capsule, CharacterController controller)
        {
            controller.radius = capsule.radius;
            controller.height = Mathf.Max(capsule.height, controller.radius * 2f);
            controller.center = capsule.center;
            controller.slopeLimit = 45f;
            controller.stepOffset = Mathf.Min(controller.height * 0.25f, 0.35f);
            controller.skinWidth = 0.08f;
            controller.minMoveDistance = 0.001f;
        }

        private static Vector3 GetMuzzleLocalPosition(Transform root, CapsuleCollider capsule)
        {
            var center = capsule.center;
            var forwardOffset = capsule.radius * 0.35f;
            var heightOffset = capsule.height * 0.25f;
            return center + Vector3.up * heightOffset + Vector3.forward * forwardOffset;
        }

        private static void WireCombatVfx()
        {
            EnsureFolder(PrefabRoot + "/Projectiles");

            var playerProjectileDefinition = AssetDatabase.LoadAssetAtPath<ProjectileDefinition>(
                ConfigRoot + "/Projectiles/PlayerProjectile.asset");
            var enemyProjectileDefinition = AssetDatabase.LoadAssetAtPath<ProjectileDefinition>(
                ConfigRoot + "/Projectiles/EnemyProjectile.asset");

            var pistolProjectile = CreateEffectCoreCombatProjectile(
                EffectCoreBlazingRedMediumRoot + "/Bullet_BlazingRed_Medium_Projectile.prefab",
                PrefabRoot + "/Projectiles/PistolBlazingRedProjectile.prefab",
                playerProjectileDefinition);
            var shotgunProjectile = CreateEffectCoreCombatProjectile(
                EffectCoreGoldFireMediumRoot + "/Bullet_GoldFire_Medium_Projectile_Template.prefab",
                PrefabRoot + "/Projectiles/ShotgunGoldFireProjectile.prefab",
                playerProjectileDefinition);
            var rifleProjectile = CreateEffectCoreCombatProjectile(
                EffectCoreBulletRoot + "/Bullet_GoldFire_Small_Projectile_Template.prefab",
                PrefabRoot + "/Projectiles/RifleGoldFireProjectile.prefab",
                playerProjectileDefinition);
            var enemyProjectile = CreateEffectCoreCombatProjectile(
                EffectCorePlasmaPurpleHazeMediumRoot + "/Plasma_PurpleHaze_Medium_Projectile.prefab",
                PrefabRoot + "/Projectiles/EnemyPlasmaPurpleHazeProjectile.prefab",
                enemyProjectileDefinition);

            WireWeaponVfx(
                WeaponsConfigRoot + "/Pistol.asset",
                pistolProjectile,
                EffectCoreBlazingRedMediumRoot + "/Bullet_BlazingRed_Medium_MuzzleFlare.prefab",
                EffectCoreBlazingRedMediumRoot + "/Bullet_BlazingRed_Medium_Impact.prefab",
                DevBrassShellPrefab,
                ejectShells: true,
                muzzleFlashScale: 0.3f);
            WireWeaponVfx(
                WeaponsConfigRoot + "/Shotgun.asset",
                shotgunProjectile,
                EffectCoreGoldFireBigRoot + "/Bullet_GoldFire_Big_MuzzleFlare_Template.prefab",
                EffectCoreGoldFireMediumRoot + "/Bullet_GoldFire_Medium_Impact_Template.prefab",
                DevBrassShellPrefab,
                ejectShells: true,
                muzzleFlashScale: 0.22f);
            WireWeaponVfx(
                WeaponsConfigRoot + "/Rifle.asset",
                rifleProjectile,
                EffectCoreBulletRoot + "/Bullet_GoldFire_Small_MuzzleFlare_Template.prefab",
                EffectCoreBulletRoot + "/Bullet_GoldFire_Small_Impact_Template.prefab",
                DevBrassShellPrefab,
                ejectShells: true,
                muzzleFlashScale: 0.25f);

            EnsureBrassShellPrefab();

            var succubus = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(ConfigRoot + "/Enemies/Succubus.asset");
            if (succubus != null)
            {
                var so = new SerializedObject(succubus);
                so.FindProperty("_projectilePrefab").objectReferenceValue = enemyProjectile;
                so.FindProperty("_muzzleFlashPrefab").objectReferenceValue = LoadEffectCorePrefab(
                    EffectCorePlasmaPurpleHazeMediumRoot + "/Plasma_PurpleHaze_Medium_MuzzleFlare.prefab");
                so.FindProperty("_impactPrefab").objectReferenceValue = LoadEffectCorePrefab(
                    EffectCorePlasmaPurpleHazeMediumRoot + "/Plasma_PurpleHaze_Medium_Impact.prefab");
                so.FindProperty("_muzzleFlashScale").floatValue = 0.35f;
                var gorePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabRoot + "/Vfx/EnemyGoreBurst.prefab");
                if (gorePrefab == null)
                {
                    DevGoreVfxSetup.CreateEnemyGoreVfx();
                    gorePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabRoot + "/Vfx/EnemyGoreBurst.prefab");
                }

                so.FindProperty("_deathGorePrefab").objectReferenceValue = gorePrefab;
                so.FindProperty("_deathGoreScale").floatValue = 1f;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(succubus);
            }

            EnsureAllWeaponPrefabFirePoints();
            CleanupLegacyPlayerFirePoints(PrefabRoot + "/Player/Player.prefab");
        }

        private static void EnsureBrassShellPrefab()
        {
            var shellPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DevBrassShellPrefab);
            if (shellPrefab == null)
            {
                DevShellSetup.CreateBrassShellPrefab();
            }
        }

        private static void WireWeaponVfx(
            string weaponPath,
            GameObject projectile,
            string muzzleFlashPath,
            string impactPath,
            string shellPath,
            bool ejectShells,
            float muzzleFlashScale)
        {
            var weapon = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(weaponPath);
            if (weapon == null)
            {
                return;
            }

            var so = new SerializedObject(weapon);
            so.FindProperty("_projectilePrefab").objectReferenceValue = projectile;
            so.FindProperty("_muzzleFlashPrefab").objectReferenceValue = LoadEffectCorePrefab(muzzleFlashPath);
            so.FindProperty("_impactPrefab").objectReferenceValue = LoadEffectCorePrefab(impactPath);
            so.FindProperty("_shellPrefab").objectReferenceValue = LoadEffectCorePrefab(shellPath);
            so.FindProperty("_ejectShells").boolValue = ejectShells;
            so.FindProperty("_muzzleFlashScale").floatValue = muzzleFlashScale;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(weapon);
        }

        private static void WireRifleGoldFireVfx()
        {
            WireCombatVfx();
        }

        private static GameObject CreateEffectCoreCombatProjectile(
            string sourcePath,
            string targetPath,
            ProjectileDefinition definition)
        {
            EnsureFolderForAsset(targetPath);
            if (!AssetDatabase.CopyAsset(sourcePath, targetPath))
            {
                Debug.LogWarning("SexShot: failed to copy EffectCore projectile from " + sourcePath);
            }

            var contents = PrefabUtility.LoadPrefabContents(targetPath);
            contents.tag = "Untagged";

            foreach (var behaviour in contents.GetComponents<MonoBehaviour>())
            {
                if (behaviour != null && behaviour.GetType().Name == "ECExplodingProjectile")
                {
                    Object.DestroyImmediate(behaviour);
                }
            }

            var rigidbody = contents.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = contents.AddComponent<Rigidbody>();
            }

            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            var collider = contents.GetComponent<SphereCollider>();
            if (collider == null)
            {
                collider = contents.AddComponent<SphereCollider>();
            }

            collider.isTrigger = true;
            collider.radius = 0.12f;
            collider.center = Vector3.zero;

            var projectile = contents.GetComponent<Projectile>();
            if (projectile == null)
            {
                projectile = contents.AddComponent<Projectile>();
            }

            var projectileSo = new SerializedObject(projectile);
            projectileSo.FindProperty("_definition").objectReferenceValue = definition;
            projectileSo.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(contents, targetPath);
            PrefabUtility.UnloadPrefabContents(contents);
            return prefab;
        }

        private static GameObject CreateRifleGoldFireProjectile(string targetPath, ProjectileDefinition definition)
        {
            return CreateEffectCoreCombatProjectile(
                EffectCoreBulletRoot + "/Bullet_GoldFire_Small_Projectile_Template.prefab",
                targetPath,
                definition);
        }

        private static GameObject LoadEffectCorePrefab(string path)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static void EnsureAllWeaponPrefabFirePoints()
        {
            EnsureWeaponPrefabFirePoints(
                PrefabRoot + "/Weapons/M1911.prefab",
                new Vector3(0f, 0.027f, -0.32f),
                new Vector3(0f, 0.021f, -0.33f),
                new Vector3(-0.0415f, 0.0273f, -0.0296f));
            EnsureWeaponPrefabFirePoints(
                PrefabRoot + "/Weapons/Bennelli_M4.prefab",
                new Vector3(0f, 0.05f, 0.28f),
                new Vector3(0f, 0.05f, 0.28f),
                new Vector3(0.05f, 0.08f, 0.1f));
            EnsureWeaponPrefabFirePoints(
                PrefabRoot + "/Weapons/M4_8.prefab",
                new Vector3(0f, 0.04f, 0.38f),
                new Vector3(0f, 0.04f, 0.38f),
                new Vector3(0.04f, 0.07f, 0.12f));
        }

        private static void EnsureWeaponPrefabFirePoints(
            string prefabPath,
            Vector3 muzzleLocal,
            Vector3 muzzleFlashLocal,
            Vector3 shellLocal)
        {
            if (!File.Exists(prefabPath))
            {
                return;
            }

            var contents = PrefabUtility.LoadPrefabContents(prefabPath);
            EnsureWeaponFirePoints(contents.transform, muzzleLocal, muzzleFlashLocal, shellLocal);
            PrefabUtility.SaveAsPrefabAsset(contents, prefabPath);
            PrefabUtility.UnloadPrefabContents(contents);
        }

        private static void EnsureWeaponFirePoints(
            Transform root,
            Vector3 muzzleLocal,
            Vector3 muzzleFlashLocal,
            Vector3 shellLocal)
        {
            var muzzleRotation = Quaternion.Euler(0f, 180f, 0f);
            EnsureChildFirePoint(root, "Muzzle", muzzleLocal, muzzleRotation);
            EnsureChildFirePoint(root, "MuzzleFlash", muzzleFlashLocal, muzzleRotation);
            EnsureChildFirePoint(root, "ShellEject", shellLocal, Quaternion.Euler(0f, 90f, 0f));
        }

        private static void EnsureChildFirePoint(Transform parent, string name, Vector3 localPosition, Quaternion localRotation)
        {
            var existing = parent.Find(name);
            if (existing != null)
            {
                existing.localPosition = localPosition;
                existing.localRotation = localRotation;
                return;
            }

            var child = new GameObject(name).transform;
            child.SetParent(parent, false);
            child.localPosition = localPosition;
            child.localRotation = localRotation;
        }

        private static void CleanupLegacyPlayerFirePoints(string playerPrefabPath)
        {
            if (!File.Exists(playerPrefabPath))
            {
                return;
            }

            var contents = PrefabUtility.LoadPrefabContents(playerPrefabPath);
            var cameraPivot = contents.transform.Find("CameraPivot");
            if (cameraPivot != null)
            {
                DestroyChildIfExists(cameraPivot, "Muzzle");
                DestroyChildIfExists(cameraPivot, "ShellEject");
            }

            PrefabUtility.SaveAsPrefabAsset(contents, playerPrefabPath);
            PrefabUtility.UnloadPrefabContents(contents);
        }

        private static void DestroyChildIfExists(Transform parent, string childName)
        {
            var child = parent.Find(childName);
            if (child != null)
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }

        private static void WirePlayerShellEjectPoint(string playerPrefabPath)
        {
            EnsureAllWeaponPrefabFirePoints();
            CleanupLegacyPlayerFirePoints(playerPrefabPath);
        }

        private static void EnsureFolderForAsset(string assetPath)
        {
            var folder = Path.GetDirectoryName(assetPath)?.Replace("\\", "/");
            if (string.IsNullOrEmpty(folder))
            {
                return;
            }

            EnsureFolder(folder);
        }
    }
}
