Shader "Unlit/DitheredLitShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor("Base Color", color) = (1,1,1,1)
        _Smoothness("Smoothness", Range(0,1)) = 0
        _Metallic("Metallic", Range(0,1)) = 0
        _DitherIntensity("Dither Intensity", Range(0,1)) = 0.5
        _DitherScale("Dither Scale", Range(1,5)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"            

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 normal : NORMAL;
                float4 texcoord1 : TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 viewDir : TEXCOORD3;
                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 4);
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _BaseColor;
            float _Smoothness, _Metallic;
            float _DitherIntensity;
            float _DitherScale;

            static const float dither[64] = {
                0.0,    0.75,  0.1875, 0.9375, 0.046875, 0.796875, 0.234375, 0.984375,
                0.5,    0.25,  0.6875, 0.4375, 0.546875, 0.296875, 0.734375, 0.484375,
                0.125,  0.875, 0.0625, 0.8125, 0.171875, 0.921875, 0.109375, 0.859375,
                0.625,  0.375, 0.5625, 0.3125, 0.671875, 0.421875, 0.609375, 0.359375,
                0.03125,0.78125,0.21875,0.96875,0.015625,0.765625,0.203125,0.953125,
                0.53125,0.28125,0.71875,0.46875,0.515625,0.265625,0.703125,0.453125,
                0.15625,0.90625,0.09375,0.84375,0.140625,0.890625,0.078125,0.828125,
                0.65625,0.40625,0.59375,0.34375,0.640625,0.390625,0.578125,0.328125
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.positionWS = TransformObjectToWorld(v.vertex.xyz);
                o.normalWS = TransformObjectToWorldNormal(v.normal.xyz);
                o.viewDir = normalize(_WorldSpaceCameraPos - o.positionWS);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);  // Ensure proper UV transformation
                o.vertex = TransformWorldToHClip(o.positionWS);

                OUTPUT_LIGHTMAP_UV(v.texcoord1, unity_LightmapST, o.lightmapUV);
                OUTPUT_SH(o.normalWS.xyz, o.vertexSH);

                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // Sample the texture
                half4 col = tex2D(_MainTex, i.uv);

                // Create input data for the lighting model
                InputData inputdata = (InputData)0;
                inputdata.positionWS = i.positionWS;
                inputdata.normalWS = normalize(i.normalWS);
                inputdata.viewDirectionWS = i.viewDir;
                inputdata.bakedGI = SAMPLE_GI(i.lightmapUV, i.vertexSH, inputdata.normalWS);

                // Create surface data for the PBR model
                SurfaceData surfacedata;
                surfacedata.albedo = _BaseColor.rgb * col.rgb; // Combine base color with texture
                surfacedata.specular = 0;
                surfacedata.metallic = _Metallic;
                surfacedata.smoothness = _Smoothness;
                surfacedata.normalTS = 0;
                surfacedata.emission = 0;
                surfacedata.occlusion = 1;
                surfacedata.alpha = 1; // Make sure alpha is set correctly
                surfacedata.clearCoatMask = 0;
                surfacedata.clearCoatSmoothness = 0;

                // Retrieve main directional light using URP functions
                Light mainLight = GetMainLight(); // Gets the main directional light

                // Calculate light direction and color
                half3 lightDir = normalize(mainLight.direction);
                half3 lightColor = mainLight.color;

                // Calculate the light intensity based on the angle
                float NdotL = max(dot(i.normalWS, lightDir), 0.0);
                float lightIntensity = length(lightColor) * NdotL;

                // Dither factor depending on light intensity
                float ditherFactor = saturate((1.0 - lightIntensity) * _DitherIntensity);

                // Calculate dither matrix indices
                float2 ditherCoord = floor(i.vertex.xy / _DitherScale) % 8;
                int x = int(ditherCoord.x) % 8;
                int y = int(ditherCoord.y) % 8;
                int index = x + y * 8;
                
                float threshold = dither[index]; // Use 8x8 matrix for smoother dithering

                // Calculate final color
                float3 finalColor = surfacedata.albedo;
                if (ditherFactor > threshold)
                {
                    finalColor *= 0.05; // Darken the color when below the threshold
                }

                // Calculate the final fragment color using the Universal PBR function
                half4 finalOutput = UniversalFragmentPBR(inputdata, surfacedata);
                finalOutput.rgb = finalColor; // Apply dithered color to output

                return finalOutput;
            }
            ENDHLSL
        }
    }
}
