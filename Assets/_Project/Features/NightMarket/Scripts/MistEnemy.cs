using System;
using System.Collections;
using TheLastEmber.Features.Combat;
using UnityEngine;

namespace TheLastEmber.Features.NightMarket
{
    public class MistEnemy : MonoBehaviour, IDamageable
    {
        [Header("Stats")]
        [SerializeField] private float maxHealth = 50f;
        [SerializeField] private float moveSpeed = 2.2f;

        [Header("Contact Pressure")]
        [SerializeField] private float attackRange = 1.3f;
        [SerializeField] private float attackCooldown = 1.2f;

        [Header("Hit Feedback")]
        [SerializeField] private Color idleColor = new Color(0.24f, 0.55f, 0.62f, 1f);
        [SerializeField] private Color hitColor = Color.white;
        [SerializeField] private float hitFlashDuration = 0.08f;

        private Transform target;
        private Renderer cachedRenderer;
        private float currentHealth;
        private float attackTimer;
        private Coroutine flashRoutine;
        private Action<MistEnemy> diedCallback;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;

        private void Awake()
        {
            currentHealth = Mathf.Max(1f, maxHealth);
            cachedRenderer = GetComponentInChildren<Renderer>();
            ApplyColor(idleColor);
        }

        private void Update()
        {
            if (target == null) return;

            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;

            float distance = toTarget.magnitude;
            if (distance > attackRange)
            {
                MoveTowardTarget(toTarget);
                return;
            }

            TickContactAttack();
        }

        public void Initialize(Transform chaseTarget, Action<MistEnemy> onDied)
        {
            target = chaseTarget;
            diedCallback = onDied;
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            currentHealth = Mathf.Max(0f, currentHealth - damageInfo.Amount);
            PlayHitFlash();

            Debug.Log($"<color=#7ee7ff>[Night Wave]</color> {name} took {damageInfo.Amount:0} damage. HP: {currentHealth:0}/{maxHealth:0}");

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        private void MoveTowardTarget(Vector3 toTarget)
        {
            Vector3 direction = toTarget.normalized;
            transform.position += direction * (moveSpeed * Time.deltaTime);

            if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        private void TickContactAttack()
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer > 0f) return;

            attackTimer = attackCooldown;
            Debug.Log("<color=#9bd0ff>[Night Wave]</color> Mist enemy reached the target.");
        }

        private void Die()
        {
            diedCallback?.Invoke(this);
            Destroy(gameObject);
        }

        private void PlayHitFlash()
        {
            if (cachedRenderer == null) return;

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            flashRoutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            ApplyColor(hitColor);
            yield return new WaitForSeconds(hitFlashDuration);
            ApplyColor(idleColor);
            flashRoutine = null;
        }

        private void ApplyColor(Color color)
        {
            if (cachedRenderer == null) return;

            foreach (Material material in cachedRenderer.materials)
            {
                if (material.HasProperty("_BaseColor"))
                {
                    material.SetColor("_BaseColor", color);
                }
                else if (material.HasProperty("_Color"))
                {
                    material.SetColor("_Color", color);
                }
            }
        }
    }
}
