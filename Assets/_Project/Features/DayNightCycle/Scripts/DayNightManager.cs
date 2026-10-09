using UnityEngine;

namespace TheLastEmber.Features.DayNightCycle
{
    public enum DayPhase
    {
        Day,
        Dusk,
        Night,
        Dawn
    }

    public class DayNightManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Light sunLight;
        [SerializeField] private GreatFurnace furnace;

        [Header("Cycle Duration (Seconds)")]
        [Tooltip("Tổng thời gian 1 chu kỳ đầy đủ (mặc định 60s để test nhanh)")]
        [SerializeField] private float fullDayLength = 60f;

        [Header("Lighting Settings")]
        [SerializeField] private Color dayColor = new Color(1f, 0.95f, 0.85f);
        [SerializeField] private Color nightColor = new Color(0.1f, 0.15f, 0.35f);
        [SerializeField] private float maxSunIntensity = 1.2f;
        [SerializeField] private float minSunIntensity = 0.05f;

        private float timeOfDay = 0f; // Chạy từ 0.0 đến 1.0
        private DayPhase currentPhase = DayPhase.Day;
        private int nightCount = 0;

        public DayPhase CurrentPhase => currentPhase;
        public int NightCount => nightCount;

        private void Start()
        {
            if (furnace == null)
            {
                furnace = FindAnyObjectByType<GreatFurnace>();
            }

            if (sunLight == null)
            {
                // Duyệt tìm Directional Light chuẩn Unity 6
                Light[] lights = FindObjectsByType<Light>(FindObjectsInactive.Exclude);
                foreach (Light l in lights)
                {
                    if (l.type == LightType.Directional)
                    {
                        sunLight = l;
                        break;
                    }
                }
            }
        }

        private void Update()
        {
            // Cập nhật tiến trình thời gian
            timeOfDay += (Time.deltaTime / fullDayLength);
            if (timeOfDay >= 1f)
            {
                timeOfDay = 0f;
            }

            UpdateLightingAndPhase();
            UpdateFurnaceRadius();
        }

        private void UpdateLightingAndPhase()
        {
            // timeOfDay: 0.0 -> 0.5 là Ban ngày, 0.5 -> 1.0 là Ban đêm
            float sunFactor = Mathf.Cos(timeOfDay * Mathf.PI * 2f); // 1 tại ban ngày, -1 tại ban đêm
            float normalizedFactor = (sunFactor + 1f) / 2f;         // Đưa về dải 0.0 -> 1.0

            // Cập nhật Phase
            DayPhase newPhase;
            if (timeOfDay < 0.4f) newPhase = DayPhase.Day;
            else if (timeOfDay < 0.5f) newPhase = DayPhase.Dusk;
            else if (timeOfDay < 0.9f) newPhase = DayPhase.Night;
            else newPhase = DayPhase.Dawn;

            if (newPhase == DayPhase.Night && currentPhase != DayPhase.Night)
            {
                nightCount++;
                Debug.Log($"<color=#9bd0ff>[Day/Night]</color> Đêm thứ {nightCount} bắt đầu.");
            }

            currentPhase = newPhase;

            // Đổi màu và cường độ ánh sáng mặt trời
            if (sunLight != null)
            {
                sunLight.color = Color.Lerp(nightColor, dayColor, normalizedFactor);
                sunLight.intensity = Mathf.Lerp(minSunIntensity, maxSunIntensity, normalizedFactor);

                // Xoay góc chiếu sáng của mặt trời tạo bóng đổ tự nhiên
                sunLight.transform.rotation = Quaternion.Euler(normalizedFactor * 90f, 30f, 0f);
            }
        }

        private void UpdateFurnaceRadius()
        {
            if (furnace == null) return;

            // Ban ngày hệ số = 1.0; Giữa đêm hệ số co lại còn 0.6
            float sunFactor = (Mathf.Cos(timeOfDay * Mathf.PI * 2f) + 1f) / 2f;
            float timeMultiplier = Mathf.Lerp(0.6f, 1.0f, sunFactor);

            furnace.UpdateRadius(timeMultiplier);
        }
    }
}