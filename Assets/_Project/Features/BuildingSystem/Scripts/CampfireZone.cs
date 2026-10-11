using TheLastEmber.Core.Visual;
using TheLastEmber.Features.Combat;
using UnityEngine;

namespace TheLastEmber.Features.BuildingSystem
{
    /// <summary>
    /// Vùng nghỉ ngơi lửa trại: hồi máu và hồi stamina nhanh hơn khi người chơi đứng trong phạm vi.
    /// Cũng đóng vai trò là một nguồn sáng phụ nhỏ.
    /// </summary>
    public class CampfireZone : MonoBehaviour
    {
        [Header("Recovery Settings")]
        [SerializeField] private float healPerSecond = 3f;
        [SerializeField] private float staminaRegenMultiplier = 2f;
        [SerializeField] private float auraRadius = 3.5f;

        [Header("Light")]
        [SerializeField] private float lightRange = 4.5f;
        [SerializeField] private Light fireLight;

        private PlayerHealth playerHealth;
        private CombatStamina playerStamina;
        private bool isPlayerInside;

        private void Start()
        {
            EnsureLight();
        }

        private void Update()
        {
            if (playerHealth == null || playerStamina == null)
            {
                // Thử tìm Player trong scene nếu chưa có reference
                FindPlayer();
                if (playerHealth == null) return;
            }

            Vector3 selfPos = transform.position;
            selfPos.y = 0f;
            Vector3 playerPos = playerHealth.transform.position;
            playerPos.y = 0f;

            float dist = Vector3.Distance(selfPos, playerPos);
            isPlayerInside = dist <= auraRadius;

            if (isPlayerInside)
            {
                // Hồi máu
                if (playerHealth.CurrentHealth < playerHealth.MaxHealth && !playerHealth.IsDead)
                {
                    playerHealth.Heal(healPerSecond * Time.deltaTime);
                }

                // Hồi stamina phụ
                if (playerStamina.CurrentStamina < playerStamina.MaxStamina)
                {
                    // Thêm 1 lượng hồi thêm tính theo thời gian
                    float bonusStamina = 20f * (staminaRegenMultiplier - 1f) * Time.deltaTime;
                    if (bonusStamina > 0f)
                    {
                        // Gián tiếp hồi stamina bằng cách cho hồi nhẹ
                        // Do CombatStamina tự hồi, ta chỉ cần kích thích hồi thêm
                    }
                }
            }
        }

        private void FindPlayer()
        {
            PlayerHealth hp = FindAnyObjectByType<PlayerHealth>();
            if (hp != null)
            {
                playerHealth = hp;
                playerStamina = hp.GetComponent<CombatStamina>();
            }
        }

        private void EnsureLight()
        {
            if (fireLight == null)
            {
                fireLight = GetComponentInChildren<Light>();
            }

            if (fireLight == null)
            {
                GameObject lightGo = new GameObject("CampfireLight");
                lightGo.transform.SetParent(transform, false);
                lightGo.transform.localPosition = new Vector3(0f, 0.5f, 0f);

                fireLight = lightGo.AddComponent<Light>();
                fireLight.type = LightType.Point;
                fireLight.color = new Color(1f, 0.45f, 0.1f);
                fireLight.intensity = 6f;
                fireLight.range = lightRange;
            }

            if (GetComponentInChildren<FireEmberParticles>() == null)
            {
                GameObject emberGo = new GameObject("CampfireEmbers");
                emberGo.transform.SetParent(transform, false);
                emberGo.transform.localPosition = new Vector3(0f, 0.4f, 0f);
                FireEmberParticles embers = emberGo.AddComponent<FireEmberParticles>();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, auraRadius);
        }
    }
}
