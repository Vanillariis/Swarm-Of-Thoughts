using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 10f;

    [Header("Flower Settings")]
    public GameObject flowerPrefab;
    public float plantDistance = 1.5f;

    // This stores our single flower reference
    private FlowerLogic placedFlower;

    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector3 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        ApplyMovement();
        ApplyRotation();
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
    }

    // This handles both Planting AND Interacting
    public void Plant(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        // If we HAVEN'T planted a flower yet
        if (placedFlower == null)
        {
            PlantNewFlower();
        }
        // If the flower ALREADY exists
        else
        {
            InteractWithFlower();
        }
    }

    private void PlantNewFlower()
    {
        Vector3 spawnPos = transform.position + (transform.forward * plantDistance);
        spawnPos.y = transform.position.y; // Keep it on the ground

        GameObject flowerObj = Instantiate(flowerPrefab, spawnPos, Quaternion.identity);

        // Save the reference to the Flower component
        placedFlower = flowerObj.GetComponent<FlowerLogic>();

        // Set the unique message (Logic you already have)
        placedFlower.SetMessage("This is my unique flower message!");

        Debug.Log("Flower planted for the first time!");
    }

    private void InteractWithFlower()
    {
        // Simply read the message from the flower we stored earlier
        string msg = placedFlower.GetMessage();

        Debug.Log("Interacting with flower! The message is: " + msg);
    }

    private void ApplyMovement()
    {
        rb.linearVelocity = new Vector3(moveDirection.x * speed, rb.linearVelocity.y, moveDirection.z * speed);
    }

    private void ApplyRotation()
    {
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        }
    }
}