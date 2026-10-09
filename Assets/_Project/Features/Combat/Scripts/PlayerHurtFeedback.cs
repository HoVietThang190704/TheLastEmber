using System.Collections;
using UnityEngine;

namespace TheLastEmber.Features.Combat
{
    [RequireComponent(typeof(PlayerHealth))]
    public class PlayerHurtFeedback : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerHealth health;
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private CharacterController characterController;

        [Header("Hit Flash")]
        [SerializeField] private Color hitColor = new Color(1f, 0.25f, 0.18f, 1f);
        [SerializeField] private Color downColor = new Color(0.35f, 0.08f, 0.08f, 1f);
        [SerializeField] private float flashDuration = 0.12f;

        [Header("Knockback")]
        [SerializeField] private float knockbackDistance = 0.75f;
        [SerializeField] private float knockbackDuration = 0.12f;

        private Color[] originalColors;
        private Coroutine flashRoutine;
        private Coroutine knockbackRoutine;
        private bool isDown;

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponent<PlayerHealth>();
            }

            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }

            if (targetRenderer == null)
            {
                targetRenderer = GetComponentInChildren<Renderer>();
            }

            CacheOriginalColors();
        }

        private void OnEnable()
        {
            if (health == null) return;

            health.DamageTaken += HandleDamageTaken;
            health.PlayerDied += HandlePlayerDied;
            health.PlayerRespawned += HandlePlayerRespawned;
        }

        private void OnDisable()
        {
            if (health == null) return;

            health.DamageTaken -= HandleDamageTaken;
            health.PlayerDied -= HandlePlayerDied;
            health.PlayerRespawned -= HandlePlayerRespawned;
        }

        private void HandleDamageTaken(DamageInfo damageInfo)
        {
            if (!isDown)
            {
                PlayHitFlash();
            }

            PlayKnockback(damageInfo.HitDirection);
        }

        private void HandlePlayerDied()
        {
            isDown = true;

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
                flashRoutine = null;
            }

            ApplyColor(downColor);
        }

        private void HandlePlayerRespawned()
        {
            isDown = false;
            RestoreOriginalColors();
        }

        private void PlayHitFlash()
        {
            if (targetRenderer == null) return;

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            flashRoutine = StartCoroutine(HitFlashRoutine());
        }

        private IEnumerator HitFlashRoutine()
        {
            ApplyColor(hitColor);
            yield return new WaitForSeconds(flashDuration);

            if (!isDown)
            {
                RestoreOriginalColors();
            }

            flashRoutine = null;
        }

        private void PlayKnockback(Vector3 direction)
        {
            if (characterController == null) return;
            if (direction.sqrMagnitude <= 0.001f) return;

            if (knockbackRoutine != null)
            {
                StopCoroutine(knockbackRoutine);
            }

            knockbackRoutine = StartCoroutine(KnockbackRoutine(direction.normalized));
        }

        private IEnumerator KnockbackRoutine(Vector3 direction)
        {
            float elapsed = 0f;
            while (elapsed < knockbackDuration)
            {
                float step = knockbackDistance * (Time.deltaTime / knockbackDuration);
                characterController.Move(direction * step);
                elapsed += Time.deltaTime;
                yield return null;
            }

            knockbackRoutine = null;
        }

        private void CacheOriginalColors()
        {
            if (targetRenderer == null) return;

            Material[] materials = targetRenderer.materials;
            originalColors = new Color[materials.Length];

            for (int i = 0; i < materials.Length; i++)
            {
                originalColors[i] = GetMaterialColor(materials[i]);
            }
        }

        private void RestoreOriginalColors()
        {
            if (targetRenderer == null || originalColors == null) return;

            Material[] materials = targetRenderer.materials;
            int count = Mathf.Min(materials.Length, originalColors.Length);
            for (int i = 0; i < count; i++)
            {
                SetMaterialColor(materials[i], originalColors[i]);
            }
        }

        private void ApplyColor(Color color)
        {
            if (targetRenderer == null) return;

            foreach (Material material in targetRenderer.materials)
            {
                SetMaterialColor(material, color);
            }
        }

        private Color GetMaterialColor(Material material)
        {
            if (material.HasProperty("_BaseColor"))
            {
                return material.GetColor("_BaseColor");
            }

            return material.HasProperty("_Color") ? material.GetColor("_Color") : Color.white;
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
