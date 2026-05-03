using UnityEngine;

public class RainMoving : MonoBehaviour
{
    public float scrollSpeed = 0.2f;
    private Material mat;
    private Vector2 offset;

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        mat = renderer.material;
    }

    void Update()
    {
        offset.y -= Time.deltaTime * scrollSpeed;
        // This updates the Tiling/Offset of the texture in the shader
        mat.SetTextureOffset("_BaseMap", offset);
    }
}
