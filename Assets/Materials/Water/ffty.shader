Shader "CustomRenderTexture/ffty" {
	Properties{
		_imgSampler("Image Sampler", 2DArray) = "" {}
		_butterflySampler("Butterfly Sampler", 2D) = "white" {}
	}
	
	CGINCLUDE
	#include "UnityCustomRenderTexture.cginc"
	ENDCG

	SubShader{

		Cull Off ZWrite Off ZTest Always

		Pass
		{			
			CGPROGRAM
			#pragma target 5.0
			#pragma require 2darray
			#pragma vertex CustomRenderTextureVertexShader
			#pragma fragment frag


			UNITY_DECLARE_TEX2DARRAY(_imgSampler);
			sampler2D _butterflySampler;
			float _pass;

			float4 fft2(int layer, float2 i, float2 w, float2 uv)
			{
				float4 input1 = UNITY_SAMPLE_TEX2DARRAY(_imgSampler, float3(uv.x, i.x, layer));
				float4 input2 = UNITY_SAMPLE_TEX2DARRAY(_imgSampler, float3(uv.x, i.y, layer));

				float res1x = w.x * input2.x - w.y * input2.y;
				float res1y = w.y * input2.x + w.x * input2.y;
				float res2x = w.x * input2.z - w.y * input2.w;
				float res2y = w.y * input2.z + w.x * input2.w;
				return input1 + float4(res1x, res1y, res2x, res2y);
			}

			float4 frag(v2f_init_customrendertexture IN) : SV_TARGET
			{
				/*
				float4 data = tex2D(_butterflySampler, float2(IN.texcoord.y, _pass));
				float2 i = data.xy;
				float2 w = data.zw;
				*/
				return float4(0, IN.texcoord.y, _CustomRenderTexture3DSlice, 1.0);
				//return fft2(_CustomRenderTexture3DSlice, i, w, IN.texcoord.xy);
			}
			ENDCG
		}
	}
}
