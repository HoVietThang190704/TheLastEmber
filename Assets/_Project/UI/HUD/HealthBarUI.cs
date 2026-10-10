using TheLastEmber.Features.Combat;
using UnityEngine;

namespace TheLastEmber.UI.HUD
{
    public class HealthBarUI : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private PlayerHealth health;

        [Header("Layout")]
        [SerializeField] private Vector2 position = new Vector2(24f, 48f);
        [SerializeField] private Vector2 size = new Vector2(220f, 18f);

        [Header("Colors")]
        [SerializeField] private Color backgroundColor = new Color(0.06f, 0.055f, 0.05f, 0.72f);
        [SerializeField] private Color fillColor = new Color(0.82f, 0.16f, 0.12f, 0.96f);
        [SerializeField] private Color borderColor = new Color(0.72f, 0.62f, 0.48f, 0.82f);

        private Texture2D whiteTexture;
        private float normalizedHealth = 1f;

        private void Awake()
        {
            whiteTexture = Texture2D.whiteTexture;
        }

        private void Start()
        {
            if (health == null)
            {
                health = FindAnyObjectByType<PlayerHealth>();
            }

            if (health == null) return;

            normalizedHealth = health.Normalized;
            health.HealthChanged += HandleHealthChanged;
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.HealthChanged -= HandleHealthChanged;
            }
        }

        private void OnGUI()
        {
            Rect backgroundRect = new Rect(position.x, position.y, size.x, size.y);
            Rect fillRect = new Rect(position.x + 3f, position.y + 3f, (size.x - 6f) * normalizedHealth, size.y - 6f);
            Rect shadowRect = new Rect(backgroundRect.x + 3f, backgroundRect.y + 3f, backgroundRect.width, backgroundRect.height);

            DrawRect(shadowRect, new Color(0f, 0f, 0f, 0.28f));
            DrawRect(backgroundRect, backgroundColor);
            DrawRect(fillRect, fillColor);
            DrawRect(new Rect(fillRect.x, fillRect.y, fillRect.width, 2f), new Color(1f, 0.45f, 0.34f, 0.32f));
            DrawBorder(backgroundRect, 1f, borderColor);
        }

        private void HandleHealthChanged(float currentHealth, float maxHealth)
        {
            normalizedHealth = maxHealth <= 0f ? 0f : Mathf.Clamp01(currentHealth / maxHealth);
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
