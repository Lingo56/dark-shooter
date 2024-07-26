//Lighting Functions copied from: Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl
//  aka: Packages/Universal RP/ShaderLibrary/Lighting.hlsl

float GetDitherThreshold(float2 uv)
{
    // Calculate pixel coordinates
    int2 pixelCoord = int2(uv * _ScreenParams.xy); // Assuming _ScreenParams.xy contains screen resolution
    int2 ditherIndex = pixelCoord % 4; // Assuming a 4x4 Bayer matrix
    int index = ditherIndex.x + ditherIndex.y * 4;

    // Bayer matrix thresholds
    static const float DITHER_THRESHOLDS[16] = {
        1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
        13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
        4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
        16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
    };

    return DITHER_THRESHOLDS[index];
}

half3 LightingCustom(BRDFData brdfData, BRDFData brdfDataClearCoat,
                                    half3 lightColor, half3 lightDirectionWS, half lightAttenuation,
                                    half3 normalWS, half3 viewDirectionWS,
                                    half clearCoatMask, bool specularHighlightsOff)
{
    half NdotL = step(_ShadowStep/*0.5*/, saturate(dot(normalWS, lightDirectionWS)));
    half3 radiance = lightColor * (lightAttenuation * NdotL);

    half3 brdf = brdfData.diffuse;
    
    #ifndef _SPECULARHIGHLIGHTS_OFF
    [branch] if (!specularHighlightsOff)
    {
        brdf += brdfData.specular * DirectBRDFSpecular(brdfData, normalWS, lightDirectionWS, viewDirectionWS);

        #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
        // Clear coat evaluates the specular a second timw and has some common terms with the base specular.
        // We rely on the compiler to merge these and compute them only once.
        half brdfCoat = kDielectricSpec.r * DirectBRDFSpecular(brdfDataClearCoat, normalWS, lightDirectionWS, viewDirectionWS);

            // Mix clear coat and base layer using khronos glTF recommended formula
            // https://github.com/KhronosGroup/glTF/blob/master/extensions/2.0/Khronos/KHR_materials_clearcoat/README.md
            // Use NoV for direct too instead of LoH as an optimization (NoV is light invariant).
            half NoV = saturate(dot(normalWS, viewDirectionWS));
            // Use slightly simpler fresnelTerm (Pow4 vs Pow5) as a small optimization.
            // It is matching fresnel used in the GI/Env, so should produce a consistent clear coat blend (env vs. direct)
            half coatFresnel = kDielectricSpec.x + kDielectricSpec.a * Pow4(1.0 - NoV);

        brdf = brdf * (1.0 - clearCoatMask * coatFresnel) + brdfCoat * clearCoatMask;
        #endif // _CLEARCOAT
    }
    #endif // _SPECULARHIGHLIGHTS_OFF

    // Apply dithering
    float2 uv = (gl_FragCoord.xy / _ScreenParams.xy);/* Compute UV coordinates based on your needs, e.g., screen space coordinates */;
    float ditherThreshold = GetDitherThreshold(uv);
    half ditherAmount = 1 * (1.0 - lightAttenuation); // Adjust dither amount based on attenuation
    
    half3 finalColor = brdf * radiance;
    finalColor = lerp(finalColor, half3(0.0, 0.0, 0.0), ditherAmount > ditherThreshold ? 1.0 : 0.0); // Apply dithering
    
    return finalColor;
}

half3 LightingCustom(BRDFData brdfData, BRDFData brdfDataClearCoat, Light light, half3 normalWS,
                                    half3 viewDirectionWS, half clearCoatMask, bool specularHighlightsOff)
{
    return LightingCustom(brdfData, brdfDataClearCoat, light.color, light.direction,
                                         light.distanceAttenuation * light.shadowAttenuation, normalWS, viewDirectionWS,
                                         clearCoatMask, specularHighlightsOff);
}

half4 UniversalFragmentCustom(InputData inputData, SurfaceData surfaceData)
{
    #ifdef _SPECULARHIGHLIGHTS_OFF
    bool specularHighlightsOff = true;
    #else
    bool specularHighlightsOff = false;
    #endif

    BRDFData brdfData;

    // NOTE: can modify alpha
    InitializeBRDFData(surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness,
                       surfaceData.alpha, brdfData);

    BRDFData brdfDataClearCoat = (BRDFData)0;
    #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
    // base brdfData is modified here, rely on the compiler to eliminate dead computation by InitializeBRDFData()
    InitializeBRDFDataClearCoat(surfaceData.clearCoatMask, surfaceData.clearCoatSmoothness, brdfData, brdfDataClearCoat);
    #endif

    // To ensure backward compatibility we have to avoid using shadowMask input, as it is not present in older shaders
    #if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
    half4 shadowMask = inputData.shadowMask;
    #elif !defined (LIGHTMAP_ON)
    half4 shadowMask = unity_ProbesOcclusion;
    #else
    half4 shadowMask = half4(1, 1, 1, 1);
    #endif

    Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, shadowMask);

    #if defined(_SCREEN_SPACE_OCCLUSION)
        AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(inputData.normalizedScreenSpaceUV);
        mainLight.color *= aoFactor.directAmbientOcclusion;
        surfaceData.occlusion = min(surfaceData.occlusion, aoFactor.indirectAmbientOcclusion);
    #endif

    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI);
    half3 color = GlobalIllumination(brdfData, brdfDataClearCoat, surfaceData.clearCoatMask,
                                     inputData.bakedGI, surfaceData.occlusion,
                                     inputData.normalWS, inputData.viewDirectionWS);
    color += LightingCustom(brdfData, brdfDataClearCoat,
                                           mainLight,
                                           inputData.normalWS, inputData.viewDirectionWS,
                                           surfaceData.clearCoatMask, specularHighlightsOff);

    #ifdef _ADDITIONAL_LIGHTS
    uint pixelLightCount = GetAdditionalLightsCount();
    for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
    {
        Light light = GetAdditionalLight(lightIndex, inputData.positionWS, shadowMask);
    #if defined(_SCREEN_SPACE_OCCLUSION)
            light.color *= aoFactor.directAmbientOcclusion;
    #endif
        color += LightingCustom(brdfData, brdfDataClearCoat,
                                         light,
                                         inputData.normalWS, inputData.viewDirectionWS,
                                         surfaceData.clearCoatMask, specularHighlightsOff);
    }
    #endif

    #ifdef _ADDITIONAL_LIGHTS_VERTEX
    color += inputData.vertexLighting * brdfData.diffuse;
    #endif

    color += surfaceData.emission;

    return half4(color, surfaceData.alpha);
}

void BuildInputData(Varyings input, SurfaceDescription surfaceDescription, out InputData inputData)
{
    inputData.positionWS = input.positionWS;

    #ifdef _NORMALMAP
    #if _NORMAL_DROPOFF_TS
    // IMPORTANT! If we ever support Flip on double sided materials ensure bitangent and tangent are NOT flipped.
    float crossSign = (input.tangentWS.w > 0.0 ? 1.0 : -1.0) * GetOddNegativeScale();
    float3 bitangent = crossSign * cross(input.normalWS.xyz, input.tangentWS.xyz);
    inputData.normalWS = TransformTangentToWorld(surfaceDescription.NormalTS,
                                                 half3x3(input.tangentWS.xyz, bitangent, input.normalWS.xyz));
    #elif _NORMAL_DROPOFF_OS
            inputData.normalWS = TransformObjectToWorldNormal(surfaceDescription.NormalOS);
    #elif _NORMAL_DROPOFF_WS
            inputData.normalWS = surfaceDescription.NormalWS;
    #endif
    #else
        inputData.normalWS = input.normalWS;
    #endif
    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    inputData.viewDirectionWS = SafeNormalize(input.viewDirectionWS);

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        inputData.shadowCoord = input.shadowCoord;
    #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
        inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
    #else
    inputData.shadowCoord = float4(0, 0, 0, 0);
    #endif

    inputData.fogCoord = input.fogFactorAndVertexLight.x;
    inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
    inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.sh, inputData.normalWS);
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);
}

PackedVaryings vert(Attributes input)
{
    Varyings output = (Varyings)0;
    output = BuildVaryings(input);
    PackedVaryings packedOutput = (PackedVaryings)0;
    packedOutput = PackVaryings(output);
    return packedOutput;
}

half4 frag(PackedVaryings packedInput) : SV_TARGET
{
    Varyings unpacked = UnpackVaryings(packedInput);
    UNITY_SETUP_INSTANCE_ID(unpacked);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(unpacked);

    SurfaceDescriptionInputs surfaceDescriptionInputs = BuildSurfaceDescriptionInputs(unpacked);
    SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);

    #if _AlphaClip
        half alpha = surfaceDescription.Alpha;
        clip(alpha - surfaceDescription.AlphaClipThreshold);
    #elif _SURFACE_TYPE_TRANSPARENT
        half alpha = surfaceDescription.Alpha;
    #else
    half alpha = 1;
    #endif

    InputData inputData;
    BuildInputData(unpacked, surfaceDescription, inputData);

    #ifdef _SPECULAR_SETUP
        float3 specular = surfaceDescription.Specular;
        float metallic = 1;
    #else
    float3 specular = 0;
    float metallic = surfaceDescription.Metallic;
    #endif

    SurfaceData surface = (SurfaceData)0;
    surface.albedo = surfaceDescription.BaseColor;
    surface.metallic = saturate(metallic);
    surface.specular = specular;
    surface.smoothness = saturate(surfaceDescription.Smoothness),
    surface.occlusion = surfaceDescription.Occlusion,
    surface.emission = surfaceDescription.Emission,
    surface.alpha = saturate(alpha);
    surface.clearCoatMask = 0;
    surface.clearCoatSmoothness = 1;

    #ifdef _CLEARCOAT
        surface.clearCoatMask       = saturate(surfaceDescription.CoatMask);
        surface.clearCoatSmoothness = saturate(surfaceDescription.CoatSmoothness);
    #endif

    half4 color = UniversalFragmentCustom(inputData, surface);

    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    return color;
}
