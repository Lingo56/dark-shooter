//UNITY_SHADER_NO_UPGRADE
Shader "Hidden/CSG/internal/customWire"
{
	Properties 
	{
	}
	SubShader 
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		LOD 200
		Offset -1, -10
		ZTest LEqual
        Lighting Off
        ZWrite Off
        Cull Off
		Blend One OneMinusSrcAlpha

        Pass 
		{
			CGPROGRAM
				
				#pragma vertex vert
				#pragma fragment frag
			
				#include "UnityCG.cginc"

				struct Interpolators 
				{
 					float4 pos   : SV_POSITION;
 					fixed4 color : COLOR0;
				};

				Interpolators vert (appdata_full v)
				{
					Interpolators o;
					o.pos	= mul (UNITY_MATRIX_MVP, v.vertex);
					//o.pos.z += 0.00105f;	// I would use Offset if it actually worked ..
					o.color = v.color;
					return o;
				}

				fixed4 frag (Interpolators input) : SV_Target
				{
					fixed4 col = input.color;
					col.rgb *= col.a;
					return col;
				}

			ENDCG
		}
	}
}
