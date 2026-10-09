using UnityEngine;

namespace TheLastEmber.Features.Combat
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float rotationSpeed = 15f;
        [SerializeField] private float gravity = -9.81f;

        [Header("State")]
        [SerializeField] private PlayerHealth health;

        private CharacterController characterController;
        private Vector3 velocity;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            if (health == null)
            {
                health = GetComponent<PlayerHealth>();
            }
        }

        private void Update()
        {
            if (health == null || !health.IsDead)
            {
                HandleMovement();
            }

            ApplyGravity();
        }

        private void HandleMovement()
        {
            // Nhận input từ bàn phím (WASD hoặc phím mũi tên)
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

            if (direction.magnitude >= 0.1f)
            {
                // Xoay nhân vật theo hướng di chuyển
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                // Di chuyển nhân vật
                characterController.Move(direction * (moveSpeed * Time.deltaTime));
            }
        }

        private void ApplyGravity()
        {
            // Đảm bảo nhân vật bám sàn
            if (characterController.isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }
    }
}
