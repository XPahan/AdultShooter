using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SexShot.Dev.Editor
{
    public static class DevMaterialLibrary
    {
        private const string DevMaterialsRoot = "Assets/_Game/Dev/Materials";

        private static readonly (string SourceRoot, string TargetRoot)[] PackMappings =
        {
            ("Assets/DemonGirlSuccubus", DevMaterialsRoot + "/DemonGirlSuccubus"),
            ("Assets/Low Poly Weapons VOL.1", DevMaterialsRoot + "/LowPolyWeapons"),
            ("Assets/EffectCore", DevMaterialsRoot + "/EffectCore")
        };

        [MenuItem("SexShot/Dev/Build URP Material Copies")]
        public static void BuildAll()
        {
            foreach (var (sourceRoot, targetRoot) in PackMappings)
            {
                BuildPackCopies(sourceRoot, targetRoot);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("SexShot: URP material copies built under Assets/_Game/Dev.");
        }

        [MenuItem("SexShot/Dev/Convert EffectCore To URP")]
        public static void ConvertEffectCoreToUrpInPlace()
        {
            ConvertPackToUrpInPlace("Assets/EffectCore");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("SexShot: EffectCore materials converted to URP in place.");
        }

        public static Material CreateUrpLitMaterial(string path, Color color, Texture mainTexture = null)
        {
            EnsureFolderForAsset(path);
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(GetUrpLitShader());
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = GetUrpLitShader();
            material.SetColor("_BaseColor", color);
            if (mainTexture != null)
            {
                material.SetTexture("_BaseMap", mainTexture);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        public static void RemapRendererMaterials(GameObject root)
        {
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                var materials = renderer.sharedMaterials;
                var changed = false;
                for (var i = 0; i < materials.Length; i++)
                {
                    var copy = GetCopyForSourceMaterial(materials[i]);
                    if (copy != null && copy != materials[i])
                    {
                        materials[i] = copy;
                        changed = true;
                    }
                }

                if (changed)
                {
                    renderer.sharedMaterials = materials;
                }
            }
        }

        public static Material GetCopyForSourceMaterial(Material source)
        {
            if (source == null)
            {
                return null;
            }

            var sourcePath = AssetDatabase.GetAssetPath(source);
            if (string.IsNullOrEmpty(sourcePath) || sourcePath.StartsWith("Assets/_Game/"))
            {
                return source;
            }

            var copyPath = ResolveCopyPath(sourcePath);
            return string.IsNullOrEmpty(copyPath)
                ? source
                : AssetDatabase.LoadAssetAtPath<Material>(copyPath);
        }

        public static GameObject CopyPrefabWithUrpMaterials(string sourcePrefabPath, string targetPrefabPath)
        {
            EnsureFolderForAsset(targetPrefabPath);
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePrefabPath);
            if (source == null)
            {
                return null;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
            RemapRendererMaterials(instance);
            var prefab = PrefabUtility.SaveAsPrefabAsset(instance, targetPrefabPath);
            Object.DestroyImmediate(instance);
            return prefab;
        }

        private static void ConvertPackToUrpInPlace(string sourceRoot)
        {
            if (!AssetDatabase.IsValidFolder(sourceRoot))
            {
                return;
            }

            var converted = 0;
            var guids = AssetDatabase.FindAssets("t:Material", new[] { sourceRoot });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith(".ttf", System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                {
                    continue;
                }

                var before = material.shader != null ? material.shader.name : string.Empty;
                ConvertCopyToUrp(material);
                var after = material.shader != null ? material.shader.name : string.Empty;
                if (before != after || after == "Universal Render Pipeline/Particles/Unlit")
                {
                    converted++;
                    EditorUtility.SetDirty(material);
                }
            }

            Debug.Log($"SexShot: converted {converted} materials under {sourceRoot}.");
        }

        private static void BuildPackCopies(string sourceRoot, string targetRoot)
        {
            if (!AssetDatabase.IsValidFolder(sourceRoot))
            {
                return;
            }

            var guids = AssetDatabase.FindAssets("t:Material", new[] { sourceRoot });
            foreach (var guid in guids)
            {
                var sourcePath = AssetDatabase.GUIDToAssetPath(guid);
                if (sourcePath.EndsWith(".ttf", System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var relative = sourcePath.Substring(sourceRoot.Length).TrimStart('/');
                var targetPath = targetRoot + "/" + relative;
                EnsureFolderForAsset(targetPath);

                if (AssetDatabase.LoadAssetAtPath<Material>(targetPath) != null)
                {
                    var existing = AssetDatabase.LoadAssetAtPath<Material>(targetPath);
                    ConvertCopyToUrp(existing);
                    EditorUtility.SetDirty(existing);
                    continue;
                }

                if (!AssetDatabase.CopyAsset(sourcePath, targetPath))
                {
                    Debug.LogWarning("SexShot: failed to copy material " + sourcePath);
                    continue;
                }

                var copy = AssetDatabase.LoadAssetAtPath<Material>(targetPath);
                if (copy == null)
                {
                    continue;
                }

                ConvertCopyToUrp(copy);
                EditorUtility.SetDirty(copy);
            }
        }

        private static string ResolveCopyPath(string sourcePath)
        {
            foreach (var (sourceRoot, targetRoot) in PackMappings)
            {
                if (!sourcePath.StartsWith(sourceRoot + "/"))
                {
                    continue;
                }

                var relative = sourcePath.Substring(sourceRoot.Length).TrimStart('/');
                return targetRoot + "/" + relative;
            }

            return null;
        }

        private static void ConvertCopyToUrp(Material material)
        {
            var shaderName = material.shader != null ? material.shader.name : string.Empty;
            if (shaderName == "Universal Render Pipeline/Particles/Unlit")
            {
                FixUrpParticleMaterial(material);
                return;
            }

            if (shaderName.Contains("Universal Render Pipeline") || shaderName.StartsWith("Shader Graphs/"))
            {
                return;
            }

            if (shaderName == "Standard" || shaderName == "Standard (Specular setup)"
                || shaderName.StartsWith("Legacy Shaders/") || shaderName == "Unlit/Color"
                || shaderName == "Hidden/InternalErrorShader")
            {
                ConvertStandardToUrpLit(material);
                return;
            }

            if (shaderName.StartsWith("Mobile/Particles/") || shaderName.StartsWith("Particles/"))
            {
                ConvertParticleToUrp(material, shaderName);
                return;
            }

            if (shaderName.StartsWith("Unlit/"))
            {
                ConvertUnlitToUrp(material, shaderName);
                return;
            }

            if (material.HasProperty("_TintColor") || material.HasProperty("_InvFade"))
            {
                ConvertParticleToUrp(material, shaderName);
            }
        }

        private static void ConvertStandardToUrpLit(Material material)
        {
            var mainTex = material.HasProperty("_MainTex") ? material.GetTexture("_MainTex") : null;
            var color = material.HasProperty("_Color") ? material.GetColor("_Color") : Color.white;
            var bump = material.HasProperty("_BumpMap") ? material.GetTexture("_BumpMap") : null;
            var metallic = material.HasProperty("_Metallic") ? material.GetFloat("_Metallic") : 0f;
            var smoothness = material.HasProperty("_Glossiness") ? material.GetFloat("_Glossiness") : 0.5f;
            var metallicGloss = material.HasProperty("_MetallicGlossMap") ? material.GetTexture("_MetallicGlossMap") : null;
            var emissionEnabled = material.IsKeywordEnabled("_EMISSION");
            var emissionColor = material.HasProperty("_EmissionColor") ? material.GetColor("_EmissionColor") : Color.black;

            material.shader = GetUrpLitShader();
            material.SetColor("_BaseColor", color);
            if (mainTex != null)
            {
                material.SetTexture("_BaseMap", mainTex);
            }

            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            if (metallicGloss != null)
            {
                material.SetTexture("_MetallicGlossMap", metallicGloss);
                material.EnableKeyword("_METALLICSPECGLOSSMAP");
            }

            if (bump != null)
            {
                material.SetTexture("_BumpMap", bump);
                material.EnableKeyword("_NORMALMAP");
            }

            if (emissionEnabled)
            {
                material.SetColor("_EmissionColor", emissionColor);
                material.EnableKeyword("_EMISSION");
            }
        }

        private static void FixUrpParticleMaterial(Material material)
        {
            var isAlphaBlend = material.name.IndexOf("alphablend", System.StringComparison.OrdinalIgnoreCase) >= 0
                || material.name.IndexOf("alphaBlend", System.StringComparison.OrdinalIgnoreCase) >= 0;

            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", isAlphaBlend ? 0f : 2f);
            material.SetFloat("_ZWrite", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            material.SetFloat("_SrcBlend", 5f);
            material.SetFloat("_DstBlend", isAlphaBlend ? 10f : 1f);
        }

        private static void ConvertParticleToUrp(Material material, string shaderName)
        {
            var mainTex = material.HasProperty("_MainTex") ? material.GetTexture("_MainTex") : null;
            var color = material.HasProperty("_Color") ? material.GetColor("_Color") : Color.white;
            var tint = material.HasProperty("_TintColor") ? material.GetColor("_TintColor") : Color.white;
            var isAlphaBlend = shaderName.Contains("Alpha Blended")
                || material.IsKeywordEnabled("_ALPHABLEND_ON")
                || material.name.IndexOf("alphablend", System.StringComparison.OrdinalIgnoreCase) >= 0
                || material.name.IndexOf("alphaBlend", System.StringComparison.OrdinalIgnoreCase) >= 0;

            material.shader = GetUrpParticleUnlitShader();
            material.SetColor("_BaseColor", color * tint);
            if (mainTex != null)
            {
                material.SetTexture("_BaseMap", mainTex);
            }

            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", isAlphaBlend ? 0f : 2f);
            material.SetFloat("_ZWrite", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            material.SetFloat("_SrcBlend", 5f);
            material.SetFloat("_DstBlend", isAlphaBlend ? 10f : 1f);
        }

        private static void ConvertUnlitToUrp(Material material, string shaderName)
        {
            var mainTex = material.HasProperty("_MainTex") ? material.GetTexture("_MainTex") : null;
            var color = material.HasProperty("_Color") ? material.GetColor("_Color") : Color.white;
            var isTransparent = shaderName.Contains("Transparent") || shaderName.Contains("Fade");

            material.shader = isTransparent ? GetUrpUnlitShader() : GetUrpLitShader();
            material.SetColor("_BaseColor", color);
            if (mainTex != null)
            {
                material.SetTexture("_BaseMap", mainTex);
            }

            if (isTransparent)
            {
                material.SetFloat("_Surface", 1f);
                material.SetFloat("_Blend", 0f);
                material.SetFloat("_ZWrite", 0f);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
        }

        private static Shader GetUrpLitShader()
        {
            return Shader.Find("Universal Render Pipeline/Lit");
        }

        private static Shader GetUrpParticleUnlitShader()
        {
            return Shader.Find("Universal Render Pipeline/Particles/Unlit");
        }

        private static Shader GetUrpUnlitShader()
        {
            return Shader.Find("Universal Render Pipeline/Unlit");
        }

        private static void EnsureFolderForAsset(string assetPath)
        {
            var folder = Path.GetDirectoryName(assetPath)?.Replace("\\", "/");
            if (string.IsNullOrEmpty(folder) || AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            var parts = folder.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
    }
}
