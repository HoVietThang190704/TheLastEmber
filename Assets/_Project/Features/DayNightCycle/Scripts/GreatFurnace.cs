using UnityEngine;

namespace TheLastEmber.Features.DayNightCycle
{
    public class GreatFurnace : MonoBehaviour
    {
        [Header("Furnace Stats")]
        [SerializeField] private int furnaceLevel = 1;
        [SerializeField] private float baseRadius = 15f;
        [SerializeField] private float maxHealth = 2000f;
        private float currentHealth;

        [Header("Fuel")]
        [SerializeField] private float secondsPerWood = 15f;
        [SerializeField] private float maxFuelSeconds = 180f;
        [SerializeField] private float minimumFueledTimeMultiplier = 0.85f;

        [Header("Light Component")]
        [SerializeField] private Light furnaceLight;

        private float currentRadius;
        private float currentFuelSeconds;
        private float lastTimeMultiplier = 1f;

        public float CurrentRadius => currentRadius;
        public float CurrentFuelSeconds => currentFuelSeconds;
        public float MaxFuelSeconds => maxFuelSeconds;
        public bool HasFuel => currentFuelSeconds > 0f;
        public bool CanAcceptFuel => currentFuelSeconds < maxFuelSeconds;
        public Vector3 Position => transform.position;

        private void Awake()
        {
            currentHealth = maxHealth;
            UpdateRadius(1.0f); // Hệ số ban ngày mặc định = 1.0
        }

        private void Update()
        {
            if (currentFuelSeconds <= 0f) return;

            currentFuelSeconds = Mathf.Max(0f, currentFuelSeconds - Time.deltaTime);
            UpdateRadius(lastTimeMultiplier);
        }

        public float AddWoodFuel(int woodAmount)
        {
            if (woodAmount <= 0) return 0f;
            if (!CanAcceptFuel) return 0f;

            float previousFuelSeconds = currentFuelSeconds;
            currentFuelSeconds = Mathf.Min(maxFuelSeconds, currentFuelSeconds + woodAmount * secondsPerWood);
            UpdateRadius(lastTimeMultiplier);

            return currentFuelSeconds - previousFuelSeconds;
        }

        /// <summary>
        /// Cập nhật bán kính an toàn dựa theo hệ số chu kỳ ngày/đêm (M_time)
        /// Công thức: R = (R_base + Level * 4) * M_time
        /// </summary>
        public void UpdateRadius(float timeMultiplier)
        {
            lastTimeMultiplier = timeMultiplier;

            float effectiveTimeMultiplier = HasFuel
                ? Mathf.Max(timeMultiplier, minimumFueledTimeMultiplier)
                : timeMultiplier;

            currentRadius = (baseRadius + furnaceLevel * 4f) * effectiveTimeMultiplier;

            if (furnaceLight != null)
            {
                furnaceLight.range = currentRadius;
            }
        }

        /// <summary>
        /// Kiểm tra xem một vị trí bất kỳ có nằm trong vùng an toàn của Hỏa Lò hay không
        /// </summary>
        public bool IsInsideSafeZone(Vector3 targetPosition)
        {
            float distance = Vector3.Distance(new Vector3(transform.position.x, 0f, transform.position.z),
                                              new Vector3(targetPosition.x, 0f, targetPosition.z));
            return distance <= currentRadius;
        }

        private void OnDrawGizmos()
        {
            // Vẽ vòng tròn bán kính an toàn màu vàng trong Scene view để dễ quan sát
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, currentRadius > 0 ? currentRadius : baseRadius + furnaceLevel * 4f);
        }
    }
}
