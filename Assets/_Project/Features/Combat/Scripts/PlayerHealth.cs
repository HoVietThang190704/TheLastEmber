using System;
using UnityEngine;

namespace TheLastEmber.Features.Combat
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [Header("Health")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private bool resetHealthOnDeath = true;

        private float currentHealth;

        public event Action<float, float> HealthChanged;
        public event Action PlayerDied;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float Normalized => maxHealth <= 0f ? 0f : currentHealth / maxHealth;

        private void Awake()
        {
            currentHealth = Mathf.Max(1f, maxHealth);
            HealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (damageInfo.Amount <= 0f) return;

            currentHealth = Mathf.Max(0f, currentHealth - damageInfo.Amount);
            HealthChanged?.Invoke(currentHealth, maxHealth);

            Debug.Log($"<color=#ff9a8a>[Player]</color> Took {damageInfo.Amount:0} damage. HP: {currentHealth:0}/{maxHealth:0}");

            if (currentHealth <= 0f)
            {
                HandleDeath();
            }
        }

        public void Heal(float amount)
        {
            if (amount <= 0f) return;
            if (currentHealth <= 0f && !resetHealthOnDeath) return;

            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            HealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private void HandleDeath()
        {
            PlayerDied?.Invoke();
            Debug.Log("<color=#ff6f61>[Player]</color> Player health reached zero.");

            if (!resetHealthOnDeath) return;

            currentHealth = maxHealth;
            HealthChanged?.Invoke(currentHealth, maxHealth);
            Debug.Log("<color=#ffcf7a>[Player]</color> Health reset for MVP play testing.");
        }
    }
}
