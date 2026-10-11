using System.Collections;
using TheLastEmber.Features.Combat;
using UnityEngine;

namespace TheLastEmber.Features.Economy
{
    /// <summary>
    /// Cây tài nguyên: nhận sát thương từ đòn đánh của Player để rơi ra Wood.
    /// Có hiệu ứng rung rinh khi bị chặt và đổ/biến mất khi hết máu kèm thưởng thêm gỗ.
    /// </summary>
    public class TreeResourceNode : MonoBehaviour, IDamageable
    {
        [Header("Wood Harvest Stats")]
        [SerializeField] private float maxHealth = 40f;
        [SerializeField] private int woodPerHit = 1;
        [SerializeField] private int bonusWoodOnFall = 3;

        [Header("Shake Feedback")]
        [SerializeField] private float shakeDuration = 0.15f;
        [SerializeField] private float shakeAngle = 4.5f;

        private float currentHealth;
        private Coroutine shakeRoutine;
        private Quaternion initialRotation;
        private ResourceWallet playerWallet;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;

        private void Awake()
        {
            currentHealth = Mathf.Max(1f, maxHealth);
            initialRotation = transform.localRotation;

            Collider col = GetComponent<Collider>();
            if (col == null)
            {
                CapsuleCollider cap = gameObject.AddComponent<CapsuleCollider>();
                cap.height = 3.5f;
                cap.radius = 0.5f;
                cap.center = new Vector3(0f, 1.75f, 0f);
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

            PlayShakeFeedback();
            AwardWood(woodPerHit);

            Debug.Log($"<color=#deb887>[Đốn Cây]</color> Chặt cây! HP cây: {currentHealth:0}/{maxHealth:0}. Nhận +{woodPerHit} Gỗ.");

            if (currentHealth <= 0f)
            {
                FellTree();
            }
        }

        private void AwardWood(int amount)
        {
            if (amount <= 0) return;

            if (playerWallet == null)
            {
                FindPlayerWallet();
            }

            if (playerWallet != null)
            {
                playerWallet.Add(ResourceType.Wood, amount);
            }
            else
            {
                Debug.LogWarning("[TreeResourceNode] Chưa tìm thấy ResourceWallet trên Player để cộng Wood!");
            }
        }

        private void FellTree()
        {
            AwardWood(bonusWoodOnFall);
            Debug.Log($"<color=#8b4513>[Đốn Cây]</color> Cây đã đổ! Thưởng thêm +{bonusWoodOnFall} Gỗ.");
            Destroy(gameObject);
        }

        private void PlayShakeFeedback()
        {
            if (shakeRoutine != null)
            {
                StopCoroutine(shakeRoutine);
            }
            shakeRoutine = StartCoroutine(ShakeRoutine());
        }

        private IEnumerator ShakeRoutine()
        {
            float elapsed = 0f;
            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / shakeDuration;
                float angle = Mathf.Sin(progress * Mathf.PI * 4f) * (1f - progress) * shakeAngle;
                transform.localRotation = initialRotation * Quaternion.Euler(angle, 0f, 0f);
                yield return null;
            }

            transform.localRotation = initialRotation;
            shakeRoutine = null;
        }

        private void FindPlayerWallet()
        {
            playerWallet = Object.FindAnyObjectByType<ResourceWallet>();
        }
    }
}
