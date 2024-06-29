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
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _BaseMap;
            float4 _Color;
            int _Mode;
            float _Fade;

            float ditherMatrix[16] = {
                0.0,  8.0,  2.0, 10.0,
                12.0, 4.0, 14.0,  6.0,
                3.0, 11.0,  1.0,  9.0,
                15.0, 7.0, 13.0,  5.0
            };

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 c = tex2D(_BaseMap, i.uv);
                if (_Mode == 1) // Use Color
                {
                    c = _Color;
                }

                float2 uv = i.uv * 4.0; // Adjust the scale to match the dither matrix
                int x = (int)fmod(floor(uv.x), 4.0);
                int y = (int)fmod(floor(uv.y), 4.0);
                int index = x + y * 4;
                float threshold = ditherMatrix[index] / 16.0;

                if (_Fade < threshold)
                    c.a = 0;
                else
                    c.a *= _Fade; // Scale alpha with the fade amount

                return c;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "CustomShaderGUI"
}
