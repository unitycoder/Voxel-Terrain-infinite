Shader "Custom/Translucent" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_BumpMap ("Normal (Normal)", 2D) = "bump" {}
		_Color ("Main Color", Color) = (1,1,1,1)
		_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
		_Shininess ("Shininess", Range (0.00, 1)) = 0.078125

		//_Thickness = Thickness texture (invert normals, bake AO).
		//_Power = "Sharpness" of translucent glow.
		//_Distortion = Subsurface distortion, shifts surface normal, effectively a refractive index.
		//_Scale = Multiplier for translucent glow - should be per-light, really.
		//_SubColor = Subsurface colour.
		_Thickness ("Thickness (R)", 2D) = "bump" {}
		_Power ("Subsurface Power", Float) = 1.0
		_Distortion ("Subsurface Distortion", Float) = 0.0
		_Scale ("Subsurface Scale", Float) = 0.5
		_SubColor ("Subsurface Color", Color) = (1.0, 1.0, 1.0, 1.0)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard
		#pragma exclude_renderers flash
		#pragma target 4.0

		sampler2D _MainTex, _BumpMap, _Thickness;
		float _Scale, _Power, _Distortion;
		fixed4 _Color, _SubColor;
		half _Shininess;

		struct Input {
			float3 lightDir;
			 float3 viewDir;
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutputStandard o) {

		IN.viewDir =  IN.viewDir ;
			IN.lightDir = IN.lightDir ;

			// Translucency.
			fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
			float3 texN = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
			half3 transLightDir = IN.lightDir + texN * _Distortion;
			float transDot = pow ( max (0, dot ( IN.viewDir, -transLightDir) ), _Power ) * _Scale;
			fixed3 transLight = (2) * ( transDot ) * tex.a * _SubColor.rgb;
			fixed3 transAlbedo = tex * _LightColor0.rgb * transLight;
		

			// Add the two together.
			fixed4 c;
			c.rgb = transAlbedo;
			c.a = _LightColor0.a * _SpecColor.a * 1;




			o.Albedo = (tex.rgb +c.rgb )* _Color.rgba;
			float4 skin = tex2D(_Thickness, IN.uv_MainTex).r;;
			o.Alpha = skin.r;
			o.Occlusion = skin.r;
			o.Smoothness = _Shininess;
			o.Normal = texN;
		}



		ENDCG
	}
	FallBack "Bumped Diffuse"
}