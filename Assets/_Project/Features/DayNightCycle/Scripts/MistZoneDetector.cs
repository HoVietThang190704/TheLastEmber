using UnityEngine;

namespace TheLastEmber.Features.DayNightCycle
{
    public class MistZoneDetector : MonoBehaviour
    {
        [Header("Furnace Reference")]
        [SerializeField] private GreatFurnace furnace;

        [Header("Mist Debuff Settings")]
        [SerializeField] private float damagePerSecond = 5f;

        private bool isInMist = false;
        public bool IsInMist => isInMist;

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

            // Kiểm tra xem vị trí của nhân vật có nằm trong vùng an toàn không
            bool insideSafeZone = furnace.IsInsideSafeZone(transform.position);

            if (!insideSafeZone && !isInMist)
            {
                isInMist = true;
                Debug.LogWarning("<color=cyan>[Sương Mù]:</color> Người chơi đã bước ra khỏi vùng sáng của Hỏa Lò!");
            }
            else if (insideSafeZone && isInMist)
            {
                isInMist = false;
                Debug.Log("<color=orange>[Hỏa Lò]:</color> Người chơi đã quay trở lại vùng sáng an toàn.");
            }

            // Xử lý trừ máu / phạt khi ở ngoài sương
            if (isInMist)
            {
                ApplyMistDamage();
            }
        }

        // SỬA THÀNH:
      private void ApplyMistDamage()
      {
          // Tạm thời log lượng sát thương nhận vào khi ở ngoài sương
          Debug.Log($"<color=red>[Mất máu]</color> Đang chịu {damagePerSecond * Time.deltaTime:F1} sát thương sương mù!");
      }
    }
}