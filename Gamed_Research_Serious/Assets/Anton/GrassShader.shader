Shader "Custom/GrassShader" {
    Properties {
        _MainTex ("Grass Blade Texture", 2D) = "white" {}
        
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
        
        [Header(Blade Settings)]
        _TipBrightness ("Tip Brightness", Range(1, 2)) = 1.2
        _Width ("Blade Width", Float) = 0.2
        _Height ("Blade Height", Float) = 0.5
    }
    SubShader {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0 
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct GrassData { float4 position; };
            StructuredBuffer<GrassData> _GrassDataBuffer;

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 finalColor : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _NoiseTex; 
            float4 _NoiseTex_ST; 

            float4 _ColorA, _ColorB, _ColorC;
            float _Step1, _Step2, _NoiseBlur;
            float _DetailScale, _DetailWeight;
            float _TipBrightness, _Width, _Height;

            // Ported blurred sampling helper for Vertex Shader
            float SampleNoiseBlurred(float2 uv, float blur)
            {
                float d = blur;
                // tex2Dlod is required when sampling textures in the Vertex Shader
                float n1 = tex2Dlod(_NoiseTex, float4(uv + float2(-d, -d), 0, 0)).r;
                float n2 = tex2Dlod(_NoiseTex, float4(uv + float2( d, -d), 0, 0)).r;
                float n3 = tex2Dlod(_NoiseTex, float4(uv + float2(-d,  d), 0, 0)).r;
                float n4 = tex2Dlod(_NoiseTex, float4(uv + float2( d,  d), 0, 0)).r;
                return (n1 + n2 + n3 + n4) * 0.25;
            }

            v2f vert (appdata_full v, uint instanceID : SV_InstanceID) {
                v2f o;

                float4 data = _GrassDataBuffer[instanceID].position;
                float3 worldPos = data.xyz;

                // --- MATCHING GROUND COLOUR LOGIC ---
                
                // Layer 1: Base Noise (Uses Tiling/Offset from Inspector)
                float2 uv1 = worldPos.xz * _NoiseTex_ST.xy + _NoiseTex_ST.zw;
                float noiseBase = SampleNoiseBlurred(uv1, _NoiseBlur);

                // Layer 2: Detail Noise (Matches the 0.53, 0.11 offset from ground shader)
                float2 uv2 = (worldPos.xz * _NoiseTex_ST.xy * _DetailScale) + float2(0.53, 0.11);
                float noiseDetail = SampleNoiseBlurred(uv2, _NoiseBlur * _DetailScale);

                // Combine exactly like the ground shader
                float noiseVal = lerp(noiseBase, noiseDetail, _DetailWeight);

                // Threshold Selection
                float3 patchColor;
                if (noiseVal < _Step1) patchColor = _ColorA.rgb;
                else if (noiseVal < _Step2) patchColor = _ColorB.rgb;
                else patchColor = _ColorC.rgb;

                // --- BILLBOARDING & WIND ---
                float3 vRight = UNITY_MATRIX_V[0].xyz; 
                float3 vUp = float3(0, 1, 0); 
                float3 billboardOffset = (vRight * v.vertex.x * _Width) + (vUp * (v.vertex.y + 0.5) * _Height);
                
                float windSpeed = _Time.y * 2.0;
                float heightFactor = v.vertex.y + 0.5; 
                float windNoise = sin(worldPos.x * 0.5 + worldPos.z * 0.5 + windSpeed);
                billboardOffset.x += windNoise * heightFactor * 0.2;

                o.pos = mul(UNITY_MATRIX_VP, float4(worldPos + billboardOffset, 1.0));
                o.uv = v.texcoord;
                o.finalColor = patchColor * lerp(1.0, _TipBrightness, v.texcoord.y);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);
                clip(col.a - 0.5); 
                return col * float4(i.finalColor, 1.0);
            }
            ENDCG
        }
    }
}