﻿//UNITY_SHADER_NO_UPGRADE
Shader "Hidden/CSG/internal/transparentSpecialSurface"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		Cull Off
		Lighting Off
		Blend One OneMinusSrcAlpha
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				//float2 uv : TEXCOORD0;
			};

			struct Interpolators
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			Interpolators vert (appdata v)
			{
				//float3 x = ddx(v.vertex);
				//float3 y = ddy(v.vertex);
				float3 x = float3(0, -1, 0);

				float3 normal = -v.normal;
				x = abs(dot(x, normal)) < 0.5 ? x : float3(0, 0, -1);

				float3 tangent = normalize(cross(normal, x));
				float3 binormal = normalize(cross(normal, tangent));

				float3x3 texMat = float3x3(tangent, binormal, normal);

				Interpolators o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(mul(texMat, v.vertex).xy, _MainTex);
				return o;
			}
			
			fixed4 frag (Interpolators i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				col.a = 0.5f;
				col.rgb *= col.a;
				return col;
			}
			ENDCG
		}
	}
}
