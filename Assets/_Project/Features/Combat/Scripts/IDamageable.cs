using UnityEngine;

namespace TheLastEmber.Features.Combat
{
    public readonly struct DamageInfo
    {
        public DamageInfo(float amount, GameObject source, Vector3 hitPoint, Vector3 hitDirection)
        {
            Amount = amount;
            Source = source;
            HitPoint = hitPoint;
            HitDirection = hitDirection;
        }

        public float Amount { get; }
        public GameObject Source { get; }
        public Vector3 HitPoint { get; }
        public Vector3 HitDirection { get; }
    }

    public interface IDamageable
    {
        void TakeDamage(DamageInfo damageInfo);
    }
}
