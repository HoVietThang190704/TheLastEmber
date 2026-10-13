using UnityEngine;

namespace TheLastEmber.Features.Combat
{
    public enum EquipmentSlot
    {
        RightHand,
        LeftHand,
        HipsLeft,
        Back
    }

    /// <summary>
    /// Component quản lý Socket trang bị dùng chung 100% cho mọi nhân vật (Player, NPC, Quái vật).
    /// Tự động gắn bất kỳ vũ khí hay phụ kiện nào vào đúng khớp xương tương ứng của cơ thể Humanoid.
    /// </summary>
    public class CharacterEquipment : MonoBehaviour
    {
        [System.Serializable]
        public struct SocketConfig
        {
            public EquipmentSlot slot;
            public Vector3 localPosition;
            public Vector3 localEulerAngles;
            public Vector3 localScale;
        }

        private Animator cachedAnimator;

        private void Awake()
        {
            cachedAnimator = GetComponentInChildren<Animator>();
        }

        public Transform GetSocketTransform(EquipmentSlot slot)
        {
            if (cachedAnimator == null)
            {
                cachedAnimator = GetComponentInChildren<Animator>();
            }

            HumanBodyBones targetBone = slot switch
            {
                EquipmentSlot.RightHand => HumanBodyBones.RightHand,
                EquipmentSlot.LeftHand => HumanBodyBones.LeftHand,
                EquipmentSlot.HipsLeft => HumanBodyBones.Hips,
                EquipmentSlot.Back => HumanBodyBones.Chest,
                _ => HumanBodyBones.RightHand
            };

            if (cachedAnimator != null && cachedAnimator.isHuman)
            {
                Transform bone = cachedAnimator.GetBoneTransform(targetBone);
                if (bone != null) return bone;
            }

            // Fallback tìm theo tên nếu chưa gán Humanoid Avatar
            string boneName = targetBone.ToString();
            return FindDeepChild(transform, boneName);
        }

        public GameObject AttachItem(GameObject itemPrefab, EquipmentSlot slot, Vector3 offsetPos, Vector3 offsetRot, Vector3 scale)
        {
            if (itemPrefab == null) return null;

            Transform socket = GetSocketTransform(slot);
            Transform parent = socket != null ? socket : transform;

            GameObject instance = Instantiate(itemPrefab, parent);
            instance.transform.localPosition = offsetPos;
            instance.transform.localEulerAngles = offsetRot;
            instance.transform.localScale = scale;

            // Xóa bỏ collider tránh va chạm vật lý với thân nhân vật
            Collider[] cols = instance.GetComponentsInChildren<Collider>();
            foreach (Collider c in cols)
            {
                Destroy(c);
            }

            return instance;
        }

        private Transform FindDeepChild(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name.Contains(name)) return child;
                Transform found = FindDeepChild(child, name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
