using TheLastEmber.Core.Audio;
using TheLastEmber.Features.Combat;
using TheLastEmber.Features.Economy;
using UnityEngine;

namespace TheLastEmber.Features.BuildingSystem
{
    /// <summary>
    /// Bàn chế tạo (Workbench): Cho phép người chơi khi đứng gần nâng cấp
    /// Rìu/Vũ khí (phím F) và Giáp (phím G) bằng tài nguyên Wood & Stone.
    /// </summary>
    public class WorkbenchInteractable : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private float interactionDistance = 2.8f;
        [SerializeField] private KeyCode weaponUpgradeKey = KeyCode.F;
        [SerializeField] private KeyCode armorUpgradeKey = KeyCode.G;

        [Header("Weapon Upgrade Levels")]
        [SerializeField] private int weaponLevel = 1;
        private readonly int[] weaponWoodCosts = { 8, 16 };
        private readonly int[] weaponStoneCosts = { 5, 12 };
        private readonly float[] weaponDamageValues = { 40f, 65f };

        [Header("Armor Upgrade Levels")]
        [SerializeField] private int armorLevel = 1;
        private readonly int[] armorWoodCosts = { 6, 14 };
        private readonly int[] armorStoneCosts = { 6, 14 };
        private readonly float[] armorHealthValues = { 150f, 220f };

        private Transform playerTransform;
        private ResourceWallet playerWallet;
        private PlayerCombat playerCombat;
        private PlayerHealth playerHealth;
        private bool isPlayerInRange;

        private Texture2D whiteTexture;
        private GUIStyle headerStyle;
        private GUIStyle bodyStyle;
        private GUIStyle keyStyle;

        private void Awake()
        {
            whiteTexture = Texture2D.whiteTexture;
        }

        private void Start()
        {
            FindPlayer();
        }

        private void Update()
        {
            if (playerTransform == null)
            {
                FindPlayer();
                if (playerTransform == null) return;
            }

            float dist = Vector3.Distance(transform.position, playerTransform.position);
            isPlayerInRange = dist <= interactionDistance;

            if (!isPlayerInRange) return;

            if (Input.GetKeyDown(weaponUpgradeKey))
            {
                TryUpgradeWeapon();
            }

            if (Input.GetKeyDown(armorUpgradeKey))
            {
                TryUpgradeArmor();
            }
        }

        private void TryUpgradeWeapon()
        {
            if (weaponLevel > weaponWoodCosts.Length)
            {
                Debug.Log("<color=yellow>[Bàn Chế Tạo]</color> Vũ khí đã đạt cấp tối đa!");
                return;
            }

            int index = weaponLevel - 1;
            int woodCost = weaponWoodCosts[index];
            int stoneCost = weaponStoneCosts[index];

            if (playerWallet == null || playerCombat == null) return;

            if (playerWallet.GetAmount(ResourceType.Wood) < woodCost || playerWallet.GetAmount(ResourceType.Stone) < stoneCost)
            {
                Debug.Log($"<color=red>[Bàn Chế Tạo]</color> Thiếu tài nguyên! Cần {woodCost} Wood & {stoneCost} Stone.");
                return;
            }

            playerWallet.TrySpend(ResourceType.Wood, woodCost);
            playerWallet.TrySpend(ResourceType.Stone, stoneCost);

            float newDamage = weaponDamageValues[index];
            playerCombat.UpgradeDamage(newDamage);
            weaponLevel++;

            AudioManager.PlayBuild();
            AudioManager.PlayHit();
        }

        private void TryUpgradeArmor()
        {
            if (armorLevel > armorWoodCosts.Length)
            {
                Debug.Log("<color=yellow>[Bàn Chế Tạo]</color> Giáp đã đạt cấp tối đa!");
                return;
            }

            int index = armorLevel - 1;
            int woodCost = armorWoodCosts[index];
            int stoneCost = armorStoneCosts[index];

            if (playerWallet == null || playerHealth == null) return;

            if (playerWallet.GetAmount(ResourceType.Wood) < woodCost || playerWallet.GetAmount(ResourceType.Stone) < stoneCost)
            {
                Debug.Log($"<color=red>[Bàn Chế Tạo]</color> Thiếu tài nguyên! Cần {woodCost} Wood & {stoneCost} Stone.");
                return;
            }

            playerWallet.TrySpend(ResourceType.Wood, woodCost);
            playerWallet.TrySpend(ResourceType.Stone, stoneCost);

            float newHealth = armorHealthValues[index];
            playerHealth.UpgradeMaxHealth(newHealth);
            armorLevel++;

            AudioManager.PlayBuild();
            AudioManager.PlayFuel();
        }

        private void OnGUI()
        {
            if (!isPlayerInRange) return;

            EnsureStyles();

            float panelWidth = 320f;
            float panelHeight = 84f;
            float x = Screen.width * 0.5f - panelWidth * 0.5f;
            float y = Screen.height - 180f;

            Rect panelRect = new Rect(x, y, panelWidth, panelHeight);
            Rect shadowRect = new Rect(x + 3f, y + 3f, panelWidth, panelHeight);

            // Nền & viền
            DrawRect(shadowRect, new Color(0f, 0f, 0f, 0.35f));
            DrawRect(panelRect, new Color(0.06f, 0.05f, 0.04f, 0.88f));
            DrawBorder(panelRect, 1.5f, new Color(0.72f, 0.58f, 0.42f, 0.9f));

            // Header
            GUI.Label(new Rect(x + 10f, y + 4f, panelWidth - 20f, 20f), "--- BÀN CHẾ TẠO (WORKBENCH) ---", headerStyle);

            // Line 1: Vũ khí
            string weaponText;
            if (weaponLevel <= weaponWoodCosts.Length)
            {
                int idx = weaponLevel - 1;
                weaponText = $"[F] Rèn Rìu Lv.{weaponLevel + 1} ({weaponDamageValues[idx]:0} Dmg) - {weaponWoodCosts[idx]}W {weaponStoneCosts[idx]}S";
            }
            else
            {
                weaponText = $"[F] Rìu Chiến: TỐI ĐA (MAX)";
            }
            GUI.Label(new Rect(x + 10f, y + 26f, panelWidth - 20f, 22f), weaponText, keyStyle);

            // Line 2: Giáp
            string armorText;
            if (armorLevel <= armorWoodCosts.Length)
            {
                int idx = armorLevel - 1;
                armorText = $"[G] Rèn Giáp Lv.{armorLevel + 1} ({armorHealthValues[idx]:0} HP) - {armorWoodCosts[idx]}W {armorStoneCosts[idx]}S";
            }
            else
            {
                armorText = $"[G] Giáp Thép: TỐI ĐA (MAX)";
            }
            GUI.Label(new Rect(x + 10f, y + 50f, panelWidth - 20f, 22f), armorText, bodyStyle);
        }

        private void EnsureStyles()
        {
            if (headerStyle != null) return;

            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            headerStyle.normal.textColor = new Color(1f, 0.72f, 0.38f);

            keyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            keyStyle.normal.textColor = new Color(1f, 0.88f, 0.6f);

            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            bodyStyle.normal.textColor = new Color(0.6f, 0.95f, 0.65f);
        }

        private void DrawRect(Rect rect, Color color)
        {
            Color old = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, whiteTexture);
            GUI.color = old;
        }

        private void DrawBorder(Rect rect, float thickness, Color color)
        {
            DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            DrawRect(new Rect(rect.x, rect.y + rect.height - thickness, rect.width, thickness), color);
            DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            DrawRect(new Rect(rect.x + rect.width - thickness, rect.y, thickness, rect.height), color);
        }

        private void FindPlayer()
        {
            if (PlayerContext.Instance != null)
            {
                playerTransform = PlayerContext.PlayerTransform;
                playerWallet = PlayerContext.PlayerWallet;
                playerCombat = PlayerContext.PlayerCombat;
                playerHealth = PlayerContext.PlayerHealth;
            }
        }
    }
}
