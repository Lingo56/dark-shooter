Shader "Custom/RadialTimerShader"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Cutoff ("Fill Amount", Range(0,1)) = 0.0
        _EdgeFadeDistance ("Edge Fade Distance", Range(0, 0.5)) = 0.1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            float _Cutoff;
            float _EdgeFadeDistance;

            static const float dither[16] = {
                0.0,  0.8,  0.2,  1.0,
                0.6,  0.4,  0.8,  0.2,
                0.2,  1.0,  0.6,  0.4,
                0.8,  0.2,  0.4,  0.6
            };

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float distToCenter : TEXCOORD1; // Store distance to center
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                // Calculate distance from UV center (0.5, 0.5)
                float2 center = float2(0.5, 0.5);
                o.distToCenter = distance(o.uv, center);

                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // Calculate normalized distance from center to the edge
                float edgeFade = 1.0 - smoothstep(0.0, _EdgeFadeDistance, i.distToCenter);

                half4 texColor = tex2D(_MainTex, i.uv);
                // Adjust alpha based on angle cutoff and edge fade
                float angle = atan2(i.uv.y - 0.5, i.uv.x - 0.5) / (2 * 3.14159265358979323846);
                if (angle < 0) angle += 1;

                if (angle > _Cutoff)
                    discard;
                
                // Apply edge fading to alpha
                texColor.a *= edgeFade;

                int x = int(i.vertex.x) % 4;
                int y = int(i.vertex.y) % 4;
                int index = x + y * 4;
                float threshold = dither[index];

                if (texColor.a < _Cutoff * threshold)
                    discard;

                return texColor;
            }
            ENDCG
        }
    }
    FallBack "Transparent/Diffuse"
}
