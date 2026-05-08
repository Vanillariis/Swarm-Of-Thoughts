using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to a 3D object (with a Renderer) OR a UI Image.
/// Every frame it mirrors the current colour from PaintScript so you
/// always have a live preview of the selected colour.
/// </summary>
public class ColorPreviewDisplay : MonoBehaviour
{
    [Tooltip("The PaintScript whose currentColor is previewed.")]
    public PaintScript paintScript;

    // ── 3-D cube / mesh ──────────────────────────────────────────────────────
    private Renderer _renderer;

    // ── UI Image ─────────────────────────────────────────────────────────────
    private Image _image;

    private Color _lastColor = new Color(-1, -1, -1, -1); // sentinel

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _image    = GetComponent<Image>();
    }

    void Update()
    {
        if (paintScript == null) return;

        Color c = paintScript.currentColor;
        if (c == _lastColor) return;   // no change – skip material/image update
        _lastColor = c;

        if (_renderer != null)
            _renderer.material.color = c;

        if (_image != null)
            _image.color = c;
    }
}

