using UnityEngine;

public class FlowerLogic : MonoBehaviour
{
    // [SerializeField] allows you to see this in the Inspector even if it's private
    [SerializeField] private string flowerMessage = "Default Message";

    public void SetMessage(string newMessage)
    {
        flowerMessage = newMessage;
    }

    public string GetMessage()
    {
        return flowerMessage;
    }
}
