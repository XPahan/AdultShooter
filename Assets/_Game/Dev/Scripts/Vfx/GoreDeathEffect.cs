using UnityEngine;

namespace SexShot.Dev.Vfx
{
    public class GoreDeathEffect : MonoBehaviour
    {
        private static Material _bloodParticleMaterial;
        private static Material _gibMaterial;

        [SerializeField] private float _lifetime = 10f;
        [SerializeField] private int _gibCount = 20;
        [SerializeField] private float _gibForceMin = 6f;
        [SerializeField] private float _gibForceMax = 18f;
        [SerializeField] private float _gibScaleMin = 0.1f;
        [SerializeField] private float _gibScaleMax = 0.35f;
        [SerializeField] private float _overallScale = 1f;

        private void Awake()
        {
            EnsureMaterials();
            var scale = Mathf.Max(0.1f, _overallScale);
            CreateBloodBurst(scale);
            CreateBloodSplatter(scale);
            // BloodMist disabled: large billboard quads without soft texture.
            // Temporarily muted: cube/sphere gibs.
            // SpawnGibs(scale);
        }

        private void Start()
        {
            Destroy(gameObject, _lifetime);
        }

        private static void EnsureMaterials()
        {
            if (_bloodParticleMaterial == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                    ?? Shader.Find("Particles/Standard Unlit");
                _bloodParticleMaterial = new Material(shader)
                {
                    color = new Color(0.55f, 0.02f, 0.02f, 1f)
                };
            }

            if (_gibMaterial == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit")
                    ?? Shader.Find("Standard");
                _gibMaterial = new Material(shader)
                {
                    color = new Color(0.32f, 0.06f, 0.05f, 1f)
                };
            }
        }

        private void CreateBloodBurst(float scale)
        {
            var go = new GameObject("BloodBurst");
            go.transform.SetParent(transform, false);

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.15f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.9f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(5f * scale, 14f * scale);
            main.startSize = new ParticleSystem.MinMaxCurve(0.03f * scale, 0.1f * scale);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(0.65f, 0.03f, 0.03f, 1f),
                new Color(0.35f, 0.01f, 0.01f, 1f));
            main.gravityModifier = 1.4f;
            main.maxParticles = 200;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)110) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.15f * scale;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = _bloodParticleMaterial;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            ps.Play();
        }

        private void CreateBloodMist(float scale)
        {
            var go = new GameObject("BloodMist");
            go.transform.SetParent(transform, false);

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.4f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f * scale, 4.5f * scale);
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f * scale, 0.35f * scale);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(0.5f, 0.02f, 0.02f, 0.7f),
                new Color(0.25f, 0.01f, 0.01f, 0.4f));
            main.gravityModifier = 0.35f;
            main.maxParticles = 60;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)28) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.18f * scale;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(0.45f, 0.02f, 0.02f), 0f),
                    new GradientColorKey(new Color(0.2f, 0.01f, 0.01f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0.85f, 0f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = gradient;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = _bloodParticleMaterial;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            ps.Play();
        }

        private void CreateBloodSplatter(float scale)
        {
            var go = new GameObject("BloodSplatter");
            go.transform.SetParent(transform, false);

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.1f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.55f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(8f * scale, 18f * scale);
            main.startSize = new ParticleSystem.MinMaxCurve(0.02f * scale, 0.06f * scale);
            main.startColor = new Color(0.75f, 0.04f, 0.03f, 1f);
            main.gravityModifier = 2.2f;
            main.maxParticles = 180;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)95) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 55f;
            shape.radius = 0.1f * scale;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = _bloodParticleMaterial;
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = 2.5f;
            renderer.velocityScale = 0.15f;

            ps.Play();
        }

        private void SpawnGibs(float scale)
        {
            for (var i = 0; i < _gibCount; i++)
            {
                var useCube = Random.value > 0.35f;
                var gib = GameObject.CreatePrimitive(useCube ? PrimitiveType.Cube : PrimitiveType.Sphere);
                gib.name = "Gib";
                gib.transform.SetParent(transform, false);

                var gibScale = Random.Range(_gibScaleMin, _gibScaleMax) * scale;
                gib.transform.localScale = Vector3.one * gibScale;
                gib.transform.localPosition = Random.insideUnitSphere * (0.15f * scale);

                var renderer = gib.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = _gibMaterial;
                }

                var collider = gib.GetComponent<Collider>();
                if (collider != null)
                {
                    Destroy(collider);
                }

                var rb = gib.AddComponent<Rigidbody>();
                rb.mass = gibScale * 8f;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

                var direction = Random.onUnitSphere;
                direction.y = Mathf.Abs(direction.y) * 0.5f + 0.35f;
                var force = Random.Range(_gibForceMin, _gibForceMax) * scale;
                rb.AddForce(direction.normalized * force, ForceMode.Impulse);
                rb.AddTorque(Random.insideUnitSphere * (force * 0.6f), ForceMode.Impulse);

                Destroy(gib, _lifetime * 0.85f);
            }
        }
    }
}
