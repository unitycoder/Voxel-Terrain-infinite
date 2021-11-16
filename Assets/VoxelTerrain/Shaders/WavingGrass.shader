// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Details/WavingDoublePass" {
Properties {
	_WavingTint ("Fade Color", Color) = (.7,.6,.5, 0)
	_MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
	_MainTexNormal ("Normal (RGB) Alpha (A)", 2D) = "white" {}
	_MainTexSpec ("Specular (RGB) Alpha (A)", 2D) = "white" {}
	//_WaveAndDistance ("Wave and distance", Vector) = (12, 3.6, 1, 1)
_Cutoff ("Alphacutoff" , Range( 0 ,1 ))= 0.5
_normalInten ("Normal Intensity" , Range( -1 ,1 ))= 0.5
_Shininess("Shininess" , Range( -1 ,1 ))= 0.5
distLimit("size" , Range( 0 ,2 ))= 0.5
}

SubShader {
	Tags {
	"Queue" = "Geometry+200"
	"IgnoreProjector"="True"
	"RenderType"="Grass"
	}
	Cull off
	LOD 200
	Lighting On
	ZWrite On
	ZTest Less
	
CGPROGRAM
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
#pragma exclude_renderers gles
#pragma surface surf Standard vertex:vert addshadow 
		float _Shininess;

sampler2D _MainTex,_MainTexNormal,_MainTexSpec;
float _Cutoff;
float4 windSpeed;
float4 _WavingTint;
float4 WindDirection;
half _normalInten;
float3 distort;
float distLimit = 0.80;

float3 pos;
uniform float4 _Obstacle;

float _speed;


struct Input {
	float2 uv_MainTex;
	float2 uv_BumpMap;
	float4 color : COLOR;
	float3 lightDir ;
};

void FastSinCos (float4 val, out float4 s, out float4 c) {
	val = val * 6.408849 - 3.1415927;
	// powers for taylor series
	float4 r5 = val * val;
	float4 r6 = r5 * r5;
	float4 r7 = r6 * r5;
	float4 r8 = r6 * r5;
	
	float4 r1 = r5 * val;
	float4 r2 = r1 * r5;
	float4 r3 = r2 * r5;
	//Vectors for taylor's series expansion of sin and cos
	float4 sin7 = {1, -0.16161616, 0.0083333, -0.00019841};
	float4 cos8  = {-0.5, 0.041666666, -0.0013888889, 0.000024801587};
	// sin
	s =  val + r1 * sin7.y + r2 * sin7.z + r3 * sin7.w;
	// cos
	c = 1 + r5 * cos8.x + r6 * cos8.y + r7 * cos8.z + r8 * cos8.w;
}
float random(float2 p )
{
  // We need irrationals for pseudo randomness.
  // Most (all?) known transcendental numbers will (generally) work.
  const float2 r = float2(
    23.1406926327792690,  // e^pi (Gelfond's constant)
     2.6651441426902251); // 2^sqrt(2) (Gelfond–Schneider constant)
  return frac( cos( min( 123456789., 1e-7 + 256. * dot(p,r) ) ) );  
}
void vert (inout appdata_full v) {
	
////////// start bending
	
	float4 _waveXSizeMove = float4(0.048, 0.06, 0.24, 0.096);
	float4 _waveYSizeMove = float4 (0.036, 0.2, -0.07, 0.059);
	float4 _waveZSizeMove = float4 (0.024, .08, 0.08, 0.2);
	
	// OBSTACLE AVOIDANCE CALC
	float3 worldPos = mul((float3x4)unity_ObjectToWorld,v.vertex.xyz);
	float3 worldPos2 = mul((float3x4)unity_ObjectToWorld,float4(0,0,0,1));
float3 bendDir = normalize(float3(worldPos2.x,worldPos2.y,worldPos2.z)- float3(_Obstacle.x,_Obstacle.y,_Obstacle.z));//direction of obstacle bend
float dist =distance(float3(worldPos2.x,worldPos2.y,worldPos2.z),float3(_Obstacle.x,_Obstacle.y,_Obstacle.z));

float distMulti = distLimit-min(distLimit,dist); //distance falloff

	float4 waves = float4(0,0,0,0);

	waves.x += worldPos.x * _waveXSizeMove*windSpeed.b;
	waves.z += worldPos.z * _waveZSizeMove*windSpeed.b;
	waves.y += worldPos.y * _waveYSizeMove*windSpeed.b;

	// Add in time to model them over time
	float origLength = length(v.vertex.xyz);
	waves += (1+ windSpeed  )*(_Time.x *  windSpeed.a) ;
	float4 s, c;
	waves = frac (waves);

	FastSinCos (waves, s,c);
	float waveAmount = (v.vertex.xz+v.vertex.y) *(v.color.a) * windSpeed.r;
	// Faster winds move the grass more than slow winds 
	s = s * v.color.a * windSpeed.b*c;
	s *= waveAmount*c;
	float3 waveMove = float3 (0,0,0);
	distort = v.vertex;
	
	//distort.y += float3(0,5,0)* bendDir.y*distMulti*v.texcoord.y*v.color.a; 
	//distort.xz += bendDir.xz*distMulti*v.texcoord.xy*v.color.a;
	
	waveMove.x = dot (s, (_waveXSizeMove*WindDirection.x))+ dot((bendDir.x * distMulti),distLimit/2);
	waveMove.y = dot (s, (_waveYSizeMove*WindDirection.y));
	waveMove.z = dot (s, (_waveZSizeMove*WindDirection.z))+dot(( bendDir.z * distMulti),distLimit/2);

	
	distort.xyz += waveMove.xyz;
	
	
		
	
	v.normal = normalize(v.normal);
	v.vertex.xyz = normalize(distort.xyz) * origLength;
	
//OBSTACLE AVOIDANCE END

//ADD OBSTACLE BENDING
	
////////// end bending
}




void surf (Input IN, inout SurfaceOutputStandard o) {
	half4 c = tex2D(_MainTex, IN.uv_MainTex);
	// add terrain lighting
	o.Albedo = c.rgb * _WavingTint;
	o.Alpha = c.a;
	o.Smoothness =  _Shininess;
	o.Normal = UnpackNormal(tex2D(_MainTexNormal, IN.uv_MainTex))*_normalInten;
	clip (o.Alpha - _Cutoff);
}
ENDCG
}
Dependency "BillboardShader" = "Hidden/Nature/Tree Soft Occlusion Leaves Rendertex"
	FallBack "Diffuse"
	
}

