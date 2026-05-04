using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckDrawing : MonoBehaviour
{
    public GameObject colorPanel;
    
    public EnemiesManager enemiesManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void drawingSubmit()
    {
        if (!AllPanelsPainted())
        {
            Debug.Log("[CheckDrawing] Not all panels are painted yet.");
            return;
        }

        Debug.Log("[CheckDrawing] All panels painted – submitting!");
        enemiesManager.stressControl.spread = 0f;
        SceneManager.LoadScene("Anton Scene");
    }

    private bool AllPanelsPainted()
    {
        int unpaintedCount = 0;
        const int allowedUnpainted = 5;

        foreach (Transform child in colorPanel.transform)
        {
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            if (sr == null) continue;

            Color c = sr.color;
            if (Mathf.Approximately(c.r, 1f) &&
                Mathf.Approximately(c.g, 1f) &&
                Mathf.Approximately(c.b, 1f))
            {
                unpaintedCount++;
            }
        }

        return unpaintedCount <= allowedUnpainted;
    }
}
