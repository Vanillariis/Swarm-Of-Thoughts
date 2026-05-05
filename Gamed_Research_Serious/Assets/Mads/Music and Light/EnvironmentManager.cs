using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum GameState { Normal, Eerie }

public class EnvironmentManager : MonoBehaviour
{
    public static EnvironmentManager Instance;

    [Header("Audio Sources")]
    public AudioSource normalMusic;
    public AudioSource eerieMusic;
    public float musicFadeSpeed = 1.0f; // Lower = slower, more cinematic transition

    [Header("Lights")]
    public List<Light> sceneLights;
    public Color normalColor = Color.white;
    public Color eerieColor = Color.red;
    public float lightChangeSpeed = 2f;

    private GameState currentState = GameState.Normal;

    void Awake() { Instance = this; }

    void Start()
    {
        // 1. Start both tracks at the exact same moment
        normalMusic.Play();
        eerieMusic.Play();

        // 2. Ensure they are perfectly synced at the start
        normalMusic.time = 0;
        eerieMusic.time = 0;

        // 3. Set initial volumes based on the starting state
        normalMusic.volume = 1f;
        eerieMusic.volume = 0f;
    }

    void Update()
    {
        TransitionEnvironment();

        // OPTIONAL: Keep them synced
        // If one track drifts (common in long sessions), force the eerie track 
        // to match the normal track's time.
        if (Mathf.Abs(normalMusic.time - eerieMusic.time) > 0.05f)
        {
            eerieMusic.time = normalMusic.time;
        }
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;
    }

    private void TransitionEnvironment()
    {
        float targetEerieVolume = (currentState == GameState.Eerie) ? 1f : 0f;
        float targetNormalVolume = (currentState == GameState.Eerie) ? 0f : 1f;
        Color targetColor = (currentState == GameState.Eerie) ? eerieColor : normalColor;

        // Smoothly swap volumes
        normalMusic.volume = Mathf.MoveTowards(normalMusic.volume, targetNormalVolume, musicFadeSpeed * Time.deltaTime);
        eerieMusic.volume = Mathf.MoveTowards(eerieMusic.volume, targetEerieVolume, musicFadeSpeed * Time.deltaTime);

        // Smoothly change light colors
        foreach (Light l in sceneLights)
        {
            l.color = Color.Lerp(l.color, targetColor, lightChangeSpeed * Time.deltaTime);
        }
    }
}