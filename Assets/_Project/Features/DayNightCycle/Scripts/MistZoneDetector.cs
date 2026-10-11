using TheLastEmber.Features.BuildingSystem;
using TheLastEmber.Features.Combat;
using UnityEngine;

namespace TheLastEmber.Features.DayNightCycle
{
    public class MistZoneDetector : MonoBehaviour
    {
        [Header("Furnace Reference")]
        [SerializeField] private GreatFurnace furnace;

        [Header("Mist Debuff Settings")]
        [SerializeField] private float damagePerSecond = 5f;
        [SerializeField] private float tickInterval = 1f;

        private bool isInMist = false;
        private float tickTimer = 0f;
        private IDamageable damageable;

        public bool IsInMist => isInMist;

        private void Awake()
        {
            damageable = GetComponentInParent<IDamageable>();
            if (damageable == null)
            {
                damageable = GetComponentInChildren<IDamageable>();
            }
        }

        private void Start()
        {
            // Tự động tìm Hỏa Lò nếu chưa gán thủ công
            if (furnace == null)
            {
                furnace = Object.FindAnyObjectByType<GreatFurnace>();
            }
        }

        private void Update()
        {
            if (furnace == null) return;

            // Kiểm tra xem vị trí của nhân vật có nằm trong vùng an toàn không (Hỏa Lò hoặc Đuốc)
            bool insideSafeZone = furnace.IsInsideSafeZone(transform.position) || TorchPost.IsInAnyTorchLight(transform.position);

            if (!insideSafeZone && !isInMist)
            {
                isInMist = true;
                tickTimer = 0f; // Sẵn sàng nhận sát thương ngay nhịp đầu
                Debug.LogWarning("<color=cyan>[Sương Mù]:</color> Người chơi đã bước ra khỏi vùng sáng an toàn!");
            }
            else if (insideSafeZone && isInMist)
            {
                isInMist = false;
                tickTimer = 0f;
                Debug.Log("<color=orange>[Hỏa Lò]:</color> Người chơi đã quay trở lại vùng sáng an toàn.");
            }

            // Xử lý trừ máu định kỳ khi ở ngoài sương
            if (isInMist)
            {
                ApplyMistDamage();
            }
        }

        private void ApplyMistDamage()
        {
            tickTimer -= Time.deltaTime;
            if (tickTimer > 0f) return;

            tickTimer = Mathf.Max(0.1f, tickInterval);
            float damageAmount = damagePerSecond * tickInterval;

            if (damageable != null)
            {
                // HitDirection = Vector3.zero để không gây knockback làm văng người chơi
                DamageInfo mistDamage = new DamageInfo(damageAmount, gameObject, transform.position, Vector3.zero);
                damageable.TakeDamage(mistDamage);
                Debug.Log($"<color=red>[Sương Mù]</color> Gây {damageAmount:0.#} sát thương sương mù lên Người chơi!");
            }
            else
            {
                Debug.LogWarning("<color=red>[Sương Mù]</color> Không tìm thấy IDamageable trên Player để gây sát thương!");
            }
        }
    }
}