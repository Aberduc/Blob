// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Outline" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_EmissionColor("Emission Color", Color) = (0.000000,0.000000,0.000000,1.000000)
		_EmissionPower("Emission Power", Range(0,2)) = 1.0
		_RimCol("Rim Colour" , Color) = (1,0,0,1)
		_RimPow("Rim Power", Float) = 1.0
	}

	SubShader{
		Pass
		{
			Name "Behind"
			Tags{ "RenderType" = "transparent" "Queue" = "Geometry+1" }
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest Greater               // here the check is for the pixel being greater or closer to the camera, in which
			Cull Back                   // case the model is behind something, so this pass runs
			ZWrite Off
			LOD 200

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f {
				float4 pos : SV_POSITION;
				float3 normal : TEXCOORD1;      // Normal needed for rim lighting
				float3 viewDir : TEXCOORD2;     // as is view direction.
			};

			float4 _RimCol;
			float _RimPow;


			v2f vert(appdata_tan v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.normal = normalize(v.normal);
				o.viewDir = normalize(ObjSpaceViewDir(v.vertex));       //this could also be WorldSpaceViewDir, which would
				return o;                                               //return the World space view direction.
			}

			half4 frag(v2f i) : COLOR
			{
				half Rim = 1 - saturate(dot(normalize(i.viewDir), i.normal));       //Calculates where the model view falloff is       
																					//for rim lighting.

				half4 RimOut = _RimCol * pow(Rim, _RimPow);
				return RimOut;
			}
			ENDCG
		}

		Tags { "RenderType" = "Opaque" }
		LOD 200
		ZWrite On
		ZTest LEqual

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0


		struct Input {
			float4 color : COLOR;
		};

		half _Glossiness;
		half _Metallic;
		half _EmissionPower;
		half3 _EmissionColor;
		fixed4 _Color;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf(Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
			o.Emission = _EmissionColor.rgb * _EmissionPower;
		}
		ENDCG
	}
		FallBack "Diffuse"
}
