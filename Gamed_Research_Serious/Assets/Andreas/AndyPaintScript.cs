using UnityEngine;
using UnityEngine.InputSystem;

public class AndyPaintScript : MonoBehaviour
{
    public Color[] colorList;
    private Color activeColor = Color.white;

    // 1. CALL THIS FROM THE BUTTONS
    // In the Button inspector, call this and enter 0 for Red, 1 for Blue, etc.
    public void SetActiveColor(int index)
    {
        if (index >= 0 && index < colorList.Length)
        {
            activeColor = colorList[index];
            activeColor.a = 1f;
        }
    }

    // 2. CALL THIS FROM THE INPUT SYSTEM (OnPaint action)
    public void OnPaint(InputValue value)
    {
        if (!value.isPressed) return;

        // Perform the raycast to find the house part
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        if (hit.collider != null)
        {
            SpriteRenderer renderer = hit.collider.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = activeColor;
            }
        }
    }
}