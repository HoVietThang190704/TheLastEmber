using UnityEngine;

namespace TheLastEmber.Core.Visual
{
    /// <summary>
    /// Tạo hiệu ứng hạt than hoa (Fire Embers & Sparks) bốc lên từ ngọn lửa bằng code C#.
    /// Tự động thiết lập ParticleSystem với màu sắc cam vàng phát quang, kết hợp Bloom URP.
    /// </summary>
    public class FireEmberParticles : MonoBehaviour
    {
        [Header("Ember Settings")]
        [SerializeField] private float emissionRate = 22f;
        [SerializeField] private float spawnRadius = 0.8f;
        [SerializeField] private float riseSpeed = 1.8f;
        [SerializeField] private float particleLifetime = 1.6f;
        [SerializeField] private float particleSize = 0.12f;

        private ParticleSystem ps;
        private ParticleSystem.EmissionModule emission;

        private void Awake()
        {
            SetupParticleSystem();
        }

        public void SetActiveEmbers(bool isBurning)
        {
            if (ps == null) return;
            emission.rateOverTime = isBurning ? emissionRate : 2f;
        }

        private void SetupParticleSystem()
        {
            ps = gameObject.GetComponent<ParticleSystem>();
            if (ps == null)
            {
                ps = gameObject.AddComponent<ParticleSystem>();
            }

            // Material phát sáng (Unlit/Lit có emission)
            ParticleSystemRenderer psr = GetComponent<ParticleSystemRenderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Particles/Standard Unlit");
            }
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material mat = new Material(shader)
            {
                name = "EmberParticleMaterial",
                hideFlags = HideFlags.DontSave
            };
            mat.SetColor("_BaseColor", new Color(1f, 0.65f, 0.2f, 1f));
            mat.SetColor("_Color", new Color(1f, 0.65f, 0.2f, 1f));
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.SetColor("_EmissionColor", new Color(1f, 0.5f, 0.1f) * 2.5f);
                mat.EnableKeyword("_EMISSION");
            }
            psr.sharedMaterial = mat;

            // Main Module
            var main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(particleLifetime * 0.7f, particleLifetime * 1.3f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(riseSpeed * 0.7f, riseSpeed * 1.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(particleSize * 0.6f, particleSize * 1.2f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.35f, 0.1f, 1f),
                new Color(1f, 0.85f, 0.3f, 1f)
            );
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = -0.08f; // Hơi bốc lên trời

            // Emission Module
            emission = ps.emission;
            emission.rateOverTime = emissionRate;

            // Shape Module (Hình nón / hình tròn trên mặt lò)
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = spawnRadius;
            shape.rotation = new Vector3(-90f, 0f, 0f);

            // Size over Lifetime (Nhỏ dần khi tàn)
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 1f);
            curve.AddKey(1f, 0f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);

            // Velocity over lifetime / Noise (Đung đưa theo gió)
            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.35f, 0.35f);
            velocity.y = new ParticleSystem.MinMaxCurve(0f, 0f);
            velocity.z = new ParticleSystem.MinMaxCurve(-0.35f, 0.35f);

            ps.Play();
        }
    }
}
