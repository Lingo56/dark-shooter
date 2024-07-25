//UNITY_SHADER_NO_UPGRADE
Shader "Hidden/CSG/internal/coloredPolygon"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		//Tags{ "Queue" = "Geometry" }
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Cull Off
		Lighting Off
		Blend One OneMinusSrcAlpha
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Interpolators
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			Interpolators vert (appdata v)
			{
				Interpolators o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (Interpolators i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				col.a = 0.5f;
				UNITY_APPLY_FOG(i.fogCoord, col);
				col.rgb *= col.a;
				return col;
			}
			ENDCG
		}
	}
}
