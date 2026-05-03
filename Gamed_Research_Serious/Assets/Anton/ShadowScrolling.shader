Shader "Custom/ShadowScrolling"
{
    Properties
    {
        [MainTexture] _BaseMap("Shadow Texture (Grayscale)", 2D) = "white" {}
        _Cutoff("Shadow Strength", Range(0.0, 1.0)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest" }

        // This pass makes the plane invisible to the camera
        Pass
        {
            Name "Invisible"
            ColorMask 0
            ZWrite Off
        }

        // This pass tells Unity how to cast the shadows
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            float4 _BaseMap_ST;
            float _Cutoff;

            Varyings vert(Attributes IN) {
                Varyings OUT;
                // Standard shadow macro
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv * _BaseMap_ST.xy + _BaseMap_ST.zw;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target {
                float shadowMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).r;
                // If the texture is dark, discard the pixel (let light through)
                // If the texture is light, keep it (block light / cast shadow)
                clip(shadowMap - _Cutoff);
                return 0;
            }
            ENDHLSL
        }
    }
}