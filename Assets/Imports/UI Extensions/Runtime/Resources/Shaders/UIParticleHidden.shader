Shader "UI/Particles/Hidden"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        Cull Off Lighting Off ZWrite Off Fog { Mode Off }
        LOD 100
 
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
 
            struct Interpolators
            {
                float4 vertex : SV_POSITION;
            };
 
            Interpolators vert ()
            {
                Interpolators o;
                o.vertex = fixed4(0, 0, 0, 0);
                return o;
            }
           
            fixed4 frag (Interpolators i) : SV_Target
            {
                discard;
                return fixed4(0, 0, 0, 0);
            }
            ENDCG
        }
    }
}