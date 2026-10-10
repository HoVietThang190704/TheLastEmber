using TheLastEmber.Features.Economy;
using UnityEngine;

namespace TheLastEmber.UI.HUD
{
    /// <summary>
    /// Displays the player's resource wallet on the HUD using OnGUI.
    /// Subscribes to ResourceWallet.ResourceChanged and updates in real-time.
    /// </summary>
    public class SilverEmberUI : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private ResourceWallet wallet;

        [Header("Layout")]
        [SerializeField] private Vector2 position = new Vector2(24f, 74f);
        [SerializeField] private Vector2 badgeSize = new Vector2(316f, 24f);

        [Header("Colors")]
        [SerializeField] private Color backgroundColor = new Color(0.06f, 0.055f, 0.05f, 0.72f);
        [SerializeField] private Color borderColor = new Color(0.72f, 0.62f, 0.48f, 0.82f);
        [SerializeField] private Color textColor = new Color(0.94f, 0.88f, 0.76f, 1f);
        [SerializeField] private Color woodColor = new Color(0.72f, 0.42f, 0.18f, 1f);
        [SerializeField] private Color stoneColor = new Color(0.58f, 0.58f, 0.52f, 1f);
        [SerializeField] private Color emberColor = new Color(1f, 0.78f, 0.25f, 1f);

        private Texture2D whiteTexture;
        private GUIStyle labelStyle;
        private int currentWood;
        private int currentStone;
        private int currentSilverEmber;

        private void Awake()
        {
            whiteTexture = Texture2D.whiteTexture;
        }

        private void Start()
        {
            if (wallet == null)
            {
                wallet = FindAnyObjectByType<ResourceWallet>();
            }

            if (wallet == null)
            {
                Debug.LogWarning("[SilverEmberUI] ResourceWallet not found.");
                return;
            }

            RefreshAmounts();
            wallet.ResourceChanged += HandleResourceChanged;
        }

        private void OnDestroy()
        {
            if (wallet != null)
            {
                wallet.ResourceChanged -= HandleResourceChanged;
            }
        }

        private void OnGUI()
        {
            if (wallet == null) return;

            EnsureLabelStyle();

            Rect backgroundRect = new Rect(position.x, position.y, badgeSize.x, badgeSize.y);
            Rect shadowRect = new Rect(backgroundRect.x + 3f, backgroundRect.y + 3f, backgroundRect.width, backgroundRect.height);
            DrawRect(shadowRect, new Color(0f, 0f, 0f, 0.28f));
            DrawRect(backgroundRect, backgroundColor);
            DrawBorder(backgroundRect, 1f, borderColor);

            float itemWidth = (badgeSize.x - 10f) / 3f;
            float itemHeight = badgeSize.y - 6f;
            float itemY = position.y + 3f;

            DrawResourceItem(new Rect(position.x + 6f, itemY, itemWidth, itemHeight), "Wood", currentWood, woodColor);
            DrawDivider(position.x + 6f + itemWidth, position.y + 5f, badgeSize.y - 10f);
            DrawResourceItem(new Rect(position.x + 6f + itemWidth, itemY, itemWidth, itemHeight), "Stone", currentStone, stoneColor);
            DrawDivider(position.x + 6f + itemWidth * 2f, position.y + 5f, badgeSize.y - 10f);
            DrawResourceItem(new Rect(position.x + 6f + itemWidth * 2f, itemY, itemWidth, itemHeight), "Ember", currentSilverEmber, emberColor);
        }

        private void HandleResourceChanged(ResourceType type, int newAmount)
        {
            switch (type)
            {
                case ResourceType.Wood:
                    currentWood = newAmount;
                    break;
                case ResourceType.Stone:
                    currentStone = newAmount;
                    break;
                case ResourceType.SilverEmber:
                    currentSilverEmber = newAmount;
                    break;
            }
        }

        private void RefreshAmounts()
        {
            currentWood = wallet.GetAmount(ResourceType.Wood);
            currentStone = wallet.GetAmount(ResourceType.Stone);
            currentSilverEmber = wallet.GetAmount(ResourceType.SilverEmber);
        }

        private void EnsureLabelStyle()
        {
            if (labelStyle != null) return;

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            labelStyle.normal.textColor = textColor;
        }

        private void DrawResourceItem(Rect rect, string label, int amount, Color iconColor)
        {
            Rect iconRect = new Rect(rect.x, rect.y + 5f, 8f, 8f);
            DrawRect(iconRect, iconColor);

            Rect textRect = new Rect(rect.x + 14f, rect.y - 1f, rect.width - 14f, rect.height + 2f);
            GUI.Label(textRect, $"{label} {amount}", labelStyle);
        }

        private void DrawDivider(float x, float y, float height)
        {
            DrawRect(new Rect(x, y, 1f, height), new Color(0.72f, 0.62f, 0.48f, 0.25f));
        }

        private void DrawRect(Rect rect, Color color)
        {
            Color previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, whiteTexture);
            GUI.color = previousColor;
        }

        private void DrawBorder(Rect rect, float thickness, Color color)
        {
            DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }
    }
}
