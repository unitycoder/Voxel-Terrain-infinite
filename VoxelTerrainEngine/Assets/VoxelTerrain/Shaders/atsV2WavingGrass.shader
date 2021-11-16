// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Details/WavingleavesCull" {
	Properties{
		_WavingTint("Fade Color", Color) = (.7,.6,.5, 0)
		_MainTex("Base (RGB) Alpha (A)", 2D) = "white" {}
		_SnowTex("Base (RGB) Alpha (A)", 2D) = "white" {}
		HasSnow("Has Snow" , Range(0 ,2)) = 0.5
			//_WaveAndDistance ("Wave and distance", Vector) = (12, 3.6, 1, 1)
		_Cutoff("Alphacutoff" , Range(0 ,1)) = 0.5
		_Shininess("Shininess" , Range(-1 ,1)) = 0.5
		distLimit("size" , Range(0 ,2)) = 0.5
	}

		SubShader{
			Tags {
			"Queue" = "Geometry+10"
			"IgnoreProjector" = "True"
			"RenderType" = "Grass"
			}
			LOD 200


		CGPROGRAM
			// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
			#pragma exclude_renderers gles
			#pragma surface surf Standard vertex:vert addshadow alphatest:_Cutoff
				UNITY_INSTANCING_BUFFER_START(Props)
	UNITY_INSTANCING_BUFFER_END(Props)
			#pragma multi_compile_instancing
					float _Shininess;

			sampler2D _MainTex,_MainTexNormal,_SnowTex;
			float4 windSpeed;
			float4 _WavingTint;
			float4 WindDirection;
			half _normalInten;
			float3 distort;
			float distLimit = 0.80;

			float3 pos;
			uniform float4 _Obstacle;

			float _speed,HasSnow;


			struct Input {
				float2 uv_MainTex;
				float2 uv_BumpMap;
				float4 color : COLOR;
				float3 lightDir;
			};
				float4 blend(float4 texture1, float a1, float4 texture2, float a2)
					{

					float depth = 0.01f;
					float ma = max(texture1.a * a1, texture2.a * a2) - depth;

					float b1 = max(texture1.a * a1 - ma, 0);
					float b2 = max(texture2.a * a2 - ma, 0);
							return ((texture1.rgba * b1) + (texture2.rgba * b2)) / (b1 + b2);
					}

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
				float factor = (1 - 0.5f - v.color.r) * 0.5;
				float3 worldPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 0.1f)).xyz;
				float3 worldPos2 = mul(unity_ObjectToWorld, v.vertex).xyz;

				float _WindSpeed = (windSpeed.g );
				float _WaveScale = windSpeed.r;

				float4 _waveXSize = float4 (0.048, 0.06, 0.24, 0.096);
				float4 _waveZSize = float4(0.024, .08, 0.08, 0.2);
				float4 waveSpeed = float4(1.2, 2, 1.6, 4.8);

				float4 _waveXmove = float4 (0.024, 0.04, -0.12, 0.096);
				float4 _waveZmove = float4 (0.006, .02, -0.02, 0.1);

				float4 waves;
				waves = worldPos.x * _waveXSize;
				waves += worldPos.z * _waveZSize;
				waves.xyz += worldPos2 * windSpeed.g;
				waves += (1 + windSpeed) * (_Time.x * windSpeed.a);

				float4 s, c;
				waves = frac(waves);
				FastSinCos(waves, s, c);
				float waveAmount = v.vertex.y * (windSpeed.r);

				s *= waveAmount;

				s *= normalize(waveSpeed);

				s = s * windSpeed.b * c;
				float fade = dot(s, 1.3);
				float3 waveMove = float3 (0, 0, 0);
				waveMove.x = dot(s, _waveXmove * WindDirection.x);
				waveMove.z = dot(s, _waveZmove * WindDirection.z);
				v.vertex.xz = lerp(v.vertex.xz, v.vertex.xz - waveMove.xz, 0.5f);
			}




			void surf(Input IN, inout SurfaceOutputStandard o) {
				half4 c = tex2D(_MainTex, IN.uv_MainTex);
				// add terrain lighting
				//half3 N = UnpackNormal(tex2D(_MainTexNormal, IN.uv_MainTex));
				//o.Normal = N;
				o.Alpha = c.a;
				o.Smoothness = c.a * _Shininess;
				if (HasSnow > 0) {
				float4 snow = tex2D(_SnowTex, IN.uv_MainTex);
				o.Albedo = blend(snow,snow.r,c * _WavingTint, c.a);
				}
			else o.Albedo = c.rgb * _WavingTint;

	   }
	   ENDCG
		}
		   FallBack "Diffuse"

}