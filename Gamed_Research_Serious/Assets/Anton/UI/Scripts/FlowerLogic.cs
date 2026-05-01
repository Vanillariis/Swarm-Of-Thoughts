using UnityEngine;

public class FlowerLogic : MonoBehaviour
{
    private string comment;

    public void SetComment(string newComment)
    {
        comment = newComment;
    }

    public string GetComment()
    {
        return comment;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            FlowerManager.Instance.ShowComment(comment);
            Debug.Log("Player entered flower. Comment: " + comment);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            FlowerManager.Instance.HideComment();
        }
    }
}