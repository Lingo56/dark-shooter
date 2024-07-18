Shader "Custom/FadeTopToBottom"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _FadeStart ("Fade Start", Float) = 0
        _FadeEnd ("Fade End", Float) = 1
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
            };

            struct v2f
            {
                float4 pos : POSITION;
                float worldY : TEXCOORD0;
            };

            fixed4 _Color;
            float _FadeStart;
            float _FadeEnd;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.worldY = worldPos.y;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float fade = saturate((i.worldY - _FadeStart) / (_FadeEnd - _FadeStart));
                return float4(_Color.rgb, _Color.a * fade);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
