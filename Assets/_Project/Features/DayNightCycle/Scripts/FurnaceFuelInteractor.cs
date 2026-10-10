using TheLastEmber.Features.Economy;
using UnityEngine;

namespace TheLastEmber.Features.DayNightCycle
{
    public class FurnaceFuelInteractor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GreatFurnace furnace;
        [SerializeField] private ResourceWallet wallet;

        [Header("Interaction")]
        [SerializeField] private KeyCode fuelKey = KeyCode.E;
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private int woodPerFuelAction = 1;

        public KeyCode FuelKey => fuelKey;
        public int WoodPerFuelAction => woodPerFuelAction;
        public int CurrentWood => wallet != null ? wallet.GetAmount(ResourceType.Wood) : 0;
        public bool FurnaceCanAcceptFuel => furnace != null && furnace.CanAcceptFuel;
        public bool IsInInteractionRange => furnace != null && Vector3.Distance(transform.position, furnace.Position) <= interactionRange;
        public bool CanFuelNow => wallet != null && IsInInteractionRange && FurnaceCanAcceptFuel && CurrentWood >= woodPerFuelAction;

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
            if (!Input.GetKeyDown(fuelKey)) return;
            TryFuelFurnace();
        }

        private void TryFuelFurnace()
        {
            if (wallet == null || furnace == null) return;

            float distance = Vector3.Distance(transform.position, furnace.Position);
            if (distance > interactionRange)
            {
                Debug.Log($"<color=yellow>[Hỏa Lò]</color> Đứng gần Hỏa Lò hơn để nạp củi. Khoảng cách hiện tại: {distance:F1}m.");
                return;
            }

            if (!furnace.CanAcceptFuel)
            {
                Debug.Log("<color=yellow>[Hỏa Lò]</color> Nhiên liệu đã đầy, chưa cần nạp thêm củi.");
                return;
            }

            if (!wallet.TrySpend(ResourceType.Wood, woodPerFuelAction)) return;

            float secondsAdded = furnace.AddWoodFuel(woodPerFuelAction);
            Debug.Log($"<color=orange>[Hỏa Lò]</color> Đã nạp {woodPerFuelAction} củi, thêm {secondsAdded:F0}s nhiên liệu.");
        }
    }
}
