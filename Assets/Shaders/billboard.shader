// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Transparent/Billboard"
{
   Properties {
      _MainTex ("Texture Image", 2D) = "white" {}
   }
   SubShader {

        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Opaque" "DisableBatching"="True"}
        Lighting On Fog { Mode Exp }
        Cull Back
        //Blend SrcAlpha One //additive blend
        //Blend SrcAlpha OneMinusSrcAlpha
        LOD 200
 
      Pass {
         CGPROGRAM
         #pragma vertex vert
         #pragma fragment frag
     #pragma fragmentoption ARB_precision_hint_fastest
         uniform sampler2D _MainTex;      
         struct vertexInput {
            float4 vertex : POSITION;
            float2 tex : TEXCOORD0;
         };
         struct vertexOutput {
            float4 pos : SV_POSITION;
            float2 tex : TEXCOORD0;
         };
         vertexOutput vert(vertexInput i)
         {
            vertexOutput o;
            float3 objSpaceCamPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos.xyz, 1)).xyz;
            float3 offsetDir = normalize(cross(float3(0.0f, 1.0f, 0.0f), objSpaceCamPos));
            o.pos.xz = i.vertex.x * offsetDir.xz;
            o.pos.yw = i.vertex.yw;
 
            o.pos = UnityObjectToClipPos(o.pos);
            o.tex = i.tex;
            return o;
         }
         float4 frag(vertexOutput input) : COLOR
         {
            float4 tex = tex2D(_MainTex, float2(input.tex.xy));   
         	clip(tex.a-0.01f);
            return tex*(unity_LightColor[0]+0.025f);
         }
         ENDCG
      }
   }
}