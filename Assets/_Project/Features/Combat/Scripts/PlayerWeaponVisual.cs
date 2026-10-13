using UnityEngine;

namespace TheLastEmber.Features.Combat
{
    /// <summary>
    /// Điều khiển hiển thị và vật liệu (Texture PBR) cho Kiếm và Vỏ kiếm.
    /// Tự động nạp Texture và cân chỉnh tỷ lệ, vị trí cầm nắm chính xác.
    /// </summary>
    public class PlayerWeaponVisual : MonoBehaviour
    {
        [Header("Weapon Models (Resources/Weapons)")]
        [SerializeField] private string swordResourcePath = "Weapons/weapon_sword";
        [SerializeField] private string sheathResourcePath = "Weapons/weapon_sheath";
        [SerializeField] private string swordTexturePath = "Weapons/weapon_sword_tex";
        [SerializeField] private string sheathTexturePath = "Weapons/weapon_sheath_tex";

        [Header("Hand Grip Offset (Kiếm trên tay phải)")]
        [SerializeField] private Vector3 swordGripPosition = new Vector3(0.04f, 0.12f, -0.05f);
        [SerializeField] private Vector3 swordGripEulerAngles = new Vector3(90f, 0f, 0f);
        [SerializeField] private Vector3 swordScale = new Vector3(0.75f, 0.75f, 0.75f);

        [Header("Sheath Socket Offset (Vỏ kiếm bên hông trái)")]
        [SerializeField] private Vector3 sheathPosition = new Vector3(-0.18f, 0.08f, -0.05f);
        [SerializeField] private Vector3 sheathEulerAngles = new Vector3(15f, 15f, -35f);
        [SerializeField] private Vector3 sheathScale = new Vector3(0.75f, 0.75f, 0.75f);

        private GameObject swordObject;
        private GameObject sheathObject;
        private CharacterEquipment equipment;

        public void EquipVisuals()
        {
            if (swordObject != null) return;

            equipment = GetComponent<CharacterEquipment>();
            if (equipment == null)
            {
                equipment = gameObject.AddComponent<CharacterEquipment>();
            }

            // 1. Gắn Kiếm & Đổ Texture màu sắc
            GameObject swordPrefab = Resources.Load<GameObject>(swordResourcePath);
            if (swordPrefab != null)
            {
                swordObject = equipment.AttachItem(swordPrefab, EquipmentSlot.RightHand, swordGripPosition, swordGripEulerAngles, swordScale);
                swordObject.name = "Player_EquippedSword";
                ApplyTexture(swordObject, swordTexturePath);
                Debug.Log("<color=green>[PlayerWeaponVisual]</color> Đã trang bị Kiếm và đổ màu Texture!");
            }

            // 2. Gắn Vỏ Kiếm & Đổ Texture màu sắc
            GameObject sheathPrefab = Resources.Load<GameObject>(sheathResourcePath);
            if (sheathPrefab != null)
            {
                sheathObject = equipment.AttachItem(sheathPrefab, EquipmentSlot.HipsLeft, sheathPosition, sheathEulerAngles, sheathScale);
                sheathObject.name = "Player_EquippedSheath";
                ApplyTexture(sheathObject, sheathTexturePath);
                Debug.Log("<color=green>[PlayerWeaponVisual]</color> Đã trang bị Vỏ Kiếm và đổ màu Texture!");
            }
        }

        private void ApplyTexture(GameObject target, string texturePath)
        {
            Texture2D tex = Resources.Load<Texture2D>(texturePath);
            if (tex == null) return;

            Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null) urpLit = Shader.Find("Standard");

            foreach (Renderer r in renderers)
            {
                Material mat = new Material(urpLit);
                if (mat.HasProperty("_BaseMap"))
                {
                    mat.SetTexture("_BaseMap", tex);
                }
                else if (mat.HasProperty("_MainTex"))
                {
                    mat.SetTexture("_MainTex", tex);
                }

                // Cân bằng độ bóng kim loại Dark Fantasy
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.45f);
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.55f);

                r.material = mat;
            }
        }

        public void PlaySlashAnimation(float duration = 0.3f)
        {
        }
    }
}
