using System.Collections.Generic;
using UnityEngine;

public class PaintAndMarkerManager : MonoBehaviour
{
    public PaintScript paintScript;

    public List<GameObject> markerList;

    private int selectedMarkerIndex = -1;

    // Call this to select a marker by its index in markerList
    public void SelectMarker(int index)
    {
        if (index < 0 || index >= markerList.Count) return;

        selectedMarkerIndex = index;
        GameObject marker = markerList[index];
        Color color = TagToColor(marker.tag);
        paintScript.currentColor = color;
    }

    public int GetSelectedMarkerIndex()
    {
        return selectedMarkerIndex;
    }

    private Color TagToColor(string tag)
    {
        switch (tag.ToLower())
        {
            case "red":     return Color.red;
            case "green":   return Color.green;
            case "blue":    return Color.blue;
            case "yellow":  return Color.yellow;
            case "white":   return Color.white;
            case "black":   return Color.black;
            case "cyan":    return Color.cyan;
            case "magenta": return Color.magenta;
            case "orange":  return new Color(1f, 0.5f, 0f);
            case "purple":  return new Color(0.5f, 0f, 0.5f);
            case "pink":    return new Color(1f, 0.41f, 0.71f);
            case "brown":   return new Color(0.65f, 0.16f, 0.16f);
            case "grey":
            case "gray":    return Color.grey;
            default:
                Debug.LogWarning($"PaintAndMarkerManager: Unknown color tag '{tag}', defaulting to white.");
                return Color.white;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
