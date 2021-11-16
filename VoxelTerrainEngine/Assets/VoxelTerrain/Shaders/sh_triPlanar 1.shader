Shader "Voxel/Tri-Planar World" {
  Properties {
		_Side("Side", 2D) = "white" {}
		_Top("Top", 2D) = "white" {}
		_Bottom("Bottom", 2D) = "white" {}
		_SideScale("Side Scale", Float) = 2
		_TopScale("Top Scale", Float) = 2
		_BottomScale ("Bottom Scale", Float) = 2
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
		#pragma target 3.0
		#pragma surface surf Standard fullforwardshadows
		#pragma exclude_renderers flash
		sampler2D _Side, _Top, _Bottom;
		float _SideScale, _TopScale, _BottomScale;
		
		struct Input {
			float3 worldPos;
			float3 worldNormal;
		};
			
		void surf (Input IN, inout SurfaceOutputStandard o) {
			float3 projNormal = saturate(pow(IN.worldNormal * 1.0, 4));
			//float3 WorldSpaceObjectPos = mul(v.vertex,UNITY_MATRIX_MV).xyz;
			float3 dist = distance(IN.worldPos,_WorldSpaceCameraPos);
			float3 Val;
			float3 x;
			float3 y;
			float3 z;
			// SIDE X
			if(distance(IN.worldPos,_WorldSpaceCameraPos) <= 100 && distance(IN.worldPos,_WorldSpaceCameraPos) >= 40.0){
			x = tex2D(_Side, frac(IN.worldPos.zy * ( _SideScale /5))) * abs(IN.worldNormal.x);
			
			// TOP / BOTTOM
			y = 0;
			if (IN.worldNormal.y > 0) {
				y = tex2D(_Top, frac(IN.worldPos.zx * (_TopScale /5))) * abs(IN.worldNormal.y);
			} else {
				y = tex2D(_Bottom, frac(IN.worldPos.zx * (_BottomScale/5))) * abs(IN.worldNormal.y);
			}
			
			// SIDE Z	
			z = tex2D(_Side, frac(IN.worldPos.xy * (_SideScale /5))) * abs(IN.worldNormal.z);}
			//iiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiii
			else if(distance(IN.worldPos,_WorldSpaceCameraPos) <= 49){
			x = tex2D(_Side, frac(IN.worldPos.zy * ( _SideScale ))) * abs(IN.worldNormal.x);
			
			// TOP / BOTTOM
			y = 0;
			if (IN.worldNormal.y > 0) {
				y = tex2D(_Top, frac(IN.worldPos.zx * (_TopScale ))) * abs(IN.worldNormal.y);
			} else {
				y = tex2D(_Bottom, frac(IN.worldPos.zx * (_BottomScale))) * abs(IN.worldNormal.y);
			}
			
			// SIDE Z	
			z = tex2D(_Side, frac(IN.worldPos.xy * (_SideScale ))) * abs(IN.worldNormal.z);}
			else if(distance(IN.worldPos,_WorldSpaceCameraPos) >= 90 && distance(IN.worldPos,_WorldSpaceCameraPos) <299){
			x = tex2D(_Side, frac(IN.worldPos.zy * ( _SideScale /20))) * abs(IN.worldNormal.x);
			
			// TOP / BOTTOM
			y = 0;
			if (IN.worldNormal.y > 0) {
				y = tex2D(_Top, frac(IN.worldPos.zx * (_TopScale /20))) * abs(IN.worldNormal.y);
			} else {
				y = tex2D(_Bottom, frac(IN.worldPos.zx * (_BottomScale/20))) * abs(IN.worldNormal.y);
			}
			
			// SIDE Z	
			z = tex2D(_Side, frac(IN.worldPos.xy * (_SideScale /20))) * abs(IN.worldNormal.z);}
			
			else if(distance(IN.worldPos,_WorldSpaceCameraPos) >= 298 && distance(IN.worldPos,_WorldSpaceCameraPos) <499){
			x = tex2D(_Side, frac(IN.worldPos.zy * ( _SideScale /50))) * abs(IN.worldNormal.x);
			
			// TOP / BOTTOM
			y = 0;
			if (IN.worldNormal.y > 0) {
				y = tex2D(_Top, frac(IN.worldPos.zx * (_TopScale /50))) * abs(IN.worldNormal.y);
			} else {
				y = tex2D(_Bottom, frac(IN.worldPos.zx * (_BottomScale/50))) * abs(IN.worldNormal.y);
			}
			
			// SIDE Z	
			z = tex2D(_Side, frac(IN.worldPos.xy * (_SideScale /50))) * abs(IN.worldNormal.z);}
			else if(distance(IN.worldPos,_WorldSpaceCameraPos) >= 48){
			x = tex2D(_Side, frac(IN.worldPos.zy * ( _SideScale /80))) * abs(IN.worldNormal.x);
			
			// TOP / BOTTOM
			y = 0;
			if (IN.worldNormal.y > 0) {
				y = tex2D(_Top, frac(IN.worldPos.zx * (_TopScale /80))) * abs(IN.worldNormal.y);
			} else {
				y = tex2D(_Bottom, frac(IN.worldPos.zx * (_BottomScale/80))) * abs(IN.worldNormal.y);
			}
			
			// SIDE Z	
			z = tex2D(_Side, frac(IN.worldPos.xy * (_SideScale /80))) * abs(IN.worldNormal.z);}
			o.Albedo = z;
			o.Albedo = lerp(o.Albedo, x, projNormal.x);
			o.Albedo = lerp(o.Albedo, y, projNormal.y);
			o.Metallic = 0;
            o.Smoothness = 0;
			if(IN.worldPos.y<-100){
			float tmp = -IN.worldPos.y;
			if(IN.worldPos.y<-800){
			tmp=800;
			}
				//o.Albedo *=(100/tmp);
			}
		} 
		ENDCG
	}
	Fallback "Diffuse"
}