using System;
using System.Collections;
using TheLastEmber.Features.Combat;
using TheLastEmber.Features.Economy;
using TheLastEmber.UI.HUD;
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
        [SerializeField] private float contactDamage = 12f;
        [SerializeField] private float attackCooldown = 1.2f;
        [SerializeField] private float obstacleDetectionRadius = 1.1f;

        [Header("Drop on Death")]
        [SerializeField] private int silverEmberDrop = 3;

        [Header("Elite / Boss")]
        [SerializeField] private bool isElite = false;

        [Header("Hit Feedback")]
        [SerializeField] private Color idleColor = new Color(0.24f, 0.55f, 0.62f, 1f);
        [SerializeField] private Color hitColor = Color.white;
        [SerializeField] private float hitFlashDuration = 0.08f;

        private Transform target;
        private IDamageable targetDamageable;
        private Renderer cachedRenderer;
        private float currentHealth;
        private float attackTimer;
        private Coroutine flashRoutine;
        private Action<MistEnemy> diedCallback;
        private ResourceWallet wallet;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsElite => isElite;

        public void ConfigureAsElite(string eliteTitle = "Mist Golem")
        {
            isElite = true;
            name = eliteTitle;
            maxHealth = 200f;
            currentHealth = maxHealth;
            contactDamage = 28f;
            moveSpeed = 1.6f;
            attackRange = 1.8f;
            attackCooldown = 1.6f;
            silverEmberDrop = 15;
            idleColor = new Color(0.65f, 0.15f, 0.22f, 1f); // Đỏ sương máu huyền bí
            ApplyColor(idleColor);
        }

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
            if (distance <= attackRange)
            {
                TickAttackTarget(targetDamageable, target.position);
                return;
            }

            // Kiểm tra xem có vật cản (hàng rào, công trình có IDamageable) chặn đường tiến không
            IDamageable obstacle = CheckForObstacleInPath(toTarget.normalized, out Vector3 obstaclePos);
            if (obstacle != null)
            {
                // Hướng mặt về vật cản và tấn công phá hủy vật cản
                Vector3 toObstacle = obstaclePos - transform.position;
                toObstacle.y = 0f;
                if (toObstacle.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.LookRotation(toObstacle);
                }

                TickAttackTarget(obstacle, obstaclePos);
                return;
            }

            MoveTowardTarget(toTarget);
        }

        public void Initialize(Transform chaseTarget, Action<MistEnemy> onDied, ResourceWallet resourceWallet = null, float statMultiplier = 1f)
        {
            target = chaseTarget;
            diedCallback = onDied;
            wallet = resourceWallet;
            targetDamageable = target != null ? target.GetComponentInParent<IDamageable>() : null;

            // Scale stats theo đầu vào của Wave
            if (statMultiplier > 1f)
            {
                maxHealth = Mathf.Round(maxHealth * statMultiplier);
                currentHealth = maxHealth;
                contactDamage = Mathf.Round(contactDamage * statMultiplier);
                moveSpeed *= Mathf.Sqrt(statMultiplier); // Tốc độ tăng chậm hơn để không quá khó
            }
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            currentHealth = Mathf.Max(0f, currentHealth - damageInfo.Amount);
            PlayHitFlash();

            FloatingCombatFeedbackUI.SpawnDamageNumber(damageInfo.HitPoint != Vector3.zero ? damageInfo.HitPoint : transform.position + Vector3.up, damageInfo.Amount, new Color(1f, 0.85f, 0.3f));
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

        private IDamageable CheckForObstacleInPath(Vector3 moveDir, out Vector3 obstaclePosition)
        {
            obstaclePosition = Vector3.zero;
            Vector3 checkCenter = transform.position + moveDir * (obstacleDetectionRadius * 0.7f) + Vector3.up * 0.5f;
            Collider[] colliders = Physics.OverlapSphere(checkCenter, obstacleDetectionRadius);

            foreach (Collider col in colliders)
            {
                if (col.transform.IsChildOf(transform) || col.transform == transform) continue;
                if (target != null && col.transform.IsChildOf(target)) continue;

                IDamageable damageable = col.GetComponentInParent<IDamageable>();
                if (damageable != null && !damageable.Equals(this))
                {
                    obstaclePosition = col.transform.position;
                    return damageable;
                }
            }

            return null;
        }

        private void TickAttackTarget(IDamageable damageableTarget, Vector3 targetPos)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer > 0f) return;

            attackTimer = attackCooldown;

            if (damageableTarget == null)
            {
                Debug.Log("<color=#9bd0ff>[Night Wave]</color> Mist enemy reached the target.");
                return;
            }

            Vector3 hitDirection = targetPos - transform.position;
            hitDirection.y = 0f;
            if (hitDirection.sqrMagnitude > 0.001f)
            {
                hitDirection.Normalize();
            }
            else
            {
                hitDirection = transform.forward;
            }

            Vector3 hitPoint = targetPos + Vector3.up * 0.75f;
            damageableTarget.TakeDamage(new DamageInfo(contactDamage, gameObject, hitPoint, hitDirection));
        }

        private void Die()
        {
            if (wallet != null && silverEmberDrop > 0)
            {
                wallet.Add(ResourceType.SilverEmber, silverEmberDrop);
                Debug.Log($"<color=#ffd700>[Night Wave]</color> {name} dropped {silverEmberDrop} SilverEmber!");
            }

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
