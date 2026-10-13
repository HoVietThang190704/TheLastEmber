using TheLastEmber.Core.Visual;
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
        [SerializeField] private string modelResourcePath = "Characters/character_player";
        [SerializeField] private Vector3 modelOffset = new Vector3(0f, 0f, 0f);
        [SerializeField] private Vector3 modelScale = Vector3.one * 1.0f;

        private const string VisualHolderName = "__PlayerVisualModel";

        private void Awake()
        {
            SetupVisual();
        }

        private void SetupVisual()
        {
            if (GetComponent<SlashArcVisual>() == null)
            {
                gameObject.AddComponent<SlashArcVisual>();
            }

            if (GetComponent<PlayerWeaponVisual>() == null)
            {
                gameObject.AddComponent<PlayerWeaponVisual>();
            }

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
                // Thử load trực tiếp từ đường dẫn thay thế nếu cần
                prefab = Resources.Load<GameObject>("Characters/character_player");
            }

            if (prefab == null)
            {
                // Fallback nếu chưa load được model từ Resources
                if (capsuleRenderer != null)
                {
                    capsuleRenderer.enabled = true;
                }
                Debug.LogWarning($"<color=red>[PlayerVisualSetup]</color> Chưa tìm thấy model tại Resources/{modelResourcePath}, dùng lại Capsule.");
                return;
            }

            GameObject modelInstance = Instantiate(prefab, transform);
            modelInstance.name = VisualHolderName;
            modelInstance.transform.localPosition = modelOffset;
            modelInstance.transform.localRotation = Quaternion.identity;
            modelInstance.transform.localScale = modelScale;
            Debug.Log($"<color=green>[PlayerVisualSetup]</color> Đã nạp thành công model: {prefab.name}");

            // Gán Animator Controller nếu có
            RuntimeAnimatorController animController = Resources.Load<RuntimeAnimatorController>("Animations/PlayerAnimatorController");
            if (animController == null)
            {
                // Tìm kiếm qua Assets
                animController = Resources.Load<RuntimeAnimatorController>("PlayerAnimatorController");
            }

            Animator animator = modelInstance.GetComponent<Animator>();
            if (animator == null)
            {
                animator = modelInstance.AddComponent<Animator>();
            }

            animator.applyRootMotion = false; // QUAN TRỌNG: Khóa Root Motion để chân tay di chuyển theo CharacterController, không bị kéo người

            if (animController != null)
            {
                animator.runtimeAnimatorController = animController;
            }

            // Gọi trang bị vũ khí ngay sau khi model đã hoàn thành nạp vào scene
            PlayerWeaponVisual weaponVisual = GetComponent<PlayerWeaponVisual>();
            if (weaponVisual != null)
            {
                weaponVisual.EquipVisuals();
            }

            // Xoá bỏ collider nếu model FBX có collider thừa tránh va chạm với CharacterController
            Collider[] colliders = modelInstance.GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders)
            {
                Destroy(col);
            }
        }
    }
}
