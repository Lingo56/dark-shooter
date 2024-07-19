Shader "Custom/RadialTimerShader"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _TintColor ("Tint Color", Color) = (1,1,1,1)
        _Cutoff ("Fill Amount", Range(0,1)) = 0.0
        _EdgeFadeDistance ("Edge Fade Distance", Range(0, 1)) = 0.1
        _DitherScale ("DitherScale", Float) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            float _Cutoff;
            float _EdgeFadeDistance;
            float4 _TintColor;
            float _DitherScale;

            static const float dither[16] = {
                0.1, 0.3, 0.2, 0.4,
                0.6, 0.9, 0.7, 0.5,
                0.8, 0.1, 0.3, 0.2,
                0.4, 0.6, 0.8, 1.0
            };

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float distToCenter : TEXCOORD1; // Store distance to center
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
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
                half4 texColorAlphaRef = tex2D(_MainTex, i.uv);

                texColor *= _TintColor;

                // Adjust alpha based on angle cutoff and edge fade
                float angle = atan2(i.uv.y - 0.5, i.uv.x - 0.5) / (2 * 3.14159265358979323846);
                if (angle < 0) angle += 1;

                // Calculate gradient based on _Cutoff
                float gradiation = 0.1;  // Default value for when _Cutoff < 0.9
                if (_Cutoff >= 0.9)
                {
                    gradiation = lerp(0.1, 0.005, (_Cutoff - 0.9) / 0.1);
                }

                if (angle > _Cutoff)
                {
                    discard;
                }   
                else if (_Cutoff >= 0.9 && angle > _Cutoff - gradiation)
                {
                    float fadeAmount = (_Cutoff - angle) / gradiation;
                    texColorAlphaRef.a *= fadeAmount;
                }
                else if (angle > _Cutoff - gradiation)
                {
                    float fadeAmount = (_Cutoff - angle) / gradiation;
                    texColorAlphaRef.a *= fadeAmount;
                } 
                else if (angle < 0.005)
                {
                    float fadeAmount = angle / 0.005;
                    texColorAlphaRef.a *= fadeAmount;
                }

                // Apply edge fading to alpha
                texColorAlphaRef.a *= edgeFade;

                float2 ditherCoord = floor(i.pos.xy / _DitherScale) % 4;
                int x = int(ditherCoord.x) % 4;
                int y = int(ditherCoord.y) % 4;
                int index = x + y * 4;
                float threshold = dither[index];

                // Modulate threshold by distance from center to amplify dither effect towards edges
                float amplifiedThreshold = threshold * (1.0 - i.distToCenter);

                if (texColorAlphaRef.a < amplifiedThreshold)
                    discard;

                return texColor;
            }
            ENDCG
        }
    }
    FallBack "Transparent/Diffuse"
}
