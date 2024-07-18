Shader "Custom/FadeWaveEffect"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _FadeStart ("Fade Start", Float) = 0
        _FadeEnd ("Fade End", Float) = 1
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _Speed ("Speed", Float) = 1.0
        _WaveHeight ("Wave Height", Float) = 0.1
        _TilingFactor ("Tiling Factor", Float) = 10.0
        _UVScale ("UV Scale", Vector) = (1, 1, 0, 0)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
                float worldY : TEXCOORD1;
            };

            fixed4 _Color;
            float _FadeStart;
            float _FadeEnd;
            sampler2D _NoiseTex;
            float _Speed;
            float _WaveHeight;
            float _TilingFactor;
            float2 _UVScale;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * _UVScale.xy; // Scale UV coordinates using _UVScale
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.worldY = worldPos.y;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate UV offset with tiling noise
                float2 noiseUV = i.uv * _TilingFactor;
                float2 uvOffset = noiseUV + float2(_Time.y * _Speed, 0);
                uvOffset = frac(uvOffset); // Ensure UVs stay within valid range

                // Sample noise texture
                float noiseValue = tex2D(_NoiseTex, uvOffset).r;

                // Apply wave effect
                float waveOffset = sin(uvOffset.x * 10.0 + _Time.y * _Speed) * _WaveHeight;

                // Calculate final fade value
                float fadeValue = lerp(_FadeStart, _FadeEnd, noiseValue + waveOffset);
                float fade = saturate((i.worldY - fadeValue) / (_FadeEnd - _FadeStart));

                // Output final color
                return float4(_Color.rgb, _Color.a * fade);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
