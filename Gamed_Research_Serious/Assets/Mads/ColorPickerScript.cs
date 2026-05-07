using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Attach this script to a single manager object in the scene.
/// On left-click it raycasts into the scene. If the hit collider's GameObject
/// name matches a known colour name (case-insensitive) it sets that colour on
/// the PaintScript. Clicking anything else does nothing.
/// </summary>
public class ColorPickerScript : MonoBehaviour
{
    [Tooltip("The PaintScript whose currentColor will be updated.")]
    public PaintScript paintScript;

    [Tooltip("Optional: visual feedback – the last selected colour collider.")]
    private GameObject _lastSelected;

    private Camera _cam;

    void Awake()
    {
        _cam = Camera.main;
    }

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.wasPressedThisFrame) return;

        Vector2 mousePos = mouse.position.ReadValue();
        Ray ray = _cam.ScreenPointToRay(mousePos);

        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        string objName = hit.collider.gameObject.name;
        Color? pickedColor = NameToColor(objName);

        if (pickedColor.HasValue)
        {
            if (paintScript != null)
                paintScript.currentColor = pickedColor.Value;

            _lastSelected = hit.collider.gameObject;
            Debug.Log($"[ColorPicker] Selected colour: {objName}  →  {pickedColor.Value}");
        }
    }

    /// <summary>
    /// Maps a GameObject name to a Unity Color.
    /// Returns null if the name is not a recognised colour.
    /// </summary>
    private Color? NameToColor(string name)
    {
        switch (name.ToLower())
        {
            case "white": return Color.white;
            case "black": return Color.black;
            // ── Vivid named colours ───────────────────────────────────────────
            case "crimson":         return HexColor("#DC143C");
            case "azure":           return HexColor("#007FFF");
            case "emerald":         return HexColor("#50C878");
            case "amber":           return HexColor("#FFBF00");
            case "slate":           return HexColor("#708090");
            case "ivory":           return HexColor("#FFFFF0");

            // ── Metallic / earthy tones ───────────────────────────────────────
            case "copper":          return HexColor("#B87333");
            case "gunmetal":        return HexColor("#2A3439");
            case "espresso":        return HexColor("#3D2B1F");
            case "caramel":         return HexColor("#AF6E4D");

            // ── Blue-green family ─────────────────────────────────────────────
            case "turquoise":       return HexColor("#40E0D0");
            case "cerulean":        return HexColor("#007BA7");
            case "aquamarine":      return HexColor("#7FFFD4");
            case "midnight":        return HexColor("#191970");

            // ── Soft / pastel family ──────────────────────────────────────────
            case "dusty rose":
            case "dustyrose":       return HexColor("#DCAE96");
            case "champagne":       return HexColor("#F7E7CE");
            case "apricot":         return HexColor("#FBCEB1");
            case "lavender":        return HexColor("#E6E6FA");

            // ── Bright accents ────────────────────────────────────────────────
            case "chartreuse":      return HexColor("#7FFF00");
            case "coral":           return HexColor("#FF7F50");
            case "periwinkle":      return HexColor("#CCCCFF");
            case "teal":            return HexColor("#008080");
            case "magenta":         return HexColor("#FF00FF");
            case "marigold":        return HexColor("#EAA221");

            // ── Natural / muted tones ─────────────────────────────────────────
            case "sage":            return HexColor("#BCB88A");
            case "terracotta":      return HexColor("#E2725B");
            case "ochre":           return HexColor("#CC7722");
            case "moss":            return HexColor("#8A9A5B");
            case "sandstone":       return HexColor("#D6C2C2");
            case "indigo":          return HexColor("#4B0082");

            // ── Dark / neutral tones ──────────────────────────────────────────
            case "charcoal":        return HexColor("#36454F");
            case "burgundy":        return HexColor("#800020");
            case "eggplant":        return HexColor("#614051");
            case "navy":            return HexColor("#000080");
            case "taupe":           return HexColor("#483C32");
            case "obsidian":        return HexColor("#0B0B0B");

            // ── Yellow family ─────────────────────────────────────────────────
            case "canary":          return HexColor("#FFEF00");
            case "saffron":         return HexColor("#F4C430");
            case "lemon chiffon":
            case "lemonchiffon":    return HexColor("#FFFACD");

            default:                return null;   // not a colour collider – do nothing
        }
    }

    /// <summary>Converts a CSS hex string (#RRGGBB) to a Unity Color.</summary>
    private static Color HexColor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }

    /// <summary>
    /// Returns the colour that was most recently picked (useful for other scripts).
    /// </summary>
    public Color GetCurrentColor()
    {
        return paintScript != null ? paintScript.currentColor : Color.white;
    }
}

