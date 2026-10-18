using TheLastEmber.Features.BuildingSystem;
using UnityEngine;

namespace TheLastEmber.Features.Combat
{
    /// <summary>
    /// Điều khiển Camera góc nhìn thứ 3 (TPP) chuẩn phong cách PUBG:
    /// - Góc nhìn qua vai phải (Over-the-shoulder).
    /// - Xoay tự do 360 độ quanh nhân vật bằng chuột (Mouse X / Mouse Y).
    /// - Hỗ trợ Zoom chuột mượt mà (cuộn chuột giữa).
    /// - Chống xuyên tường/vật cản/mặt đất (Camera Collision SphereCast).
    /// - Tự động khóa trỏ chuột (Cursor Lock) chuẩn game bắn súng/hành động, nhấn Alt/Esc để mở khóa chuột.
    /// </summary>
    public class IsometricCameraFollow : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("PUBG Third-Person Perspective")]
        [Tooltip("Góc ngẩng/cúi của camera (Pitch)")]
        [Range(-30f, 75f)]
        [SerializeField] private float pitchAngle = 14f;

        [Tooltip("Góc xoay ngang 360 độ quanh nhân vật (Yaw)")]
        [SerializeField] private float yawAngle = 0f;

        [Tooltip("Giới hạn góc cúi thấp nhất")]
        [SerializeField] private float pitchMin = -28f;

        [Tooltip("Giới hạn góc ngẩng cao nhất")]
        [SerializeField] private float pitchMax = 68f;

        [Header("Distance & Zoom")]
        [Tooltip("Khoảng cách mặc định từ camera tới nhân vật (chuẩn PUBG TPP)")]
        [SerializeField] private float distance = 3.8f;
        [SerializeField] private float minDistance = 1.6f;
        [SerializeField] private float maxDistance = 6.5f;
        [SerializeField] private float zoomSpeed = 3.5f;

        [Header("PUBG Over-the-Shoulder Offset")]
        [Tooltip("Độ lệch qua vai phải (X: lệch phải 0.45m, Y: ngang tầm mắt 1.55m)")]
        [SerializeField] private Vector3 targetOffset = new Vector3(0.45f, 1.55f, 0f);

        [Header("Mouse Controls")]
        [SerializeField] private bool enableMouseLook = true;
        [SerializeField] private float mouseSensitivityX = 2.2f;
        [SerializeField] private float mouseSensitivityY = 1.8f;
        [SerializeField] private bool invertY = false;

        [Header("Smoothing & Collision")]
        [SerializeField] private float smoothSpeed = 16f;
        [SerializeField] private float collisionRadius = 0.25f;
        [SerializeField] private LayerMask collisionLayers = ~0;

        private float currentDistance;
        private bool isCursorLocked = true;
        private BuildingPlacer buildingPlacer;

        public float YawAngle => yawAngle;
        public float PitchAngle => pitchAngle;
        public bool IsCursorLocked => isCursorLocked;

        private void Start()
        {
            currentDistance = distance;
            FindPlayerTarget();
            buildingPlacer = FindAnyObjectByType<BuildingPlacer>();

            if (Application.isPlaying)
            {
                SetCursorLock(true);
            }
        }

        private void Update()
        {
            HandleCursorLock();
            HandleMouseLook();
            HandleZoom();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                FindPlayerTarget();
                if (target == null) return;
            }

            // Tính toán vị trí focus (qua vai nhân vật)
            Quaternion yawRotation = Quaternion.Euler(0f, yawAngle, 0f);
            Vector3 worldShoulderOffset = yawRotation * targetOffset;
            Vector3 focusPoint = target.position + worldShoulderOffset;

            // Xoay góc nhìn
            Quaternion rotation = Quaternion.Euler(pitchAngle, yawAngle, 0f);

            // Kiểm tra chống xuyên tường/đất (Camera Collision)
            Vector3 desiredDirection = -(rotation * Vector3.forward);
            float targetDist = currentDistance;

            if (Physics.SphereCast(focusPoint, collisionRadius, desiredDirection, out RaycastHit hit, currentDistance, collisionLayers, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform != target && !hit.transform.IsChildOf(target))
                {
                    targetDist = Mathf.Max(minDistance * 0.6f, hit.distance - 0.1f);
                }
            }

            Vector3 targetPosition = focusPoint + desiredDirection * targetDist;

            // Nội suy mượt mà vị trí và hướng nhìn
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
            transform.rotation = rotation;
        }

        private void HandleMouseLook()
        {
            if (!enableMouseLook || !isCursorLocked) return;

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            yawAngle += mouseX * mouseSensitivityX;
            yawAngle = Mathf.Repeat(yawAngle + 180f, 360f) - 180f;

            float yFactor = invertY ? 1f : -1f;
            pitchAngle += mouseY * mouseSensitivityY * yFactor;
            pitchAngle = Mathf.Clamp(pitchAngle, pitchMin, pitchMax);
        }

        private void HandleZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
            {
                currentDistance = Mathf.Clamp(currentDistance - scroll * zoomSpeed * 4f, minDistance, maxDistance);
            }
        }

        private void HandleCursorLock()
        {
            // Nếu đang trong chế độ đặt công trình, tạm mở khóa chuột để rê đặt
            if (buildingPlacer != null && buildingPlacer.SelectedBuildable != null)
            {
                if (isCursorLocked) SetCursorLock(false);
                return;
            }

            // Nhấn Alt hoặc Esc để mở/khóa chuột tự do
            if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt) || Input.GetKeyDown(KeyCode.Escape))
            {
                SetCursorLock(!isCursorLocked);
            }
            else if (!isCursorLocked && Input.GetMouseButtonDown(0))
            {
                SetCursorLock(true);
            }
        }

        public void SetCursorLock(bool locked)
        {
            isCursorLocked = locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
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
