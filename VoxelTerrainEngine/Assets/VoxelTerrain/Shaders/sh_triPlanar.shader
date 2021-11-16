// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Tri-Planar World" {
  Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_Side("Side", 2D) = "white" {}
		_Siden("Side", 2D) = "white" {}
		_Top("Top", 2D) = "white" {}
		_Topn("Top", 2D) = "white" {}
		_Bottom("Bottom", 2D) = "white" {}
		_Bottomn("Bottom", 2D) = "white" {}
		_SideScale("Side Scale", Float) = 2
		_TopScale("Top Scale", Float) = 2
		_BottomScale ("Bottom Scale", Float) = 2
		_Offset ("Bottom Scale",Range (0.0, 2.0)) = 2
		_normScale ("Normal Scale",Range (-1.0, 1.0)) = 1
	}
	
	SubShader {
		Tags {
			"Queue"="Geometry"
			"IgnoreProjector"="False"
			"RenderType"="Opaque"
		}
 		LOD 200
		Cull Back
		ZWrite On
		
		CGPROGRAM
		#pragma target 4.0
		#pragma surface surf Standard vertex:vertWorld
		#pragma exclude_renderers flash
		#include "Tessellation.cginc"
		sampler2D _Side, _Top, _Bottom, _Siden, _Topn, _Bottomn;
		float _SideScale, _TopScale, _BottomScale,_normScale,_Offset;
  		fixed4 _Color;

		 struct Input
                {	
                    float3 thisPos;        
                    float3 thisNormal; 
                };  
                
                void vertWorld (inout appdata_full v, out Input o)
                {
               
                o.thisNormal     = UnityObjectToWorldNormal( v.normal);
                    o.thisPos         = mul(unity_ObjectToWorld, v.vertex);
                
                    

                }
			
		void surf (Input IN, inout SurfaceOutputStandard o) {
			float3 projNormal = saturate(pow(IN.thisNormal * 2, 4));
			//float3 WorldSpaceObjectPos = mul(v.vertex,UNITY_MATRIX_MV).xyz;
			float3 Val;
			float4 x;
			float4 y;
			float4 z;
			
			float4 xn;
			float4 yn;
			float4 zn;
			// SIDE X

			//iiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiii
			x = tex2D(_Side, frac(IN.thisPos.zy +_Offset * ( _SideScale ))) * abs(IN.thisNormal.x);
			
			// TOP / BOTTOM
			y = 0;
			if (IN.thisNormal.y > 0) {
				y = tex2D(_Top, frac(IN.thisPos.zx * (_TopScale ))+_Offset ) * abs(IN.thisNormal.y);
			} else {
				y = tex2D(_Bottom, frac(IN.thisPos.zx * (_BottomScale))) * abs(IN.thisNormal.y);
			}
			
			// SIDE Z	
			z = tex2D(_Side, frac(IN.thisPos.xy * (_SideScale ))+_Offset ) * abs(IN.thisNormal.z);
			
			
			
			xn = tex2D(_Siden, frac(IN.thisPos.zy * ( _SideScale ))+_Offset ) * abs(IN.thisNormal.x);
			
			if (IN.thisNormal.y > 0) {
				yn = tex2D(_Topn, frac(IN.thisPos.zx * (_TopScale ))+_Offset ) * abs(IN.thisNormal.y);
			} else {
				yn = tex2D(_Bottomn, frac(IN.thisPos.zx * (_BottomScale))+_Offset ) * abs(IN.thisNormal.y);
			}
			
			// SIDE Z	
			zn = tex2D(_Siden, frac(IN.thisPos.xy * (_SideScale ))+_Offset ) * abs(IN.thisNormal.z);
			

			float4 tex = z;
			tex = lerp(tex, x, projNormal.x);
			tex = lerp(tex, y, projNormal.y);
			o.Albedo = tex *_Color;
			float4 col =zn ;
			
	
			col = lerp(col, xn, projNormal.x);
			col = lerp(col, yn, projNormal.y);
			o.Normal = UnpackNormal(col* _normScale) ;
		} 
		ENDCG
	}
	Fallback "Diffuse"
}