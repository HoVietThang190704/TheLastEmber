using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheLastEmber.Features.Economy
{
    public class ResourceWallet : MonoBehaviour
    {
        [Header("Starting Resources")]
        [SerializeField] private int startingWood;
        [SerializeField] private int startingStone;
        [SerializeField] private int startingSilverEmber;

        private readonly Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();

        public event Action<ResourceType, int> ResourceChanged;

        private void Awake()
        {
            InitializeResource(ResourceType.Wood, startingWood);
            InitializeResource(ResourceType.Stone, startingStone);
            InitializeResource(ResourceType.SilverEmber, startingSilverEmber);
        }

        public int GetAmount(ResourceType resourceType)
        {
            return resources.TryGetValue(resourceType, out int amount) ? amount : 0;
        }

        public void Add(ResourceType resourceType, int amount)
        {
            if (amount <= 0) return;

            int newAmount = GetAmount(resourceType) + amount;
            resources[resourceType] = newAmount;
            ResourceChanged?.Invoke(resourceType, newAmount);

            Debug.Log($"<color=green>[Tài nguyên]</color> +{amount} {resourceType}. Hiện có: {newAmount}");
        }

        public bool TrySpend(ResourceType resourceType, int amount)
        {
            if (amount <= 0) return true;

            int currentAmount = GetAmount(resourceType);
            if (currentAmount < amount)
            {
                Debug.LogWarning($"<color=yellow>[Tài nguyên]</color> Không đủ {resourceType}. Cần {amount}, hiện có {currentAmount}.");
                return false;
            }

            int newAmount = currentAmount - amount;
            resources[resourceType] = newAmount;
            ResourceChanged?.Invoke(resourceType, newAmount);

            Debug.Log($"<color=orange>[Tài nguyên]</color> -{amount} {resourceType}. Còn lại: {newAmount}");
            return true;
        }

        private void InitializeResource(ResourceType resourceType, int amount)
        {
            resources[resourceType] = Mathf.Max(0, amount);
        }
    }
}
