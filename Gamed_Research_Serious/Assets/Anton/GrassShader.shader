Shader "Custom/GrassInstanced" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (0, 0.5, 0, 1)
        _TipColor ("Tip Color", Color) = (0.5, 1, 0.5, 1)
        _Width ("Blade Width", Float) = 0.2
        _Height ("Blade Height", Float) = 0.5
    }
    SubShader {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct GrassData { float4 position; };
            StructuredBuffer<GrassData> _GrassDataBuffer;

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _BaseColor, _TipColor;
            float _Width;
            float _Height;

            v2f vert (appdata_full v, uint instanceID : SV_InstanceID) {
                v2f o;

                // 1. Get the World Position from our buffer (calculated by Compute Shader)
                float4 data = _GrassDataBuffer[instanceID].position;
                float3 worldPos = data.xyz;

                // 2. Billboarding Math (same as before, these are world-space vectors)
                float3 vRight = UNITY_MATRIX_V[0].xyz; 
                float3 vUp = float3(0, 1, 0); 
    
                // v.vertex.x/y are the local coordinates of your Quad mesh
                // We multiply them by world vectors to keep them in World Space
                float3 billboardOffset = (vRight * v.vertex.x * _Width) + (vUp * (v.vertex.y + 0.5) * _Height);

                // 3. Wind Effect
                float windSpeed = _Time.y * 2.0;
                float heightFactor = v.vertex.y + 0.5; 
                float noise = sin(worldPos.x * 0.5 + worldPos.z * 0.5 + windSpeed);
                billboardOffset.x += noise * heightFactor * 0.2;

                // --- THE FIX ---
                // Instead of UnityObjectToClipPos, we add the world position and the offset,
                // then multiply by the View-Projection matrix directly.
                float3 finalWorldPos = worldPos + billboardOffset;
                o.pos = mul(UNITY_MATRIX_VP, float4(finalWorldPos, 1.0));
                // ----------------

                o.uv = v.texcoord;
                o.color = lerp(_BaseColor, _TipColor, v.texcoord.y).rgb;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);
                clip(col.a - 0.5); // Simple alpha cutout
                return col * float4(i.color, 1.0);
            }
            ENDCG
        }
    }
}