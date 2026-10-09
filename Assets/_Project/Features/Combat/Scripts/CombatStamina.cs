using System;
using UnityEngine;

namespace TheLastEmber.Features.Combat
{
    public class CombatStamina : MonoBehaviour
    {
        [Header("Stamina")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float regenPerSecond = 22f;
        [SerializeField] private float regenDelayAfterSpend = 0.65f;

        private float currentStamina;
        private float regenDelayTimer;

        public event Action<float, float> StaminaChanged;

        public float CurrentStamina => currentStamina;
        public float MaxStamina => maxStamina;
        public float Normalized => maxStamina <= 0f ? 0f : currentStamina / maxStamina;

        private void Awake()
        {
            currentStamina = Mathf.Max(0f, maxStamina);
            StaminaChanged?.Invoke(currentStamina, maxStamina);
        }

        private void Update()
        {
            if (regenDelayTimer > 0f)
            {
                regenDelayTimer -= Time.deltaTime;
                return;
            }

            if (currentStamina >= maxStamina) return;

            currentStamina = Mathf.Min(maxStamina, currentStamina + regenPerSecond * Time.deltaTime);
            StaminaChanged?.Invoke(currentStamina, maxStamina);
        }

        public bool CanSpend(float amount)
        {
            return amount <= 0f || currentStamina >= amount;
        }

        public bool TrySpend(float amount)
        {
            if (amount <= 0f) return true;
            if (!CanSpend(amount)) return false;

            currentStamina = Mathf.Max(0f, currentStamina - amount);
            regenDelayTimer = regenDelayAfterSpend;
            StaminaChanged?.Invoke(currentStamina, maxStamina);

            return true;
        }
    }
}
