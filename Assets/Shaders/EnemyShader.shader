Shader "Unlit/EnemyShader"
{
    Properties
    {
        _Mode ("Mode", Int) = 0 // 0 for Texture, 1 for Color
        _BaseMap ("Albedo", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Fade ("Fade Amount", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            //TODO Fix dither not working
            // Interesting shader effect is happening though
            sampler2D _BaseMap;
            float4 _Color;
            int _Mode;
            float _Fade;

            // Screen-door transparency matrix
            float4x4 thresholdMatrix = float4x4(
                1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
                13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
                4.0 / 17.0,  12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
                16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
            );

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 col : COLOR;
                float4 spos : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.col = v.color;
                o.spos = ComputeScreenPos(o.pos); // Calculate screen position
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 c;
                if (_Mode == 0) // Texture mode
                {
                    c = tex2D(_BaseMap, i.spos.xy); // Sample texture using normalized screen coordinates
                }
                else // Color mode
                {
                    c = _Color;
                }

                // Apply screen-door transparency effect
                float threshold = thresholdMatrix[(int)(i.spos.x) % 4][(int)(i.spos.y) % 4];
                clip(_Fade - threshold);

                return c;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "CustomShaderGUI"
}