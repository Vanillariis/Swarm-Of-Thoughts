Shader "Custom/GroundColourClean"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map (Dirt Texture)", 2D) = "white" {}
        [MainColor] _BaseColor("Global Tint", Color) = (1, 1, 1, 1)

        [Header(Noise Texture Coloring)]
        _NoiseTex ("Color Map Texture (Grayscale)", 2D) = "white" {}
        _NoiseBlur ("Noise Softness", Range(0, 0.02)) = 0.005
        
        [Header(Anti Tiling Settings)]
        _DetailScale ("Detail Layer Scale", Float) = 2.17
        _DetailWeight ("Detail Layer Strength", Range(0, 1)) = 0.5
        
        [Header(Color Thresholds)]
        _ColorA ("Color A (Low)", Color) = (0.1, 0.4, 0.1, 1)
        _ColorB ("Color B (Mid)", Color) = (0.2, 0.6, 0.2, 1)
        _ColorC ("Color C (High)", Color) = (0.4, 0.8, 0.3, 1)
        _Step1 ("Threshold A->B", Range(0, 1)) = 0.33
        _Step2 ("Threshold B->C", Range(0, 1)) = 0.66
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1; 
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _NoiseTex_ST;
                half4 _BaseColor;
                float4 _ColorA, _ColorB, _ColorC;
                float _Step1, _Step2, _NoiseBlur;
                float _DetailScale, _DetailWeight;
            CBUFFER_END

            // Helper function to handle the 4-tap sampling to keep the frag shader clean
            float SampleNoiseBlurred(float2 uv, float blur)
            {
                float d = blur;
                float n1 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, uv + float2(-d, -d)).r;
                float n2 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, uv + float2(d, -d)).r;
                float n3 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, uv + float2(-d, d)).r;
                float n4 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, uv + float2(d, d)).r;
                return (n1 + n2 + n3 + n4) * 0.25;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionCS = vertexInput.positionCS;
                OUT.positionWS = vertexInput.positionWS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // LAYER 1: Base Noise
                // Uses the Tiling/Offset from the Inspector
                float2 uv1 = IN.positionWS.xz * _NoiseTex_ST.xy + _NoiseTex_ST.zw;
                float noiseBase = SampleNoiseBlurred(uv1, _NoiseBlur);

                // LAYER 2: Detail Noise (Breaks the tiling)
                // We use a different scale and a fixed offset to ensure it never aligns with Layer 1
                float2 uv2 = (IN.positionWS.xz * _NoiseTex_ST.xy * _DetailScale) + float2(0.53, 0.11);
                float noiseDetail = SampleNoiseBlurred(uv2, _NoiseBlur * _DetailScale);

                // Combine the layers
                // This creates "Turbulent" noise which hides the grid pattern
                float noiseVal = lerp(noiseBase, noiseDetail, _DetailWeight);

                // Color Selection
                half3 patchColor;
                if (noiseVal < _Step1) patchColor = _ColorA.rgb;
                else if (noiseVal < _Step2) patchColor = _ColorB.rgb;
                else patchColor = _ColorC.rgb;

                // Final Mix
                half4 baseTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half3 finalRGB = baseTex.rgb * patchColor * _BaseColor.rgb;

                return half4(finalRGB, baseTex.a * _BaseColor.a);
            }
            ENDHLSL
        }
    }
}