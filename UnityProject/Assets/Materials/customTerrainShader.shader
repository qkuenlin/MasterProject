Shader "Custom/customTerrainShader_hlsl" {
	Properties{
		_ClassesDGR("Segmentation Image Dirt/Gravel/Rock", 2D) = "white" {}
		_ClassesWSGT("Segmentation Image Water/Snow/Grass/Forest", 2D) = "white" {}
		_Sat("Satellite Image", 2D) = "white" {}
		_ShaderTest("Shader Test", Range(0, 1)) = 0
		_Color("Color", Color) = (1, 1, 1, 1)
		_Roughness("Roughness", Range(0, 1)) = 0.5
		_GlobalIllumination("Global Illumination", Range(0,1)) = 1
		_Specular("Specular", Range(0, 1)) = 1
		_LightSampleCount("Light Sample Count for GI", Int) = 16
	}
	SubShader
	{
		Pass
		{
			Tags { "LightMode" = "ForwardBase" }
			LOD 200
			CGPROGRAM
			#include "UnityImageBasedLighting.cginc"
			#include "AutoLight.cginc"
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityStandardBRDF.cginc"
			#include "UnityGlobalIllumination.cginc"
			#include "UnityStandardConfig.cginc"

			#define M_1_PI 0.318309886183706f
			#define M_PI   3.141592653589793f
			#define M_2PI  6.283185307179586f
			#define M_SQRT_PI 1.772453850905515f

			#pragma vertex vertex //vert
			#pragma fragment frag
			//#pragma hull hull
			//#pragma domain domain
			//#pragma geometry geometry
			#pragma require 2darray
			#pragma multi_compile_fwdbase
			#pragma multi_compile DEBUG

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 5.0

			int _ShaderTest;
			int _GlobalIllumination;
			int _Specular;
			float _Roughness;
			float4 _Color;

			/* WATER PARAMETERS*/
			fixed4 _WaterColor;
			float4 _lods;
			float _nyquistMin = 1.0f;
			float _nyquistMax = 1.5f;
			float _nbWaves = 60;
			sampler2D _wavesSampler;
			float2 _sigmaSqTotal;
			float4x4 _worldToWind;
			float4x4 _windToWorld;
			/* -------- */

			UNITY_DECLARE_TEX2DARRAY(_HeightTextures);
			UNITY_DECLARE_TEX2DARRAY(_NormalTextures);
			UNITY_DECLARE_TEX2DARRAY(_ColorTextures);

			sampler2D _ClassesDGR;
			sampler2D _ClassesWSGT;
			sampler2D _Sat;

			int _MaterialDebug;
			float4 _GravelDebug;
			float4 _DirtDebug;
			float4 _RockDebug;
			float4 _GrassDebug;
			float4 _ForestDebug;
			float4 _WaterDebug;
			float4 _SnowDebug;

			int _N_NOISE;
			int _enableNormalMap;
			int _enableDetails;
			int _parallax;

			float _SatelliteProportion;

			half _RockNormalDetailStrength;
			half _RockRoughnessModifier;
			half _RockRoughnessModifierStrength;
			float _RockUVLargeMultiply;
			float _RockUVDetailMultiply;
			float _RockDetailStrength;
			float _RockNormalLargeStrength;
			float _RockHeightStrength;
			float _RockHeightOffset;

			half _SnowNormalDetailStrength;
			half _SnowRoughnessModifier;
			half _SnowRoughnessModifierStrength;
			float _SnowUVLargeMultiply;
			float _SnowUVDetailMultiply;
			float _SnowDetailStrength;
			float _SnowNormalLargeStrength;
			float _SnowHeightStrength;
			float _SnowHeightOffset;

			half _GravelNormalDetailStrength;
			half _GravelHeightStrength;
			half _GravelHeightOffset;

			half _GravelRoughnessModifier;
			half _GravelRoughnessModifierStrength;
			float _GravelUVLargeMultiply;
			float _GravelUVDetailMultiply;
			float _GravelDetailStrength;
			float _GravelNormalLargeStrength;

			half _DirtNormalDetailStrength;
			half _DirtHeightStrength;
			half _DirtHeightOffset;

			half _DirtRoughnessModifier;
			half _DirtRoughnessModifierStrength;
			float _DirtUVLargeMultiply;
			float _DirtUVDetailMultiply;
			float _DirtDetailStrength;
			float _DirtNormalLargeStrength;

			half _GrassNormalDetailStrength;
			half _GrassHeightStrength;
			half _GrassHeightOffset;

			half _GrassRoughnessModifier;
			half _GrassRoughnessModifierStrength;
			float _GrassUVLargeMultiply;
			float _GrassUVDetailMultiply;
			float _GrassDetailStrength;
			float _GrassNormalLargeStrength;

			float _CommonUVDetailMultiply;
			float _CommonNormalDetailStrength;
			float _CommonDetailStrength;
			float _CommonHeightDetailStrength;
			float _CommonHeightDetailOffset;

			float _ForestUVLargeMultiply;
            float _ForestNormalLargeStrength;
			float _ForestRoughnessModifier;
            float _ForestRoughnessModifierStrength;
			float _ForestHeightStrength;
            float _ForestHeightOffset;

			float _LODDistance0;
			float _LODDistance1;
			float _LODDistance2;
			float _LODDistance3;
			float _LODDistance4;
			int _LODDebug;

			int _SlopeModifierDebug;
			int _SlopeModifierEnabled;
			float _SlopeModifierThreshold;
			float _SlopeModifierStrength;

			float _2TanFOVHeight;

			int _LightSampleCount;

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
				float4 worldSpacePosition : TEXCOORD1;
				float3 lightDirection: TEXCOORD2;

				half3 tangent : TEXCOORD3; // tangent.x, bitangent.x, normal.x
				half3 bitangent : TEXCOORD4; // tangent.y, bitangent.y, normal.y
				half3 normal : TEXCOORD5; // tangent.z, bitangent.z, normal.z

				float2 u : TEXCOORD6;
				float3 dPdu : TEXCOORD7;
				float3 dPdv : TEXCOORD8;
				float dP : TEXCOORD9;
				float2 sigmaSq : TEXCOORD10;
				float3 P : TEXCOORD11;
				float lod : TEXCOORD12;

				SHADOW_COORDS(13)
			};

			uint ReverseBits32(uint bits)
			{
#if 0 // Shader model 5
				return reversebits(bits);
#else
				bits = (bits << 16) | (bits >> 16);
				bits = ((bits & 0x00ff00ff) << 8) | ((bits & 0xff00ff00) >> 8);
				bits = ((bits & 0x0f0f0f0f) << 4) | ((bits & 0xf0f0f0f0) >> 4);
				bits = ((bits & 0x33333333) << 2) | ((bits & 0xcccccccc) >> 2);
				bits = ((bits & 0x55555555) << 1) | ((bits & 0xaaaaaaaa) >> 1);
				return bits;
#endif
			}

			//-----------------------------------------------------------------------------

			float RadicalInverse_VdC(uint bits)
			{
				return float(ReverseBits32(bits)) * 2.3283064365386963e-10; // 0x100000000
			}

			//-----------------------------------------------------------------------------

			float2 Hammersley2d(uint i, uint maxSampleCount)
			{
				return float2(float(i) / float(maxSampleCount), RadicalInverse_VdC(i));
			}

			//-----------------------------------------------------------------------------
			float Hash(uint s)
			{
				s = s ^ 2747636419u;
				s = s * 2654435769u;
				s = s ^ (s >> 16);
				s = s * 2654435769u;
				s = s ^ (s >> 16);
				s = s * 2654435769u;
				return float(s) / 4294967295.0f;
			}

			//-----------------------------------------------------------------------------
			float2 InitRandom(float2 input)
			{
				float2 r;
				r.x = Hash(uint(input.x * 4294967295.0f));
				r.y = Hash(uint(input.y * 4294967295.0f));

				return r;
			}

			// generate an orthonormalBasis from 3d unit vector.
			void GetLocalFrame(float3 N, out float3 tangentX, out float3 tangentY)
			{
				float3 upVector = abs(N.z) < 0.999f ? float3(0.0f, 0.0f, 1.0f) : float3(1.0f, 0.0f, 0.0f);
				tangentX = normalize(cross(upVector, N));
				tangentY = cross(N, tangentX);
			}

			v2f vertex(appdata_tan v)
			{
				v2f o;
				o.uv = v.texcoord;

				half3 wNormal = UnityObjectToWorldNormal(v.normal);
				half3 wTangent = UnityObjectToWorldDir(v.tangent.xyz);
				// compute bitangent from cross product of normal and tangent
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 wBitangent = cross(wNormal, wTangent) * tangentSign;
				// output the tangent space matrix
				o.normal = wNormal;
				o.tangent = wTangent;
				o.bitangent = wBitangent;

				TRANSFER_SHADOW(o)

				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldSpacePosition = mul(unity_ObjectToWorld, v.vertex);
				o.lightDirection = _WorldSpaceLightPos0.xyz - o.worldSpacePosition.xyz * _WorldSpaceLightPos0.w;

				/* OCEAN SIMULATION VERTEX PART*/
								
				//if (tex2Dlod(_ClassesWSGT, float4(o.uv, 0, 1)).x > 0)
				{
					float3 V = (_WorldSpaceCameraPos - o.worldSpacePosition.xyz);
					float t = length(V);
					V = normalize(V);

					o.u =  mul(_worldToWind, float4(v.vertex.xz , 0, 0) ).xy;
					o.lod = sqrt(atan(_lods.y / t) * _lods.x * dot(V, wNormal));

					float3 dPdu = float3(1.0, 0.0, 0.0);
					float3 dPdv = float3(0.0, 1.0, 0.0);
					float2 sigmaSq = _sigmaSqTotal;

					float3 dP = float3(0.0, 0.0, 0.0);

					float iMin = max(0.0, floor((log2(_nyquistMin * _lods.y) - _lods.z) * _lods.w));

					[loop]
					for (float i = iMin; i < _nbWaves; i += 1.0f)
					{
						float4 wt = tex2Dlod(_wavesSampler, float4((i + 0.5) / _nbWaves, 0,0,0));
						float phase = wt.y * _Time.y/5 -dot(wt.zw, o.u);
						float s = sin(phase);
						float c = cos(phase);
						float overk = 9.81f / (wt.y * wt.y);

						float wp = smoothstep(_nyquistMin, _nyquistMax, (M_2PI) * overk / _lods.y);

						float3 factor = wp * wt.x * float3(wt.zw * overk, 1.0);
						dP += factor * float3(s, s, c);

						float3 dPd = factor * float3(c, c, -s);
						dPdu -= dPd * wt.z;
						dPdv -= dPd * wt.w;

						wt.zw *= overk;
						float kh = wt.x / overk;
						sigmaSq -= float2(wt.z * wt.z, wt.w * wt.w) * (1.0 - sqrt(1.0 - kh * kh));
					}

					o.P = v.vertex.xyz + dP.xzy;
					
					
					if (tex2Dlod(_ClassesWSGT, float4(o.uv, 0, 1)).x >= 1) {
						o.pos = UnityObjectToClipPos(float4(o.P.x, o.P.y, o.P.z, 1.0));
						o.worldSpacePosition = mul(unity_ObjectToWorld, float4(o.P.x, o.P.y, o.P.z, 1.0));
					}					
					
					o.dP = dP.z;
					o.dPdu = dPdu;
					o.dPdv = dPdv;
					o.sigmaSq = sigmaSq;

				}

				TRANSFER_VERTEX_TO_FRAGMENT(o);

				return(o);
			}
			
			// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
			// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
			// #pragma instancing_options assumeuniformscaling
			UNITY_INSTANCING_BUFFER_START(Props)
				// put more per-instance properties here
			UNITY_INSTANCING_BUFFER_END(Props)



			float3 V;
			float3 L;
			float3 H;
			float3 Normal;
			float3 VertexNormal;

			float Depth;
			float4 satellite;
			float2 UV;

			float blendSize;

			float wetness;

			float3 tspace0;
			float3 tspace1;
			float3 tspace2;

			float Shadows;

			float4 DGR;
			float4 WSGT;

			float texLod;
			float snowTexLod;
			float dirtTexLod;
			float gravelTexLod;
			float grassTexLod;
			float rockTexLod;
			float commonTexLod;


			struct Material {
				float4 Albedo;
				float3 Normal;
				float Roughness;
			};

			Material lerpMaterial(Material m0, Material m1, float w) {
				Material m;
				m.Albedo = lerp(m0.Albedo, m1.Albedo, w);
				m.Normal = normalize(lerp(m0.Normal, m1.Normal, w));
				m.Roughness = lerp(m0.Roughness, m1.Roughness, w);

				return m;
			}

			float3 SampleReflection(float3 wo, float roughness) {
				half mip = perceptualRoughnessToMipmapLevel(roughness);

				return DecodeHDR(UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, wo, mip), unity_SpecCube0_HDR);
			}

			float3 RGB2HSL(float3 color) {
				float _max = max(color.r, max(color.g, color.b));
				float _min = min(color.r, min(color.g, color.b));

				float l = (_max + _min) / 2.0;

				if (_max == _min) return float3(0, 0, l);

				float d = _max - _min;
				float s = l > 0.5 ? d / (2 - _max - _min) : d / (_max + _min);
				float h = 0;
				if (_max == color.r) h = (color.g - color.b) / d + (color.g < color.b ? 6 : 0);
				else if (_max == color.g) h = (color.b - color.r) / d + 2;
				else if (_max == color.b) h = (color.r - color.g) / d + 4;

				h /= 6.0;

				return float3(h, s, l);

			}

			float Hue2RGB(float p, float q, float t) {
				if (t < 0.0) t += 1.0;
				if (t > 1.0) t -= 1.0;
				if (t < 1.0 / 6.0) return p + (q - p) * 6.0 * t;
				if (t < 1.0 / 2.0) return q;
				if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6.0;
				return p;
			}

			float3 HSL2RGB(float3 color) {
				float h = color.r;
				float s = color.g;
				float l = color.b;

				if (s == 0) return float3(l, l, l);

				float q = l < 0.5 ? l * (1 + s) : l + s - l*s;
				float p = 2 * l - q;

				return float3(Hue2RGB(p, q, h + 0.33333), Hue2RGB(p, q, h), Hue2RGB(p, q, h - 0.33333));
			}

			float4 ColorTransfer(float4 c1, float4 c2, float4 mc2) {
				return c2 + (c1 - mc2);
			}

			float4 ColorTransfer(float4 c1, float4 c2, float strength) {
				return ColorTransfer(c1, c2, float4(0, 0, 0, 0));
			}
			
			float V_SmithGGXCorrelated(float NdotL, float NdotV, float r) {

				float a2 = r * r;

				float Lambda_GGXV = NdotL * sqrt((-NdotV * a2 + NdotV) * NdotV + a2);
				float Lambda_GGXL = NdotV * sqrt((-NdotL * a2 + NdotL) * NdotL + a2);

				return 0.5f / (Lambda_GGXV + Lambda_GGXL);
			}

			float V_SmithGGX(float NdotL, float NdotV, float r) {
				float a2 = r * r *r *r;

				float G1 = 2.0 / (1 + sqrt(1 + a2 * (1 - NdotL * NdotL) / (NdotL*NdotL)));
				float G2 = 2.0 / (1 + sqrt(1 + a2 * (1 - NdotV * NdotV) / (NdotV*NdotV)));

				return G1 * G1;
				
			}

			float D_GGX(float NdotH, float r) {
				float a = r * r;
				float a2 = a * a;
				float f = (NdotH * a2 - NdotH) * NdotH + 1;

				return a2 / (f*f);
			}

			float F_Schlick(float u, float f0=0) {
				float m = saturate(1 - u);
				float m2 = m * m;
				return f0 + (1-f0) * m2 * m2 * m;
			}

			void ImportanceSampleCosDir(float2 u,
				float3 N,
				float3 tangentX,
				float3 tangentY,
				out float3 L)
			{
				// Cosine sampling - ref: http://www.rorydriscoll.com/2009/01/07/better-sampling/
				float cosTheta = sqrt(max(0.0f, 1.0f - u.x));
				float sinTheta = sqrt(u.x);
				float phi = UNITY_TWO_PI * u.y;

				// Transform from spherical into cartesian
				L = float3(sinTheta * cos(phi), sinTheta * sin(phi), cosTheta);
				// Local to world
				L = tangentX * L.x + tangentY * L.y + N * L.z;
			}

			//-------------------------------------------------------------------------------------
			void ImportanceSampleGGXDir(float2 u,
				float3 V,
				float3 N,
				float3 tangentX,
				float3 tangentY,
				float roughness,
				out float3 H,
				out float3 L)
			{
				// GGX NDF sampling
				float cosThetaH = sqrt((1.0f - u.x) / (1.0f + (roughness * roughness - 1.0f) * u.x));
				float sinThetaH = sqrt(max(0.0f, 1.0f - cosThetaH * cosThetaH));
				float phiH = UNITY_TWO_PI * u.y;

				// Transform from spherical into cartesian
				H = float3(sinThetaH * cos(phiH), sinThetaH * sin(phiH), cosThetaH);
				// Local to world
				H = tangentX * H.x + tangentY * H.y + N * H.z;

				// Convert sample from half angle to incident angle
				L = reflect(-V, H);
			}

			// weightOverPdf return the weight (without the Fresnel term) over pdf. Fresnel term must be apply by the caller.
			void ImportanceSampleGGX(
				float2 u,
				float3 V,
				float3 N,
				float3 tangentX,
				float3 tangentY,
				float roughness,
				float NdotV,
				out float3 L,
				out float VdotH,
				out float NdotL,
				out float weightOverPdf)
			{
				float3 H;
				ImportanceSampleGGXDir(u, V, N, tangentX, tangentY, roughness, H, L);

				float NdotH = saturate(dot(N, H));
				// Note: since L and V are symmetric around H, LdotH == VdotH
				VdotH = saturate(dot(V, H));
				NdotL = saturate(dot(N, L));

				// Importance sampling weight for each sample
				// pdf = D(H) * (N.H) / (4 * (L.H))
				// weight = fr * (N.L) with fr = F(H) * G(V, L) * D(H) / (4 * (N.L) * (N.V))
				// weight over pdf is:
				// weightOverPdf = F(H) * G(V, L) * (L.H) / ((N.H) * (N.V))
				// weightOverPdf = F(H) * 4 * (N.L) * V(V, L) * (L.H) / (N.H) with V(V, L) = G(V, L) / (4 * (N.L) * (N.V))
				// F is apply outside the function

				float Vis = SmithJointGGXVisibilityTerm(NdotL, NdotV, roughness);
				weightOverPdf = 4.0f * Vis * NdotL * VdotH / NdotH;
			}

			// ----------------------------------------------------------------------------
			// weightOverPdf return the weight (without the diffuseAlbedo term) over pdf. diffuseAlbedo term must be apply by the caller.
			void ImportanceSampleLambert(
				float2 u,
				float3 N,
				float3 tangentX,
				float3 tangentY,
				out float3 L,
				out float NdotL,
				out float weightOverPdf)
			{
				ImportanceSampleCosDir(u, N, tangentX, tangentY, L);

				NdotL = saturate(dot(N, L));

				// Importance sampling weight for each sample
				// pdf = N.L / PI
				// weight = fr * (N.L) with fr = diffuseAlbedo / PI
				// weight over pdf is:
				// weightOverPdf = (diffuseAlbedo / PI) * (N.L) / (N.L / PI)
				// weightOverPdf = diffuseAlbedo
				// diffuseAlbedo is apply outside the function

				weightOverPdf = 1.0f;
			}

			float meanFresnel(float cosThetaV, float sigmaV) {
				return pow(1.0 - cosThetaV, 5.0 * exp(-2.69 * sigmaV)) / (1.0 + 22.7 * pow(sigmaV, 1.5));
			}

			float meanFresnel(float3 V, float3 N, float2 sigmaSq) {
				float2 v = V.xy; // view direction in wind space
				float2 t = v * v / (1.0 - V.z * V.z); // cos^2 and sin^2 of view direction
				float sigmaV2 = dot(t, sigmaSq); // slope variance in view direction
				return meanFresnel(dot(V, N), sqrt(sigmaV2));
			}

			float3 blendNormal(float3 n1, float3 n2, float strength) {
				float3 n = float3(n1.x + n2.x*strength, n1.y + n2.y*strength, n1.z);
				return normalize(n);
			}

			float GrayScale(float4 c) {
				return max(c.x, max(c.y, c.z));
				return (c.x + c.y + c.z) / 3.0;
			}

			float3 Texture2Normal() {
				float d = 0.001;
				float2 dx0 = float2(d, 0);//ddx(UV);
				float2 dy0 = float2(0, d);//ddy(UV);

				float2 dx1 = float2(-d, 0);//ddx(UV);
				float2 dy1 = float2(0, -d);//ddy(UV);

				if (UV.x + dx0.x >= 1) dx0.x = 1 - UV.x;
				if (UV.y + dy0.y >= 1) dy0.y = 1 - UV.y;

				if (UV.x + dx1.x < 0) dx1.x += dx1.x + UV.x;
				if (UV.y + dy1.y < 0) dy1.y += dy1.y + UV.y;
				
				float h0 = GrayScale(tex2D(_Sat, saturate(UV + dx0)));
				float h1 = GrayScale(tex2D(_Sat, saturate(UV + dx1)));
				float h2 = GrayScale(tex2D(_Sat, saturate(UV + dy0)));
				float h3 = GrayScale(tex2D(_Sat, saturate(UV + dy1)));


				float3 v1 = normalize(float3(dx0.x - dx1.x, 0, (h1 - h0)/2));
				float3 v2 = normalize(float3(0, dy0.y  - dy1.y, (h3 - h2)/2));

				return normalize(cross(v1,v2));
			}

			float Principled_DisneyDiffuse(float NdotV, float NdotL, float LdotH, float roughness) {
				float FL = F_Schlick(NdotL);
				float FV = F_Schlick(NdotV);

				float fd90 = 0.5f + 2.0f * LdotH * LdotH * roughness;

				float Fd = (1.0f * (1.0f - FL) + fd90 * FL) * (1.0f * (1.0f - FV) + fd90 * FV);

				return NdotL * Fd * M_1_PI;
			}

			float4 microfacet(Material m) {
				if (_MaterialDebug == 1) {
					return m.Albedo;
				}

				float3 N = float3(0, 0, 0);
				
				N.x = dot(tspace0, m.Normal.xyz);
				N.y = dot(tspace1, m.Normal.xyz);
				N.z = dot(tspace2, m.Normal.xyz);

				float NdotV = abs(dot(N, V)) + 1e-5f;

				float4 Direct = 0;
				/* DIRECT SUN ILLUMINATION */
				if(Shadows > 0)
				{
					float LdotH = saturate(dot(L, H));
					float NdotH = saturate(dot(N, H));
					float NdotL = saturate(dot(N, L));

					float3 Fd = 0;
					float3 Fr = 0;
					if (NdotL > 0 && NdotV > 0) {
						//Disney Principled Diffuse
						Fd = m.Albedo.rgb * Principled_DisneyDiffuse(NdotV, NdotL, LdotH, m.Roughness);

						//Specular
						float3 F = F_Schlick(LdotH, 0.04);
						float G = V_SmithGGXCorrelated(NdotV, NdotL, 0.5 + m.Roughness / 2.0);
						float D = D_GGX(NdotH, m.Roughness);
						Fr = D * F * G * M_1_PI * NdotL;
					}

					Direct = float4(_LightColor0 * (Fd + Fr) * Shadows, 1.0);
				}
				float4 Indirect = 0;

				if (_GlobalIllumination == 1) {
					float3 tangentX, tangentY;
					GetLocalFrame(N, tangentX, tangentY);

					float2 randNum = InitRandom(N.xz * 0.5f + 0.5f);

					float3 Fd = 0;

					float r = 1.0f * _LightSampleCount / 64.0f;
					r = saturate(1 - r);
					r = r * r;
					r = r * r;
					/* INDIRECT DIFFUSE */
					[loop]
					for (int i = 0; i < _LightSampleCount; i++) {
						float2 u = Hammersley2d(i, _LightSampleCount);
						u = frac(u + randNum + 0.5f);
						float3 L;
						float NdotL;
						float weightOverPdf = 0;

						// for Disney we still use a Cosine importance sampling, true Disney importance sampling imply a look up table
						ImportanceSampleLambert(u, N, tangentX, tangentY, L, NdotL, weightOverPdf);

						NdotL = saturate(dot(N, L));
						if (NdotL > 0 && NdotV > 0) {
							float3 H = normalize(V + L);
							float LdotH = saturate(dot(L, H));
							float3 NdotH = saturate(dot(N, H));

							float3 sky = SampleReflection(L.xyz, r);
							Fd += sky * m.Albedo.rgb * Principled_DisneyDiffuse(NdotV, NdotL, LdotH, m.Roughness) * weightOverPdf;
						}
					}

					/* INDIRECT SPECULAR */
					float3 Fr = 0;

					float3 L = reflect(-V, N);
					float NdotL = saturate(dot(N, L));
					if (_Specular == 1) {
						if (NdotL > 0.0f) {
							float3 H = normalize(V + L);
							float LdotH = saturate(dot(L, H));
							float NdotH = saturate(dot(N, H));
							float weightOverPdf = 0;
							float VdotH = dot(V, H);

							float Vis = V_SmithGGXCorrelated(NdotL, NdotV, 0.5 + m.Roughness / 2.0);
							weightOverPdf = 4.0f * Vis * NdotL * VdotH / NdotH;

							//Specular	
							if (weightOverPdf > 0)
							{
								float3 sky = SampleReflection(L.xyz, m.Roughness);
								Fr += F_Schlick(VdotH, 0.02) * sky * weightOverPdf;
							}						
						}
					}

					Indirect = float4(Fd/_LightSampleCount + Fr, 1);
				}

				return Direct + Indirect;				
			}

			Material Gravel(float wetness, float height) {
				if (_MaterialDebug == 1) {
					Material m;
					m.Albedo = _GravelDebug;
					return m;
				}

				float4 albedoL = (UNITY_SAMPLE_TEX2DARRAY(_ColorTextures, float3(UV*_GravelUVLargeMultiply, 6+0.0)));
				float4 albedoLM = (UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorTextures, float3(UV*_GravelUVLargeMultiply, 6+0.0), 9));

				float roughness = UNITY_SAMPLE_TEX2DARRAY(_ColorTextures, float3(UV*_GravelUVLargeMultiply, 6+2.0)).x;

				float4 albedo = ColorTransfer(satellite, albedoL, albedoLM);

				float3 N = _enableNormalMap ? lerp(float3(0,0,1), UnpackNormal(UNITY_SAMPLE_TEX2DARRAY(_NormalTextures, float3(UV*_GravelUVLargeMultiply, 4))), _GravelNormalLargeStrength) : float3(0, 0, 1);

				if (_enableDetails && (Depth < _LODDistance1)) {
					float4 albedoD = (UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorTextures, float3(UV*_GravelUVDetailMultiply, 6+1.0), gravelTexLod));
					float4 albedoDM = (UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorTextures, float3(UV*_GravelUVDetailMultiply, 6+1.0), 8+ gravelTexLod));

					float roughnessD = UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorTextures, float3(UV*_GravelUVDetailMultiply, 6+3.0), gravelTexLod).x;

					float4 colorD = ColorTransfer(albedo, albedoD, albedoDM);

					float3 NormalD = _enableNormalMap ? UnpackNormal(UNITY_SAMPLE_TEX2DARRAY_LOD(_NormalTextures, float3(UV*_GravelUVDetailMultiply, 5), gravelTexLod)) : float3(0, 0, 1);

					if (Depth >= _LODDistance1 * (1 - blendSize)) {
						float w = (Depth - _LODDistance1 * (1 - blendSize)) / (_LODDistance1*blendSize);
						albedo = lerp(colorD, albedo, w);
						N = blendNormal(N, NormalD, 1 - w);
						roughness = lerp((roughness + roughnessD) / 2, roughness, w);
					}
					else {
						if (Depth < _LODDistance0) {
							albedo = lerp(albedoD, colorD, (Depth) / (_LODDistance0));
						}
						else {
							albedo = colorD;
						}

						N = blendNormal(N, NormalD, 1);
						roughness = (roughness + roughnessD) / 2;
					}
				}

				roughness = lerp(roughness, _GravelRoughnessModifier, _GravelRoughnessModifierStrength);

				float wetratio = saturate(2.0*wetness - pow(1.2*(height), 2));
				roughness = lerp(roughness, 0.1, wetratio);

				albedo *= lerp(1.0, 0.6, wetratio);

				Material m;
				m.Albedo = albedo;
				m.Normal = N;
				m.Roughness = roughness;
				return m;
			}

			Material Dirt(float wetness, float height) {
				if (_MaterialDebug == 1) {
					Material m;
					m.Albedo = _DirtDebug;
					return m;
				}

				float4 albedoL = (UNITY_SAMPLE_TEX2DARRAY(_ColorTextures, float3(UV*_GravelUVLargeMultiply, 10 + 0.0)));
				float4 albedoLM = (UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorTextures, float3(UV*_GravelUVLargeMultiply, 10 + 0.0), 9));

				float roughness = UNITY_SAMPLE_TEX2DARRAY(_ColorTextures, float3(UV*_GravelUVLargeMultiply, 10 + 2.0)).x;

				float4 albedo = ColorTransfer(satellite, albedoL, albedoLM);

				float3 N = _enableNormalMap ? lerp(float3(0, 0, 1), UnpackNormal(UNITY_SAMPLE_TEX2DARRAY(_NormalTextures, float3(UV*_GravelUVLargeMultiply, 6))), _GravelNormalLargeStrength) : float3(0, 0, 1);

				if (_enableDetails && (Depth < _LODDistance1)) {
					float4 albedoD = (UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorTextures, float3(UV*_DirtUVDetailMultiply, 10 + 1.0), dirtTexLod));
					float4 albedoDM = (UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorTextures, float3(UV*_DirtUVDetailMultiply, 10 + 1.0), 8 + dirtTexLod));

					float roughnessD = UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorTextures, float3(UV*_DirtUVDetailMultiply, 10 + 3.0), dirtTexLod).x;

					float4 colorD = ColorTransfer(albedo, albedoD, albedoDM);

					float3 NormalD = _enableNormalMap ? UnpackNormal(UNITY_SAMPLE_TEX2DARRAY_LOD(_NormalTextures, float3(UV*_DirtUVDetailMultiply, 7), dirtTexLod)) : float3(0, 0, 1);

					if (Depth >= _LODDistance1 * (1 - blendSize)) {
						float w = (Depth - _LODDistance1 * (1 - blendSize)) / (_LODDistance1*blendSize);
						albedo = lerp(colorD, albedo, w);
						N = blendNormal(N, NormalD, 1 - w);
						roughness = lerp((roughness + roughnessD) / 2, roughness, w);
					}
					else {
						if (Depth < _LODDistance0) {
							albedo = lerp(albedoD, colorD, (Depth) / (_LODDistance0));
						}
						else {
							albedo = colorD;
						}

						N = blendNormal(N, NormalD, 1);
						roughness = (roughness + roughnessD) / 2;
					}
				}

				roughness = lerp(roughness, _GravelRoughnessModifier, _GravelRoughnessModifierStrength);

				float wetratio = saturate(2.0*wetness - pow(1.2*(height), 2));
				roughness = lerp(roughness, 0.1, wetratio);

				albedo *= lerp(1.0, 0.6, wetratio);

				Material m;
				m.Albedo = albedo;
				m.Normal = N;
				m.Roughness = roughness;
				return m;
			}

			Material Rock(float wetness, float height, float height2) {
				if (_MaterialDebug == 1) {
					Material m;
					m.Albedo = _RockDebug;
					return m;
				}

				float uvMultiply = height > height2 ? _RockUVLargeMultiply : _CommonUVDetailMultiply;

				float4 albedoL = (UNITY_SAMPLE_TEX2DARRAY(_ColorTextures, float3(UV*uvMultiply, height > height2 ? 0 : 20)));
				float4 albedoLM = height > height2 ? (UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorTextures, float3(UV*uvMultiply, 1), 9)) : 0;

				float roughness = UNITY_SAMPLE_TEX2DARRAY(_ColorTextures, float3(UV*_RockUVLargeMultiply, 2.0)).x;

				float4 albedo = height > height2 ? ColorTransfer(satellite, albedoL, albedoLM) : albedoL;

				float3 N = _enableNormalMap ? UnpackNormal(UNITY_SAMPLE_TEX2DARRAY(_NormalTextures, float3(UV*uvMultiply,  height > height2 ? 1 : 10))) : float3(0,0,1);

				if (_enableDetails && (Depth < _LODDistance1)) {
					float4 albedoD = (UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorTextures, float3(UV*_RockUVDetailMultiply, 1), rockTexLod));
					float4 albedoDM = (UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorTextures, float3(UV*_RockUVDetailMultiply, 1), 8 + rockTexLod));

					float roughnessD = UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorTextures, float3(UV*_RockUVDetailMultiply, 0 + 3.0), rockTexLod).x;

					float4 colorD = ColorTransfer(albedo, albedoD, albedoDM);

					float3 NormalD = _enableNormalMap ? UnpackNormal(UNITY_SAMPLE_TEX2DARRAY_LOD(_NormalTextures, float3(UV*_RockUVDetailMultiply, 1), rockTexLod)) : float3(0, 0, 1);
					if (Depth >= _LODDistance1 * (1 - blendSize)) {
						float w = (Depth - _LODDistance1 * (1 - blendSize)) / (_LODDistance1*blendSize);
						albedo = lerp(colorD, albedo, w);
						N = blendNormal(N, NormalD, 1-w);
						roughness = lerp((roughness + roughnessD) / 2, roughness, w);
					}
					else {
						albedo = colorD;
						N = blendNormal(N, NormalD, 1);
						roughness = (roughness + roughnessD) / 2;
					}
				}

				roughness = lerp(roughness, _RockRoughnessModifier, _RockRoughnessModifierStrength);

				float wetratio = saturate(2.0*wetness - pow(1.2*(max(height, height2)), 2));
				roughness = lerp(roughness, 0.1, wetratio);

				albedo *= lerp(1.0, 0.6, wetratio);

				Material m;
				m.Albedo = albedo;
				m.Normal = N;
				m.Roughness = roughness;
				return m;
			}

			Material Forest() {
				if (_MaterialDebug == 1) {
					Material m;
					m.Albedo = _ForestDebug;
					return m;
				}

				float4 albedoL = (UNITY_SAMPLE_TEX2DARRAY(_ColorTextures, float3(UV*_ForestUVLargeMultiply, 18 + 0.0)));
				float4 albedoLM = (UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorTextures, float3(UV*_ForestUVLargeMultiply, 18 + 0.0), 9));

				float roughness = UNITY_SAMPLE_TEX2DARRAY(_ColorTextures, float3(UV*_ForestUVLargeMultiply, 18 + 1.0)).x;

				float4 albedo = lerp(ColorTransfer(satellite, albedoL, albedoLM), albedoL, 0.7);

				float3 N = _enableNormalMap ? lerp(float3(0, 0, 1), UnpackNormal(UNITY_SAMPLE_TEX2DARRAY(_NormalTextures, float3(UV*_ForestUVLargeMultiply, 11))), _ForestNormalLargeStrength) : float3(0, 0, 1);

				roughness = lerp(roughness, _ForestRoughnessModifier, _ForestRoughnessModifierStrength);

				Material m;
				m.Albedo = albedo;
				m.Normal = N;
				m.Roughness = roughness;
				return m;
			}

			Material Grass(float wetness, float height) {
				if (_MaterialDebug == 1) {
					Material m;
					m.Albedo = _GrassDebug;
					return m;
				}

				float4 albedoL = (UNITY_SAMPLE_TEX2DARRAY(_ColorTextures, float3(UV*_GrassUVLargeMultiply, 14 + 0.0)));
				float4 albedoLM = (UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorTextures, float3(UV*_GrassUVLargeMultiply, 14 + 0.0), 9));

				float roughness = UNITY_SAMPLE_TEX2DARRAY(_ColorTextures, float3(UV*_GrassUVLargeMultiply, 14 + 2.0)).x;

				float4 albedo = lerp(ColorTransfer(satellite, albedoL, albedoLM), satellite, 0.7);

				float3 N = _enableNormalMap ? lerp(float3(0, 0, 1), UnpackNormal(UNITY_SAMPLE_TEX2DARRAY(_NormalTextures, float3(UV*_GrassUVLargeMultiply, 8))), _GrassNormalLargeStrength) : float3(0, 0, 1);

				if (_enableDetails && (Depth < _LODDistance1)) {
					float4 albedoD = (UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorTextures, float3(UV*_GrassUVDetailMultiply, 14 + 1.0), grassTexLod));
					float4 albedoDM = (UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorTextures, float3(UV*_GrassUVDetailMultiply, 14 + 1.0), 8 + grassTexLod));

					float roughnessD = UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorTextures, float3(UV*_GrassUVDetailMultiply, 14 + 3.0), grassTexLod).x;

					float4 colorD = ColorTransfer(albedo, albedoD, albedoDM);

					float3 NormalD = _enableNormalMap ? UnpackNormal(UNITY_SAMPLE_TEX2DARRAY_LOD(_NormalTextures, float3(UV*_GrassUVDetailMultiply, 9), grassTexLod)) : float3(0, 0, 1);

					if (Depth >= _LODDistance1 * (1 - blendSize)) {
						float w = (Depth - _LODDistance1 * (1 - blendSize)) / (_LODDistance1*blendSize);
						albedo = lerp(colorD, albedo, w);
						N = blendNormal(N, NormalD, 1 - w);
						roughness = lerp((roughness + roughnessD) / 2, roughness, w);
					}
					else {
						if (Depth < _LODDistance0) {
							albedo = lerp(albedoD, colorD, (Depth) / (_LODDistance0));
						}
						else {
							albedo = colorD;
						}

						N = blendNormal(N, NormalD, 1);
						roughness = (roughness + roughnessD) / 2;
					}
				}

				roughness = lerp(roughness, _GrassRoughnessModifier, _GrassRoughnessModifierStrength);

				float wetratio = saturate(2.0*wetness - pow(1.2*(height), 2));
				roughness = lerp(roughness, 0.1, wetratio);

				albedo *= lerp(1.0, 0.6, wetratio);

				Material m;
				m.Albedo = albedo;
				m.Normal = N;
				m.Roughness = roughness;
				return m;
			}

			Material Snow(float weight) {
				if (_MaterialDebug == 1) {
					Material m;
					m.Albedo = _SnowDebug;
					return m;
				}

				float roughness = UNITY_SAMPLE_TEX2DARRAY(_ColorTextures, float3(UV*_SnowUVLargeMultiply, 4)).x;

				float4 albedo = lerp(float4(1,1,1,1), satellite, 0.7);

				float3 N = _enableNormalMap ? lerp(float3(0, 0, 1), UnpackNormal(UNITY_SAMPLE_TEX2DARRAY(_NormalTextures, float3(UV*_SnowUVLargeMultiply, 2))), _SnowNormalLargeStrength) : float3(0, 0, 1);

				if (_enableDetails && (Depth < _LODDistance1)) {
					float roughnessD = UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorTextures, float3(UV*_SnowUVDetailMultiply, 5), snowTexLod).x;

					float4 colorD = float4(1,1,1,1);

					float3 NormalD = _enableNormalMap ? UnpackNormal(UNITY_SAMPLE_TEX2DARRAY_LOD(_NormalTextures, float3(UV*_SnowUVDetailMultiply, 3), snowTexLod)) : float3(0, 0, 1);

					if (Depth >= _LODDistance1 * (1 - blendSize))
					{
						float w = (Depth - _LODDistance1 * (1 - blendSize)) / (_LODDistance1*blendSize);
						albedo = lerp(colorD, albedo, w);
						N = blendNormal(N, NormalD, 1 - w);
						roughness = lerp((roughness + roughnessD) / 2, roughness, w);
					}
					else
					{
						albedo = colorD;						

						N = blendNormal(N, NormalD, 1);
						roughness = (roughness + roughnessD) / 2;
					}
				}

				roughness = lerp(roughness, _SnowRoughnessModifier, _SnowRoughnessModifierStrength);

				float wetratio = saturate(pow(1-weight, 2));
				roughness = lerp(roughness, 0.1, wetratio);

				albedo *= lerp(1.0, 0.6, wetratio);

				Material m;
				m.Albedo = albedo;
				m.Normal = N;
				m.Roughness = roughness;
				return m;
			}

			float RockH(float weight) {
				float height = 0;

				[unroll(3)]
				for (int i = 0; i < 2 * weight * _parallax + 1; i++) {
					height = (UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(UV * _RockUVLargeMultiply, 0.0 + _N_NOISE))).r;

					if (_parallax == 1) UV -= 0.01 * (height - 0.5) * VertexNormal.y * V.xz * weight / _RockUVLargeMultiply;

				}

				if (_enableDetails && (Depth < _LODDistance1)) {
					float heightD = (UNITY_SAMPLE_TEX2DARRAY_LOD(_HeightTextures, float3(UV*_RockUVDetailMultiply, 1.0 + _N_NOISE), rockTexLod)).r * _RockDetailStrength;

					if (Depth >= _LODDistance1 * (1 - blendSize)) {
						float w = (1 - ((Depth - _LODDistance1 * (1 - blendSize)) / (_LODDistance1*blendSize)));
						height = (height + heightD * w) / (1 + w);
					}
					else
						height = (height + heightD) / 2.0;
				}

				return (height * _RockHeightStrength + _RockHeightOffset) * weight;
			}

			float GrassH(float weight) {

				float height = 0;
				[unroll(2)]
				for (int i = 0; i < 1 * weight * _parallax + 1; i++) {
					height = UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(UV*_GrassUVDetailMultiply, 8.0 + _N_NOISE)).x *_GrassHeightStrength + _GrassHeightOffset;

					if (_parallax == 1) UV -= 0.005 * (height - 0.5) * VertexNormal.y * V.xz * weight / _RockUVLargeMultiply;
				}

				if (_enableDetails && (Depth < _LODDistance1)) {
					float heightD = (UNITY_SAMPLE_TEX2DARRAY_LOD(_HeightTextures, float3(UV*_GrassUVDetailMultiply, 9.0 + _N_NOISE), grassTexLod).x*_GrassHeightStrength + _GrassHeightOffset);

					if (Depth >= _LODDistance1 * (1 - blendSize)) {
						float w = (1 - ((Depth - _LODDistance1 * (1 - blendSize)) / (_LODDistance1*blendSize)));
						height = (height + heightD * w) / (1 + w);
					}
					else height = (height + heightD) * 0.5;
				}

				return height * weight;
			}

			float ForestH(float weight) {

				float height = 0;

				[unroll(4)]
				for (int i = 0; i < 3 * weight * _parallax + 1; i++) {
					height = UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(UV*_ForestUVLargeMultiply, 11.0 + _N_NOISE)).x *_ForestHeightStrength + _ForestHeightOffset;

					if (_parallax == 1) UV -= 0.04 * (height - 0.5) * VertexNormal.y * V.xz * weight / _RockUVLargeMultiply;
				}

				return height * weight;
			}

			float GravelH(float weight, float offset) {
				float height = 0;

				[unroll(2)]
				for (int i = 0; i < 1 * weight * _parallax + 1; i++) {
					height = (UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(UV*_GravelUVLargeMultiply, 4.0 + _N_NOISE))).r;

					if (_parallax == 1) UV -= 0.005 * (height + offset - 0.5) * VertexNormal.y * V.xz * weight / _RockUVLargeMultiply;
				}

				if (_enableDetails && (Depth < _LODDistance1)) {
					float heightD = (UNITY_SAMPLE_TEX2DARRAY_LOD(_HeightTextures, float3(UV*_GravelUVDetailMultiply, 5.0 + _N_NOISE), gravelTexLod)).r * _GravelDetailStrength;

					if (Depth >= _LODDistance1 * (1 - blendSize)) {
						float w = (1 - ((Depth - _LODDistance1 * (1 - blendSize)) / (_LODDistance1*blendSize)));
						height = (height + heightD * w) / (1 + w);
					}
					else
						height = (height + heightD) / 2.0;
				}
				height += offset;

				return (height*_GravelHeightStrength + _GravelHeightOffset) * weight;
			}

			float DirtH(float weight) {
				float height = 0;

				[unroll(2)]
				for (int i = 0; i < 1 * weight * _parallax + 1; i++) {
					height = UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(UV*_DirtUVLargeMultiply, 6.0 + _N_NOISE)).x *_DirtHeightStrength + _DirtHeightOffset;
					if (_parallax == 1) UV -= 0.005 * (height - 0.5) * VertexNormal.y * V.xz * weight / _RockUVLargeMultiply;

				}
				if (_enableDetails && (Depth < _LODDistance1)) {
					float heightD = (UNITY_SAMPLE_TEX2DARRAY_LOD(_HeightTextures, float3(UV*_DirtUVDetailMultiply, 7.0 + _N_NOISE), dirtTexLod).x*_DirtHeightStrength + _DirtHeightOffset);

					if (Depth >= _LODDistance1 * (1 - blendSize)) {
						float w = (1 - ((Depth - _LODDistance1 * (1 - blendSize)) / (_LODDistance1*blendSize)));
						height = (height + heightD * w) / (1 + w);
					}
					else
						height = (height + heightD) / 2.0;
				}
				return height * weight;
			}

			float SnowH(float weight) {
				float height = 0;

				[unroll(3)]
				for (int i = 0; i < 2 * weight * _parallax + 1; i++) {
					height = (UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(UV*_SnowUVLargeMultiply, 2.0 + _N_NOISE))).r;
					if (_parallax == 1) UV -= 0.002 * (height - 0.5) * VertexNormal.y * V.xz * weight / _RockUVLargeMultiply;

				}

				if (_enableDetails && (Depth < _LODDistance1)) {
					float heightD = (UNITY_SAMPLE_TEX2DARRAY_LOD(_HeightTextures, float3(UV*_SnowUVDetailMultiply, 3.0 + _N_NOISE), snowTexLod)).r * _SnowDetailStrength;

					if (Depth >= _LODDistance1 * (1 - blendSize)) {
						float w = (1 - ((Depth - _LODDistance1 * (1 - blendSize)) / (_LODDistance1*blendSize)));
						height = (height + heightD * w) / (1 + w);
					}
					else
						height = (height + heightD) / 2.0;
				}

				return (height*_SnowHeightStrength + _SnowHeightOffset) * weight;
			}

			float CommonH(float weight, float offset, float layerHeight = 0) {

				float height = 0;

				if (_enableDetails && (Depth < _LODDistance1)) {
					[unroll(3)]
					for (int i = 0; i < 2 * weight * _parallax + 1; i++) {
						float heightD = ((UNITY_SAMPLE_TEX2DARRAY_LOD(_HeightTextures, float3(UV*_CommonUVDetailMultiply, 10 + _N_NOISE), commonTexLod)).r*_CommonHeightDetailStrength + _CommonHeightDetailOffset);

						if (Depth >= _LODDistance1 * (1 - blendSize)) {
							height = lerp(heightD, height, (Depth - _LODDistance1 * (1 - blendSize)) / (_LODDistance1*blendSize));
						}
						else height = heightD;

						if (height < layerHeight) break;

						height += offset;
						if (_parallax == 1) UV -= 0.006 * (height - 0.5) * VertexNormal.y * V.xz * weight / _CommonUVDetailMultiply;
					}
				}

				return height * weight;
			}


			void findMaxH(float rockH, float gravelH, float dirtH, float ForestH, float grassH, float snowH, float commonH, out float maxH0, out float maxH1) {
				maxH0 = max(rockH, gravelH);
				maxH1 = min(gravelH, rockH);

				if (maxH0 < dirtH) {
					if (maxH1 < maxH0) maxH1 = maxH0;
					maxH0 = dirtH;
				}
				else if (maxH1 < dirtH) maxH1 = dirtH;

				if (maxH0 < ForestH) {
					if (maxH1 < maxH0) maxH1 = maxH0;
					maxH0 = ForestH;
				}
				else if (maxH1 < ForestH) maxH1 = ForestH;

				if (maxH0 < grassH) {
					if (maxH1 < maxH0) maxH1 = maxH0;
					maxH0 = grassH;
				}
				else if (maxH1 < grassH) maxH1 = grassH;

				if (maxH0 < snowH) {
					if (maxH1 < maxH0) maxH1 = maxH0;
					maxH0 = snowH;
				}
				else if (maxH1 < snowH) maxH1 = snowH;

				if (maxH0 < commonH) {
					if (maxH1 < maxH0) maxH1 = maxH0;
					maxH0 = commonH;
				}
				else if (maxH1 < commonH) maxH1 = commonH;

				float tmp = maxH0;
				maxH0 = max(maxH0, maxH1);
				maxH1 = min(tmp, maxH1);
			}

			Material Height2Material(float rockH, float gravelH, float dirtH, float ForestH, float grassH, float snowH, float commonH, float maxH) {
				if (maxH == ForestH) return Forest();
				else if (maxH == snowH) return Snow(WSGT.y);
				else if (maxH == gravelH) return Gravel(wetness, gravelH);
				else if (maxH == grassH) return Grass(wetness, grassH);
				else if (maxH == dirtH) return Dirt(wetness, dirtH);
				else if (maxH == rockH || maxH == commonH) return Rock(wetness, rockH, commonH);

				return Dirt(wetness, dirtH);
			}

			float erfc(float x) {
				return 2.0 * exp(-x * x) / (2.319 * x + sqrt(4.0 + 1.52 * x * x));
			}

			float Lambda(float cosTheta, float sigmaSq) {
				float v = cosTheta / sqrt((1.0 - cosTheta * cosTheta) * (2.0 * sigmaSq));
				return max(0.0, (exp(-v * v) - v * M_SQRT_PI * erfc(v)) / (2.0 * v * M_SQRT_PI));
				//return (exp(-v * v)) / (2.0 * v * sqrt(3.141592)); // approximate, faster formula
			}

			// V, N, Tx, Ty in world space
			float2 U(float2 zeta, float3 V, float3 N, float3 Tx, float3 Ty) {
				float3 f = normalize(float3(-zeta, 1.0)); // tangent space
				float3 F = f.x * Tx + f.y * Ty + f.z * N; // world space
				float3 R = 2.0 * dot(F, V) * F - V;
				return R.xy / (1.0 + R.z);
			}

			// V, N, Tx, Ty in world space;
			float3 meanSkyRadiance(float3 V, float3 N, float3 Tx, float3 Ty, float2 sigmaSq) {
				float4 result;

				const float eps = 0.001;
				float3 wo = reflect(-V, N);

				float2 u0 = U(float2(0,0), V, N, Tx, Ty);

				float3 dux = float3(2.0 * (U(float2(eps, 0.0), V, N, Tx, Ty) - u0) / eps * sqrt(sigmaSq.x),0);
				float3 duy = float3(2.0 * (U(float2(0.0, eps), V, N, Tx, Ty) - u0) / eps * sqrt(sigmaSq.y),0);

				half mip = max(0.0, max(length(dux.xzy * (0.5 / 1.1)), length(duy.xzy * (0.5 / 1.1)))*9);

				result = float4(DecodeHDR(UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, wo.xzy, mip), unity_SpecCube0_HDR), 1.0);

				return result.rgb;
			}

			// L, V, N, Tx, Ty in world space
			float reflectedSunRadiance(float3 L, float3 V, float3 N, float3 Tx, float3 Ty, float2 sigmaSq) {
				float3 H = normalize(L + V);

				float zH = dot(H, N); // cos of facet normal zenith angle

				float zetax = dot(H, Tx) / zH;
				float zetay = dot(H, Ty) / zH;

				float zL = dot(L, N); // cos of source zenith angle
				float zV = dot(V, N); // cos of receiver zenith angle
				float zH2 = zH * zH;

				float p = exp(-0.5 * (zetax * zetax / sigmaSq.x + zetay * zetay / sigmaSq.y)) / (M_2PI * sqrt(sigmaSq.x * sigmaSq.y));

				float tanV = atan2(dot(V, Ty), dot(V, Tx));
				float cosV2 = 1.0 / (1.0 + tanV * tanV);
				float sigmaV2 = sigmaSq.x * cosV2 + sigmaSq.y * (1.0 - cosV2);

				float tanL = atan2(dot(L, Ty), dot(L, Tx));
				float cosL2 = 1.0 / (1.0 + tanL * tanL);
				float sigmaL2 = sigmaSq.x * cosL2 + sigmaSq.y * (1.0 - cosL2);

				float fresnel = 0.02 + 0.98 * pow(1.0 - dot(V, H), 5.0);

				zL = max(zL, 0.01);
				zV = max(zV, 0.01);

				return fresnel * p / ((1.0 + Lambda(zL, sigmaL2) + Lambda(zV, sigmaV2)) * zV * zH2 * zH2 * 4.0);
			}

			float3 Esky(float sH) {
				if (sH < -0.15) return float3(0, 0, 0);				
				if (sH < 0) return lerp(float3(0, 0, 0), float3(0.40, 0.42, 0.39), (sH + 0.15) / 0.15);
				if (sH == 0) return float3(0.40, 0.42, 0.39);
				if (sH < 0.1) return lerp(float3(0.40, 0.42, 0.39), float3(0.6, 0.75, 0.83), sH / 0.1);
				if (sH == 0.1) return float3(0.6, 0.75, 0.83);
				if (sH < 1) return lerp(float3(0.6, 0.75, 0.83), float3(0.72, 0.92, 1), (sH - 0.1) / 0.9);
				else return float3(0.72, 0.92, 1);
			}

			float4 Water(v2f input, float weight) {
				if (_MaterialDebug == 1) return _WaterDebug;
				Material shore;
				float ShoreHeight = 0;			

				weight = (weight - 0.1) / 0.9;
				
				float3 dPdu = input.dPdu;
				float3 dPdv = input.dPdv;
				float2 sigmaSq = input.sigmaSq;

				float iMAX = min(ceil((log2(_nyquistMax * Depth / input.lod) - _lods.z) * _lods.w), _nbWaves - 1.0);
				float iMax = floor((log2(_nyquistMin * Depth / input.lod) - _lods.z) * _lods.w);
				float iMin = max(0.0, floor((log2(_nyquistMin * _lods.y / input.lod) - _lods.z) * _lods.w));
				
				float dP = input.dP;

				[loop]
				for (float i = iMin; i <= iMAX; i += 1.0) {
					float4 wt = tex2Dlod(_wavesSampler, float4((i + 0.5f) / _nbWaves, 0, 0, 0));
					float phase = wt.y * _Time.y/5 - dot(wt.zw, input.u);
					float s = sin(phase);
					float c = cos(phase);
					float overk = 9.81 / (wt.y * wt.y);

					float wp = smoothstep(_nyquistMin, _nyquistMax, (M_2PI) * overk / _lods.y);
					float wn = smoothstep(_nyquistMin, _nyquistMax, (M_2PI) * overk / _lods.y * input.lod);

					float3 factor = (1.0 - wp) * wn * wt.x * float3(wt.zw * overk, 1.0);

					dP += factor * float3(s, c, s);

					float3 dPd = factor * float3(c, c, -s);
					dPdu -= dPd * wt.z;
					dPdv -= dPd * wt.w;

					wt.zw *= overk;
					float kh = i < iMax ? wt.x / overk : 0.0;
					float wkh = (1.0 - wn) * kh;
					sigmaSq -= float2(wt.z * wt.z, wt.w * wt.w) * (sqrt(1.0 - wkh * wkh) - sqrt(1.0 - kh * kh));
				}

				sigmaSq = max(sigmaSq, 2e-5);

				float3 windNormal = normalize(cross(dPdu, dPdv));

				float3 N = float3(mul(_windToWorld, float4(windNormal.xy, 0, 0)).xy, windNormal.z);				
				
				if (dot(V.xzy, N) < 0.0) {
					N = reflect(N, V.xzy); // reflects backfacing normals
				}			
				
				float3 Ty = normalize(cross(N, float3(_windToWorld[0].xy, 0.0)));
				float3 Tx = cross(Ty, N);

				float fresnel = 0.02 + 0.98 * meanFresnel(V.xzy, N, sigmaSq);

				float3 Lsun = Shadows > 0 ? reflectedSunRadiance(L.xzy, V.xzy, N, Tx, Ty, sigmaSq) * _LightColor0 * Shadows : 0;

				float3 Lsky = fresnel * meanSkyRadiance(V.xzy, N, Tx, Ty, sigmaSq) * saturate(Shadows + 0.25);

				float3 Lsea = (1 - fresnel) * lerp(_WaterColor.rgb, satellite.rgb, 0.5) * _WaterColor.a * Esky(L.y * saturate(Shadows + 0.1)) * M_1_PI;

				float4 WaterColor = float4(Lsea + Lsky + Lsun, 1.0);

				float shoreWeight = min(0, (0.1 - weight)/0.9);

				weight += weight >= 1 ? 0 : weight*dP;

				if (weight < 1) {

					gravelTexLod = log2(_GravelUVDetailMultiply * texLod);
					gravelTexLod = floor(gravelTexLod < 0 ? 0 : gravelTexLod);
					commonTexLod = log2(_CommonUVDetailMultiply * texLod);
					commonTexLod = floor(commonTexLod < 0 ? 0 : commonTexLod);
					rockTexLod = log2(_RockUVDetailMultiply * texLod);
					rockTexLod = floor(rockTexLod < 0 ? 0 : rockTexLod);

					if (Depth < _LODDistance1) {					

						float gravelH = GravelH(1, shoreWeight);
						float commonH = CommonH(1, shoreWeight);

						ShoreHeight = max(gravelH, commonH);

						if (ShoreHeight < 0.5 + weight * dP)
						{
							UV = lerp(UV + N.xy*weight*0.0002, UV, pow(saturate(ShoreHeight + 0.3 - dP), 2));
							bool tmp = _parallax;
							_parallax = 0;
							gravelH = GravelH(1, shoreWeight);
							commonH = CommonH(1, shoreWeight);
							_parallax = tmp;

							ShoreHeight = max(gravelH, commonH);
						}

						float maxH0;
						float maxH1;

						findMaxH(0, gravelH, 0, 0, 0, 0, commonH, maxH0, maxH1);

						float heightBlend = 1.0*(Depth / _LODDistance2);

						if (maxH1 == 0 || maxH0 - maxH1 >= heightBlend) shore = Height2Material(0, gravelH, 0, 0, 0, 0, commonH, maxH0);

						else {
							Material MaterialH0 = Height2Material(0, gravelH, 0, 0, 0, 0, commonH, maxH0);
							Material MaterialH1 = Height2Material(0, gravelH, 0, 0, 0, 0, commonH, maxH1);

							shore = lerpMaterial(MaterialH0, MaterialH1, 0.5*pow(saturate(1 - (maxH0 - maxH1) / heightBlend), 2));
						}

						if (ShoreHeight >= 0.5 + weight * dP) {
							return microfacet(shore);
						}
					}
					else {
						shore = Gravel(pow(2 * weight, 0.5), 1);
					}
				}

				if (weight >= 1) {
					return WaterColor;
				}
				else {
					shore.Roughness = 1;
					float3 WaterHSL = RGB2HSL(WaterColor.rgb);
					WaterHSL.g = lerp(WaterHSL.g, 0, pow(saturate(ShoreHeight + 0.3 - dP), 2));
					WaterColor = float4(HSL2RGB(WaterHSL), 1);
					return lerp(WaterColor, microfacet(shore), pow(saturate(ShoreHeight + 0.3 - dP), 2));
				}
			} 			


			float4 frag(v2f _input) : COLOR
			{
				v2f input = _input;

				Shadows = SHADOW_ATTENUATION(input);

				float3 Tangent = normalize(input.tangent);
				float3 Biangent = normalize(input.bitangent);

				VertexNormal = normalize(input.normal);

				V = normalize(_WorldSpaceCameraPos - input.worldSpacePosition.xyz);
				L = normalize(input.lightDirection.xyz);
				H = normalize(V + L);
				UV = input.uv;

				if (_ShaderTest == 1) {
					Normal = VertexNormal;

					tspace0 = float3(Tangent.x, Biangent.x, Normal.x);
					tspace1 = float3(Tangent.y, Biangent.y, Normal.y);
					tspace2 = float3(Tangent.z, Biangent.z, Normal.z);

					Material def;
					def.Albedo = _Color;
					def.Roughness = _Roughness;
					def.Normal = float3(0, 0, 1);

					return microfacet(def);
				}
				
				satellite = tex2D(_Sat, input.uv);

				Material satmat;
				satmat.Albedo = satellite;
				satmat.Roughness = 1;
				satmat.Normal = float3(0, 0, 1);

				float ns = 0.1;
				if (_enableNormalMap) {
					float3 n = Texture2Normal();
					Normal = normalize(VertexNormal + ns * float3(n.x, 0, n.y));
				}
				else {
					Normal = VertexNormal;
				}

				tspace0 = float3(Tangent.x, Biangent.x, Normal.x);
				tspace1 = float3(Tangent.y, Biangent.y, Normal.y);
				tspace2 = float3(Tangent.z, Biangent.z, Normal.z);				
								

				if (input.pos.x - _SatelliteProportion > 0) {

					blendSize = 0.15;

					Depth = length(_WorldSpaceCameraPos - input.worldSpacePosition.xyz);

					if (_LODDebug == 1) {
						if (Depth < _LODDistance0) {
							if (Depth >= _LODDistance0 * (1 - blendSize)) return lerp(float4(1, 0, 0, 1), float4(0.5, 0.5, 0, 1), (Depth - _LODDistance0 * (1 - blendSize)) / (_LODDistance0*blendSize));
							else return float4(1, 0, 0, 1);
						}
						else if (Depth < _LODDistance1) {
							if (Depth >= _LODDistance1 * (1 - blendSize)) return lerp(float4(0.5, 0.5, 0, 1), float4(0, 1, 0, 1), (Depth - _LODDistance1 * (1 - blendSize)) / (_LODDistance1*blendSize));
							else return float4(0.5, 0.5, 0, 1);
						}
						else if (Depth < _LODDistance2) {
							if (Depth >= _LODDistance2 * (1 - blendSize)) return lerp(float4(0, 1, 0, 1), float4(0, 0.5, 0.5, 1), (Depth - _LODDistance2 * (1 - blendSize)) / (_LODDistance2*blendSize));
							else return float4(0, 1, 0, 1);
						}
						else if (Depth < _LODDistance3) {
							if (Depth >= _LODDistance3 * (1 - blendSize)) return lerp(float4(0, 0.5, 0.5, 1), float4(0, 0, 1, 1), (Depth - _LODDistance3 * (1 - blendSize)) / (_LODDistance3*blendSize));
							else return float4(0, 0.5, 0.5, 1);
						}
						else if (Depth < _LODDistance4) {
							if (Depth >= _LODDistance4 * (1 - blendSize)) return lerp(float4(0, 0, 1, 1), float4(0, 0, 0, 1), (Depth - _LODDistance4 * (1 - blendSize)) / (_LODDistance4*blendSize));
							else return float4(0, 0, 1, 1);
						}
						else {
							return float4(0, 0, 0, 0);
						}
					}
					
					float4 Color = float4(0, 0, 0, 1.0);					
					WSGT = tex2D(_ClassesWSGT, input.uv);
					WSGT = saturate(sin(M_PI * (WSGT - 0.5))*0.5 + 0.5);

					WSGT.w = (1.0 - WSGT.w);
					if (Depth < _LODDistance3) {
						DGR = tex2D(_ClassesDGR, input.uv);
						DGR = saturate(sin(M_PI * (DGR - 0.5))*0.5 + 0.5);

						if (_SlopeModifierEnabled == 1) {
							float slopeRockModifier = saturate((-Normal.y + _SlopeModifierThreshold) * _SlopeModifierStrength);

							if (_SlopeModifierDebug == 1) return slopeRockModifier;

							DGR.z += slopeRockModifier;
						}

						float waterH = WSGT.x;
						float snowH = 0;
						float grassH = 0;
						float forestH = 0;
						float rockH = 0;
						float dirtH = 0;
						float gravelH = 0;
						float commonH = 0;
						
						float sum = DGR.x + DGR.y + DGR.z + WSGT.x + WSGT.y + WSGT.z + WSGT.w;
						DGR /= sum;
						WSGT /= sum;	

						float VdotN = dot(V, VertexNormal);
						if (VdotN < 1.0 - 1e-7) {
							float theta = M_PI / 2.0 - acos(VdotN);
							float m_1_cosTheta = 1.0 / cos(theta);
							float tanTheta = tan(theta);
							float x = _2TanFOVHeight * Depth / (tanTheta - _2TanFOVHeight);
							float y = _2TanFOVHeight * Depth / (tanTheta + _2TanFOVHeight);

							texLod = x * m_1_cosTheta + y * m_1_cosTheta;
							texLod /= 1.8;
						}
						else {
							texLod = _2TanFOVHeight * Depth;
						}

						if (waterH > 0.1) Color =  Water(input, waterH);
						else {							
							satellite = tex2D(_Sat, input.uv);

							wetness = pow(saturate(3.5*WSGT.y), 1.5);

							float4 ColorH = float4(0, 0, 0, 0);
							float4 ColorB = float4(0, 0, 0, 0);
							Material MaterialB;

							/* CALCULATE TEXTURES LODS */
							{
								//Snow
								if (WSGT.y > 0)
								{
									snowTexLod = log2(_SnowUVDetailMultiply * texLod);
									snowTexLod = floor(snowTexLod < 0 ? 0 : snowTexLod);
								}
								//Grass
								if (WSGT.z > 0)
								{
									grassTexLod = log2(_GrassUVDetailMultiply * texLod);
									grassTexLod = floor(grassTexLod < 0 ? 0 : grassTexLod);
								}
								//Dirt
								if (DGR.x > 0)
								{
									dirtTexLod = log2(_DirtUVDetailMultiply * texLod);
									dirtTexLod = floor(dirtTexLod < 0 ? 0 : dirtTexLod);
								}
								//Gravel
								if (DGR.y > 0 || (waterH > 0))
								{
									gravelTexLod = log2(_GravelUVDetailMultiply * texLod);
									gravelTexLod = floor(gravelTexLod < 0 ? 0 : gravelTexLod);
								}
								//Rock
								if (DGR.z > 0 || DGR.y > 0 || DGR.x > 0 || WSGT.z > 0 || (waterH > 0))
								{
									rockTexLod = log2(_RockUVDetailMultiply * texLod);
									rockTexLod = floor(rockTexLod < 0 ? 0 : rockTexLod);
								}
								//Common
								if (DGR.y > 0 || DGR.x > 0 || WSGT.z > 0 || (waterH > 0)) {
									commonTexLod = log2(_CommonUVDetailMultiply * texLod);
									commonTexLod = floor(commonTexLod < 0 ? 0 : commonTexLod);
								}
							}

							if (Depth < _LODDistance2) {
								//Snow
								if (WSGT.y > 0)
								{
									snowH = SnowH(WSGT.y);
								}
								//Grass
								if (WSGT.z > 0)
								{
									grassH = GrassH(WSGT.z);
								}
								//Forest
								if (WSGT.w > 0)
								{
									forestH = ForestH(WSGT.w);
								}
								//Dirt
								if (DGR.x > 0)
								{
									dirtH = DirtH(DGR.x);
								}
								//Gravel
								if (DGR.y > 0 || (waterH > 0))
								{
									gravelH = GravelH(max(DGR.y, saturate(10 * waterH)), 0);
								}
								//Rock
								if (DGR.z > 0)
								{
									rockH = RockH(DGR.z);
								}
								//Common
								if (DGR.y > 0 || DGR.x > 0 || WSGT.z > 0 || (waterH > 0))
								{
									commonH = CommonH(max(DGR.y, max(DGR.x, max(WSGT.z, saturate((10 * waterH))))), 0, max(gravelH, max(dirtH, grassH)));
								}

								float maxH0;
								float maxH1;

								findMaxH(rockH, gravelH, dirtH, forestH, grassH, snowH, commonH, maxH0, maxH1);

								float heightBlend = 1.0*(Depth / _LODDistance2);

								Material MaterialH;

								if (maxH1 == 0 || maxH0 - maxH1 >= heightBlend) MaterialH = Height2Material(rockH, gravelH, dirtH, forestH, grassH, snowH, commonH, maxH0);
								
								else {
									Material MaterialH0 = Height2Material(rockH, gravelH, dirtH, forestH, grassH, snowH, commonH, maxH0);
									Material MaterialH1 = Height2Material(rockH, gravelH, dirtH, forestH, grassH, snowH, commonH, maxH1);

									MaterialH = lerpMaterial(MaterialH0, MaterialH1, 0.5*pow(saturate(1 - (maxH0 - maxH1) / heightBlend), 2));
								}
								
								ColorH = microfacet(MaterialH);
							}

							if (Depth >= _LODDistance2 * (1 - blendSize)) {
								float4 water = float4(0, 0, 0, 0);
								Material snow = satmat;
								Material grass = satmat;
								Material forest = satmat;
								Material dirt = satmat;
								Material gravel = satmat;
								Material rock = satmat;

								//Snow
								if (WSGT.y > 0)
								{
									snow = Snow(WSGT.y);
								}
								//Grass
								if (WSGT.z > 0)
								{
									grass = Grass(wetness, 0);
								}
								//Forest
								if (WSGT.w > 0)
								{
									if (_parallax) ForestH(WSGT.w);
									forest = Forest();
								}
								//Dirt
								if (DGR.x > 0)
								{
									dirt = Dirt(wetness, 0);
								}
								//Gravel
								if (DGR.y > 0 || WSGT.x > 0)
								{
									gravel = Gravel(wetness, 0);
								}
								//Rock
								if (DGR.z > 0)
								{
									rock = Rock(wetness, 1, 0);
								}

								MaterialB.Albedo = snow.Albedo * WSGT.y + grass.Albedo * WSGT.z + forest.Albedo * WSGT.w + dirt.Albedo * DGR.x + gravel.Albedo * max(DGR.y, WSGT.x) + rock.Albedo * DGR.z;
								MaterialB.Roughness = snow.Roughness * WSGT.y + grass.Roughness * WSGT.z + forest.Roughness * WSGT.w + dirt.Roughness * DGR.x + gravel.Roughness * max(DGR.y, WSGT.x) + rock.Roughness * DGR.z;
								MaterialB.Normal = normalize(snow.Normal * WSGT.y + grass.Normal * WSGT.z + forest.Normal * WSGT.w + dirt.Normal * DGR.x + gravel.Normal * max(DGR.y, WSGT.x) + rock.Normal * DGR.z);

								ColorB = microfacet(MaterialB);

								if (waterH > 0) ColorB = ColorB + float4(Water(input, waterH).rgb * waterH, 1.0);
							}


							if (Depth < _LODDistance2 && Depth >= _LODDistance2 * (1 - blendSize))
								Color = lerp(ColorH, ColorB, saturate((Depth - _LODDistance2 * (1 - blendSize)) / (_LODDistance2*blendSize)));
							else if (Depth < _LODDistance2)
								Color = ColorH;
							else
								Color = ColorB;


							if (Depth >= _LODDistance3 * (1 - blendSize)) {
								Color = lerp(Color, microfacet(satmat), (Depth - _LODDistance3 * (1 - blendSize)) / (_LODDistance3*blendSize));
							}
						}						
					}					
					else if (WSGT.x > 0.1) {
						Color = Water(input, 1);
					}
					else if (Depth < _LODDistance4) {
						Color = microfacet(satmat);

						if (Depth >= _LODDistance4 * (1 - blendSize)) {
							Color = lerp(Color, satellite, (Depth - _LODDistance4 * (1 - blendSize)) / (_LODDistance4*blendSize));
						}
					}
					else Color = float4(satellite.rgb, 1.0);


					return float4(Color.rgb, 1.0);
					
				}
				else
				{
					return microfacet(satmat);
					return float4(satellite.rgb, 1.0);
				}

			}
			ENDCG
		}
	}

	FallBack "Diffuse"
}
