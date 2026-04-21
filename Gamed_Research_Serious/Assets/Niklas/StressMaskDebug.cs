using UnityEngine;

public class StressMaskDebug : MonoBehaviour
{
    public Texture2D debugTex;
    public int resolution = 128;

    void Start()
    {
        debugTex = new Texture2D(resolution, resolution, TextureFormat.RGB24, false);
    }

    void Update()
    {
        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                float u = (float)x / resolution;
                float v = (float)y / resolution;

                Vector2 uv = new Vector2(u, v);

                float dist = Vector2.Distance(uv, new Vector2(0f, 1f));
                float noise = StressMask.GetNoise(uv) * 0.2f;

                float value = dist + noise;

                bool inside = value < StressMask.spread;

                debugTex.SetPixel(x, y, inside ? Color.white : Color.black);
            }
        }

        debugTex.Apply();
    }

    void OnGUI()
    {
        GUI.DrawTexture(new Rect(10, 10, 256, 256), debugTex);
    }
}