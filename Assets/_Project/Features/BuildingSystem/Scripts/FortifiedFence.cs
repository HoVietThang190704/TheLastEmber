using System;
using System.Collections;
using TheLastEmber.Features.Combat;
using UnityEngine;

namespace TheLastEmber.Features.BuildingSystem
{
    /// <summary>
    /// Hàng rào kiên cố: vật cản vật lý và có thanh máu riêng.
    /// Nhận sát thương khi quái tấn công và vỡ (Destroy) khi HP về 0.
    /// </summary>
    public class FortifiedFence : MonoBehaviour, IDamageable
    {
        [Header("Defense Stats")]
        [SerializeField] private float maxHealth = 120f;
        [SerializeField] private float currentHealth;

        [Header("Hit Feedback")]
        [SerializeField] private Color hitColor = new Color(1f, 0.3f, 0.3f, 1f);
        [SerializeField] private float flashDuration = 0.08f;

        private Renderer cachedRenderer;
        private Color originalColor = Color.white;
        private Coroutine flashRoutine;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;

        private void Awake()
        {
            currentHealth = Mathf.Max(1f, maxHealth);
            cachedRenderer = GetComponentInChildren<Renderer>();
            if (cachedRenderer != null && cachedRenderer.material != null)
            {
                originalColor = cachedRenderer.material.color;
            }

            // Đảm bảo có BoxCollider hoặc MeshCollider để cản đường
            Collider col = GetComponent<Collider>();
            if (col == null)
            {
                col = gameObject.AddComponent<BoxCollider>();
            }
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (damageInfo.Amount <= 0f) return;

            currentHealth = Mathf.Max(0f, currentHealth - damageInfo.Amount);
            PlayHitFlash();

            Debug.Log($"<color=#d4a373>[Hàng Rào]</color> Nhận {damageInfo.Amount:0} sát thương. HP: {currentHealth:0}/{maxHealth:0}");

            if (currentHealth <= 0f)
            {
                DestroyFence();
            }
        }

        private void PlayHitFlash()
        {
            if (cachedRenderer == null) return;

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            flashRoutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            if (cachedRenderer != null && cachedRenderer.material != null)
            {
                cachedRenderer.material.color = hitColor;
            }

            yield return new WaitForSeconds(flashDuration);

            if (cachedRenderer != null && cachedRenderer.material != null)
            {
                cachedRenderer.material.color = originalColor;
            }

            flashRoutine = null;
        }

        private void DestroyFence()
        {
            Debug.Log($"<color=#e76f51>[Hàng Rào]</color> Hàng rào tại {transform.position} đã bị phá hủy hoàn toàn!");
            Destroy(gameObject);
        }
    }
}
