using UnityEngine;

namespace TheLastEmber.Features.Combat
{
    /// <summary>
    /// Điều khiển Camera Isometric theo dõi nhân vật:
    /// - Góc nghiêng chuẩn 50–60 độ nhìn từ trên xuống bao quát chiến trường.
    /// - Cho phép xoay nhẹ theo trục Y (khoảng 30-45 độ) tạo chiều sâu 2.5D/3D kinh điển.
    /// - Hỗ trợ Zoom chuột mượt mà (cuộn chuột giữa).
    /// - Bao quát được cả Player, Đại Hỏa Lò và khu vực tài nguyên / quái sương mù xung quanh.
    /// </summary>
    public class IsometricCameraFollow : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("Camera Pitch Angle (Nghiêng 50-60 độ)")]
        [Range(40f, 75f)]
        [SerializeField] private float pitchAngle = 55f;

        [Tooltip("Góc xoay chéo nhìn isometric (0 = nhìn thẳng từ Nam ra Bắc, 35-45 = góc chéo isometric chuẩn)")]
        [Range(-180f, 180f)]
        [SerializeField] private float yawAngle = 0f;

        [Header("Distance & Zoom")]
        [Tooltip("Khoảng cách camera tới nhân vật")]
        [SerializeField] private float distance = 17f;
        [SerializeField] private float minDistance = 10f;
        [SerializeField] private float maxDistance = 28f;
        [SerializeField] private float zoomSpeed = 4f;

        [Header("Smoothing")]
        [SerializeField] private float smoothSpeed = 6f;
        [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.0f, 0f);

        private float currentDistance;

        private void Start()
        {
            currentDistance = distance;
            ApplyRotation();
        }

        private void Update()
        {
            HandleZoom();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                FindPlayerTarget();
                if (target == null) return;
            }

            ApplyRotation();

            // Tính vị trí camera dựa trên góc nghiêng, góc xoay và khoảng cách
            Vector3 focusPoint = target.position + targetOffset;
            Quaternion rotation = Quaternion.Euler(pitchAngle, yawAngle, 0f);
            Vector3 desiredPosition = focusPoint - (rotation * Vector3.forward * currentDistance);

            // Nội suy mượt mà
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.rotation = rotation;
        }

        private void HandleZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
            {
                currentDistance = Mathf.Clamp(currentDistance - scroll * zoomSpeed * 5f, minDistance, maxDistance);
            }
        }

        private void ApplyRotation()
        {
            transform.rotation = Quaternion.Euler(pitchAngle, yawAngle, 0f);
        }

        private void FindPlayerTarget()
        {
            if (PlayerContext.PlayerTransform != null)
            {
                target = PlayerContext.PlayerTransform;
            }
        }
    }
}
