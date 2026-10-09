using System;
using System.Collections;
using UnityEngine;

namespace TheLastEmber.Features.Combat
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [Header("Health")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private bool resetHealthOnDeath = true;
        [SerializeField] private float respawnDelay = 1.5f;

        private float currentHealth;
        private bool isDead;
        private Coroutine respawnRoutine;

        public event Action<float, float> HealthChanged;
        public event Action<DamageInfo> DamageTaken;
        public event Action PlayerDied;
        public event Action PlayerRespawned;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float Normalized => maxHealth <= 0f ? 0f : currentHealth / maxHealth;
        public bool IsDead => isDead;

        private void Awake()
        {
            currentHealth = Mathf.Max(1f, maxHealth);
            HealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (damageInfo.Amount <= 0f) return;
            if (isDead) return;

            currentHealth = Mathf.Max(0f, currentHealth - damageInfo.Amount);
            HealthChanged?.Invoke(currentHealth, maxHealth);
            DamageTaken?.Invoke(damageInfo);

            Debug.Log($"<color=#ff9a8a>[Player]</color> Took {damageInfo.Amount:0} damage. HP: {currentHealth:0}/{maxHealth:0}");

            if (currentHealth <= 0f)
            {
                HandleDeath();
            }
        }

        public void Heal(float amount)
        {
            if (amount <= 0f) return;
            if (isDead) return;

            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            HealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private void HandleDeath()
        {
            if (isDead) return;

            isDead = true;
            PlayerDied?.Invoke();
            Debug.Log("<color=#ff6f61>[Player]</color> Player health reached zero.");

            if (!resetHealthOnDeath) return;

            if (respawnRoutine != null)
            {
                StopCoroutine(respawnRoutine);
            }

            respawnRoutine = StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(respawnDelay);

            currentHealth = maxHealth;
            isDead = false;
            respawnRoutine = null;
            HealthChanged?.Invoke(currentHealth, maxHealth);
            PlayerRespawned?.Invoke();
            Debug.Log("<color=#ffcf7a>[Player]</color> Health reset for MVP play testing.");
        }
    }
}
