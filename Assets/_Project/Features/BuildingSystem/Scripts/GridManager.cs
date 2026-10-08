using System.Collections.Generic;
using TheLastEmber.Features.DayNightCycle;
using UnityEngine;

namespace TheLastEmber.Features.BuildingSystem
{
    public class GridManager : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField] private float cellSize = 2f;
        [SerializeField] private Vector3 origin;

        [Header("Build Bounds")]
        [SerializeField] private GreatFurnace furnace;

        private readonly HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();

        public float CellSize => cellSize;

        private void Start()
        {
            if (furnace == null)
            {
                furnace = FindAnyObjectByType<GreatFurnace>();
            }
        }

        public Vector2Int WorldToCell(Vector3 worldPosition)
        {
            Vector3 localPosition = worldPosition - origin;
            int x = Mathf.FloorToInt(localPosition.x / cellSize);
            int y = Mathf.FloorToInt(localPosition.z / cellSize);

            return new Vector2Int(x, y);
        }

        public Vector3 CellToWorldCenter(Vector2Int cell)
        {
            float x = origin.x + (cell.x + 0.5f) * cellSize;
            float z = origin.z + (cell.y + 0.5f) * cellSize;

            return new Vector3(x, origin.y, z);
        }

        public Vector3 SnapToGrid(Vector3 worldPosition)
        {
            return CellToWorldCenter(WorldToCell(worldPosition));
        }

        public bool CanPlace(Vector2Int anchorCell, Vector2Int footprint)
        {
            foreach (Vector2Int cell in GetFootprintCells(anchorCell, footprint))
            {
                if (occupiedCells.Contains(cell))
                {
                    return false;
                }

                if (!IsCellInsideSafeZone(cell))
                {
                    return false;
                }
            }

            return true;
        }

        public void Occupy(Vector2Int anchorCell, Vector2Int footprint)
        {
            foreach (Vector2Int cell in GetFootprintCells(anchorCell, footprint))
            {
                occupiedCells.Add(cell);
            }
        }

        private bool IsCellInsideSafeZone(Vector2Int cell)
        {
            if (furnace == null) return true;
            return furnace.IsInsideSafeZone(CellToWorldCenter(cell));
        }

        private IEnumerable<Vector2Int> GetFootprintCells(Vector2Int anchorCell, Vector2Int footprint)
        {
            Vector2Int safeFootprint = new Vector2Int(Mathf.Max(1, footprint.x), Mathf.Max(1, footprint.y));

            for (int x = 0; x < safeFootprint.x; x++)
            {
                for (int y = 0; y < safeFootprint.y; y++)
                {
                    yield return new Vector2Int(anchorCell.x + x, anchorCell.y + y);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);

            int previewRadius = 12;
            for (int x = -previewRadius; x <= previewRadius; x++)
            {
                for (int y = -previewRadius; y <= previewRadius; y++)
                {
                    Vector3 center = CellToWorldCenter(new Vector2Int(x, y));
                    Gizmos.DrawWireCube(center, new Vector3(cellSize, 0.05f, cellSize));
                }
            }
        }
    }
}
