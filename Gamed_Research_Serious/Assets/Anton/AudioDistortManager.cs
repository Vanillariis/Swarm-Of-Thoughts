using UnityEngine;
using System.Collections;

public class AudioDistortManager : MonoBehaviour
{
    public static AudioDistortManager Instance;

    private AudioSource audioSource;
    private AudioDistortionFilter distortionFilter;

    [Header("Settings")]
    public float distortIntensity = 0.8f; // How "crunchy" it gets
    public float pitchDrop = 0.8f;       // How much the pitch slows down
    public float effectDuration = 0.2f;  // How long the peak distortion lasts
    public float recoverySpeed = 5f;     // How fast it returns to normal

    private void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();

        // Automatically add the filter if it's missing
        distortionFilter = GetComponent<AudioDistortionFilter>();
        if (distortionFilter == null)
            distortionFilter = gameObject.AddComponent<AudioDistortionFilter>();

        distortionFilter.distortionLevel = 0f; // Start clean
    }

    public void TriggerHitDistortion()
    {
        StopAllCoroutines(); // Reset if hit multiple times quickly
        StartCoroutine(DistortRoutine());
    }

    private IEnumerator DistortRoutine()
    {
        // 1. Instant Distortion
        distortionFilter.distortionLevel = distortIntensity;
        audioSource.pitch = pitchDrop;

        // 2. Wait for a brief moment
        yield return new WaitForSeconds(effectDuration);

        // 3. Smoothly fade back to normal
        while (distortionFilter.distortionLevel > 0 || audioSource.pitch < 1f)
        {
            distortionFilter.distortionLevel = Mathf.MoveTowards(distortionFilter.distortionLevel, 0f, Time.deltaTime * recoverySpeed);
            audioSource.pitch = Mathf.MoveTowards(audioSource.pitch, 1f, Time.deltaTime * recoverySpeed);
            yield return null;
        }
    }
}