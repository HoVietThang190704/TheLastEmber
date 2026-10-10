using TheLastEmber.Features.DayNightCycle;
using UnityEngine;

namespace TheLastEmber.UI.HUD
{
    public class FurnaceInteractionPromptUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private FurnaceFuelInteractor fuelInteractor;
        [SerializeField] private FurnaceUpgradeInteractor upgradeInteractor;

        [Header("Layout")]
        [SerializeField] private Vector2 position = new Vector2(24f, 128f);
        [SerializeField] private Vector2 panelSize = new Vector2(280f, 48f);

        [Header("Colors")]
        [SerializeField] private Color backgroundColor = new Color(0.06f, 0.055f, 0.05f, 0.76f);
        [SerializeField] private Color borderColor = new Color(0.72f, 0.62f, 0.48f, 0.82f);
        [SerializeField] private Color activeTextColor = new Color(1f, 0.86f, 0.56f, 1f);
        [SerializeField] private Color blockedTextColor = new Color(0.72f, 0.66f, 0.56f, 0.9f);

        private Texture2D whiteTexture;
        private GUIStyle activeStyle;
        private GUIStyle blockedStyle;

        private void Awake()
        {
            whiteTexture = Texture2D.whiteTexture;
        }

        private void Start()
        {
            if (fuelInteractor == null)
            {
                fuelInteractor = FindAnyObjectByType<FurnaceFuelInteractor>();
            }

            if (upgradeInteractor == null)
            {
                upgradeInteractor = FindAnyObjectByType<FurnaceUpgradeInteractor>();
            }
        }

        private void OnGUI()
        {
            bool showFuel = fuelInteractor != null && fuelInteractor.IsInInteractionRange;
            bool showUpgrade = upgradeInteractor != null && upgradeInteractor.IsInInteractionRange;
            if (!showFuel && !showUpgrade) return;

            EnsureStyles();

            Rect panelRect = new Rect(position.x, position.y, panelSize.x, panelSize.y);
            Rect shadowRect = new Rect(panelRect.x + 3f, panelRect.y + 3f, panelRect.width, panelRect.height);

            DrawRect(shadowRect, new Color(0f, 0f, 0f, 0.28f));
            DrawRect(panelRect, backgroundColor);
            DrawBorder(panelRect, 1f, borderColor);

            float lineY = position.y + 6f;
            if (showFuel)
            {
                DrawPromptLine(lineY, GetFuelPromptText(), fuelInteractor.CanFuelNow);
                lineY += 20f;
            }

            if (showUpgrade)
            {
                DrawPromptLine(lineY, GetUpgradePromptText(), upgradeInteractor.CanUpgradeNow);
            }
        }

        private string GetFuelPromptText()
        {
            string key = KeyToText(fuelInteractor.FuelKey);

            if (!fuelInteractor.FurnaceCanAcceptFuel)
            {
                return $"[{key}] Nhien lieu da day";
            }

            if (fuelInteractor.CurrentWood < fuelInteractor.WoodPerFuelAction)
            {
                return $"[{key}] Thieu {fuelInteractor.WoodPerFuelAction} Wood";
            }

            return $"[{key}] Nap cui";
        }

        private string GetUpgradePromptText()
        {
            string key = KeyToText(upgradeInteractor.UpgradeKey);

            if (upgradeInteractor.IsFurnaceMaxLevel)
            {
                return $"[{key}] Hoa Lo da MAX";
            }

            int cost = upgradeInteractor.CurrentUpgradeCost;
            if (cost < 0)
            {
                return $"[{key}] Chua the nang cap";
            }

            if (upgradeInteractor.CurrentSilverEmber < cost)
            {
                return $"[{key}] Can {cost} Ember";
            }

            return $"[{key}] Nang cap ({cost} Ember)";
        }

        private void DrawPromptLine(float y, string text, bool isActive)
        {
            GUIStyle style = isActive ? activeStyle : blockedStyle;
            Rect labelRect = new Rect(position.x + 10f, y, panelSize.x - 20f, 18f);
            GUI.Label(labelRect, text, style);
        }

        private void EnsureStyles()
        {
            if (activeStyle != null && blockedStyle != null) return;

            activeStyle = CreateLabelStyle(activeTextColor);
            blockedStyle = CreateLabelStyle(blockedTextColor);
        }

        private static GUIStyle CreateLabelStyle(Color color)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };

            style.normal.textColor = color;
            return style;
        }

        private static string KeyToText(KeyCode keyCode)
        {
            return keyCode.ToString().ToUpperInvariant();
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
