Shader "Custom/Clouds" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_MainDamage ("Damage (RGB)", 2D) = "white" {}
		_MainNorm ("Normal (RGB)", 2D) = "white" {}
		_MainParallax("Parallax (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(-1,1)) = 0.5
		_NIntensity ("normal Intensity", Range(-1,1)) = 0.5
		_Height ("Height", Range (-0.2, 0.2)) = 0.02
		_Damage ("_Damage", float) = 0.02
		_DamageStart ("_DamageStart", float) = 1

	}
	SubShader {
	
		Tags { "RenderType"="Opaque" }

		LOD 200
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 4.0
		int parallax = 1;
		float _NIntensity,_DamageStart;
		float _Damage;
		sampler2D _MainTex,_MainParallax,_MainNorm,_MainDamage;
		struct Input {
		float3 viewDir;
			float2 uv_MainTex;
		};

		float4 blend(float4 texture1, float a1, float4 texture2, float a2)
		{
	
    	float depth = 0.01f;
    	float ma = max(texture1.a * a1, texture2.a * a2) - depth;

    	float b1 = max(texture1.a * a1 - ma, 0);
    	float b2 = max(texture2.a * a2 - ma, 0);
    	return ((texture1.rgba * b1) + (texture2.rgba * b2)) / (b1 + b2);
		}

inline float2 POffset( half height, half3 viewDir ,float2 uv, sampler2D relief )
{
const float minlayers = 10;
const float maxlayers = 60;

float numlayers = lerp(maxlayers,minlayers , abs(dot(float3(0,0,0),viewDir)));
float layerheight = 1.0f/numlayers;
float currentlayerheight = 0;
float2 dtex = height * viewDir.xy/viewDir.z/numlayers;

float2 currentcoords = uv;

float heightfromtex = tex2D(relief,currentcoords).r;
int l = 0;
while(heightfromtex>currentlayerheight){

if(l<=maxlayers){
currentlayerheight+=layerheight;
currentcoords-=dtex;
heightfromtex = tex2D(relief,currentcoords).r; 
}
else break;
l++;
}
float2 deltacoord = dtex/2;
float deltaheight = layerheight/2;

currentcoords += deltacoord;
currentlayerheight-=deltaheight;
const int numsearches = 10;
for(int i =0;i < numsearches;i++){
deltacoord/=2;
deltaheight/=2;
heightfromtex = tex2D(relief,currentcoords).r;
if(heightfromtex>currentlayerheight) {
currentcoords -=deltacoord;
currentlayerheight +=deltaheight;
}
else {
currentcoords +=deltacoord;
currentlayerheight -=deltaheight;
}
}
return currentcoords;
}
		half _Glossiness,_Height;
		fixed4 _Color;
		void surf (Input IN, inout SurfaceOutputStandard o) {
		float2 newUvs = IN.uv_MainTex;

		   newUvs =   POffset(_Height,IN.viewDir,IN.uv_MainTex,_MainParallax);
			// Albedo comes from a texture tinted by color
			float4 c = tex2D (_MainTex, newUvs);
			float4 d = 0;
			float4 Tex = c;
			if(_Damage>0){
			d = tex2D (_MainDamage, newUvs);
			Tex = blend(c,c.a*_DamageStart,d,_Damage);}
			float3 norm = UnpackNormal(tex2D (_MainNorm, newUvs));
			o.Albedo = Tex.rgb* _Color.rgb;
			  
			// Metallic and smoothness come from slider variables
			o.Normal = norm * _NIntensity;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a* _Color.a;

		}
		ENDCG
	} 
	FallBack "Diffuse"
}