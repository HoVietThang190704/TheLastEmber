using UnityEngine;

namespace TheLastEmber.Features.Combat
{
    public class IsometricCameraFollow : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("Camera Settings")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 12f, -8f);
        [SerializeField] private float smoothSpeed = 5f;

        private void Start()
        {
            // Thiết lập góc nghiêng chuẩn nhìn từ trên xuống
            transform.rotation = Quaternion.Euler(50f, 0f, 0f);
        }

        private void LateUpdate()
        {
            if (target == null) return;

            // Tính toán vị trí mong muốn dựa trên vị trí nhân vật + khoảng cách offset
            Vector3 desiredPosition = target.position + offset;

            // Làm mượt chuyển động của camera (tránh bị giật khi nhân vật đổi hướng)
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            transform.position = smoothedPosition;
        }
    }
}