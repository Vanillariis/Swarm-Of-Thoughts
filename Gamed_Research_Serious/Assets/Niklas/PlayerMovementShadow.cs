using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementShadow : MonoBehaviour
{

    public float speed = 50f; // Speed of the player movement
    public RectTransform canvasRect; // Reference to the Canvas component
    private Vector2 moveInput; // Variable to store the movement input

    public int playerHealth = 10; // Player's health

    public System.Action OnDeath;


    private RectTransform rectTransform; // Reference to the RectTransform component
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rectTransform = GetComponent<RectTransform>(); // Get the RectTransform component attached to the player
        playerHealth = 10; // Initialize player's health
    }
    public void OnMovement(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
    public void ClampToCanvas()
    {
        Vector2 pos = rectTransform.anchoredPosition;

        Vector2 canvasSize = canvasRect.sizeDelta;
        Vector2 objectSize = rectTransform.sizeDelta;

        float minX = -canvasSize.x / 2 + objectSize.x / 2;
        float maxX = canvasSize.x / 2 - objectSize.x / 2;

        float minY = -canvasSize.y / 2 + objectSize.y / 2;
        float maxY = canvasSize.y / 2 - objectSize.y / 2;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        rectTransform.anchoredPosition = pos;
    }
    // Update is called once per frame
    void Update()
    {

        moveInput = moveInput.normalized;

        Vector2 movement = moveInput * speed * Time.deltaTime;

        rectTransform.anchoredPosition += movement;


        // Push player back if outside
        if (!StressMask.IsInside(rectTransform))
        {
            rectTransform.anchoredPosition -= movement * 1f;
        }

        ClampToCanvas();
    }
}
