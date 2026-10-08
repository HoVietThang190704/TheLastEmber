using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheLastEmber.Features.BuildingSystem
{
    [CreateAssetMenu(fileName = "NewBuildable", menuName = "The Last Ember/Building/Buildable Definition")]
    public class BuildableDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string displayName = "New Building";

        [Header("Placement")]
        [SerializeField] private GameObject prefab;
        [SerializeField] private Vector2Int footprint = Vector2Int.one;
        [SerializeField] private float yOffset;

        [Header("Cost")]
        [SerializeField] private BuildingCost[] costs;

        public string DisplayName => displayName;
        public GameObject Prefab => prefab;
        public Vector2Int Footprint => new Vector2Int(Mathf.Max(1, footprint.x), Mathf.Max(1, footprint.y));
        public float YOffset => yOffset;
        public IReadOnlyList<BuildingCost> Costs => costs ?? Array.Empty<BuildingCost>();
    }
}
