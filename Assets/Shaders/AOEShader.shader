Shader "Unlit/AOEShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TintColor ("Tint Color", Color) = (1,1,1,1)
        _EdgeFadeDistance ("Edge Fade Distance", Range(0, 1)) = 0.1
        _DitherScale ("DitherScale", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float distToCenter : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _TintColor;
            float _EdgeFadeDistance;
            float _DitherScale;

            static const float dither[64] = {
                0.0, 0.75, 0.1875, 0.9375, 0.046875, 0.796875, 0.234375, 0.984375,
                0.5, 0.25, 0.6875, 0.4375, 0.546875, 0.296875, 0.734375, 0.484375,
                0.125, 0.875, 0.0625, 0.8125, 0.171875, 0.921875, 0.109375, 0.859375,
                0.625, 0.375, 0.5625, 0.3125, 0.671875, 0.421875, 0.609375, 0.359375,
                0.03125, 0.78125, 0.21875, 0.96875, 0.015625, 0.765625, 0.203125, 0.953125,
                0.53125, 0.28125, 0.71875, 0.46875, 0.515625, 0.265625, 0.703125, 0.453125,
                0.15625, 0.90625, 0.09375, 0.84375, 0.140625, 0.890625, 0.078125, 0.828125,
                0.65625, 0.40625, 0.59375, 0.34375, 0.640625, 0.390625, 0.578125, 0.328125
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);

                float2 center = float2(0.5, 0.5);
                o.distToCenter = distance(o.uv, center);
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate normalized distance from center to the edge
                float edgeFade = 0.9 - smoothstep(0.0, _EdgeFadeDistance, i.distToCenter); // Start at 0.9 to add a bit of dither to the center
                
                // sample the texture
                half4 texColor = tex2D(_MainTex, i.uv);
                half4 texColorAlphaRef = tex2D(_MainTex, i.uv);

                texColor *= _TintColor;

                // Apply edge fading to alpha
                texColorAlphaRef.a *= edgeFade;

                float2 ditherCoord = floor(i.vertex.xy / _DitherScale) % 4;
                int x = int(ditherCoord.x) % 4;
                int y = int(ditherCoord.y) % 4;
                int index = x + y * 4;
                float threshold = dither[index];

                // Modulate threshold by distance from center to amplify dither effect towards edges
                float amplifiedThreshold = threshold * (1.0 - i.distToCenter);

                if (texColorAlphaRef.a < amplifiedThreshold)
                    discard;
                
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return texColor;
            }
            ENDCG
        }
    }
}
