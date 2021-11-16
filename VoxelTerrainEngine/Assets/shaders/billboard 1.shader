// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Transparent/Billboard1"
{
   Properties {
     _MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
   }
   SubShader {

        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "DisableBatching"="True"}
        Lighting On Fog { Mode Off }
        Cull Back
        //Blend SrcAlpha One //additive blend
        //Blend SrcAlpha OneMinusSrcAlpha
        LOD 200
 
         CGPROGRAM
#pragma surface surf Standard vertex:vert 
         uniform sampler2D _MainTex;   

                  
struct Input {
	float2 uv_MainTex;
};

         void vert (inout appdata_full v)
         {
            float3 objSpaceCamPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos.xyz, 1)).xyz;
            float3 offsetDir = normalize(cross(float3(0.0f, 1.0f, 0.0f), objSpaceCamPos));
            float4 myvert =  v.vertex;
            myvert.xz = offsetDir.xz * v.vertex.x;
           myvert.yw =  v.vertex.yw;
            myvert = UnityObjectToClipPos(myvert);
             v.vertex = myvert;
         }
void surf (Input IN, inout SurfaceOutputStandard o) {
            float4 tex = tex2D(_MainTex, IN.uv_MainTex);   
         clip(tex.a-0.01f);
            tex*= (unity_LightColor[0]+0.025f);
            o.Albedo = tex.rgb;
         }
       ENDCG
   }
}