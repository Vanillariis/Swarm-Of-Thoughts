using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    public float rotationSpeed = 10f;

    [Header("Flower Settings")]
    public float plantDistance = 1.5f;

    [Header("Animation")]
    public Animator animator;

    [Header("Audio")]
    public AudioSource walkingAudioSource;

    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector3 moveDirection;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        ApplyRotation();
        HandleWalkingSound();
        UpdateAnimation();
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
    }

    public void Plant(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        Vector3 spawnPos = transform.position + transform.forward * plantDistance;
        spawnPos.y = transform.position.y;

        FlowerManager.Instance.PlantFlower(spawnPos);
    }

    private void ApplyMovement()
    {
        rb.linearVelocity = new Vector3(
            moveDirection.x * speed,
            rb.linearVelocity.y,
            moveDirection.z * speed
        );
    }

    private void ApplyRotation()
    {
        if (moveDirection.sqrMagnitude <= 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

        rb.MoveRotation(
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            )
        );
    }

    private void HandleWalkingSound()
    {
        if (walkingAudioSource == null)
            return;

        bool isMoving = moveDirection.sqrMagnitude > 0.01f;

        if (isMoving && !walkingAudioSource.isPlaying)
        {
            walkingAudioSource.Play();
        }
        else if (!isMoving && walkingAudioSource.isPlaying)
        {
            walkingAudioSource.Stop();
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null)
            return;

        bool isMoving = moveDirection.sqrMagnitude > 0.01f;
        animator.SetBool("isRunning", isMoving);
    }
}