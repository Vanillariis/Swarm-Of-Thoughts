using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemiesManager : MonoBehaviour
{
    public List<GameObject> enemyPrefabs;
    public GameObject player;
    public RectTransform canvas;

    private PlayerMovementShadow movement;
    public List<RectTransform> activeEnemies = new List<RectTransform>();

    public StressControl stressControl;

    public int currentEnemies = 0;
    public int maxEnemies = 5; // adjust as needed
    public float minDistance = 30f; // adjust based on enemy size

    

    private bool waveActive = false;
    private bool isGameOver = false;

    void Start()
    {
        movement = player.GetComponent<PlayerMovementShadow>();
        StartCoroutine(StartWaveWithDelay());
        stressControl.spread = 0.6f;

    }
    public void StartWave()
    {
        waveActive = true;

        int enemyCount = GetMaxEnemies();

        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy();
        }
    }
    public void UpdateSpread()
    {
        float targetSpread;

        if (isGameOver)
        {
            targetSpread = 0f;
        }
        else
        {
            float health01 = movement.playerHealth / 10f;
            targetSpread = Mathf.Lerp(2.5f, 0.6f, health01);
        }

        stressControl.spread = Mathf.Lerp(
            stressControl.spread,
            targetSpread,
            Time.deltaTime * 5f
        );

        //if (Mathf.Abs(stressControl.spread - targetSpread) < 0.01f)
        //{
        //    stressControl.spread = targetSpread;
        //}
    }
    private void Update()
    {
        UpdateSpread();
        DeadEnd();
    }



    public void SpawnEnemy()
    {
        if (isGameOver) return;

        if (currentEnemies >= maxEnemies)
            return;

        GameObject enemy = Instantiate(
            enemyPrefabs[Random.Range(0, enemyPrefabs.Count)],
            canvas
        );

        RectTransform rt = enemy.GetComponent<RectTransform>();

        float x = Random.Range(-canvas.rect.width / 2 - 2850f,-canvas.rect.width / 2 - 100f);

        Vector2 spawnPos;
        int attempts = 0;

        do
        {
            RectTransform playerRT = player.GetComponent<RectTransform>();
            float playerY = playerRT.anchoredPosition.y;

            float canvasHeight = canvas.rect.height;

            float[] yOffsets = new float[]
            {
                -canvasHeight * 0.3f,
                -canvasHeight * 0.15f,
                    0f,
                    canvasHeight * 0.15f,
                    canvasHeight * 0.3f
            };

            // Copy list so we can remove used ones
            List<float> availableOffsets = new List<float>(yOffsets);

            // Remove offsets that are already occupied
            foreach (RectTransform enemyRT in activeEnemies)
            {
                float usedOffset = enemyRT.anchoredPosition.y - playerY;

                availableOffsets.RemoveAll(o => Mathf.Abs(o - usedOffset) < 20f);
            }

            // Pick a free offset if possible
            float y;

            // Try to use center lane first
            if (availableOffsets.Contains(0f))
            {
                y = playerY; // force center
            }
            else if (availableOffsets.Count > 0)
            {
                y = playerY + availableOffsets[Random.Range(0, availableOffsets.Count)];
            }
            else
            {
                y = playerY + Random.Range(-100f, 100f);
            }
            

            float halfHeight = canvas.rect.height / 2f;
            float padding = rt.rect.height / 2f;

            y = Mathf.Clamp(y, -halfHeight + padding, halfHeight - padding);

            spawnPos = new Vector2(x, y);

            spawnPos = new Vector2(x, y);
            attempts++;

        } while (!IsFarEnough(spawnPos) && attempts < 200);

        rt.anchoredPosition = spawnPos;

        currentEnemies++;
        activeEnemies.Add(rt);

        EnemyMovement em = enemy.GetComponent<EnemyMovement>();
        em.manager = this;
    }
    public int GetMaxEnemies()
    {
        if (stressControl.spread < 0.8f) return 1;
        if (stressControl.spread > 0.8f && stressControl.spread < 1.2f) return 2;
        if (stressControl.spread > 1.2f && stressControl.spread < 1.5f) return 3;
        return 4;
    }
    private bool IsFarEnough(Vector2 pos)
    {
        foreach (RectTransform enemy in activeEnemies)
        {
            if (Mathf.Abs(enemy.anchoredPosition.y - pos.y) < 80f)
                return false;
        }
        return true;
    }
    public void OnEnemyDestroyed(RectTransform enemyRT)
    {
        currentEnemies = Mathf.Max(0, currentEnemies - 1);
        activeEnemies.Remove(enemyRT);

        if (currentEnemies <= 0 && waveActive)
        {
            waveActive = false;
            StartCoroutine(StartWaveWithDelay());
        }
    }

    IEnumerator StartWaveWithDelay()
    {
        yield return new WaitForSeconds(0.5f);
        StartWave();
    }

    public void DeadEnd()
    {
        if (stressControl.spread >= 2.2f && !isGameOver)
        {
            isGameOver = true;

            Debug.Log("You are too stressed!");
            StopCoroutine(StartWaveWithDelay());
            
            // load scene skift her :)
        }
        
        if (stressControl.spread <= 0.01f && isGameOver)
        {
            SceneManager.LoadScene("Anton Scene");
        }
    }
}