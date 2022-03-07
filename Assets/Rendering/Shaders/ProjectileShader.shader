Shader "Unlit/ProjectileShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader {
		Tags {
			"RenderType" = "Qpaque"
			"IgnoreProjector" = "True"
			"Queue" = "Geometry"
		}
		Cull Off
		Lighting Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
			#pragma multi_compile_instancing
			#pragma instancing_options procedural:setup
			
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex  : POSITION;
				float2 uv   : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 vertex  : SV_POSITION;
				float2 uv   : TEXCOORD0;
			};

	#if SHADER_TARGET >= 45
			StructuredBuffer<float3> positionBuffer;
	#endif

			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert (appdata v, uint instanceID : SV_InstanceID)
			{
			#if SHADER_TARGET >= 45
				float3 position = positionBuffer[instanceID];
			#else
				float3 position = 0;
			#endif

				const float scale = 1.0f;
				const float radius = 0.25f;
				float4 worldPosition = float4(position + v.vertex - radius / 2, scale);

				v2f o;
				o.vertex = mul(UNITY_MATRIX_VP, worldPosition);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			float4 frag(v2f i) : SV_Target {
				return tex2D(_MainTex, i.uv);
			}
			ENDCG
		}
	}
}
