using TheLastEmber.Core.Audio;
using TheLastEmber.Features.Combat;
using TheLastEmber.Features.Economy;
using UnityEngine;

namespace TheLastEmber.Features.BuildingSystem
{
    /// <summary>
    /// Rương chứa đồ (Storage Chest):
    /// Cho phép người chơi cất giữ hoặc lấy Wood, Stone, SilverEmber khi đứng gần (phím E/J/K/L/O).
    /// </summary>
    public class StorageChestInteractable : MonoBehaviour
    {
        [Header("Interaction")]
        [SerializeField] private float interactionDistance = 3.5f;

        [Header("Stored Resources")]
        [SerializeField] private int storedWood = 0;
        [SerializeField] private int storedStone = 0;
        [SerializeField] private int storedEmber = 0;

        private Transform playerTransform;
        private ResourceWallet playerWallet;
        private bool isPlayerInRange;

        public int StoredWood => storedWood;
        public int StoredStone => storedStone;
        public int StoredEmber => storedEmber;

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

            float distSqr = (transform.position - playerTransform.position).sqrMagnitude;
            isPlayerInRange = distSqr <= interactionDistance * interactionDistance;

            if (isPlayerInRange && playerWallet != null)
            {
                HandleInput();
            }
        }

        private void FindPlayer()
        {
            PlayerMovement pm = FindAnyObjectByType<PlayerMovement>();
            if (pm != null)
            {
                playerTransform = pm.transform;
                playerWallet = pm.GetComponent<ResourceWallet>();
            }
            else
            {
                GameObject player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                    playerWallet = player.GetComponent<ResourceWallet>();
                }
            }
        }

        private void HandleInput()
        {
            // J: Cất 5 Gỗ
            if (Input.GetKeyDown(KeyCode.J))
            {
                DepositResource(ResourceType.Wood, 5);
            }
            // K: Lấy 5 Gỗ
            else if (Input.GetKeyDown(KeyCode.K))
            {
                WithdrawResource(ResourceType.Wood, 5);
            }
            // L: Cất toàn bộ tài nguyên (Quick Deposit All)
            else if (Input.GetKeyDown(KeyCode.L))
            {
                DepositAll();
            }
            // O: Lấy toàn bộ tài nguyên (Quick Withdraw All)
            else if (Input.GetKeyDown(KeyCode.O))
            {
                WithdrawAll();
            }
        }

        public void DepositResource(ResourceType type, int amount)
        {
            if (playerWallet == null || amount <= 0) return;

            int current = playerWallet.GetAmount(type);
            int transfer = Mathf.Min(current, amount);
            if (transfer <= 0) return;

            if (playerWallet.TrySpend(type, transfer))
            {
                AddStored(type, transfer);
                AudioManager.PlayPickup();
            }
        }

        public void WithdrawResource(ResourceType type, int amount)
        {
            if (playerWallet == null || amount <= 0) return;

            int stored = GetStored(type);
            int transfer = Mathf.Min(stored, amount);
            if (transfer <= 0) return;

            SubStored(type, transfer);
            playerWallet.Add(type, transfer);
            AudioManager.PlayPickup();
        }

        public void DepositAll()
        {
            if (playerWallet == null) return;

            int w = playerWallet.GetAmount(ResourceType.Wood);
            int s = playerWallet.GetAmount(ResourceType.Stone);
            int e = playerWallet.GetAmount(ResourceType.SilverEmber);

            if (w == 0 && s == 0 && e == 0) return;

            if (w > 0 && playerWallet.TrySpend(ResourceType.Wood, w)) AddStored(ResourceType.Wood, w);
            if (s > 0 && playerWallet.TrySpend(ResourceType.Stone, s)) AddStored(ResourceType.Stone, s);
            if (e > 0 && playerWallet.TrySpend(ResourceType.SilverEmber, e)) AddStored(ResourceType.SilverEmber, e);

            AudioManager.PlayPickup();
        }

        public void WithdrawAll()
        {
            if (playerWallet == null) return;

            if (storedWood == 0 && storedStone == 0 && storedEmber == 0) return;

            if (storedWood > 0)
            {
                playerWallet.Add(ResourceType.Wood, storedWood);
                storedWood = 0;
            }
            if (storedStone > 0)
            {
                playerWallet.Add(ResourceType.Stone, storedStone);
                storedStone = 0;
            }
            if (storedEmber > 0)
            {
                playerWallet.Add(ResourceType.SilverEmber, storedEmber);
                storedEmber = 0;
            }

            AudioManager.PlayPickup();
        }

        private int GetStored(ResourceType type)
        {
            return type switch
            {
                ResourceType.Wood => storedWood,
                ResourceType.Stone => storedStone,
                ResourceType.SilverEmber => storedEmber,
                _ => 0
            };
        }

        private void AddStored(ResourceType type, int amount)
        {
            switch (type)
            {
                case ResourceType.Wood: storedWood += amount; break;
                case ResourceType.Stone: storedStone += amount; break;
                case ResourceType.SilverEmber: storedEmber += amount; break;
            }
        }

        private void SubStored(ResourceType type, int amount)
        {
            switch (type)
            {
                case ResourceType.Wood: storedWood = Mathf.Max(0, storedWood - amount); break;
                case ResourceType.Stone: storedStone = Mathf.Max(0, storedStone - amount); break;
                case ResourceType.SilverEmber: storedEmber = Mathf.Max(0, storedEmber - amount); break;
            }
        }

        private void OnGUI()
        {
            if (!isPlayerInRange) return;

            int panelW = 340;
            int panelH = 180;
            int x = (Screen.width - panelW) / 2;
            int y = Screen.height - 230;

            // Background & Border
            GUI.color = new Color(0.08f, 0.08f, 0.12f, 0.92f);
            GUI.DrawTexture(new Rect(x, y, panelW, panelH), Texture2D.whiteTexture);
            GUI.color = new Color(0.95f, 0.72f, 0.35f, 0.85f);
            GUI.DrawTexture(new Rect(x, y, panelW, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(x, y + panelH - 2, panelW, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(x, y, 2, panelH), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(x + panelW - 2, y, 2, panelH), Texture2D.whiteTexture);

            GUI.color = Color.white;
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            titleStyle.normal.textColor = new Color(1f, 0.85f, 0.45f);
            GUI.Label(new Rect(x, y + 6, panelW, 24), "📦 RƯƠNG TIẾP TẾ (STORAGE CHEST)", titleStyle);

            GUIStyle contentStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleLeft
            };

            int playerW = playerWallet != null ? playerWallet.GetAmount(ResourceType.Wood) : 0;
            int playerS = playerWallet != null ? playerWallet.GetAmount(ResourceType.Stone) : 0;
            int playerE = playerWallet != null ? playerWallet.GetAmount(ResourceType.SilverEmber) : 0;

            GUI.Label(new Rect(x + 16, y + 36, panelW - 32, 22), 
                $"• Gỗ (Wood): <color=#d4a373>{storedWood}</color> (Túi: {playerW})", contentStyle);
            GUI.Label(new Rect(x + 16, y + 58, panelW - 32, 22), 
                $"• Đá (Stone): <color=#adb5bd>{storedStone}</color> (Túi: {playerS})", contentStyle);
            GUI.Label(new Rect(x + 16, y + 80, panelW - 32, 22), 
                $"• Tàn Bạc (Ember): <color=#ffd166>{storedEmber}</color> (Túi: {playerE})", contentStyle);

            GUIStyle hintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Italic,
                alignment = TextAnchor.MiddleCenter
            };
            hintStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);

            GUI.Label(new Rect(x, y + 108, panelW, 20), "[J] Cất 5 Gỗ   |   [K] Lấy 5 Gỗ", hintStyle);
            GUI.Label(new Rect(x, y + 128, panelW, 20), "[L] Cất Hết (Deposit All)   |   [O] Lấy Hết (Take All)", hintStyle);

            // Nút bấm nhanh bằng chuột
            if (GUI.Button(new Rect(x + 20, y + 150, 140, 22), "Cất Hết (L)"))
            {
                DepositAll();
            }
            if (GUI.Button(new Rect(x + 180, y + 150, 140, 22), "Lấy Hết (O)"))
            {
                WithdrawAll();
            }
        }
    }
}
