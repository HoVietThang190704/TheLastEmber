using UnityEngine;

namespace TheLastEmber.Features.Combat
{
    public class TrainingDummy : MonoBehaviour, IDamageable
    {
        [Header("Health")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private bool resetHealthOnDepleted = true;

        [Header("Hit Feedback")]
        [SerializeField] private Color hitFlashColor = Color.red;
        [SerializeField] private float hitFlashDuration = 0.1f;

        private float currentHealth;
        private Renderer[] cachedRenderers;
        private Color[][] originalColors;
        private Coroutine flashRoutine;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;

        private void Awake()
        {
            currentHealth = Mathf.Max(1f, maxHealth);
            CacheRendererColors();
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            currentHealth = Mathf.Max(0f, currentHealth - damageInfo.Amount);
            PlayHitFlash();

            Debug.Log($"<color=red>[Combat]</color> {name} nhận {damageInfo.Amount:0} damage. HP: {currentHealth:0}/{maxHealth:0}");

            if (currentHealth > 0f) return;

            Debug.Log($"<color=red>[Combat]</color> {name} đã bị hạ.");

            if (resetHealthOnDepleted)
            {
                currentHealth = maxHealth;
                Debug.Log($"<color=green>[Combat]</color> {name} hồi lại để tiếp tục test.");
            }
        }

        private void CacheRendererColors()
        {
            cachedRenderers = GetComponentsInChildren<Renderer>();
            originalColors = new Color[cachedRenderers.Length][];

            for (int rendererIndex = 0; rendererIndex < cachedRenderers.Length; rendererIndex++)
            {
                Material[] materials = cachedRenderers[rendererIndex].materials;
                originalColors[rendererIndex] = new Color[materials.Length];

                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    originalColors[rendererIndex][materialIndex] = GetMaterialColor(materials[materialIndex]);
                }
            }
        }

        private void PlayHitFlash()
        {
            if (cachedRenderers == null || cachedRenderers.Length == 0) return;

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
                RestoreOriginalColors();
            }

            flashRoutine = StartCoroutine(FlashRoutine());
        }

        private System.Collections.IEnumerator FlashRoutine()
        {
            ApplyColor(hitFlashColor);
            yield return new WaitForSeconds(hitFlashDuration);
            RestoreOriginalColors();
            flashRoutine = null;
        }

        private void ApplyColor(Color color)
        {
            foreach (Renderer targetRenderer in cachedRenderers)
            {
                foreach (Material material in targetRenderer.materials)
                {
                    SetMaterialColor(material, color);
                }
            }
        }

        private void RestoreOriginalColors()
        {
            for (int rendererIndex = 0; rendererIndex < cachedRenderers.Length; rendererIndex++)
            {
                Material[] materials = cachedRenderers[rendererIndex].materials;

                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    SetMaterialColor(materials[materialIndex], originalColors[rendererIndex][materialIndex]);
                }
            }
        }

        private Color GetMaterialColor(Material material)
        {
            if (material.HasProperty("_BaseColor")) return material.GetColor("_BaseColor");
            if (material.HasProperty("_Color")) return material.GetColor("_Color");
            return Color.white;
        }

        private void SetMaterialColor(Material material, Color color)
        {
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            else if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
        }
    }
}
