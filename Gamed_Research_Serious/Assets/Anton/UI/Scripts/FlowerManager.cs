using System.IO;
using TMPro;
using UnityEngine;

public class FlowerManager : MonoBehaviour
{
    public static FlowerManager Instance;

    [Header("Flowers")]
    public GameObject[] flowerPrefabs;

    [Header("Write Comment UI")]
    public GameObject writeCommentPanel;
    public TMP_InputField commentInputField;

    [Header("Show Comment UI")]
    public GameObject showCommentPanel;
    public TMP_Text showCommentText;
    
    [Header("Audio")]
    public AudioSource plantAudioSource;
    public AudioClip plantSound;

    private FlowerSaveData saveData = new FlowerSaveData();
    private FlowerLogic pendingFlower;
    private string savePath;

    private void Awake()
    {
        Instance = this;
        savePath = Path.Combine(Application.persistentDataPath, "flowers.json");
    }

    private void Start()
    {
        //DeleteSavedFlowers();
        
        writeCommentPanel.SetActive(false);
        showCommentPanel.SetActive(false);

        SetupCommentInputField();

        LoadFlowers();
    }

    private void SetupCommentInputField()
    {
        if (commentInputField == null)
            return;

        commentInputField.lineType = TMP_InputField.LineType.MultiLineNewline;

        if (commentInputField.textComponent != null)
        {
            commentInputField.textComponent.textWrappingMode = TextWrappingModes.Normal;
            commentInputField.textComponent.overflowMode = TextOverflowModes.Overflow;
        }

        if (commentInputField.placeholder is TMP_Text placeholderText)
        {
            placeholderText.textWrappingMode = TextWrappingModes.Normal;
            placeholderText.overflowMode = TextOverflowModes.Overflow;
        }
    }

    public void PlantFlower(Vector3 position)
    {
        if (flowerPrefabs == null || flowerPrefabs.Length == 0)
            return;

        GameObject prefab = flowerPrefabs[Random.Range(0, flowerPrefabs.Length)];
        GameObject flowerObj = Instantiate(prefab, position, Quaternion.identity);

        pendingFlower = flowerObj.GetComponent<FlowerLogic>();

        if (plantAudioSource != null && plantSound != null)
        {
            plantAudioSource.PlayOneShot(plantSound);
        }

        SetupCommentInputField();

        commentInputField.text = "";
        writeCommentPanel.SetActive(true);
        commentInputField.ActivateInputField();
    }

    public void SubmitComment()
    {
        if (pendingFlower == null) return;

        string comment = commentInputField.text;

        if (string.IsNullOrWhiteSpace(comment))
            return;

        pendingFlower.SetComment(comment);
        SaveFlower(pendingFlower.transform.position, comment);

        pendingFlower = null;
        commentInputField.text = "";
        writeCommentPanel.SetActive(false);
    }

    private void SaveFlower(Vector3 position, string comment)
    {
        saveData.flowers.Add(new FlowerData
        {
            x = position.x,
            y = position.y,
            z = position.z,
            comment = comment
        });

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(savePath, json);
    }

    private void LoadFlowers()
    {
        if (!File.Exists(savePath))
            return;

        string json = File.ReadAllText(savePath);
        saveData = JsonUtility.FromJson<FlowerSaveData>(json);

        foreach (FlowerData data in saveData.flowers)
        {
            Vector3 position = new Vector3(data.x, data.y, data.z);

            GameObject flowerPrefab = flowerPrefabs[Random.Range(0, flowerPrefabs.Length)];
            GameObject flowerObj = Instantiate(flowerPrefab, position, Quaternion.identity);

            FlowerLogic flower = flowerObj.GetComponent<FlowerLogic>();
            flower.SetComment(data.comment);
        }
    }

    public void ShowComment(string comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
            return;

        showCommentText.text = comment;
        showCommentText.textWrappingMode = TextWrappingModes.Normal;
        showCommentText.overflowMode = TextOverflowModes.Overflow;

        showCommentPanel.SetActive(true);
    }

    public void HideComment()
    {
        showCommentPanel.SetActive(false);
    }

    public void DeleteSavedFlowers()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Deleted saved flowers file.");
        }
    }
}