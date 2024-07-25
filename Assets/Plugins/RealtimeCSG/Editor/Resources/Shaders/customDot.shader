//UNITY_SHADER_NO_UPGRADE
Shader "Hidden/CSG/internal/customDot"
{
	Properties 
	{
	}
	SubShader 
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		LOD 200

		Lighting Off
		ZTest Off
        Cull Off
        ZWrite Off
		//Offset -1 -1 // why do I get a parser error here?
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
					o.pos.z += 0.00105f;	// I would use Offset if it actually worked ..
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
