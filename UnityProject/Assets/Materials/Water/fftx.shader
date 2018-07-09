Shader "CustomRenderTexture/fftx" {
	Properties {
		_imgSampler("Image Sampler", 3D) = "" {}
		_butterflySampler("Butterfly Sampler", 2D) = "white" {}
	}

	SubShader {
		Lighting Off
		Blend One Zero

		Pass
		{
			CGPROGRAM
			#include "UnityCustomRenderTexture.cginc"
			#pragma vertex CustomRenderTextureVertexShader
			#pragma fragment frag
			#pragma target 5.0
			#pragma require 2darray

			sampler3D _imgSampler;
			sampler2D _butterflySampler;
			float _pass;

			float4 fft2(int layer, float2 i, float2 w, float2 uv)
			{
				float4 input1 = tex3D(_imgSampler, float3(i.x, uv.y, layer));
				float4 input2 = tex3D(_imgSampler, float3(i.y, uv.y, layer));

				float res1x = w.x * input2.x - w.y * input2.y;
				float res1y = w.y * input2.x + w.x * input2.y;
				float res2x = w.x * input2.z - w.y * input2.w;
				float res2y = w.y * input2.z + w.x * input2.w;
				return input1 + float4(res1x, res1y, res2x, res2y);
			}

			float4 frag(v2f_init_customrendertexture IN) : COLOR
			{
				float4 data = tex2D(_butterflySampler, float2(IN.texcoord.x, _pass));
				float2 i = data.xy;
				float2 w = data.zw;

				return fft2(_CustomRenderTexture3DSlice, i, w, IN.texcoord.xy);
			}
			ENDCG
		}
	}
}
