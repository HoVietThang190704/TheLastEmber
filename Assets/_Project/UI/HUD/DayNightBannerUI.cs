using TheLastEmber.Features.DayNightCycle;
using UnityEngine;

namespace TheLastEmber.UI.HUD
{
    /// <summary>
    /// Hiển thị dải Banner điện ảnh thông báo chuyển giao Đêm/Ngày giữa màn hình:
    /// - Khi Đêm tới: "ĐÊM THỨ [N] - SƯƠNG MÙ TRỖI DẬY"
    /// - Khi Bình minh tới: "BÌNH MINH - ÁNH SÁNG PHỤC SINH"
    /// </summary>
    public class DayNightBannerUI : MonoBehaviour
    {
        [SerializeField] private DayNightManager dayNightManager;
        [SerializeField] private float bannerDuration = 4.0f;

        private DayPhase lastPhase = DayPhase.Day;
        private string primaryText = "";
        private string secondaryText = "";
        private Color primaryColor = Color.white;
        private float bannerTimer = 0f;

        private void Start()
        {
            if (dayNightManager == null)
            {
                dayNightManager = FindAnyObjectByType<DayNightManager>();
            }

            if (dayNightManager != null)
            {
                lastPhase = dayNightManager.CurrentPhase;
            }
        }

        private void Update()
        {
            if (bannerTimer > 0f)
            {
                bannerTimer -= Time.deltaTime;
            }

            if (dayNightManager == null) return;

            DayPhase currentPhase = dayNightManager.CurrentPhase;
            if (currentPhase != lastPhase)
            {
                OnPhaseChanged(lastPhase, currentPhase);
                lastPhase = currentPhase;
            }
        }

        private void OnPhaseChanged(DayPhase from, DayPhase to)
        {
            if (to == DayPhase.Night)
            {
                int night = dayNightManager != null ? dayNightManager.NightCount : 1;
                primaryText = $"❖ ĐÊM THỨ {night} ❖";
                secondaryText = night >= 2 
                    ? "Sương mù trở nên đặc quánh... Tiếng bước chân nặng nề của Mist Golem rền vang!"
                    : "Sương mù trỗi dậy. Giữ vững ngọn lửa để sống sót.";
                primaryColor = new Color(0.95f, 0.25f, 0.35f); // Đỏ huyền bí
                bannerTimer = bannerDuration;
            }
            else if (to == DayPhase.Dawn || to == DayPhase.Day)
            {
                if (from == DayPhase.Night || from == DayPhase.Dawn)
                {
                    primaryText = "☀ BÌNH MINH ĐÃ ĐẾN ☀";
                    secondaryText = "Quái vật sương mù tan biến. Thu thập tài nguyên chuẩn bị cho đêm tiếp theo.";
                    primaryColor = new Color(1f, 0.88f, 0.42f); // Vàng kim rực rỡ
                    bannerTimer = bannerDuration;
                }
            }
        }

        private void OnGUI()
        {
            if (bannerTimer <= 0f) return;

            // Tính Alpha mượt mà (Fade in 0.5s, giữ nguyên, Fade out 0.8s)
            float alpha = 1f;
            float elapsed = bannerDuration - bannerTimer;
            if (elapsed < 0.5f)
            {
                alpha = Mathf.Clamp01(elapsed / 0.5f);
            }
            else if (bannerTimer < 0.8f)
            {
                alpha = Mathf.Clamp01(bannerTimer / 0.8f);
            }

            int bannerW = 680;
            int bannerH = 75;
            int x = (Screen.width - bannerW) / 2;
            int y = 95;

            // Nền đen mờ điện ảnh
            GUI.color = new Color(0.04f, 0.05f, 0.08f, 0.82f * alpha);
            GUI.DrawTexture(new Rect(x, y, bannerW, bannerH), Texture2D.whiteTexture);

            // Viền vàng/đỏ phía trên và dưới
            GUI.color = new Color(primaryColor.r, primaryColor.g, primaryColor.b, 0.9f * alpha);
            GUI.DrawTexture(new Rect(x, y, bannerW, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(x, y + bannerH - 2, bannerW, 2), Texture2D.whiteTexture);

            // Tiêu đề chính
            GUIStyle mainStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            mainStyle.normal.textColor = new Color(primaryColor.r, primaryColor.g, primaryColor.b, alpha);
            GUI.Label(new Rect(x, y + 8, bannerW, 32), primaryText, mainStyle);

            // Dòng phụ chú thích
            GUIStyle subStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Italic,
                alignment = TextAnchor.MiddleCenter
            };
            subStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f, 0.85f * alpha);
            GUI.Label(new Rect(x + 10, y + 42, bannerW - 20, 24), secondaryText, subStyle);
        }
    }
}
