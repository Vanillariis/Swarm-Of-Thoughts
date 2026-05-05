using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioDistortManager : MonoBehaviour
{
    public static AudioDistortManager Instance;

    [Header("References")]
    // Drag your Normal and Eerie music sources here in the Inspector
    public AudioSource normalMusic;
    public AudioSource eerieMusic;

    // We need two filters because filters must be on the same object as the source
    private AudioDistortionFilter normalFilter;
    private AudioDistortionFilter eerieFilter;

    [Header("Settings")]
    public float distortIntensity = 0.8f;
    public float pitchDrop = 0.8f;
    public float effectDuration = 0.2f;
    public float recoverySpeed = 5f;

    private void Awake()
    {
        Instance = this;
        SetupFilters();
    }

    private void SetupFilters()
    {
        // Setup filter for Normal Music
        if (normalMusic != null)
        {
            normalFilter = normalMusic.gameObject.GetComponent<AudioDistortionFilter>();
            if (normalFilter == null) normalFilter = normalMusic.gameObject.AddComponent<AudioDistortionFilter>();
            normalFilter.distortionLevel = 0f;
        }

        // Setup filter for Eerie Music
        if (eerieMusic != null)
        {
            eerieFilter = eerieMusic.gameObject.GetComponent<AudioDistortionFilter>();
            if (eerieFilter == null) eerieFilter = eerieMusic.gameObject.AddComponent<AudioDistortionFilter>();
            eerieFilter.distortionLevel = 0f;
        }
    }

    public void TriggerHitDistortion()
    {
        StopAllCoroutines();
        StartCoroutine(DistortRoutine());
    }

    private IEnumerator DistortRoutine()
    {
        // 1. Instant Distortion & Pitch Drop on BOTH
        if (normalFilter) normalFilter.distortionLevel = distortIntensity;
        if (eerieFilter) eerieFilter.distortionLevel = distortIntensity;

        if (normalMusic) normalMusic.pitch = pitchDrop;
        if (eerieMusic) eerieMusic.pitch = pitchDrop;

        // 2. Wait
        yield return new WaitForSeconds(effectDuration);

        // 3. Smoothly fade back
        while (GetCurrentDistortion() > 0 || GetCurrentPitch() < 1f)
        {
            float dMove = Time.deltaTime * recoverySpeed;

            if (normalFilter) normalFilter.distortionLevel = Mathf.MoveTowards(normalFilter.distortionLevel, 0f, dMove);
            if (eerieFilter) eerieFilter.distortionLevel = Mathf.MoveTowards(eerieFilter.distortionLevel, 0f, dMove);

            if (normalMusic) normalMusic.pitch = Mathf.MoveTowards(normalMusic.pitch, 1f, dMove);
            if (eerieMusic) eerieMusic.pitch = Mathf.MoveTowards(eerieMusic.pitch, 1f, dMove);

            yield return null;
        }
    }

    // Helper functions to check status
    private float GetCurrentDistortion() => normalFilter ? normalFilter.distortionLevel : 0f;
    private float GetCurrentPitch() => normalMusic ? normalMusic.pitch : 1f;
}