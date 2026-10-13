using TheLastEmber.Features.Combat;
using TheLastEmber.Features.Economy;
using UnityEngine;

namespace TheLastEmber.Features.Combat
{
    /// <summary>
    /// PlayerContext: Đăng ký tập trung duy nhất cho đối tượng Player.
    /// Giúp các component khác truy xuất Player (Transform, Health, Wallet, Movement)
    /// với độ phức tạp O(1), loại bỏ hoàn toàn việc gọi FindAnyObjectByType trong Update/Start.
    /// </summary>
    public class PlayerContext : MonoBehaviour
    {
        private static PlayerContext instance;
        public static PlayerContext Instance => instance;

        public static Transform PlayerTransform => instance != null ? instance.cachedTransform : null;
        public static Vector3 PlayerPosition => instance != null ? instance.cachedTransform.position : Vector3.zero;
        public static ResourceWallet PlayerWallet => instance != null ? instance.cachedWallet : null;
        public static PlayerHealth PlayerHealth => instance != null ? instance.cachedHealth : null;
        public static PlayerCombat PlayerCombat => instance != null ? instance.cachedCombat : null;
        public static PlayerMovement PlayerMovement => instance != null ? instance.cachedMovement : null;

        private Transform cachedTransform;
        private ResourceWallet cachedWallet;
        private PlayerHealth cachedHealth;
        private PlayerCombat cachedCombat;
        private PlayerMovement cachedMovement;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }

            instance = this;
            cachedTransform = transform;
            cachedWallet = GetComponent<ResourceWallet>();
            cachedHealth = GetComponent<PlayerHealth>();
            cachedCombat = GetComponent<PlayerCombat>();
            cachedMovement = GetComponent<PlayerMovement>();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
