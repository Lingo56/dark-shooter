Shader "Unlit/EnemyShaderURP"
{
    Properties
    {
        _Mode ("Mode", Int) = 0 // 0 for Texture, 1 for Color
        _BaseMap ("Albedo", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Fade ("Fade Amount", Range(0, 1)) = 0
        _JitterAmount ("Jitter Amount", Range(0, 1)) = 0.1
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Geometry"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 color : COLOR;
                float4 screenPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _Color;
                int _Mode;
                float _Fade;
                float _JitterAmount;
            CBUFFER_END

            Varyings vert(Attributes v)
            {
                Varyings o;

                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.screenPos = ComputeScreenPos(o.positionHCS);
                o.color = v.color;

                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);

                return o;
            }

            float isDithered(float2 pos, float alpha) 
            {
                pos *= _ScreenParams.xy;

                float DITHER_THRESHOLDS[16] =
                {
                    1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
                    13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
                    4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
                    16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
                };

                int index = (int(pos.x) % 4) * 4 + int(pos.y) % 4;
                return alpha - DITHER_THRESHOLDS[index];
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 c;

                if (_Mode == 0) // Texture mode
                {
                    c = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
                }
                else // Color mode
                {
                    c = _Color;
                }

                c.a = _Fade;

                float ditherValue = isDithered(i.screenPos.xy / i.screenPos.w, c.a);
                clip(ditherValue);

                return _Color;
            }
            ENDHLSL
        }
    }
    FallBack "Unlit"
    CustomEditor "CustomShaderGUI"
}
