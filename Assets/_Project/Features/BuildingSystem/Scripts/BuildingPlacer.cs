using TheLastEmber.Features.Economy;
using UnityEngine;

namespace TheLastEmber.Features.BuildingSystem
{
    public class BuildingPlacer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera placementCamera;
        [SerializeField] private GridManager gridManager;
        [SerializeField] private ResourceWallet wallet;

        [Header("Building")]
        [SerializeField] private BuildableDefinition selectedBuildable;

        [Header("Input")]
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private KeyCode cancelKey = KeyCode.Escape;

        [Header("Preview Colors")]
        [SerializeField] private Color validColor = new Color(0.25f, 1f, 0.45f, 0.55f);
        [SerializeField] private Color invalidColor = new Color(1f, 0.2f, 0.2f, 0.55f);

        private GameObject previewInstance;
        private Vector2Int currentCell;
        private bool hasValidPointerPosition;
        private bool canPlaceAtCurrentCell;

        private void Start()
        {
            if (placementCamera == null)
            {
                placementCamera = Camera.main;
            }

            if (gridManager == null)
            {
                gridManager = FindAnyObjectByType<GridManager>();
            }

            if (wallet == null)
            {
                wallet = GetComponent<ResourceWallet>();
            }

            RefreshPreview();
        }

        private void Update()
        {
            if (selectedBuildable == null || gridManager == null) return;

            UpdatePointerPosition();
            UpdatePreview();

            if (Input.GetKeyDown(cancelKey))
            {
                SetSelectedBuildable(null);
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                TryPlaceSelectedBuildable();
            }
        }

        public void SetSelectedBuildable(BuildableDefinition buildableDefinition)
        {
            selectedBuildable = buildableDefinition;
            RefreshPreview();
        }

        private void UpdatePointerPosition()
        {
            hasValidPointerPosition = false;

            if (placementCamera == null) return;

            Ray ray = placementCamera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 200f, groundMask, QueryTriggerInteraction.Ignore)) return;

            currentCell = gridManager.WorldToCell(hit.point);
            hasValidPointerPosition = true;
            canPlaceAtCurrentCell = CanAffordSelectedBuildable() && gridManager.CanPlace(currentCell, selectedBuildable.Footprint);
        }

        private void UpdatePreview()
        {
            if (previewInstance == null) return;

            previewInstance.SetActive(hasValidPointerPosition);
            if (!hasValidPointerPosition) return;

            Vector3 snappedPosition = gridManager.CellToWorldCenter(currentCell);
            snappedPosition.y += selectedBuildable.YOffset;
            previewInstance.transform.position = snappedPosition;

            ApplyPreviewColor(canPlaceAtCurrentCell ? validColor : invalidColor);
        }

        private void TryPlaceSelectedBuildable()
        {
            if (!hasValidPointerPosition || !canPlaceAtCurrentCell) return;
            if (selectedBuildable.Prefab == null) return;

            SpendSelectedBuildableCost();

            Vector3 buildPosition = gridManager.CellToWorldCenter(currentCell);
            buildPosition.y += selectedBuildable.YOffset;

            Instantiate(selectedBuildable.Prefab, buildPosition, Quaternion.identity);
            gridManager.Occupy(currentCell, selectedBuildable.Footprint);

            Debug.Log($"<color=cyan>[Xây dựng]</color> Đã đặt {selectedBuildable.DisplayName} tại ô {currentCell}.");
        }

        private bool CanAffordSelectedBuildable()
        {
            if (selectedBuildable == null) return false;
            if (wallet == null) return true;

            foreach (BuildingCost cost in selectedBuildable.Costs)
            {
                if (wallet.GetAmount(cost.ResourceType) < cost.Amount)
                {
                    return false;
                }
            }

            return true;
        }

        private void SpendSelectedBuildableCost()
        {
            if (wallet == null) return;

            foreach (BuildingCost cost in selectedBuildable.Costs)
            {
                wallet.TrySpend(cost.ResourceType, cost.Amount);
            }
        }

        private void RefreshPreview()
        {
            if (previewInstance != null)
            {
                Destroy(previewInstance);
            }

            if (selectedBuildable == null || selectedBuildable.Prefab == null) return;

            previewInstance = Instantiate(selectedBuildable.Prefab);
            previewInstance.name = $"{selectedBuildable.DisplayName}_Preview";
            DisablePreviewColliders();
            ApplyPreviewColor(invalidColor);
        }

        private void DisablePreviewColliders()
        {
            Collider[] colliders = previewInstance.GetComponentsInChildren<Collider>();
            foreach (Collider previewCollider in colliders)
            {
                previewCollider.enabled = false;
            }
        }

        private void ApplyPreviewColor(Color color)
        {
            Renderer[] renderers = previewInstance.GetComponentsInChildren<Renderer>();
            foreach (Renderer previewRenderer in renderers)
            {
                foreach (Material material in previewRenderer.materials)
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
}
