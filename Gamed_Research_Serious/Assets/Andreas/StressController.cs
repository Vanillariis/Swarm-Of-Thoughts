using UnityEngine;
using UnityEngine.Rendering.Universal;

public class StressController : MonoBehaviour
{
    [Range(0, 1.5f)] public float spread = 0;
    public UniversalRendererData rendererData; // Drag your 'Universal Renderer Data' asset here

    public Material internalMaterial;

    void Update()
    {
        if (internalMaterial == null && rendererData != null)
        {
            // This looks through your Renderer Features to find the Full Screen Pass
            foreach (var feature in rendererData.rendererFeatures)
            {
                if (feature is FullScreenPassRendererFeature fullScreenPass)
                {
                    internalMaterial = fullScreenPass.passMaterial;
                }
            }
        }

        Shader.SetGlobalFloat("_SpreadAmount", spread);

        if (internalMaterial != null)
        {
            internalMaterial.SetFloat("_SpreadAmount", spread);
        }
    }
}