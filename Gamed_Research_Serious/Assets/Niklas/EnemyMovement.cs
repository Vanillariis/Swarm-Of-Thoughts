using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float normalSpeed = 100f;
    public float entrySpeed = 400f;

    private float currentSpeed;

    private RectTransform rt;
    private RectTransform canvas;

    private float slowDownX;

    public EnemiesManager manager;

    private CanvasGroup cg;
    void Start()
    {
        if (manager == null)
            manager = FindObjectOfType<EnemiesManager>(); // fallback only

        rt = GetComponent<RectTransform>();
        canvas = manager.canvas;

        currentSpeed = entrySpeed;

        // 1/4 into screen
        slowDownX = -canvas.rect.width / 2f;

        cg = GetComponent<CanvasGroup>();

        // If missing, add one automatically
        if (cg == null)
        {
            cg = gameObject.AddComponent<CanvasGroup>();
        }
    }

    void Update()
    {
        // Move
        rt.anchoredPosition += Vector2.right * currentSpeed * Time.deltaTime;

        if (rt.anchoredPosition.x >= slowDownX)
        {
            currentSpeed = normalSpeed;
        }

        // ✅ Check mask visibility
        bool inside = StressMask.IsInside(rt);
        
        // Smooth visibility (optional but nicer)
        //cg.alpha = Mathf.Lerp(cg.alpha, inside ? 1f : 0f, Time.deltaTime * 10f);
        //cg.alpha = 1f;
        //cg.alpha = inside ? 1f : 0f;
        // Optional: disable interaction when hidden
        cg.blocksRaycasts = inside;
        cg.interactable = inside;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Enemy hit player!");

            PlayerMovementShadow player = collision.GetComponent<PlayerMovementShadow>();
            player.playerHealth -= 1;

            if (AudioDistortManager.Instance != null)
            {
                AudioDistortManager.Instance.TriggerHitDistortion();
            }

            manager.OnEnemyDestroyed(rt);
            Destroy(gameObject);
        }

        if (collision.CompareTag("Wall"))
        {
            Debug.Log("Enemy hit wall!");

            manager.OnEnemyDestroyed(rt);
            Destroy(gameObject);
        }
    }
}