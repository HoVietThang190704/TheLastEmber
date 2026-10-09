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
        [SerializeField] private Color backgroundColor = new Color(0.08f, 0.08f, 0.08f, 0.75f);
        [SerializeField] private Color fillColor = new Color(0.9f, 0.2f, 0.16f, 0.95f);
        [SerializeField] private Color borderColor = new Color(0.95f, 0.95f, 0.95f, 0.8f);

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
            Rect fillRect = new Rect(position.x + 2f, position.y + 2f, (size.x - 4f) * normalizedHealth, size.y - 4f);

            DrawRect(backgroundRect, backgroundColor);
            DrawRect(fillRect, fillColor);
            DrawBorder(backgroundRect, 2f, borderColor);
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
