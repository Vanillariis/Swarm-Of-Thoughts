using UnityEngine;
using UnityEngine.InputSystem;

public class PaintScript : MonoBehaviour
{
    public Color[] colorList;
    public int colorCount;

    public void OnPaint(InputValue value)
    {
        if (!value.isPressed) return;

        Color c = colorList[colorCount];
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