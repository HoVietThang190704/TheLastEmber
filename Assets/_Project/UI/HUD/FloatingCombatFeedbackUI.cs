using System.Collections.Generic;
using TheLastEmber.Features.BuildingSystem;
using TheLastEmber.Features.NightMarket;
using UnityEngine;

namespace TheLastEmber.UI.HUD
{
    /// <summary>
    /// Hiển thị thanh máu nổi (Overhead Health Bar) trên đầu quái vật/công trình bị thương,
    /// và số sát thương nảy lên (Floating Damage Numbers) khi có va chạm chiến đấu.
    /// </summary>
    public class FloatingCombatFeedbackUI : MonoBehaviour
    {
        private static FloatingCombatFeedbackUI instance;
        public static FloatingCombatFeedbackUI Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<FloatingCombatFeedbackUI>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("FloatingCombatFeedbackUI");
                        instance = go.AddComponent<FloatingCombatFeedbackUI>();
                    }
                }
                return instance;
            }
        }

        private struct FloatingText
        {
            public Vector3 worldPosition;
            public string text;
            public Color color;
            public float timeRemaining;
            public float totalDuration;
            public float floatSpeed;
        }

        private readonly List<FloatingText> activeTexts = new List<FloatingText>();
        private Camera cachedCamera;
        private GUIStyle damageTextStyle;
        private Texture2D whiteTexture;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }

            instance = this;
            cachedCamera = Camera.main;
            whiteTexture = Texture2D.whiteTexture;
        }

        private void Update()
        {
            if (cachedCamera == null)
            {
                cachedCamera = Camera.main;
            }

            // Cập nhật vị trí các số sát thương bay lên
            for (int i = activeTexts.Count - 1; i >= 0; i--)
            {
                FloatingText ft = activeTexts[i];
                ft.timeRemaining -= Time.deltaTime;
                ft.worldPosition += Vector3.up * (ft.floatSpeed * Time.deltaTime);

                if (ft.timeRemaining <= 0f)
                {
                    activeTexts.RemoveAt(i);
                }
                else
                {
                    activeTexts[i] = ft;
                }
            }
        }

        public static void SpawnDamageNumber(Vector3 worldPos, float damage, Color color)
        {
            Instance?.AddFloatingText(worldPos + Vector3.up * 0.5f, $"-{damage:0}", color);
        }

        public void AddFloatingText(Vector3 worldPos, string text, Color color, float duration = 0.75f, float floatSpeed = 1.2f)
        {
            activeTexts.Add(new FloatingText
            {
                worldPosition = worldPos,
                text = text,
                color = color,
                timeRemaining = duration,
                totalDuration = duration,
                floatSpeed = floatSpeed
            });
        }

        private void OnGUI()
        {
            if (cachedCamera == null) return;

            DrawEnemyOverheadHealthBars();
            DrawFenceOverheadHealthBars();
            DrawFloatingDamageTexts();
        }

        private void DrawEnemyOverheadHealthBars()
        {
            MistEnemy[] enemies = FindObjectsByType<MistEnemy>(FindObjectsInactive.Exclude);
            foreach (MistEnemy enemy in enemies)
            {
                if (enemy == null || enemy.CurrentHealth <= 0f) continue;
                if (enemy.CurrentHealth >= enemy.MaxHealth) continue; // Chỉ hiện khi đã bị mất máu

                Vector3 headWorldPos = enemy.transform.position + Vector3.up * 1.55f;
                DrawOverheadBar(headWorldPos, enemy.CurrentHealth / enemy.MaxHealth, 48f, 5f, new Color(0.95f, 0.25f, 0.25f, 0.9f));
            }
        }

        private void DrawFenceOverheadHealthBars()
        {
            FortifiedFence[] fences = FindObjectsByType<FortifiedFence>(FindObjectsInactive.Exclude);
            foreach (FortifiedFence fence in fences)
            {
                if (fence == null || fence.CurrentHealth <= 0f) continue;
                if (fence.CurrentHealth >= fence.MaxHealth) continue;

                Vector3 headWorldPos = fence.transform.position + Vector3.up * 1.3f;
                DrawOverheadBar(headWorldPos, fence.CurrentHealth / fence.MaxHealth, 52f, 5f, new Color(0.85f, 0.65f, 0.35f, 0.9f));
            }
        }

        private void DrawOverheadBar(Vector3 worldPos, float fillRatio, float barWidth, float barHeight, Color fillColor)
        {
            Vector3 screenPos = cachedCamera.WorldToScreenPoint(worldPos);
            if (screenPos.z <= 0f) return; // Phía sau camera

            float x = screenPos.x - barWidth * 0.5f;
            float y = Screen.height - screenPos.y - barHeight * 0.5f;

            Rect bgRect = new Rect(x, y, barWidth, barHeight);
            Rect fillRect = new Rect(x, y, barWidth * Mathf.Clamp01(fillRatio), barHeight);

            // Nền đen viền
            DrawRect(new Rect(x - 1, y - 1, barWidth + 2, barHeight + 2), new Color(0f, 0f, 0f, 0.7f));
            DrawRect(bgRect, new Color(0.12f, 0.12f, 0.12f, 0.85f));
            // Máu
            DrawRect(fillRect, fillColor);
        }

        private void DrawFloatingDamageTexts()
        {
            EnsureTextStyle();

            foreach (FloatingText ft in activeTexts)
            {
                Vector3 screenPos = cachedCamera.WorldToScreenPoint(ft.worldPosition);
                if (screenPos.z <= 0f) continue;

                float alpha = Mathf.Clamp01(ft.timeRemaining / (ft.totalDuration * 0.5f));
                Color c = ft.color;
                c.a = alpha;
                damageTextStyle.normal.textColor = c;

                float x = screenPos.x - 30f;
                float y = Screen.height - screenPos.y - 12f;

                // Bóng đen chữ
                GUIStyle shadowStyle = new GUIStyle(damageTextStyle);
                shadowStyle.normal.textColor = new Color(0f, 0f, 0f, alpha * 0.8f);
                GUI.Label(new Rect(x + 1f, y + 1f, 60f, 24f), ft.text, shadowStyle);

                // Chữ chính
                GUI.Label(new Rect(x, y, 60f, 24f), ft.text, damageTextStyle);
            }
        }

        private void EnsureTextStyle()
        {
            if (damageTextStyle != null) return;

            damageTextStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
        }

        private void DrawRect(Rect rect, Color color)
        {
            Color old = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, whiteTexture);
            GUI.color = old;
        }
    }
}
