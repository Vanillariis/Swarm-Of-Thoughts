Shader "Custom/GrassShader" {
    Properties {
        _MainTex ("Grass Blade Texture", 2D) = "white" {}
        
        [Header(Noise Texture Coloring)]
        _NoiseTex ("Color Map Texture (Grayscale)", 2D) = "white" {}
        _NoiseScale1 ("Base Tiling", Float) = 0.02
        _NoiseScale2 ("Detail Tiling (Break Tiling)", Float) = 0.077
        
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
            float _Step1, _Step2;
            float _NoiseScale1, _NoiseScale2;
            float _TipBrightness, _Width, _Height;

            v2f vert (appdata_full v, uint instanceID : SV_InstanceID) {
                v2f o;

                float4 data = _GrassDataBuffer[instanceID].position;
                float3 worldPos = data.xyz;

                // --- FIX 1: BREAK TILING ---
                // Sample 1: Large scale base
                float2 uv1 = worldPos.xz * _NoiseScale1 + _NoiseTex_ST.zw;
                float n1 = tex2Dlod(_NoiseTex, float4(uv1, 0, 0)).r;

                // Sample 2: Faster, smaller scale, shifted offset
                float2 uv2 = worldPos.xz * _NoiseScale2 + float2(0.5, 0.2);
                float n2 = tex2Dlod(_NoiseTex, float4(uv2, 0, 0)).r;

                // Blend them (average) to create a non-repeating pattern
                float noiseVal = (n1 + n2) * 0.5;

                // --- FIX 2: ADJUSTABLE THRESHOLDS ---
                float3 patchColor;
                if (noiseVal < _Step1) patchColor = _ColorA.rgb;
                else if (noiseVal < _Step2) patchColor = _ColorB.rgb;
                else patchColor = _ColorC.rgb;

                // Billboarding & Wind
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