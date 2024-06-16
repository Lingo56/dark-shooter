Shader "Custom/RadialTimerShader"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Cutoff ("Fill Amount", Range(0,1)) = 0.0
        _DitherPattern ("Dither Pattern", 2D) = "white" {}
        _DitherStrength ("Dither Strength", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            float _Cutoff;
            sampler2D _DitherPattern;
            float _DitherStrength;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float2 center = float2(0.5, 0.5);
                float angle = atan2(i.uv.y - center.y, i.uv.x - center.x) / (2 * 3.14159265358979323846);
                if (angle < 0) angle += 1;

                half4 texColor = tex2D(_MainTex, i.uv);
                if (angle > _Cutoff)
                    discard;

                return texColor;
            }
            ENDCG
        }
    }
    FallBack "Transparent/Diffuse"
}
