using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Attach to every 3D marker.
/// - Left-click  : select (marker follows mouse, maintaining its depth from the camera)
/// - Left-click another marker OR right-click : deselect and snap back to original position
/// </summary>
public class SelectMarkerScript : MonoBehaviour
{
    [Header("Visual feedback")]
    [Tooltip("Material to apply while selected. Leave empty to skip.")]
    public Material selectedMaterial;

    // ── static: which marker is currently held ─────────────────────────────────
    private static SelectMarkerScript _current;

    // ── instance state ─────────────────────────────────────────────────────────
    private Camera   _cam;
    private Renderer _renderer;
    private Material _originalMaterial;
    private bool     _isSelected;
    private Vector3  _originPosition;

    // Depth of the marker from the camera at the moment of selection.
    private float    _camDepth;
    // Screen-space offset so the marker doesn't jump on grab.
    private Vector3  _grabScreenOffset;

    // ── Unity messages ─────────────────────────────────────────────────────────
    void Awake()
    {
        _cam      = Camera.main;
        _renderer = GetComponent<Renderer>();
        if (_renderer != null)
            _originalMaterial = _renderer.material;
    }

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        bool leftDown  = mouse.leftButton.wasPressedThisFrame;
        bool rightDown = mouse.rightButton.wasPressedThisFrame;
        Vector2 mousePos = mouse.position.ReadValue();

        // ── Left mouse button pressed ─────────────────────────────────────────
        if (leftDown)
        {
            Ray ray = _cam.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                SelectMarkerScript clickedMarker = hit.collider.GetComponent<SelectMarkerScript>();

                if (clickedMarker == this)
                {
                    if (_isSelected)
                        Deselect(returnToOrigin: true);
                    else
                    {
                        if (_current != null && _current != this)
                            _current.Deselect(returnToOrigin: true);
                        Select(mousePos);
                    }
                }
                else if (_isSelected)
                {
                    Deselect(returnToOrigin: true);
                }
            }
            else if (_isSelected)
            {
                Deselect(returnToOrigin: true);
            }
        }

        // ── Right mouse button → cancel ───────────────────────────────────────
        if (_isSelected && rightDown)
        {
            Deselect(returnToOrigin: true);
            return;
        }

        // ── Drag: follow mouse in screen space at the same camera depth ────────
        if (_isSelected)
        {
            Vector3 screenPoint = new Vector3(mousePos.x, mousePos.y, _camDepth);
            transform.position = _cam.ScreenToWorldPoint(screenPoint) + _grabScreenOffset;
        }
    }

    // ── helpers ────────────────────────────────────────────────────────────────
    void Select(Vector2 mousePos)
    {
        _originPosition = transform.position;
        _isSelected     = true;
        _current        = this;

        // Record depth so the marker stays the same distance from the camera.
        _camDepth = _cam.WorldToScreenPoint(transform.position).z;

        // Record the world-space offset between the marker and the mouse ray at grab depth,
        // so the marker doesn't jump to the cursor centre.
        Vector3 screenPoint = new Vector3(mousePos.x, mousePos.y, _camDepth);
        _grabScreenOffset = transform.position - _cam.ScreenToWorldPoint(screenPoint);

        if (_renderer != null && selectedMaterial != null)
            _renderer.material = selectedMaterial;

        Debug.Log($"[SelectMarker] Selected: {name}");
    }

    void Deselect(bool returnToOrigin)
    {
        _isSelected = false;
        if (_current == this) _current = null;

        if (returnToOrigin)
            transform.position = _originPosition;

        if (_renderer != null && _originalMaterial != null)
            _renderer.material = _originalMaterial;

        Debug.Log($"[SelectMarker] Deselected: {name}  returned={returnToOrigin}");
    }
}
