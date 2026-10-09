using UnityEngine;

namespace TheLastEmber.Features.Combat
{
    public class TrainingDummy : MonoBehaviour, IDamageable
    {
        [Header("Health")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private bool resetHealthOnDepleted = true;

        private float currentHealth;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;

        private void Awake()
        {
            currentHealth = Mathf.Max(1f, maxHealth);
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            currentHealth = Mathf.Max(0f, currentHealth - damageInfo.Amount);

            Debug.Log($"<color=red>[Combat]</color> {name} nhận {damageInfo.Amount:0} damage. HP: {currentHealth:0}/{maxHealth:0}");

            if (currentHealth > 0f) return;

            Debug.Log($"<color=red>[Combat]</color> {name} đã bị hạ.");

            if (resetHealthOnDepleted)
            {
                currentHealth = maxHealth;
                Debug.Log($"<color=green>[Combat]</color> {name} hồi lại để tiếp tục test.");
            }
        }
    }
}
