// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/Terrain" 
{
	Properties 

	{	
    _MainTex("main", 2DArray) = "white" {}
    [Normal]_MainTexNorm("main Normal", 2DArray) = "bump" {}

    _Color ("Color", Color) = (1,1,1,1)
    _Raining("Raining", 2D) = "white" {}
    normalIntensity ("normal Intensity" , Range( -1.0 ,2 ))= 0.5
    shimmerIntensity("shimmer Intensity" , Range(-1.0 ,5)) = 0.5
    HeightIntensity ("TriBlending Intensity" , Range( 0.0 ,18 ))= 0.5
    blendingIntensity ("blending Intensity" , Range( 0.0 ,0.4 ))= 0.1
    BaseMapDistance("BaseMap Distance" , Range(0.0 ,500.0)) = 0.0
    DebugC("Debug Color" , Range(0.0 ,1.0)) = 0.1
        DebugU("Debug UVs" , Range(0.0 ,1.0)) = 0.1
	}
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		//Cull off
			

		CGPROGRAM
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.5
        #include "UnityStandardUtils.cginc"
        #include "UnityCG.cginc"
		UNITY_DECLARE_TEX2DARRAY( _MainTex);
        UNITY_DECLARE_TEX2DARRAY( _MainTexNorm);
        float normalIntensity,HeightIntensity,blendingIntensity, shimmerIntensity, DebugC, DebugU, BaseMapDistance;
        half4 blend(half4 texture1, half a1, half4 texture2, half a2)
		{
	
    	half depth = blendingIntensity;
    	half ma = max(texture1.a * a1, texture2.a * a2) - depth;

    	half b1 = max(texture1.a * a1 - ma, 0);
    	half b2 = max(texture2.a * a2 - ma, 0);
    	    	return ((texture1.rgba * b1) + (texture2.rgba * b2)) / (b1 + b2);
		}

		
		struct Input 
		{   
        	float2 uv_MainTex;
			float4 color : COLOR;
            float3 worldPos;
            float3 worldNormal;
            INTERNAL_DATA
		};


        half4 TriplanarSample(int tex, half3 worldPosition, half3 projNormal, half scale)
        {
            half4 worldpos = 0;
            worldpos.xyz = worldPosition.xyz;

            worldpos.w = tex;





            worldpos.zy *= scale;
            float3 blendWeight = pow(saturate(abs(projNormal)), HeightIntensity);
            blendWeight /= (blendWeight.x + blendWeight.y + blendWeight.z);
            half4 X = 0;
            if(blendWeight.x>0)
            X = UNITY_SAMPLE_TEX2DARRAY(_MainTex, (worldpos.zyw));
            worldpos.xyz = worldPosition.xyz;
            worldpos.xz *= scale;
            half4 Y = 0;
            if (blendWeight.y > 0)
                Y = UNITY_SAMPLE_TEX2DARRAY(_MainTex, (worldpos.xzw));
            worldpos.xyz = worldPosition.xyz;
            worldpos.xy *= scale;
            half4 Z = 0;
            if (blendWeight.z > 0)
                Z = UNITY_SAMPLE_TEX2DARRAY(_MainTex, (worldpos.xyw));
            

            return X * blendWeight.x + Y * blendWeight.y + Z * blendWeight.z;

        }
        half4 TriplanarSampleTex(sampler2D tex, half3 worldPosition, half3 projNormal, half scale)
        {
            half4 worldpos = 0;
            worldpos.xyz = worldPosition.xyz;






            worldpos.xy *= scale;
            half4 XY = tex2D(tex, (worldpos.xy));
            worldpos.xyz = worldPosition.xyz;
            worldpos.zy *= scale;
            half4 ZY = tex2D(tex, (worldpos.zy));
            worldpos.xyz = worldPosition.xyz;
            worldpos.zx *= scale;
            half4 ZX = tex2D(tex, (worldpos.zx));

            XY = lerp(XY, ZY, projNormal.x);
            return lerp(XY, ZX, projNormal.y);

        }

        float3 BlendTriplanarNormal(float3 mappedNormal, float3 surfaceNormal) {
            float3 n;
            n.xy = mappedNormal.xy + surfaceNormal.xy;
            n.z = mappedNormal.z * surfaceNormal.z;
            return n;
        }
        half3 blend_rnm(half3 n1, half3 n2)
        {
            n1.z += 1;
            n2.xy = -n2.xy;

            return n1 * dot(n1, n2) / n1.z - n2;
        }
          half3 TriplanarNormal(int tex, half3 worldPosition, half3 projNormal, half scale)
        {
              half4 worldpos = 0;
              worldpos.xyz = worldPosition.xyz;

              worldpos.w = tex;


              half3 absVertNormal = abs(projNormal);
              float3 blendWeight = pow(saturate(abs(projNormal)), HeightIntensity);

              blendWeight /= (blendWeight.x + blendWeight.y + blendWeight.z);
              half3 X = 0;
              worldpos.zy *= scale;
              if(blendWeight.x>0)
              X = UnpackNormal(UNITY_SAMPLE_TEX2DARRAY(_MainTexNorm, (worldpos.zyw)));
              worldpos.xyz = worldPosition.xyz;
              worldpos.xz *= scale;
              half3 Y = 0;
              if (blendWeight.y > 0)
                  Y = UnpackNormal(UNITY_SAMPLE_TEX2DARRAY(_MainTexNorm, (worldpos.xzw))) ;
              worldpos.xyz = worldPosition.xyz;
              worldpos.xy *= scale;
              half3 Z = 0;
              if (blendWeight.z > 0)
              Z = UnpackNormal(UNITY_SAMPLE_TEX2DARRAY(_MainTexNorm, (worldpos.xyw)));

              
              // Flip tangent normal z to account for surface normal facing
              
              // Swizzle world normals to match tangent space and apply RNM blend
              X = blend_rnm(half3(projNormal.zy, absVertNormal.x), X );
              Y = blend_rnm(half3(projNormal.xz, absVertNormal.y), Y );
              Z = blend_rnm(half3(projNormal.xy, absVertNormal.z), Z );
              // Get the sign (-1 or 1) of the surface normal
              half3 axisSign = projNormal < 0 ? -1 : 1;

              X.z *= axisSign.x;

              Y.z *= axisSign.y;
                 
              Z.z *= axisSign.z;



              return normalize((X.zyx * blendWeight.x + Y.xzy * blendWeight.y + Z * blendWeight.z ) );

        }
          float3 WorldToTangentNormalVector(Input IN, float3 normal) {
              float3 t2w0 = WorldNormalVector(IN, float3(1, 0, 0));
              float3 t2w1 = WorldNormalVector(IN, float3(0, 1, 0));
              float3 t2w2 = WorldNormalVector(IN, float3(0, 0, 1));
              float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
              return normalize(mul(t2w, normal) );
          }

       
		void surf(Input IN, inout SurfaceOutputStandard o) 
		{	

            half tile = 1.0f;
			half4 controlmap = IN.color;
			half2 Tex = IN.uv_MainTex;
            Tex.y = floor(Tex.y);
            IN.worldNormal = WorldNormalVector(IN, float3(0, 0, 1));
            float3 projNormal = IN.worldNormal;
            half4 col = 0;
            half4 glow = 0;
            half3 worldpos = IN.worldPos;
            if (distance(_WorldSpaceCameraPos, worldpos) > BaseMapDistance)
                tile = 0.1f;
            half4 m_normal = 0;
            half3 colN = 0;
            float m = 0;
            if (Tex.x <= 0.0f) {
            if(controlmap.r<=1 && controlmap.r>0){
            
            half4 rock = TriplanarSample(0, worldpos, projNormal, tile * 0.1f);
            
            half3 rockN = TriplanarNormal(0, worldpos, projNormal, tile * 0.1f);
            colN = blend(half4(colN, col.a), col.a, half4(rockN.xyz, rock.a), controlmap.r).xyz;
            col = blend(col, col.a, rock, controlmap.r);
            m = lerp(m, 0, controlmap.r);
            }
           
            if (controlmap.g <= 1 && controlmap.g > 0) {
                half4 gravel = TriplanarSample(1, worldpos, projNormal, tile);
                half3 gravelN = TriplanarNormal(1, worldpos, projNormal, tile);
                colN = blend(half4(colN, col.a), col.a, half4(gravelN, gravel.a), controlmap.g).xyz;
                col = blend(col, col.a, gravel, controlmap.g);

                m = lerp(m, 0, controlmap.g);
            }
           
			
            if(controlmap.a<=1 && controlmap.a>0){
            half4 sand = TriplanarSample(2, worldpos, projNormal, tile);
            half3 sandN = TriplanarNormal(2, worldpos, projNormal, tile);
            colN = blend(half4(colN, col.a), col.a, half4(sandN, sand.a), controlmap.a).xyz;
            col = blend(col,col.a,sand,controlmap.a);
            m = lerp(m, 0, controlmap.a);
            
            }
            
            
            if(controlmap.b<=1 && controlmap.b>0){
            if(Tex.y<=0.0f){
            half4 grass = TriplanarSample(3, worldpos, projNormal, tile);
            half3 grassN = TriplanarNormal(3, worldpos, projNormal, tile);
            colN = blend(half4(colN, col.a), col.a, half4(grassN, grass.a), controlmap.b).xyz;
            col = blend(col,col.a,grass,controlmap.b);
            
            m = lerp(m, 0, controlmap.b);

            }
            if(Tex.y>=1.0f && Tex.y<=1.0f ){

            half4 forest = TriplanarSample(4, worldpos, projNormal, tile);
            half3 forestN = TriplanarNormal(4, worldpos, projNormal, tile);
            colN = blend(half4(colN, col.a), col.a, half4(forestN, forest.a), controlmap.b).xyz;
            col = blend(col,col.a,forest,controlmap.b);
            m = lerp(m, 0, controlmap.b);
            }
            if(Tex.y>=2.0f && Tex.y<=2.0f){

            half4 Tropics = TriplanarSample(5, worldpos, projNormal, tile);
            half3 TropicsN = TriplanarNormal(5, worldpos, projNormal, tile);
            colN = blend(half4(colN, col.a), col.a, half4(TropicsN, Tropics.a), controlmap.b).xyz;
            col = blend(col,col.a,Tropics,controlmap.b);
            m = lerp(m, 0, controlmap.b);
            }

            if(Tex.y>=3.0f && Tex.y<=3.0f){
            half4 snow = TriplanarSample(6, worldpos, projNormal, tile);
            half3 snowN = TriplanarNormal(6, worldpos, projNormal, tile);
            colN = blend(half4(colN, col.a), col.a, half4(snowN, snow.a), controlmap.b).xyz;
            col = blend(col,col.a,snow,controlmap.b);
            m = lerp(m, 0, controlmap.b);


            }
            if(Tex.y>=4.0f && Tex.y<=4.0f){
            half4 sand = TriplanarSample(7, worldpos, projNormal, tile);
            half3 sandN = TriplanarNormal(7, worldpos, projNormal, tile);
            colN = blend(half4(colN, col.a), col.a, half4(sandN, sand.a), controlmap.b).xyz;
            col = blend(col,col.a,sand,controlmap.b);
            m = lerp(m, 0, controlmap.b);
            }
            if (Tex.y >= 5.0f && Tex.y <= 5.1f) {
                half3 sandN = TriplanarNormal(8, worldpos + float3(_Time.x, _Time.x, _Time.x)* projNormal/16, projNormal, tile * 0.1f);
                //half4 Tex = TriplanarSampleTex(_Raining, worldpos + (float3(_Time.x, _Time.x, _Time.x) * projNormal), projNormal, 0.5f);
                half d = sandN.r * 1.0f;
                half3 p = worldpos.xyz;
                p.xyz += d ;
                half4 sand = TriplanarSample(8, p  + float3(_Time.x, _Time.x, _Time.x), projNormal, tile * 0.1f);
		half4 Rock = TriplanarSample(0, worldpos, projNormal, tile * 0.1f);
		half3 RockN = TriplanarNormal(0, worldpos, projNormal, tile * 0.1f);
                //colN = blend(half4(colN, col.a), col.a, half4(sandN, sand.a), controlmap.b).xyz;

		
                col = blend(col, col.a/4, sand, controlmap.b/4);
		col = blend(col, col.a/4, Rock ,controlmap.b);
		colN = blend(half4(colN, col.a), col.a, half4(RockN, Rock.a), controlmap.b).xyz; 
                glow =col - col.a;
		m = lerp(m, 0, controlmap.b);
                
            }
            }
          
           

            
			



			}
          
            if (Tex.x > 0.00f && Tex.x <= 1.00f) {
                ///////////////////////////////////////////////
                if (controlmap.r <= 1 && controlmap.r > 0.01f) {
                    half4 Iron = TriplanarSample(9, worldpos, projNormal, tile * 0.1f);
                    half3 IronN = TriplanarNormal(9, worldpos, projNormal, tile * 0.1f);
                    colN = blend(half4(colN, col.a), col.a, half4(IronN, Iron.a), controlmap.r * Tex.x).xyz;
                    col = blend(col, col.a, Iron, controlmap.r * Tex.x);
                    m = lerp(m, 1, controlmap.r);
                }

                if (controlmap.g <= 1 && controlmap.g > 0.01f) {
                    half4 Gold = TriplanarSample(10, worldpos, projNormal, tile * 0.1f);

                    half3 GoldN = TriplanarNormal(10, worldpos, projNormal, tile * 0.1f);
                    colN = blend(half4(colN, col.a), col.a, half4(GoldN, Gold.a), controlmap.g * Tex.x).xyz;
                    col = blend(col, col.a, Gold, controlmap.g * Tex.x);
                    m = lerp(m, 0, controlmap.g);
                }
                if (controlmap.b <= 1 && controlmap.b > 0.01f) {
                    half4 GunPowder = TriplanarSample(11, worldpos, projNormal, tile * 0.1f);

                    half3 GunPowderN = TriplanarNormal(11, worldpos, projNormal, tile * 0.1f);
                    colN = blend(half4(colN, col.a), col.a, half4(GunPowderN, GunPowder.a), controlmap.b * Tex.x).xyz;

                    col = blend(col, col.a, GunPowder, controlmap.b * Tex.x);
                    m = lerp(m, 0, controlmap.b);
                }

                if (controlmap.a <= 1 && controlmap.a > 0.01f) {
                    half4 Tungsten = TriplanarSample(12, worldpos, projNormal, tile * 0.1f);

                    half3 TungstenN = TriplanarNormal(12, worldpos, projNormal, tile * 0.1f);
                    colN = blend(half4(colN, col.a), col.a, half4(TungstenN, Tungsten.a), controlmap.a * Tex.x).xyz;
                    col = blend(col, col.a, Tungsten, controlmap.a * Tex.x);
                    m = lerp(m, 1, controlmap.a);

                }
            }
            if(controlmap.r<=0.0f && controlmap.g <=0.0f && controlmap.b <=0.0f && controlmap.a <=0.0f && Tex.x >=1.8f){
                half4 BedRock = TriplanarSample(13,worldpos, projNormal, tile * 0.1f);
                m = lerp(m, 0, 0.25f);
            col = BedRock;
            half3 BedRockN = TriplanarNormal(13, worldpos, projNormal, tile * 0.1f);
            colN = BedRockN;

            }
            
            //half4 BedRock = TriplanarSampleT( worldpos, projNormal, 1);
            //col = BedRock;
            //float4 BedRockN = TriplanarNormalT( worldpos, projNormal, 1);
            //colN = BedRockN;
            
            if (DebugC == 0 && DebugU==0)
                o.Albedo = col;
            else if (DebugC > 0)
                o.Albedo = controlmap;
            else if(DebugU>0)
                o.Albedo = float4(Tex.x,0, Tex.y,0);
            if (DebugC == 0 && DebugU == 0)
            o.Smoothness = col.a * shimmerIntensity;
            if(Tex.y>=4.5f && Tex.y<=5.5f){
if(glow.r>0.0f||glow.g>0.0f||glow.b>0.0f||glow.a>0.0f)
            o.Emission = glow;
            }
            //_OffSet is usually 2 - 8
            if (DebugC == 0 && DebugU == 0)
            o.Metallic = col.a* m;
            if (DebugC == 0 && DebugU == 0)
            o.Normal = (WorldToTangentNormalVector(IN, colN) );
            //o.Normal = (colN.xyz * normalIntensity);
           // o.Normal = float3(o.Normal.x,o.Normal.y,o.Normal.z);

            
			
		}
		ENDCG
	} 
	Fallback "VertexLit"

}
