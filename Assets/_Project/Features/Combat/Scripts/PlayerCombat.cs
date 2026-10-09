using System.Collections.Generic;
using UnityEngine;

namespace TheLastEmber.Features.Combat
{
    [RequireComponent(typeof(CombatStamina))]
    public class PlayerCombat : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CombatStamina stamina;
        [SerializeField] private PlayerHealth health;

        [Header("Input")]
        [SerializeField] private KeyCode attackKey = KeyCode.Mouse1;

        [Header("Attack")]
        [SerializeField] private float damage = 25f;
        [SerializeField] private float staminaCost = 20f;
        [SerializeField] private float cooldown = 0.55f;
        [SerializeField] private float windupDuration = 0.12f;
        [SerializeField] private float activeDuration = 0.1f;
        [SerializeField] private float recoverDuration = 0.28f;

        [Header("Hit Detection")]
        [SerializeField] private float hitRange = 1.6f;
        [SerializeField] private float hitRadius = 0.75f;
        [SerializeField] private float hitHeight = 1f;
        [SerializeField] private LayerMask hitMask = ~0;

        private readonly Collider[] hitBuffer = new Collider[12];
        private readonly HashSet<IDamageable> damagedThisSwing = new HashSet<IDamageable>();

        private CombatState currentState = CombatState.Idle;
        private float stateTimer;
        private float cooldownTimer;
        private bool hasAppliedHit;

        public CombatState CurrentState => currentState;
        public bool IsAttacking => currentState == CombatState.Attack || currentState == CombatState.Recover;

        private void Awake()
        {
            if (stamina == null)
            {
                stamina = GetComponent<CombatStamina>();
            }

            if (health == null)
            {
                health = GetComponent<PlayerHealth>();
            }
        }

        private void Update()
        {
            if (health != null && health.IsDead) return;

            UpdateCooldown();
            UpdateState();
            HandleInput();
        }

        private void UpdateCooldown()
        {
            if (cooldownTimer <= 0f) return;
            cooldownTimer = Mathf.Max(0f, cooldownTimer - Time.deltaTime);
        }

        private void UpdateState()
        {
            switch (currentState)
            {
                case CombatState.Attack:
                    stateTimer += Time.deltaTime;

                    if (!hasAppliedHit && stateTimer >= windupDuration)
                    {
                        ApplyHit();
                        hasAppliedHit = true;
                    }

                    if (stateTimer >= windupDuration + activeDuration)
                    {
                        ChangeState(CombatState.Recover);
                    }
                    break;

                case CombatState.Recover:
                    stateTimer += Time.deltaTime;
                    if (stateTimer >= recoverDuration)
                    {
                        ChangeState(GetMovementState());
                    }
                    break;

                default:
                    ChangeState(GetMovementState());
                    break;
            }
        }

        private void HandleInput()
        {
            if (!Input.GetKeyDown(attackKey)) return;
            TryStartAttack();
        }

        private void TryStartAttack()
        {
            if (currentState == CombatState.Attack || currentState == CombatState.Recover) return;
            if (cooldownTimer > 0f) return;
            if (stamina != null && !stamina.TrySpend(staminaCost))
            {
                Debug.Log("<color=yellow>[Combat]</color> Không đủ stamina để tấn công.");
                return;
            }

            damagedThisSwing.Clear();
            hasAppliedHit = false;
            cooldownTimer = cooldown;
            ChangeState(CombatState.Attack);

            Debug.Log("<color=cyan>[Combat]</color> Player bắt đầu tấn công.");
        }

        private CombatState GetMovementState()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            return new Vector2(horizontal, vertical).sqrMagnitude > 0.01f ? CombatState.Move : CombatState.Idle;
        }

        private void ChangeState(CombatState nextState)
        {
            if (currentState == nextState) return;

            currentState = nextState;
            stateTimer = 0f;
        }

        private void ApplyHit()
        {
            Vector3 hitCenter = transform.position + Vector3.up * hitHeight + transform.forward * hitRange;
            int hitCount = Physics.OverlapSphereNonAlloc(hitCenter, hitRadius, hitBuffer, hitMask, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hitCount; i++)
            {
                Collider targetCollider = hitBuffer[i];
                if (targetCollider == null) continue;
                if (targetCollider.transform.root == transform.root) continue;

                IDamageable damageable = targetCollider.GetComponentInParent<IDamageable>();
                if (damageable == null) continue;
                if (!damagedThisSwing.Add(damageable)) continue;

                Vector3 closestPoint = targetCollider.ClosestPoint(hitCenter);
                Vector3 hitDirection = (targetCollider.transform.position - transform.position).normalized;
                damageable.TakeDamage(new DamageInfo(damage, gameObject, closestPoint, hitDirection));
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = currentState == CombatState.Attack ? Color.red : Color.cyan;
            Vector3 hitCenter = transform.position + Vector3.up * hitHeight + transform.forward * hitRange;
            Gizmos.DrawWireSphere(hitCenter, hitRadius);
        }
    }
}
