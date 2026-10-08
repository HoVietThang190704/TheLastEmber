using UnityEngine;

namespace TheLastEmber.Features.Economy
{
    [RequireComponent(typeof(Collider))]
    public class CollectibleResource : MonoBehaviour
    {
        [Header("Resource")]
        [SerializeField] private ResourceType resourceType = ResourceType.Wood;
        [SerializeField] private int amount = 1;

        [Header("Pickup")]
        [SerializeField] private bool destroyOnPickup = true;

        private bool hasBeenCollected;

        private void Reset()
        {
            Collider resourceCollider = GetComponent<Collider>();
            resourceCollider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasBeenCollected) return;

            ResourceWallet wallet = other.GetComponent<ResourceWallet>();
            if (wallet == null) return;

            hasBeenCollected = true;
            wallet.Add(resourceType, amount);

            if (destroyOnPickup)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
