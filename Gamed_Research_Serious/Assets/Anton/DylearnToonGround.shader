Shader "Custom/DylearnToonGround"
{
    Properties
    {
        [Header(Colours)]
        _Albedo1("Base Colour", Color) = (1, 1, 1, 1)
        
        [Header(Albedo 2 Layer)]
        _Albedo2("Albedo 2", Color) = (0.8, 0.8, 0.8, 1)
        _Albedo2Noise("Albedo 2 Noise", 2D) = "white" {}
        _Albedo2Scale("Albedo 2 Scale", Range(0, 1)) = 0.005
        _Albedo2Threshold("Albedo 2 Threshold", Range(0, 1)) = 0.604

        [Header(Albedo 3 Layer)]
        _Albedo3("Albedo 3", Color) = (0.6, 0.6, 0.6, 1)
        _Albedo3Noise("Albedo 3 Noise", 2D) = "white" {}
        _Albedo3Scale("Albedo 3 Scale", Range(0, 0.1)) = 0.003
        _Albedo3Threshold("Albedo 3 Threshold", Range(0, 1)) = 0.661

        [Header(Lighting)]
        _Cuts("Toon Cuts", Range(1, 8)) = 3
        _Wrap("Light Wrap", Range(-2.0, 2.0)) = 0.0
        _Steepness("Steepness", Range(1.0, 8.0)) = 1.0
        
        [Header(Clouds)]
        _CloudSpeed("Cloud Speed", Vector) = (0.1, 0.1, 0, 0)
        _CloudScale("Cloud Scale", Float) = 0.05
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // Required to receive shadows and light data
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                DECLARE_LIGHTMAP_OR_SHADOWCOORD(shadowCoord, 2);
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Albedo1, _Albedo2, _Albedo3;
                float _Albedo2Scale, _Albedo2Threshold;
                float _Albedo3Scale, _Albedo3Threshold;
                int _Cuts;
                float _Wrap, _Steepness;
                float2 _CloudSpeed;
                float _CloudScale;
            CBUFFER_END

            TEXTURE2D(_Albedo2Noise); SAMPLER(sampler_Albedo2Noise);
            TEXTURE2D(_Albedo3Noise); SAMPLER(sampler_Albedo3Noise);

            // Placeholder Cloud Noise (Replaces the .gdshaderinc)
            float GetCloudNoise(float3 worldPos) {
                float2 uv = worldPos.xz * _CloudScale + (_Time.y * _CloudSpeed);
                // Simple pseudo-random/sine clouds
                return saturate(sin(uv.x) * cos(uv.y) * 2.0 + 0.5);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionCS = posInputs.positionCS;
                OUT.positionWS = posInputs.positionWS;
                OUT.normalWS = normInputs.normalWS;

                // Setup shadow coordinates for receiving shadows
                OUTPUT_LIGHTMAP_OR_SHADOWCOORD(posInputs.positionWS, OUT.shadowCoord, 2);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // 1. SELECT ALBEDO (Noise Threshold Logic)
                float a2_noise = SAMPLE_TEXTURE2D(_Albedo2Noise, sampler_Albedo2Noise, IN.positionWS.xz * _Albedo2Scale).r;
                float a3_noise = SAMPLE_TEXTURE2D(_Albedo3Noise, sampler_Albedo3Noise, IN.positionWS.xz * _Albedo3Scale).r;

                float3 baseAlbedo = _Albedo1.rgb;
                if (a2_noise > _Albedo2Threshold) baseAlbedo = _Albedo2.rgb;
                if (a3_noise > _Albedo3Threshold) baseAlbedo = _Albedo3.rgb;

                // 2. GET LIGHT DATA
                float4 shadowCoord = GetShadowCoord(IN);
                Light mainLight = GetMainLight(shadowCoord);
                float3 L = mainLight.direction;
                float3 N = normalize(IN.normalWS);

                // 3. DIFFUSE CALCULATION (Matching Godot Light logic)
                float NdotL = dot(N, L);
                // mainLight.distanceAttenuation handles falloff, shadowAttenuation handles shadows
                float attenuation = mainLight.distanceAttenuation * mainLight.shadowAttenuation;
                
                float diffuse_amount = NdotL + (attenuation - 1.0) + _Wrap;
                diffuse_amount *= _Steepness;

                // 4. CLOUD SHADOWS
                float cloud_value = GetCloudNoise(IN.positionWS);
                diffuse_amount = min(diffuse_amount, cloud_value);

                // 5. TOON STEPPING (Quantization)
                float cuts_f = (float)_Cuts;
                float cuts_inv = 1.0 / cuts_f;
                float original_index = ceil(diffuse_amount * cuts_f);
                float diffuse_stepped = clamp(original_index * cuts_inv, 0.0, 1.0);

                // 6. FINAL COLOUR ASSEMBLY
                // Godot: ALBEDO * LIGHT_COLOR / PI
                float3 diffuse_final = baseAlbedo * (mainLight.color / 3.14159);
                diffuse_final *= diffuse_stepped;

                // Simple Ambient (Optional, to prevent pure black)
                float3 ambient = baseAlbedo * 0.1; 

                return half4(diffuse_final + ambient, 1.0);
            }
            ENDHLSL
        }
    }
}