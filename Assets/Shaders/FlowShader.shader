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
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        LOD 200

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
                float4 pos : SV_POSITION;
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

            static const float dither[16] = {
                0.1, 0.3, 0.2, 0.4,
                0.6, 0.9, 0.7, 0.5,
                0.8, 0.1, 0.3, 0.2,
                0.4, 0.6, 0.8, 1.0
            };

            // From: https://github.com/keijiro/NoiseShader/blob/master/Packages/jp.keijiro.noiseshader/Shader/SimplexNoise2D.hlsl
            float wglnoise_mod289(float x)
            {
                return x - floor(x / 289) * 289;
            }

            float2 wglnoise_mod289(float2 x)
            {
                return x - floor(x / 289) * 289;
            }

            float3 wglnoise_mod289(float3 x)
            {
                return x - floor(x / 289) * 289;
            }

            float4 wglnoise_mod289(float4 x)
            {
                return x - floor(x / 289) * 289;
            }

            float3 wglnoise_permute(float3 x)
            {
                return wglnoise_mod289((x * 34 + 1) * x);
            }

            float4 wglnoise_permute(float4 x)
            {
                return wglnoise_mod289((x * 34 + 1) * x);
            }

            float3 SimplexNoiseGrad(float2 v)
            {
                const float C1 = (3 - sqrt(3)) / 6;
                const float C2 = (sqrt(3) - 1) / 2;

                // First corner
                float2 i  = floor(v + dot(v, C2));
                float2 x0 = v -   i + dot(i, C1);

                // Other corners
                float2 i1 = x0.x > x0.y ? float2(1, 0) : float2(0, 1);
                float2 x1 = x0 + C1 - i1;
                float2 x2 = x0 + C1 * 2 - 1;

                // Permutations
                i = wglnoise_mod289(i); // Avoid truncation effects in permutation
                float3 p = wglnoise_permute(    i.y + float3(0, i1.y, 1));
                       p = wglnoise_permute(p + i.x + float3(0, i1.x, 1));

                // Gradients: 41 points uniformly over a unit circle.
                // The ring size 17*17 = 289 is close to a multiple of 41 (41*7 = 287)
                float3 phi = p / 41 * 3.14159265359 * 2;
                float2 g0 = float2(cos(phi.x), sin(phi.x));
                float2 g1 = float2(cos(phi.y), sin(phi.y));
                float2 g2 = float2(cos(phi.z), sin(phi.z));

                // Compute noise and gradient at P
                float3 m  = float3(dot(x0, x0), dot(x1, x1), dot(x2, x2));
                float3 px = float3(dot(g0, x0), dot(g1, x1), dot(g2, x2));

                m = max(0.5 - m, 0);
                float3 m3 = m * m * m;
                float3 m4 = m * m3;

                float3 temp = -8 * m3 * px;
                float2 grad = m4.x * g0 + temp.x * x0 +
                              m4.y * g1 + temp.y * x1 +
                              m4.z * g2 + temp.z * x2;

                return 99.2 * float3(grad, dot(m4, px));
            }

            float SimplexNoise(float2 v)
            {
                return SimplexNoiseGrad(v).z;
            }

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
                // Calculate UV offset with simplex noise
                float2 noiseUV = i.uv * _TilingFactor;
                float2 uvOffset = noiseUV + float2(_Time.y * _Speed, 0);

                // Sample noise using simplex noise function
                float noiseValue = SimplexNoise(uvOffset);

                // Apply wave effect
                float waveOffset = sin(uvOffset.x * 10.0 + _Time.y * _Speed) * _WaveHeight;

                // Calculate final fade value
                float fadeValue = lerp(_FadeStart, _FadeEnd, waveOffset);
                float fade = saturate((i.worldY - fadeValue) / (_FadeEnd - _FadeStart));

                float4 texColor = (_Color.rgb, _Color.a * fade);

                int x = int(i.pos.x) % 4;
                int y = int(i.pos.y) % 4;
                int index = x + y * 4;
                float threshold = dither[index];

                if (texColor.a < threshold)
                    discard;

                // Output final color
                return texColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
