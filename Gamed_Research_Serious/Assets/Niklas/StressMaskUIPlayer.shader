Shader "UI/StressMaskUIPlayer"
{
    Properties
    {
        _MainTex ("Sprite", 2D) = "grey" {}
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "RenderPipeline"="UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "UI"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            float _SpreadAmount;
            float _NoiseScale;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                o.screenPos = ComputeScreenPos(o.pos);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float2 uv = i.screenPos.xy / i.screenPos.w;

                float diagonal = uv.x + (1.0 - uv.y);

                float2 noiseUV = uv * _NoiseScale;
                float noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, noiseUV).r * 0.2;

                float value = diagonal + noise;

                // distortion
                float distortion = smoothstep(0.1, 0.0, _SpreadAmount - value);
                float2 distortedUV = i.uv + (noise - 0.5) * 0.05 * distortion;

                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, distortedUV);

                // color shift
                float t = saturate(value);
                float3 stressColor = lerp(
                    float3(0.2, 0.8, 1.0),
                    float3(1.0, 0.1, 0.1),
                    t
                );

                col.rgb *= stressColor;

                // clip LAST before return
                clip(_SpreadAmount - value);

                return col;
            }

            ENDHLSL
        }
    }
}