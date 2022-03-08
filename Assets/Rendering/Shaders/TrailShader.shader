// This shader fills the mesh shape with a color predefined in the code.
Shader "TrailShader"
{
	Properties {
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_Color ("Tint", Color) = (0,0,0,1)
	}

	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" }
		LOD 100

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

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

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 vertex2 : TEXCOORD1;

			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.vertex2 = v.vertex;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				float temp = step(0, i.vertex2.x)*(i.vertex2.x);
				col.a = temp*col.r;
				return col * _Color;
			}
			ENDCG
		}
	}
}