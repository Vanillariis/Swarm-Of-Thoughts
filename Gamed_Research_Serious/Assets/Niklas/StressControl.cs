using UnityEngine;
using UnityEngine.Rendering.Universal;

public class StressControl : MonoBehaviour
{
    [Range(0, 2.5f)] public float spread = 0.6f;

    public UniversalRendererData rendererData;
    public Material internalMaterial;

    public Texture2D noiseTexture;
    public float noiseScale = 1f;

    [Range(0f, 360f)]
    public float angle = 45f;

    private void Start()
    {
        StressMask.noiseTex = noiseTexture;
    }

    void Update()
    {
        // 🔍 Find fullscreen material (only once ideally)
        if (internalMaterial == null && rendererData != null)
        {
            foreach (var feature in rendererData.rendererFeatures)
            {
                if (feature is FullScreenPassRendererFeature fullScreenPass)
                {
                    internalMaterial = fullScreenPass.passMaterial;
                }
            }
        }

        // ✅ Sync your static logic
        StressMask.spread = spread;

        // ✅ GLOBAL SYNC (THIS is what your UI shader uses)
        Shader.SetGlobalFloat("_SpreadAmount", spread);
        Shader.SetGlobalTexture("_NoiseTex", noiseTexture);
        Shader.SetGlobalFloat("_NoiseScale", noiseScale);

        // ⚠️ Optional (not really needed if using globals)
        if (internalMaterial != null)
        {
            internalMaterial.SetFloat("_SpreadAmount", spread);
        }

        float rad = angle * Mathf.Deg2Rad;

        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        Shader.SetGlobalVector("_Direction", new Vector4(dir.x, dir.y, 0, 0));
    }
}