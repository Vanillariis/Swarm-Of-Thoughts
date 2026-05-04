using UnityEngine;

public class ButtonEndBehaviour : MonoBehaviour
{
    // You can drag the Canvas GameObject into this slot in the Inspector
    public GameObject canvasToDisable;

    public void CloseCanvas()
    {
        if (canvasToDisable != null)
        {
            canvasToDisable.SetActive(false);
        }
    }
}
