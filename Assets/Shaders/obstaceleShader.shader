Shader "Details/bark" {
Properties {
	_WavingTint ("Fade Color", Color) = (.7,.6,.5, 0)
	_MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
	_MainTexNormal ("Normal (RGB) Alpha (A)", 2D) = "white" {}
	//_WaveAndDistance ("Wave and distance", Vector) = (12, 3.6, 1, 1)
_normalInten ("Normal Intensity" , Range( -1 ,1 ))= 0.5
_Shininess("Shininess" , Range( -1 ,1 ))= 0.5
distLimit("size" , Range( 0 ,2 ))= 0.5
}

SubShader {
	Tags {
	"Queue" = "Geometry+200"
	"IgnoreProjector"="True"
	"RenderType"="Opaque"
	}
	LOD 200
	
CGPROGRAM
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
#pragma exclude_renderers gles
#pragma surface surf Standard fullforwardshadows vertex:vert addshadow 
	UNITY_INSTANCING_BUFFER_START(Props)
	UNITY_INSTANCING_BUFFER_END(Props)
	#pragma multi_compile_instancing
		float _Shininess;

sampler2D _MainTex,_MainTexNormal;
float _Cutoff,distLimit;
float4 windSpeed;
float4 _WavingTint;
float4 WindDirection;
half _normalInten;


float3 pos;

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
    float3 worldPos = mul (unity_ObjectToWorld,float4(0,0,0,0.1f)).xyz;


 float _WindSpeed = ( windSpeed.g  );
 float _WaveScale = windSpeed.r;

 float4 _waveXSize =float4 (0.048 , 0.06 ,0.24 , 0.096 );
 float4 _waveZSize = float4(0.024 , .08, 0.08 , 0.2 );
 float4 waveSpeed = float4(1.2 , 2, 1.6 , 4.8 );

float4 _waveXmove= float4 (0.024 , 0.04 ,- 0.12 , 0.096 );
float4 _waveZmove= float4 ( 0.006 , .02,- 0.02 , 0.1 ) ;

float4 waves;
waves = worldPos. x *_waveXSize;
waves += worldPos.z* _waveZSize;

waves += (1+ windSpeed  )*(_Time.x *  windSpeed.a);

float4 s, c;
waves = frac(waves );
FastSinCos ( waves,s,c );
float waveAmount = v.vertex .y *(windSpeed.r );

s *= waveAmount;

s *= normalize(waveSpeed );

s = s  * windSpeed.b*c;
float fade = dot ( s,1.3 );
float3 waveMove =float3 ( 0, 0, 0) ;
waveMove. x = dot(s, _waveXmove*WindDirection.x );
waveMove. z = dot(s, _waveZmove*WindDirection.z );
v. vertex. xz = lerp(v. vertex. xz,v. vertex. xz- waveMove. xz,distLimit);
}




void surf (Input IN, inout SurfaceOutputStandard o) {
	half4 c = tex2D(_MainTex, IN.uv_MainTex);
	// add terrain lighting
	o.Albedo = c.rgb * _WavingTint;
	o.Alpha = c.a;
	o.Smoothness =  c.a * _Shininess;
	o.Normal = UnpackNormal(tex2D(_MainTexNormal, IN.uv_MainTex) * _normalInten);
}
ENDCG
}
	FallBack "Diffuse"
	
}
