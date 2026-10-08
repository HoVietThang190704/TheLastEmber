using System;
using TheLastEmber.Features.Economy;
using UnityEngine;

namespace TheLastEmber.Features.BuildingSystem
{
    [Serializable]
    public struct BuildingCost
    {
        [SerializeField] private ResourceType resourceType;
        [SerializeField] private int amount;

        public ResourceType ResourceType => resourceType;
        public int Amount => amount;
    }
}
