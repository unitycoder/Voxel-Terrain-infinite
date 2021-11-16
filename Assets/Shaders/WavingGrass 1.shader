// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Details/WavingPlants" {
	Properties{
		_MainTex("Base (RGB) Alpha (A)", 2D) = "white" {}
	_SnowTex("Base (RGB) Alpha (A)", 2D) = "white" {}
	//_WaveAndDistance ("Wave and distance", Vector) = (12, 3.6, 1, 1)
_Cutoff("Alphacutoff" , Range(0.01 ,1)) = 0.5
HasSnow("Has Snow" , Range(0 ,2)) = 0.5
distLimit("size" , Range(0 ,4)) = 0.5
_Color("Color", Color) = (0,0,255,0)
	}

		SubShader{
			Tags {
			"Queue" = "Geometry+10"
			"IgnoreProjector" = "True"
			"RenderType" = "Grass"
			}
			LOD 200
			   Cull Off

		CGPROGRAM
	// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
	#pragma exclude_renderers gles
	// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
	#pragma surface surf Standard fullforwardshadows alphatest:_Cutoff vertex:vert 
	UNITY_INSTANCING_BUFFER_START(Props)
	UNITY_INSTANCING_BUFFER_END(Props)
	#pragma multi_compile_instancing
			float _Shininess;

	sampler2D _MainTex, _SnowTex;
	half4 windSpeed;
	half4 WindDirection;
	half _normalInten;
	half3 distort;
	half4 _Color;
	half Distance = 50;
	half distLimit = 0.5;
	half HasSnow;
	uniform float4 _Obstacle;



	struct Input {
		float2 uv_MainTex;
		float2 uv_BumpMap;
		float4 color : COLOR;
		float3 lightDir;
		float3 worldPos;
	};

	void FastSinCos(half4 val, out half4 s, out half4 c) {
		val = val * 6.408849 - 3.1415927;
		// powers for taylor series
		half4 r5 = val * val;
		half4 r6 = r5 * r5;
		half4 r7 = r6 * r5;
		half4 r8 = r6 * r5;
		half4 r1 = r5 * val;
		half4 r2 = r1 * r5;
		half4 r3 = r2 * r5;
		//Vectors for taylor's series expansion of sin and cos
		half4 sin7 = {1, -0.16161616, 0.0083333, -0.00019841};
		half4 cos8 = {-0.5, 0.041666666, -0.0013888889, 0.000024801587};
		// sin
		s = val + r1 * sin7.y + r2 * sin7.z + r3 * sin7.w;
		// cos
		c = 1 + r5 * cos8.x + r6 * cos8.y + r7 * cos8.z + r8 * cos8.w;
	}


	void vert(inout appdata_full v) {

		////////// start bending
		half factor = (1 - 0.5f - v.color.r) * 0.5;
		half3 worldPos = mul(unity_ObjectToWorld, float4(0,0,0,0.1f)).xyz;


		half _WindSpeed = (windSpeed.g );
		half _WaveScale = windSpeed.r;

		half4 _waveXSize = float4 (0.048, 0.06, 0.24, 0.096);
		half4 _waveZSize = float4(0.024, .08, 0.08, 0.2);
		half4 waveSpeed = float4(1.2, 2, 1.6, 4.8);

		half4 _waveXmove = float4 (0.024, 0.04, -0.12, 0.096);
		half4 _waveZmove = float4 (0.006, .02, -0.02, 0.1);

		half4 waves;
		waves = worldPos.x * _waveXSize;
		waves += worldPos.z * _waveZSize;

		waves += (1 + windSpeed) * (_Time.x * windSpeed.a);

		half4 s, c;
		waves = frac(waves);
		FastSinCos(waves, s, c);
		half waveAmount = v.vertex.y * (windSpeed.r);

		s *= waveAmount;

		s *= normalize(waveSpeed);

		s = s * windSpeed.b * c;
		half fade = dot(s, 1.3);
		half3 waveMove = half3 (0, 0, 0);
		waveMove.x = dot(s, _waveXmove * WindDirection.x);
		waveMove.z = dot(s, _waveZmove * WindDirection.z);






		float3 worldPos2 = mul((float3x4)unity_ObjectToWorld, float4(0,0,0,1));


		float3 bendDir = normalize(float3(worldPos2.x, worldPos2.y, worldPos2.z) - float3(_Obstacle.x, _Obstacle.y, _Obstacle.z));//direction of obstacle bend
		float dist = distance(float3(worldPos2.x, worldPos2.y, worldPos2.z), float3(_Obstacle.x, _Obstacle.y, _Obstacle.z));
		float distMulti = distLimit - min(distLimit, dist); //distance falloff
		



		// Add in time to model them over time
		float origLength = length(v.vertex.xyz);

		distort = v.vertex;

		distort.y += float3(0, 5, 0) * bendDir.y * distMulti * v.texcoord.y * v.color.a;
		distort.xz += bendDir.xz * distMulti * v.texcoord.y * v.color.a;




		distort.xyz += waveMove.xyz;


		v.normal = normalize(v.normal);
		v.vertex.xyz = normalize(distort.xyz) * origLength;
	}

	float4 blend(float4 texture1, float a1, float4 texture2, float a2)
	{

		float depth = 0.01f;
		float ma = max(texture1.a * a1, texture2.a * a2) - depth;

		float b1 = max(texture1.a * a1 - ma, 0);
		float b2 = max(texture2.a * a2 - ma, 0);
		return ((texture1.rgba * b1) + (texture2.rgba * b2)) / (b1 + b2);
	}


	void surf(Input IN, inout SurfaceOutputStandard o) {
		half4 c = tex2D(_MainTex, IN.uv_MainTex);
		// add terrain lighting
		if (HasSnow > 0) {
			float4 snow = float4(1,1,1,1);
			o.Albedo = blend(snow, snow.r, c * _Color, c.a);
		}
		else o.Albedo = c.rgb * _Color.rgb;
		o.Alpha = c.a;
		half mydist = (Distance - Distance / 4) - distance(IN.worldPos,_WorldSpaceCameraPos);
		mydist = clamp(mydist,-1,(o.Alpha - 0.2f));
		o.Alpha = mydist;
	}
	ENDCG

		


}
}

