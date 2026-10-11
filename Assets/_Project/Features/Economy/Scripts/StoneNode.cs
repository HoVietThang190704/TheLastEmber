using System.Collections;
using TheLastEmber.Features.Combat;
using UnityEngine;

namespace TheLastEmber.Features.Economy
{
    /// <summary>
    /// Mỏ đá tự nhiên: nhận sát thương từ đòn đánh của Player để rơi tài nguyên Stone.
    /// Vỡ hoàn toàn khi HP về 0 và thưởng thêm đá.
    /// </summary>
    public class StoneNode : MonoBehaviour, IDamageable
    {
        [Header("Mining Stats")]
        [SerializeField] private float maxHealth = 60f;
        [SerializeField] private int stonePerHit = 1;
        [SerializeField] private int bonusStoneOnBreak = 2;

        [Header("Hit Feedback")]
        [SerializeField] private Color hitColor = new Color(1f, 1f, 0.8f, 1f);
        [SerializeField] private float flashDuration = 0.08f;
        [SerializeField] private bool shrinkOnDamage = true;

        private float currentHealth;
        private Renderer cachedRenderer;
        private Color originalColor = Color.white;
        private Coroutine flashRoutine;
        private Vector3 initialScale;
        private ResourceWallet playerWallet;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;

        private void Awake()
        {
            currentHealth = Mathf.Max(1f, maxHealth);
            initialScale = transform.localScale;

            cachedRenderer = GetComponentInChildren<Renderer>();
            if (cachedRenderer != null && cachedRenderer.material != null)
            {
                originalColor = cachedRenderer.material.color;
            }

            Collider col = GetComponent<Collider>();
            if (col == null)
            {
                col = gameObject.AddComponent<BoxCollider>();
            }
        }

        private void Start()
        {
            FindPlayerWallet();
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (damageInfo.Amount <= 0f) return;
            if (currentHealth <= 0f) return;

            currentHealth = Mathf.Max(0f, currentHealth - damageInfo.Amount);
            PlayHitFeedback();

            // Nhận tài nguyên mỗi nhát cuốc/chém trúng
            AwardStone(stonePerHit);

            Debug.Log($"<color=#b0c4de>[Mỏ Đá]</color> Khai thác! HP mỏ đá: {currentHealth:0}/{maxHealth:0}. Nhận +{stonePerHit} Đá.");

            if (currentHealth <= 0f)
            {
                BreakNode();
            }
        }

        private void AwardStone(int amount)
        {
            if (amount <= 0) return;

            if (playerWallet == null)
            {
                FindPlayerWallet();
            }

            if (playerWallet != null)
            {
                playerWallet.Add(ResourceType.Stone, amount);
            }
            else
            {
                Debug.LogWarning("[StoneNode] Chưa tìm thấy ResourceWallet trên Player để cộng Stone!");
            }
        }

        private void BreakNode()
        {
            AwardStone(bonusStoneOnBreak);
            Debug.Log($"<color=#708090>[Mỏ Đá]</color> Đã vỡ hoàn toàn! Thưởng thêm +{bonusStoneOnBreak} Đá.");
            Destroy(gameObject);
        }

        private void PlayHitFeedback()
        {
            if (cachedRenderer != null)
            {
                if (flashRoutine != null)
                {
                    StopCoroutine(flashRoutine);
                }
                flashRoutine = StartCoroutine(FlashRoutine());
            }

            if (shrinkOnDamage && maxHealth > 0f)
            {
                float healthRatio = Mathf.Clamp01(currentHealth / maxHealth);
                float scaleFactor = Mathf.Lerp(0.65f, 1f, healthRatio);
                transform.localScale = initialScale * scaleFactor;
            }
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

        private void FindPlayerWallet()
        {
            playerWallet = Object.FindAnyObjectByType<ResourceWallet>();
        }
    }
}
