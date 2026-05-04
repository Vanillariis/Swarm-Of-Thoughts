using UnityEngine;
using UnityEngine.InputSystem;

public class PaintScript : MonoBehaviour
{
    public Color currentColor = Color.blue; // one editable color

    public void OnPaint(InputValue value)
    {
        if (!value.isPressed) return;

        Color c = currentColor;
        c.a = 1f;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        if (hit.collider != null)
        {
            SpriteRenderer renderer = hit.collider.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = c;
            }
        }
    }
}