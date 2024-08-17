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
        _DitherScale ("DitherScale", Float) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
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

            struct Interpolators
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

            Interpolators vert (appdata v)
            {
                Interpolators o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * _UVScale.xy;
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.worldY = worldPos.y;
                return o;
            }

            fixed4 frag (Interpolators i) : SV_Target
            {
                // Calculate UV offset with simplex noise
                float2 noiseUV = i.uv * _TilingFactor;
                float2 uvOffset = noiseUV + float2(_Time.y * _Speed, 0);

                // Apply wave effect
                float waveOffset = sin(uvOffset.x * 10.0 + _Time.y * _Speed) * _WaveHeight;

                // Calculate final fade value
                float fadeValue = lerp(_FadeStart, _FadeEnd, waveOffset);
                float fade = saturate((i.worldY - fadeValue) / (_FadeEnd - _FadeStart));

                float4 texColor = float4(_Color.rgb, _Color.a * fade);

                // Apply Dither
                float2 ditherCoord = floor(i.pos.xy / _DitherScale) % 8;
                int x = int(ditherCoord.x) % 8;
                int y = int(ditherCoord.y) % 8;
                int index = x + y * 8;
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
