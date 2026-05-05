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
    public float musicFadeSpeed = 1.0f;

    [Header("Lights")]
    public List<Light> sceneLights;
    public Color normalColor = Color.white;
    public Color eerieColor = Color.red;
    public float lightChangeSpeed = 2f;

    [Header("Timer Settings")]
    public float eerieDuration = 8f; // How long to stay eerie
    private float stateTimer = 0f;   // The actual countdown

    private GameState currentState = GameState.Normal;

    void Awake() { Instance = this; }

    void Start()
    {
        normalMusic.Play();
        eerieMusic.Play();
        normalMusic.time = 0;
        eerieMusic.time = 0;
        normalMusic.volume = 1f;
        eerieMusic.volume = 0f;
    }

    void Update()
    {
        TransitionEnvironment();
        HandleTimer();
        SyncAudio();
    }

    // This handles the countdown
    private void HandleTimer()
    {
        if (currentState == GameState.Eerie)
        {
            stateTimer -= Time.deltaTime;

            if (stateTimer <= 0)
            {
                ChangeState(GameState.Normal);
            }
        }
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;

        // If we switch to Eerie (or are already Eerie and get hit again)
        // reset the timer to the full duration.
        if (newState == GameState.Eerie)
        {
            stateTimer = eerieDuration;
        }
    }

    private void TransitionEnvironment()
    {
        float targetEerieVolume = (currentState == GameState.Eerie) ? 1f : 0f;
        float targetNormalVolume = (currentState == GameState.Eerie) ? 0f : 1f;
        Color targetColor = (currentState == GameState.Eerie) ? eerieColor : normalColor;

        normalMusic.volume = Mathf.MoveTowards(normalMusic.volume, targetNormalVolume, musicFadeSpeed * Time.deltaTime);
        eerieMusic.volume = Mathf.MoveTowards(eerieMusic.volume, targetEerieVolume, musicFadeSpeed * Time.deltaTime);

        foreach (Light l in sceneLights)
        {
            l.color = Color.Lerp(l.color, targetColor, lightChangeSpeed * Time.deltaTime);
        }
    }

    private void SyncAudio()
    {
        if (Mathf.Abs(normalMusic.time - eerieMusic.time) > 0.05f)
        {
            eerieMusic.time = normalMusic.time;
        }
    }
}