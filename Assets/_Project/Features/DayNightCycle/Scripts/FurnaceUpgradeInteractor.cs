using TheLastEmber.Features.Economy;
using UnityEngine;

namespace TheLastEmber.Features.DayNightCycle
{
    /// <summary>
    /// Gắn lên Player. Nhấn phím UpgradeKey khi đứng gần Hỏa Lò để tiêu SilverEmber nâng cấp.
    /// </summary>
    public class FurnaceUpgradeInteractor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GreatFurnace furnace;
        [SerializeField] private ResourceWallet wallet;

        [Header("Interaction")]
        [SerializeField] private KeyCode upgradeKey = KeyCode.U;
        [SerializeField] private float interactionRange = 3f;

        public KeyCode UpgradeKey => upgradeKey;
        public bool IsInInteractionRange => furnace != null && Vector3.Distance(transform.position, furnace.Position) <= interactionRange;
        public bool IsFurnaceMaxLevel => furnace != null && furnace.IsMaxLevel;
        public int CurrentUpgradeCost => furnace != null ? furnace.GetUpgradeCost() : -1;
        public int CurrentSilverEmber => wallet != null ? wallet.GetAmount(ResourceType.SilverEmber) : 0;
        public bool CanUpgradeNow
        {
            get
            {
                int cost = CurrentUpgradeCost;
                return wallet != null && IsInInteractionRange && !IsFurnaceMaxLevel && cost >= 0 && CurrentSilverEmber >= cost;
            }
        }

        private void Start()
        {
            if (wallet == null)
            {
                wallet = GetComponent<ResourceWallet>();
            }

            if (furnace == null)
            {
                furnace = FindAnyObjectByType<GreatFurnace>();
            }
        }

        private void Update()
        {
            if (!Input.GetKeyDown(upgradeKey)) return;
            TryUpgradeFurnace();
        }

        private void TryUpgradeFurnace()
        {
            if (wallet == null || furnace == null) return;

            float distance = Vector3.Distance(transform.position, furnace.Position);
            if (distance > interactionRange)
            {
                Debug.Log($"<color=yellow>[Hỏa Lò]</color> Đứng gần Hỏa Lò hơn để nâng cấp. Khoảng cách: {distance:F1}m.");
                return;
            }

            if (furnace.IsMaxLevel)
            {
                Debug.Log("<color=yellow>[Hỏa Lò]</color> Hỏa Lò đã đạt cấp độ tối đa!");
                return;
            }

            int cost = furnace.GetUpgradeCost();
            if (cost < 0) return;

            int currentEmber = wallet.GetAmount(ResourceType.SilverEmber);
            if (currentEmber < cost)
            {
                Debug.Log($"<color=yellow>[Hỏa Lò]</color> Cần {cost} Silver Ember để nâng cấp. Hiện có: {currentEmber}.");
                return;
            }

            if (!wallet.TrySpend(ResourceType.SilverEmber, cost)) return;

            furnace.Upgrade();
        }
    }
}
