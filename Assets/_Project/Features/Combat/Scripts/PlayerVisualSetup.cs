using UnityEngine;

namespace TheLastEmber.Features.Combat
{
    /// <summary>
    /// Quản lý hiển thị mô hình nhân vật Player.
    /// Tự động nạp model 3D (character-archer) làm con, ẩn Capsule placeholder nhưng giữ collider.
    /// </summary>
    public class PlayerVisualSetup : MonoBehaviour
    {
        [Header("Model Settings")]
        [SerializeField] private string modelResourcePath = "Characters/character-archer";
        [SerializeField] private Vector3 modelOffset = new Vector3(0f, -1f, 0f);
        [SerializeField] private Vector3 modelScale = Vector3.one * 1.0f;

        private const string VisualHolderName = "__PlayerVisualModel";

        private void Awake()
        {
            SetupVisual();
        }

        private void SetupVisual()
        {
            // Ẩn MeshRenderer của Capsule placeholder trên Player
            MeshRenderer capsuleRenderer = GetComponent<MeshRenderer>();
            if (capsuleRenderer != null)
            {
                capsuleRenderer.enabled = false;
            }

            // Kiểm tra xem đã có model con chưa
            Transform existingHolder = transform.Find(VisualHolderName);
            if (existingHolder != null)
            {
                return;
            }

            GameObject prefab = Resources.Load<GameObject>(modelResourcePath);
            if (prefab == null)
            {
                // Fallback nếu chưa load được model từ Resources
                if (capsuleRenderer != null)
                {
                    capsuleRenderer.enabled = true;
                }
                Debug.LogWarning($"[PlayerVisualSetup] Chưa tìm thấy model tại Resources/{modelResourcePath}, dùng lại Capsule.");
                return;
            }

            GameObject modelInstance = Instantiate(prefab, transform);
            modelInstance.name = VisualHolderName;
            modelInstance.transform.localPosition = modelOffset;
            modelInstance.transform.localRotation = Quaternion.identity;
            modelInstance.transform.localScale = modelScale;

            // Xoá bỏ collider nếu model FBX có collider thừa tránh va chạm với CharacterController
            Collider[] colliders = modelInstance.GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders)
            {
                Destroy(col);
            }
        }
    }
}
