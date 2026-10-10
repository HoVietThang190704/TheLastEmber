using TheLastEmber.Features.DayNightCycle;
using UnityEngine;

namespace TheLastEmber.UI.HUD
{
    /// <summary>
    /// Hiển thị Level hiện tại của Hỏa Lò trên HUD dưới dạng text badge.
    /// Cập nhật real-time qua event GreatFurnace.LevelChanged.
    /// </summary>
    public class FurnaceLevelUI : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private GreatFurnace furnace;

        [Header("Layout")]
        [SerializeField] private Vector2 position = new Vector2(24f, 100f);
        [SerializeField] private Vector2 badgeSize = new Vector2(140f, 22f);

        [Header("Colors")]
        [SerializeField] private Color backgroundColor = new Color(0.06f, 0.055f, 0.05f, 0.72f);
        [SerializeField] private Color borderColor = new Color(0.72f, 0.62f, 0.48f, 0.82f);
        [SerializeField] private Color textColor = new Color(1f, 0.6f, 0.26f, 1f);

        private Texture2D whiteTexture;
        private GUIStyle labelStyle;
        private int currentLevel = 1;
        private int maxLevel = 5;

        private void Awake()
        {
            whiteTexture = Texture2D.whiteTexture;
        }

        private void Start()
        {
            if (furnace == null)
            {
                furnace = FindAnyObjectByType<GreatFurnace>();
            }

            if (furnace == null)
            {
                Debug.LogWarning("[FurnaceLevelUI] GreatFurnace not found.");
                return;
            }

            currentLevel = furnace.FurnaceLevel;
            maxLevel = furnace.MaxLevel;
            furnace.LevelChanged += HandleLevelChanged;
        }

        private void OnDestroy()
        {
            if (furnace != null)
            {
                furnace.LevelChanged -= HandleLevelChanged;
            }
        }

        private void OnGUI()
        {
            if (furnace == null) return;

            EnsureLabelStyle();

            Rect backgroundRect = new Rect(position.x, position.y, badgeSize.x, badgeSize.y);
            Rect shadowRect = new Rect(backgroundRect.x + 3f, backgroundRect.y + 3f, backgroundRect.width, backgroundRect.height);
            DrawRect(shadowRect, new Color(0f, 0f, 0f, 0.28f));
            DrawRect(backgroundRect, backgroundColor);
            DrawBorder(backgroundRect, 1f, borderColor);

            string levelText = currentLevel >= maxLevel
                ? $"\u25cf Lv.{currentLevel} MAX"
                : $"\u25cf Lv.{currentLevel}/{maxLevel}";

            Rect labelRect = new Rect(position.x + 6f, position.y + 2f, badgeSize.x - 8f, badgeSize.y - 4f);
            GUI.Label(labelRect, levelText, labelStyle);
        }

        private void HandleLevelChanged(int newLevel)
        {
            currentLevel = newLevel;
        }

        private void EnsureLabelStyle()
        {
            if (labelStyle != null) return;

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
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
