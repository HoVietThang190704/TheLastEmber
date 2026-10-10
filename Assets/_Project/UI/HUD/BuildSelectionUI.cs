using System.Text;
using TheLastEmber.Features.BuildingSystem;
using UnityEngine;

namespace TheLastEmber.UI.HUD
{
    public class BuildSelectionUI : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private BuildingPlacer buildingPlacer;

        [Header("Layout")]
        [SerializeField] private Vector2 position = new Vector2(24f, 184f);
        [SerializeField] private Vector2 panelSize = new Vector2(316f, 72f);

        [Header("Colors")]
        [SerializeField] private Color backgroundColor = new Color(0.06f, 0.055f, 0.05f, 0.74f);
        [SerializeField] private Color borderColor = new Color(0.72f, 0.62f, 0.48f, 0.82f);
        [SerializeField] private Color activeTextColor = new Color(0.94f, 0.88f, 0.76f, 1f);
        [SerializeField] private Color mutedTextColor = new Color(0.72f, 0.66f, 0.56f, 0.9f);
        [SerializeField] private Color affordableColor = new Color(0.55f, 0.95f, 0.58f, 1f);
        [SerializeField] private Color blockedColor = new Color(1f, 0.46f, 0.36f, 1f);

        private readonly StringBuilder textBuilder = new StringBuilder(96);
        private Texture2D whiteTexture;
        private GUIStyle headerStyle;
        private GUIStyle lineStyle;
        private GUIStyle costStyle;

        private void Awake()
        {
            whiteTexture = Texture2D.whiteTexture;
        }

        private void Start()
        {
            if (buildingPlacer == null)
            {
                buildingPlacer = FindAnyObjectByType<BuildingPlacer>();
            }
        }

        private void OnGUI()
        {
            if (buildingPlacer == null) return;

            EnsureStyles();

            Rect panelRect = new Rect(position.x, position.y, panelSize.x, panelSize.y);
            Rect shadowRect = new Rect(panelRect.x + 3f, panelRect.y + 3f, panelRect.width, panelRect.height);

            DrawRect(shadowRect, new Color(0f, 0f, 0f, 0.28f));
            DrawRect(panelRect, backgroundColor);
            DrawBorder(panelRect, 1f, borderColor);

            BuildableDefinition selected = buildingPlacer.SelectedBuildable;
            string selectedName = selected != null ? selected.DisplayName : "None";
            GUI.Label(new Rect(position.x + 8f, position.y + 5f, panelSize.x - 16f, 18f), $"Build: {selectedName}", headerStyle);

            GUI.Label(new Rect(position.x + 8f, position.y + 27f, panelSize.x - 16f, 18f), BuildHotkeyText(), lineStyle);

            string costText = selected != null ? BuildCostText(selected) : $"Press a hotkey. {KeyToText(buildingPlacer.CancelKey)} cancels.";
            costStyle.normal.textColor = selected == null || buildingPlacer.CanAfford(selected) ? affordableColor : blockedColor;
            GUI.Label(new Rect(position.x + 8f, position.y + 49f, panelSize.x - 16f, 18f), costText, costStyle);
        }

        private string BuildHotkeyText()
        {
            textBuilder.Clear();

            foreach (BuildingPlacer.BuildHotkeySlot slot in buildingPlacer.HotkeySlots)
            {
                if (slot.Buildable == null) continue;

                if (textBuilder.Length > 0)
                {
                    textBuilder.Append("   ");
                }

                textBuilder.Append(KeyToText(slot.Hotkey));
                textBuilder.Append(' ');
                textBuilder.Append(slot.Buildable.DisplayName);
            }

            return textBuilder.Length > 0 ? textBuilder.ToString() : "No build hotkeys assigned.";
        }

        private string BuildCostText(BuildableDefinition buildable)
        {
            textBuilder.Clear();
            textBuilder.Append("Cost: ");

            bool hasCost = false;
            foreach (BuildingCost cost in buildable.Costs)
            {
                if (hasCost)
                {
                    textBuilder.Append(", ");
                }

                textBuilder.Append(cost.Amount);
                textBuilder.Append(' ');
                textBuilder.Append(cost.ResourceType);
                hasCost = true;
            }

            if (!hasCost)
            {
                textBuilder.Append("Free");
            }

            return textBuilder.ToString();
        }

        private void EnsureStyles()
        {
            if (headerStyle != null && lineStyle != null && costStyle != null) return;

            headerStyle = CreateStyle(12, FontStyle.Bold, activeTextColor);
            lineStyle = CreateStyle(11, FontStyle.Normal, mutedTextColor);
            costStyle = CreateStyle(11, FontStyle.Bold, affordableColor);
        }

        private static GUIStyle CreateStyle(int fontSize, FontStyle fontStyle, Color color)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                fontStyle = fontStyle,
                alignment = TextAnchor.MiddleLeft
            };

            style.normal.textColor = color;
            return style;
        }

        private static string KeyToText(KeyCode keyCode)
        {
            string keyText = keyCode.ToString();
            return keyText.StartsWith("Alpha") ? keyText.Substring(5) : keyText;
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
