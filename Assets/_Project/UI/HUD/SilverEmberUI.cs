using TheLastEmber.Features.Economy;
using UnityEngine;

namespace TheLastEmber.UI.HUD
{
    /// <summary>
    /// Displays the player's SilverEmber count on the HUD using OnGUI.
    /// Subscribes to ResourceWallet.ResourceChanged and updates in real-time.
    /// </summary>
    public class SilverEmberUI : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private ResourceWallet wallet;

        [Header("Layout")]
        [SerializeField] private Vector2 position = new Vector2(24f, 74f);
        [SerializeField] private Vector2 badgeSize = new Vector2(140f, 22f);

        [Header("Colors")]
        [SerializeField] private Color backgroundColor = new Color(0.08f, 0.08f, 0.08f, 0.75f);
        [SerializeField] private Color borderColor = new Color(0.95f, 0.95f, 0.95f, 0.8f);
        [SerializeField] private Color textColor = new Color(1f, 0.88f, 0.35f, 1f);

        private Texture2D whiteTexture;
        private GUIStyle labelStyle;
        private int currentAmount;

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

            currentAmount = wallet.GetAmount(ResourceType.SilverEmber);
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
            DrawRect(backgroundRect, backgroundColor);
            DrawBorder(backgroundRect, 2f, borderColor);

            // Label: icon + amount, vertically centered in badge
            Rect labelRect = new Rect(position.x + 6f, position.y + 2f, badgeSize.x - 8f, badgeSize.y - 4f);
            GUI.Label(labelRect, $"\u25c6 {currentAmount} Silver Ember", labelStyle);
        }

        private void HandleResourceChanged(ResourceType type, int newAmount)
        {
            if (type == ResourceType.SilverEmber)
            {
                currentAmount = newAmount;
            }
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
