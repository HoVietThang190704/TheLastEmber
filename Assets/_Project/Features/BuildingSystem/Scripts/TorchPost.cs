using System.Collections.Generic;
using TheLastEmber.Core.Visual;
using UnityEngine;

namespace TheLastEmber.Features.BuildingSystem
{
    /// <summary>
    /// Registry quản lý tất cả các nguồn sáng phụ (TorchPost, Campfire...).
    /// Cung cấp hàm kiểm tra nhanh xem một vị trí có nằm trong vùng sáng của bất kỳ ngọn đuốc nào không.
    /// </summary>
    public class TorchPost : MonoBehaviour
    {
        private static readonly HashSet<TorchPost> activeTorches = new HashSet<TorchPost>();

        [Header("Light Range")]
        [SerializeField] private float lightRadius = 6f;
        [SerializeField] private Light attachedLight;

        public float LightRadius => lightRadius;

        private void OnEnable()
        {
            activeTorches.Add(this);
            EnsureLightConfigured();
        }

        private void OnDisable()
        {
            activeTorches.Remove(this);
        }

        private void EnsureLightConfigured()
        {
            if (attachedLight == null)
            {
                attachedLight = GetComponentInChildren<Light>();
            }

            if (attachedLight == null)
            {
                GameObject lightGo = new GameObject("TorchLight");
                lightGo.transform.SetParent(transform, false);
                lightGo.transform.localPosition = new Vector3(0f, 1.8f, 0f);

                attachedLight = lightGo.AddComponent<Light>();
                attachedLight.type = LightType.Point;
                attachedLight.color = new Color(1f, 0.65f, 0.25f);
                attachedLight.intensity = 8f;
                attachedLight.range = lightRadius;
            }
            else
            {
                attachedLight.range = lightRadius;
            }

            // Gắn hạt lửa than hoa nếu chưa có
            if (GetComponentInChildren<FireEmberParticles>() == null)
            {
                GameObject emberGo = new GameObject("TorchEmbers");
                emberGo.transform.SetParent(transform, false);
                emberGo.transform.localPosition = new Vector3(0f, 1.8f, 0f);
                FireEmberParticles embers = emberGo.AddComponent<FireEmberParticles>();
            }
        }

        public bool IsInsideLight(Vector3 targetPosition)
        {
            Vector3 pos = transform.position;
            pos.y = 0f;
            targetPosition.y = 0f;
            return Vector3.Distance(pos, targetPosition) <= lightRadius;
        }

        /// <summary>
        /// Kiểm tra xem một vị trí có nằm trong vùng an toàn của bất kỳ ngọn đuốc nào đang hoạt động hay không.
        /// </summary>
        public static bool IsInAnyTorchLight(Vector3 worldPosition)
        {
            foreach (TorchPost torch in activeTorches)
            {
                if (torch != null && torch.IsInsideLight(worldPosition))
                {
                    return true;
                }
            }

            return false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, lightRadius);
        }
    }
}
