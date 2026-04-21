using UnityEngine;

public static class StressMask
{
    public static float spread;

    public static Texture2D noiseTex;

    // 🔥 MUST MATCH shader value if you scale UV there
    public static float noiseScale = 1f;

    public static float GetNoise(Vector2 uv)
    {
        if (noiseTex == null) return 0f;

        // ✅ Match shader tiling
        uv *= noiseScale;

        // ✅ Match shader repeat wrapping
        uv.x = uv.x % 1f;
        uv.y = uv.y % 1f;

        if (uv.x < 0) uv.x += 1f;
        if (uv.y < 0) uv.y += 1f;

        // ✅ Match shader sampling (bilinear)
        return noiseTex.GetPixelBilinear(uv.x, uv.y).r;
    }

    public static bool IsInside(RectTransform rt)
    {
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, rt.position);

        float u = (screenPos.x + 0.5f) / Screen.width;
        float v = (screenPos.y + 0.5f) / Screen.height;

        Vector2 uv = new Vector2(u, v);

        // ✅ Diagonal instead of distance
        float diagonal = uv.x + (1f - uv.y);

        // ✅ Same noise as shader
        float noise = GetNoise(uv) * 0.2f;

        float value = diagonal + noise;

        return value < spread;
    }
}