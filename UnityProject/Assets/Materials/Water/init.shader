Shader "CustomRenderTexture/init" {
	Properties{
		_spectrum("Spectrum", 2DArray) = "white" {}

	}
		SubShader{
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

		UNITY_DECLARE_TEX2DARRAY(_spectrum);
	int FFT_SIZE;
	float4 INVERSE_GRID_SIZES;


	float2 getSpectrum(float k, float2 s0, float2 s0c) {
		float w = sqrt(9.81 * k * (1.0 + k * k / (370.0 * 370.0)));
		float c = cos(w * _Time);
		float s = sin(w * _Time);
		return float2((s0.x + s0c.x) * c - (s0.y + s0c.y) * s, (s0.x - s0c.x) * s + (s0.y - s0c.y) * c);
	}

	float2 i(float2 z) {
		return float2(-z.y, z.x); // returns i times z (complex number)
	}


	float4 frag(v2f_init_customrendertexture IN) : COLOR
	{
		float2 uv = IN.texcoord.xy;
		float2 st = float2( floor(1.0f * uv.x * FFT_SIZE) / FFT_SIZE, floor(1.0f* uv.y * FFT_SIZE) / FFT_SIZE);
		float x = uv.x > 0.5f ? st.x - 1.0f : st.x;
		float y = uv.y > 0.5f ? st.y - 1.0f : st.y;

		float4 s12 = UNITY_SAMPLE_TEX2DARRAY(_spectrum, float3(uv.x, uv.y, 0.0f));
		float4 s34 = UNITY_SAMPLE_TEX2DARRAY(_spectrum, float3(uv.x, uv.y, 1.0f));

		float2 tmp = float2(1.0f + 0.5f / FFT_SIZE, 1.0f + 0.5f / FFT_SIZE) - st;
		float4 s12c = UNITY_SAMPLE_TEX2DARRAY(_spectrum, float3(tmp.x, tmp.y, 0.0f));
		float4 s34c = UNITY_SAMPLE_TEX2DARRAY(_spectrum, float3(tmp.x, tmp.y, 1.0f));

		float2 k1 = float2(x, y) * INVERSE_GRID_SIZES.x;
		float2 k2 = float2(x, y) * INVERSE_GRID_SIZES.y;
		float2 k3 = float2(x, y) * INVERSE_GRID_SIZES.z;
		float2 k4 = float2(x, y) * INVERSE_GRID_SIZES.w;

		float K1 = length(k1);
		float K2 = length(k2);
		float K3 = length(k3);
		float K4 = length(k4);

		float IK1 = K1 == 0.0 ? 0.0 : 1.0 / K1;
		float IK2 = K2 == 0.0 ? 0.0 : 1.0 / K2;
		float IK3 = K3 == 0.0 ? 0.0 : 1.0 / K3;
		float IK4 = K4 == 0.0 ? 0.0 : 1.0 / K4;

		float2 h1 = getSpectrum(K1, s12.xy, s12c.xy);
		float2 h2 = getSpectrum(K2, s12.zw, s12c.zw);
		float2 h3 = getSpectrum(K3, s34.xy, s34c.xy);
		float2 h4 = getSpectrum(K4, s34.zw, s34c.zw);

		if (_CustomRenderTexture3DSlice == 0) {
			return float4(h1 + i(h2), h3 + i(h4));
		}
		else if (_CustomRenderTexture3DSlice == 1) {
			return float4(i(k1.x * h1) - k1.y * h1, i(k2.x * h2) - k2.y * h2);
		}
		else if (_CustomRenderTexture3DSlice == 2) {
			return float4(i(k3.x * h3) - k3.y * h3, i(k4.x * h4) - k4.y * h4);
		}
		else if (_CustomRenderTexture3DSlice == 3) {
			return float4(i(k1.x * h1) - k1.y * h1, i(k2.x * h2) - k2.y * h2) * float4(IK1, IK1, IK2, IK2);
		}
		else if (_CustomRenderTexture3DSlice == 4) {
			return float4(i(k3.x * h3) - k3.y * h3, i(k4.x * h4) - k4.y * h4) * float4(IK3, IK3, IK4, IK4);
		}
		else return float4(0,0,0,0);
			
		}
		ENDCG
	}
	}
}
