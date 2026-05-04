Shader "Custom/PageGame"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        _Rotation("Rotation", Range(0.0, 360.0)) = 0.0
    }

    SubShader
    {
        // "UniversalForward" is the magic tag that tells Unity to send light data here
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline" 
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // REQUIRED for URP Lighting and Shadows
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS // Supports point/spot lights too
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float3 positionWS   : TEXCOORD1;
                float3 normalWS     : TEXCOORD2;
                float2 uv           : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                float _Rotation;
            CBUFFER_END

            float2 RotateUV(float2 uv, float rotation, float2 pivot)
            {
                float angle = rotation * PI / 180.0;
                float s, c;
                sincos(angle, s, c);
                uv -= pivot;
                float2 r = float2(uv.x * c - uv.y * s, uv.x * s + uv.y * c);
                return r + pivot;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                
                // Get World Space positions and normals
                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionHCS = posInputs.positionCS;
                OUT.positionWS = posInputs.positionWS;
                OUT.normalWS = normInputs.normalWS;

                // Scale and Rotate UVs
                float2 uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.uv = RotateUV(uv, _Rotation, float2(0.5, 0.5));

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // 1. Setup Data
                float2 uv = IN.uv;
                half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv) * _BaseColor;
                float3 normalWS = normalize(IN.normalWS);
    
                // 2. Main Light (Directional Light)
                float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                Light mainLight = GetMainLight(shadowCoord);
    
                half NdotL = saturate(dot(normalWS, mainLight.direction));
                half3 directLight = mainLight.color * (NdotL * mainLight.shadowAttenuation);
    
                // 3. Additional Lights (Point Lights, Spot Lights)
                half3 additionalLightSum = half3(0, 0, 0);
    
                // Get the total number of non-directional lights affecting this object
                uint pixelLightCount = GetAdditionalLightsCount();
    
                for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
                {
                    // Get light data (attenuation, color, direction)
                    // Note: For URP 10+, use GetAdditionalLight(lightIndex, IN.positionWS);
                    Light light = GetAdditionalLight(lightIndex, IN.positionWS);
        
                    half3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
                    half lightNdotL = saturate(dot(normalWS, light.direction));
        
                    additionalLightSum += attenuatedLightColor * lightNdotL;
                }

                // 4. Ambient Lighting (Environment)
                half3 ambient = SampleSH(normalWS) * texColor.rgb;

                // 5. Combine everything
                // Final color = (Main Light + Additional Lights) * Texture + Ambient
                half3 finalRGB = ((directLight + additionalLightSum) * texColor.rgb) + ambient;

                return half4(finalRGB, texColor.a);
            }
            ENDHLSL
        }

        // Standard Shadow Caster Pass
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0
            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
}