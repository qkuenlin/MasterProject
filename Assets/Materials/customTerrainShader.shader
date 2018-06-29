// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/customTerrainShader_hlsl" {
	Properties{
		_ClassesDGR("Segmentation Image Dirt/Gravel/Rock", 2D) = "white" {}
		_ClassesWSGT("Segmentation Image Water/Snow/Grass/Forest", 2D) = "white" {}
		_Sat("Satellite Image", 2D) = "white" {}
	}
	SubShader
	{
		Pass
		{
			Tags { "LightMode" = "ForwardBase" }
			LOD 200
			CGPROGRAM

			#include "AutoLight.cginc"
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityStandardBRDF.cginc"
			#include "UnityGlobalIllumination.cginc"
			#include "UnityStandardConfig.cginc"
			#include "UnityImageBasedLighting.cginc"


			#pragma vertex vertex //vert
			#pragma fragment frag
			//#pragma hull hull
			//#pragma domain domain
			//#pragma geometry geometry
			#pragma require 2darray
			#pragma multi_compile_fwdbase


			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 5.0

			float4x4 screenToCamera;
			float4x4 cameraToWorld;

			/* WATER PARAMETERS*/
			half _WaterRoughness;
			fixed4 _WaterColor;
			int4 GRID_SIZE;
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
			samplerCUBE _ReflectionCubeMap;

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
			int _heightBasedMix;
			int _tesselation;
			float _SatelliteProportion;
			int _enableNoise;
			float _noiseStrength;

			int _enableNoiseHue;
			float _noiseHueStrength;

			int _enableNoiseLum;
			float _noiseLumStrength;

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

			struct v2f
			{
				float2 uv : TEXCOORD0;
				//SHADOW_COORDS(1)
				float4 pos : SV_POSITION;
				float4 worldSpacePosition : TEXCOORD2;
				float3 lightDirection: TEXCOORD6;

				float3 _dPdu : TEXCOORD7;
				float3 _dPdv : TEXCOORD8;

				half3 tangent : TEXCOORD3; // tangent.x, bitangent.x, normal.x
				half3 bitangent : TEXCOORD4; // tangent.y, bitangent.y, normal.y
				half3 normal : TEXCOORD5; // tangent.z, bitangent.z, normal.z

				float lod : TEXCOORD9;
				float2 u : TEXCOORD10;
				float3 dPdu : TEXCOORD11;
				float3 dPdv : TEXCOORD12;
				float2 sigmaSq : TEXCOORD13;
				float3 P : TEXCOORD14;

				LIGHTING_COORDS(15,16)

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

			float RockH(v2f input, float weight) {
				float hL = (UNITY_SAMPLE_TEX2DARRAY_LOD(_HeightTextures, float3(input.uv*_RockUVLargeMultiply, 0.0 + _N_NOISE),0)).r;
				return ((hL)*_RockHeightStrength / (2.0) + _RockHeightOffset) * weight;
			}

			float SnowH(v2f input, float weight) {		
				float hL = (UNITY_SAMPLE_TEX2DARRAY_LOD(_HeightTextures, float3(input.uv*_SnowUVLargeMultiply, 2.0 + _N_NOISE),0)).r;
				return ((hL)*_SnowHeightStrength / (2.0) + _SnowHeightOffset) * weight;
			}

			appdata_tan vert(appdata_tan v) {
				return v;
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

				//TRANSFER_SHADOW(o)

				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldSpacePosition = mul(unity_ObjectToWorld, v.vertex);
				o.lightDirection = _WorldSpaceLightPos0.xyz - o.worldSpacePosition.xyz * _WorldSpaceLightPos0.w;

				/* DISPLACEMENT */			
				/*
				float4 DGR = tex2Dlod(_ClassesDGR, float4(o.uv, 0, 0));
				float4 WSGT = tex2Dlod(_ClassesWSGT, float4(o.uv, 0, 0));
				float snowH = 0;
				float rockH = 0;
				//Snow
				if (WSGT.y > 0)
				{
					snowH = SnowH(o, WSGT.y);
				}
				//Rock
				if (DGR.z > 0)
				{
					rockH = RockH(o, DGR.z)+1.5*DGR.z;
				}
				float disp = 0;
				if (_heightBasedMix == 1) {
					disp = max(rockH, snowH);
				}
				else {
					disp = rockH*DGR.z + snowH*WSGT.y;
				}

				float4 pos = v.vertex + float4(0, 1.5*disp, 0, 0);
				o.pos = UnityObjectToClipPos(pos);
				o.worldSpacePosition = mul(unity_ObjectToWorld, pos);
				*/
				/* OCEAN SIMULATION VERTEX PART*/
								
				//if (tex2Dlod(_ClassesWSGT, float4(o.uv, 0, 1)).x > 0)
				{
					float3 cameraDir = normalize(mul(screenToCamera, v.vertex).xyz);
					float3 worldDir = mul(cameraToWorld, float4(cameraDir, 0.0)).xyz;

					float t = length(_WorldSpaceCameraPos - o.worldSpacePosition.xyz);

					o.lod = _lods.y;//length(_WorldSpaceCameraPos - o.worldSpacePosition.xyz); // size in meters of one grid cell, projected on the sea surface

					o.u =  mul(_worldToWind, float4(v.vertex.xz , 0, 0) ).xy;//mul(_worldToWind, _WorldSpaceCameraPos.xz + t * worldDir.xy);

					float3 dPdu = float3(1.0, 0.0, 0.0);
					float3 dPdv = float3(0.0, 0.0, 1.0);
					float2 sigmaSq = _sigmaSqTotal;

					float3 dP = float3(0.0, 0.0, 0.0);

					float iMin = max(0.0, floor((log2(_nyquistMin * o.lod) - _lods.z) * _lods.w));

					for (float i = iMin; i < _nbWaves; i += 1.0f)
					{
						float4 wt = tex2Dlod(_wavesSampler, float4((i + 0.5) / _nbWaves, 0,0,0));
						float phase = wt.y * _Time.x - dot(wt.zw, o.u);
						float s = sin(phase);
						float c = cos(phase);
						float overk = 9.81f / (wt.y * wt.y);

						float wp = smoothstep(_nyquistMin, _nyquistMax, (2.0 * 3.141592) * overk / o.lod);

						float3 factor = wp* wt.x * float3(wt.z * overk, 1.0, wt.w * overk);
						dP += factor * float3(s, c, s);

						float3 dPd = factor * float3(c, -s, c);
						dPdu -= dPd * wt.z;
						dPdv -= dPd * wt.w;

						wt.zw *= overk;
						float kh = wt.x / overk;
						sigmaSq -= float2(wt.z * wt.z, wt.w * wt.w) * (1.0 - sqrt(1.0 - kh * kh));
					}

					o.P = v.vertex.xyz + dP.xyz;
					
					if (tex2Dlod(_ClassesWSGT, float4(o.uv, 0, 1)).x > 0.5) {
						o.pos = UnityObjectToClipPos(float4(o.P.x, o.P.y, o.P.z, 1.0));
						o.worldSpacePosition = mul(unity_ObjectToWorld, float4(o.P.x, o.P.y, o.P.z, 1.0));
					}					

					o.dPdu = dPdu;
					o.dPdv = dPdv;
					o.sigmaSq = sigmaSq;

				}

				TRANSFER_VERTEX_TO_FRAGMENT(o);

				return(o);
			}

			/*
			struct TesselationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			float TesselationEdgeFactor(float3 p0, float3 p1)
			{
				float edgeLength = distance(p0, p1);

				float3 edgeCenter = (p0 + p1)*0.5;
				float viewDistance = distance(edgeCenter, _WorldSpaceCameraPos);
				return clamp(pow(64.0*edgeLength /viewDistance, 2), 1, 1024);
			}

			TesselationFactors MyPatchConstantFunction(InputPatch<appdata_tan, 3> patch)
			{
				TesselationFactors f;

				if (_tesselation == 0) {
					f.edge[0] = 1;
					f.edge[1] = 1;
					f.edge[2] = 1;
					f.inside = 1;
				}
				else {
					float3 p0 = mul(unity_ObjectToWorld, patch[0].vertex).xyz;
					float3 p1 = mul(unity_ObjectToWorld, patch[1].vertex).xyz;
					float3 p2 = mul(unity_ObjectToWorld, patch[2].vertex).xyz;

					f.edge[0] = TesselationEdgeFactor(p1, p2);
					f.edge[1] = TesselationEdgeFactor(p2, p0);
					f.edge[2] = TesselationEdgeFactor(p0, p1);
					f.inside = (TesselationEdgeFactor(p1, p2) + TesselationEdgeFactor(p2, p0) + TesselationEdgeFactor(p0, p1)) / 3.0;
				}
				return f;
			}

			[UNITY_domain("tri")]
			[UNITY_outputcontrolpoints(3)]
			[UNITY_outputtopology("triangle_cw")]
			[UNITY_partitioning("integer")]
			[UNITY_patchconstantfunc("MyPatchConstantFunction")]
			appdata_tan hull(InputPatch<appdata_tan, 3> patch, uint id : SV_OutputControlPointID) {
				return patch[id];
			}

			[UNITY_domain("tri")]
			v2f domain(TesselationFactors factors, OutputPatch<appdata_tan, 3> patch, float3 barycentricCoordinates: SV_DomainLocation) {
				appdata_tan data;
				#define MY_DOMAIN_PROGRAM_INTEROPLATE(fieldName) data.fieldName = patch[0].fieldName * barycentricCoordinates.x + patch[1].fieldName * barycentricCoordinates.y + patch[2].fieldName * barycentricCoordinates.z;

				MY_DOMAIN_PROGRAM_INTEROPLATE(vertex);
				MY_DOMAIN_PROGRAM_INTEROPLATE(normal);
				MY_DOMAIN_PROGRAM_INTEROPLATE(tangent);
				MY_DOMAIN_PROGRAM_INTEROPLATE(texcoord);

				return vertex(data);
			}

			struct InterpolatorGeometry {
				v2f data;
				float2 barycentric : TEXCOORD15;
			};

			[maxvertexcount(3)]
			void geometry(triangle v2f i[3], inout TriangleStream<InterpolatorGeometry> stream) {
				InterpolatorGeometry o0, o1, o2;			

				o0.data = i[0];
				o1.data = i[1];
				o2.data = i[2];

				o0.barycentric = float2(1, 0);
				o1.barycentric = float2(0, 1);
				o2.barycentric = float2(0, 0);

				stream.Append(o0);
				stream.Append(o1);
				stream.Append(o2);

			}
			*/
			
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
			float LOD;
			float4 satellite;
			float4 Noise;
			float4 Noise_Classes;
			float2 UV;

			float blendSize;

			float wetness;

			float3 tspace0;
			float3 tspace1;
			float3 tspace2;

			float DetailMaterialWeight;


			float4 DGR;
			float4 WSGT;

			float M_PI = 3.141592f;

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


			float3 LAB2RGB(float3 color) {
				float y = (color.x + 16.0) / 116.0;
				float x = color.y / 500.0 + y;
				float z = y - color.z / 200.0;

				x = 0.95047 * ((x * x * x > 0.008856) ? x * x * x : (x - 16.0 / 116.0) / 7.787);
				y = 1.00000 * ((y * y * y > 0.008856) ? y * y * y : (y - 16.0 / 116.0) / 7.787);
				z = 1.08883 * ((z * z * z > 0.008856) ? z * z * z : (z - 16.0 / 116.0) / 7.787);

				float r = x * 3.2406 + y * -1.5372 + z * -0.4986;
				float g = x * -0.9689 + y * 1.8758 + z * 0.0415;
				float b = x * 0.0557 + y * -0.2040 + z * 1.0570;

				r = (r > 0.0031308) ? (1.055 * pow(r, 1.0 / 2.4) - 0.055) : 12.92 * r;
				g = (g > 0.0031308) ? (1.055 * pow(g, 1.0 / 2.4) - 0.055) : 12.92 * g;
				b = (b > 0.0031308) ? (1.055 * pow(b, 1.0 / 2.4) - 0.055) : 12.92 * b;

				return saturate(float3(r, g, b));
			}

			float3 RGB2LAB(float3 color) {
				float r = color.r;
				float g = color.g;
				float b = color.b;

				r = (r > 0.04045) ? pow((r + 0.055) / 1.055, 2.4) : r / 12.92;
				g = (g > 0.04045) ? pow((g + 0.055) / 1.055, 2.4) : g / 12.92;
				b = (b > 0.04045) ? pow((b + 0.055) / 1.055, 2.4) : b / 12.92;

				float x = (r * 0.4124 + g * 0.3576 + b * 0.1805) / 0.95047;
				float y = (r * 0.2126 + g * 0.7152 + b * 0.0722) / 1.00000;
				float z = (r * 0.0193 + g * 0.1192 + b * 0.9505) / 1.08883;

				x = (x > 0.008856) ? pow(x, 1.0 / 3.0) : (7.787 * x) + 16.0 / 116.0;
				y = (y > 0.008856) ? pow(y, 1.0 / 3.0) : (7.787 * y) + 16.0 / 116.0;
				z = (z > 0.008856) ? pow(z, 1.0 / 3.0) : (7.787 * z) + 16.0 / 116.0;

				return float3(116.0 * y - 16.0, 500.0 * (x - y), 200.0 * (y - z));
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

			float4 ColorPerturbation(float4 c) {
				float3 hsl = RGB2HSL(c.rgb);
				hsl.x += Noise.z * _noiseHueStrength;
				hsl.z = lerp(hsl.z, hsl.z*Noise.w, _noiseLumStrength);
				float3 rgb = HSL2RGB(hsl);
				return float4(rgb, 1.0);
			}

			float GGX(float3 N, float3 H, float a)
			{
				return (a*a) / (3.141592*pow(1.0+(dot(N, H)*dot(N, H))*(a*a - 1.0), 2.0));
			}

			float beckman(float cosThetaH, float m)
			{
				float cosThetaH2 = cosThetaH * cosThetaH;
				float e = (cosThetaH2 - 1) / (m*m*cosThetaH2);
				float tmp = 3.141592 * m*m*cosThetaH2*cosThetaH2;

				return exp(e) / tmp;
			}

			float blinnPhong(float cosThetaH, float m) {
				float n = 2.0 / (m * m) - 2.0;

				return (n + 2.0) / (2.0*3.141592) * pow(cosThetaH, n);
			}

			float fresnel(float cosTheta, float n2)
			{
				float cos_theta_m1 = 1 - cosTheta;
				float R0 = 0.02;

				return R0 + (1 - R0)*cos_theta_m1*cos_theta_m1*cos_theta_m1*cos_theta_m1*cos_theta_m1;
			}

			float smithShadowing(float cosThetaI, float cosThetaO, float m) {
				float a = m * 0.79788; //sqrt(2/pi)

				return 1.0 / ((cosThetaO * (1.0 - a) + a)*(cosThetaI*(1.0 - a) + a));
			}

			float V_SmithGGXCorrelated(float NdotL, float NdotV, float alphaG) {
				float alphaG2 = alphaG * alphaG;

				float Lambda_GGXV = NdotL * sqrt((-NdotV * alphaG2 + NdotV) * NdotV + alphaG2);
				float Lambda_GGXL = NdotV * sqrt((-NdotL * alphaG2 + NdotL) * NdotL + alphaG2);

				return 0.5f / (Lambda_GGXV + Lambda_GGXL);
			}

			float D_GGX(float NdotH, float m) {
				float m2 = m * m;
				float f = (NdotH * m2 - NdotH) * NdotH + 1;

				return m2 / (f*f);
			}

			float F_Schlick(float3 f0, float f90, float u) {
				return f0 + (f90 - f0) * pow(1.0f - u, 5.0f);
			}

			float Fr_DisneyDiffuse(float NdotV, float NdotL, float LdotH, float linearRoughness) {
				float energyBias = lerp(0, 0.5, linearRoughness);
				float energyFactor = lerp(1.0, 1.0 / 1.51, linearRoughness);
				float fd90 = energyBias + 2.0 * LdotH*LdotH*linearRoughness;
				float f0 = float3(1.0f, 1.0f, 1.0f);

				float lightScatter = F_Schlick(f0, fd90, NdotL).r;
				float viewScatter = F_Schlick(f0, fd90, NdotV).r;

				return lightScatter * viewScatter * energyFactor;
			}

			void importanceSampleCosDir(in float2 u, in float3 N, out float3 wo, out float NdotL, out float pdf) {
				//Build Local Referential
				float3 upVector = abs(N.z) < 0.999 ? float3(0, 0, 1) : float3(1, 0, 0);
				float3 tangentX = normalize(cross(upVector, N));
				float3 tangentY = cross(N, tangentX);

				float r = sqrt(u.x);
				float phi = u.y * 3.141592 * 2;

				wo = float3(r*cos(phi), r*sin(phi), sqrt(max(0.0f, 1.0f - u.x)));
				wo = normalize(tangentX * wo.y + tangentY * wo.x + N * wo.z);

				NdotL = dot(wo, N);
				pdf = NdotL / 3.141592;
			}

			/*
			float3 evaluateIBLDiffuseCubeReference(float3 N, float3 wi, float roughness) {
				float3 accLight = 0;

				int _sampleCount = 128;
				[unroll(128)]
				for (int i = 0; i < _sampleCount; ++i) {
					float2 u = Hammersley2d(i, _sampleCount);
					float3 wo;
					float NdotL;
					float pdf;
					importanceSampleCosDir(u, N, wo, NdotL, pdf);
					if (NdotL > 0) {
						float cosD = sqrt((dot(wi, wo) + 1.0f)*0.5);
						float NdotV = saturate(dot(N, wi));
						float NdotL_sat = saturate(NdotL);
						
						float fd90 = 0.5 + 2 * cosD*cosD*sqrt(roughness);
						float lightScatter = 1 + (fd90 - 1)*pow(1 - NdotL_sat, 5);
						float viewScatter = 1 + (fd90 - 1)*pow(1 - NdotV, 5);

						accLight += texCUBE(_ReflectionCubeMap, L).rgb * viewScatter * lightScatter;
					}
				}
				return accLight *(1.0f / _sampleCount);
			}
			
			// Based on Appendix A of Moving Frostbite to PBR
			float3 evaluateSpecularIBLReference(float3 N, float3 V, float roughness, float f0, float f90) {
				//Build Local Referential
				float3 upVector = abs(N.z) < 0.999 ? float3(0, 0, 1) : float3(1, 0, 0);
				float3 tangentX = normalize(cross(upVector, N));
				float3 tangentY = cross(N, tangentX);

				float3 accLight = 0;

				int _sampleCount = 32;
				for (int i = 0; i < _sampleCount; ++i) {
					float2 u = Hammersley2d(i, _sampleCount);

					float cosThetaH = sqrt((1 - u.x) / (1 + (roughness*roughness - 1)*u.x));
					float sinThetaH = sqrt(1 - min(1.0, cosThetaH*cosThetaH));
					float phiH = u.y * 3.141592 * 2;

					float3 H;
					H = float3(sinThetaH*cos(phiH), sinThetaH*sin(phiH), cosThetaH);
					H = normalize(tangentX * H.y + tangentY * H.x + N * H.z);
					L = normalize(2.0f * dot(V, H) * H - V);

					if (dot(L, N) > 0) {
						float LdotH = saturate(dot(H, L));
						float NdotH = saturate(dot(H, N));
						float NdotV = saturate(dot(V, N));
						float NdotL = saturate(dot(L, N));

						float D = D_GGX(NdotH, roughness);
						float pdfH = D * NdotH;
						float pdf = pdfH / (4.0f * LdotH);

						float3 F = F_Schlick(f0, f90, LdotH);
						float G = V_SmithGGXCorrelated(NdotL, NdotV, roughness);
						float weight = F * G * D / (4.0 * NdotV);

						if (pdf > 0 && weight > 0) {
							accLight += texCUBE(_ReflectionCubeMap, L).rgb * weight / pdf;
						}
					}
				}

				return accLight / _sampleCount;
			}
			*/
			float Geom(float3 v, float3 H, float3 N, float roughness)
			{
				float cosTheta = dot(v, N);
				float tmp = 1 - cosTheta * cosTheta;

				if (tmp <= 0) return 0.0;
				float tanTheta = abs(sqrt(tmp) / cosTheta);

				if (dot(v, H) / dot(v, N) <= 0) {
					return 0.0;
				}

				float b = 1.0f / (roughness * tanTheta);
				if (b > 1.6) return 1.0f;

				return (3.535f*b + 2.181f*b*b) / (1.0f + 2.276f*b + 2.577f*b*b);
			}

			float erfc(float x) {
				return 2.0 * exp(-x * x) / (2.319 * x + sqrt(4.0 + 1.52 * x * x));
			}

			float Lambda(float cosTheta, float sigmaSq) {
				float v = cosTheta / sqrt((1.0 - cosTheta * cosTheta) * (2.0 * sigmaSq));
				return max(0.0, (exp(-v * v) - v * sqrt(M_PI) * erfc(v)) / (2.0 * v * sqrt(M_PI)));
				//return (exp(-v * v)) / (2.0 * v * sqrt(M_PI)); // approximate, faster formula
			}

			// L, V, N, Tx, Ty in world space
			float reflectedSunRadiance(float3 N, float3 Tx, float3 Ty, float2 sigmaSq) {
				float zetax = dot(H, Tx) / dot(H, N);
				float zetay = dot(H, Ty) / dot(H, N);

				float zL = dot(L, N); // cos of source zenith angle
				float zV = dot(V, N); // cos of receiver zenith angle
				float zH = dot(H, N); // cos of facet normal zenith angle
				float zH2 = zH * zH;

				float p = exp(-0.5 * (zetax * zetax / sigmaSq.x + zetay * zetay / sigmaSq.y)) / (2.0 * M_PI * sqrt(sigmaSq.x * sigmaSq.y));

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

			float4 microfacet(Material m) {
				if (_MaterialDebug == 1) {
					return m.Albedo;
				}

				float3 Normal = float3(0, 0, 0);
				
				Normal.x = dot(tspace0, m.Normal.xyz);
				Normal.y = dot(tspace1, m.Normal.xyz);
				Normal.z = dot(tspace2, m.Normal.xyz);

				//return float4(Normal, 1.0);
				//return albedo * Fr_DisneyDiffuse(dot(Normal, V.xzy), dot(Normal, L.xzy), dot(Normal, H.xzy), roughness);
				
				float cosThetaO = dot(L, Normal);
				float cosThetaI = dot(V, Normal);
				float cosThetaH = dot(H, Normal);

				//return Fr_DisneyDiffuse(cosThetaI, cosThetaO, cosThetaH, sqrt(roughness));


				/* DIRECT ILLUMINATION + SPECULAR HIGHLIGHT*/
				fixed4 specular = fixed4(0.0, 0, 0, 0);
				if (cosThetaH > 0 && cosThetaO > 0) {
					float F = fresnel(dot(L, H), 1.5f);
					float D = blinnPhong(cosThetaH, m.Roughness) * 3.141592 / 4.0;
					float G = smithShadowing(cosThetaI, cosThetaO, m.Roughness);// (cosThetaO*cosThetaI);//Geom(V, H, Normal, roughness)*Geom(L, H, Normal, roughness) / (cosThetaO*cosThetaI);
					
					specular = saturate(D * F * G) * _LightColor0;
				}

				float dotNL_ = (cosThetaO+1)/2.0;
				dotNL_ *= dotNL_;
				fixed4 diffuse = saturate(m.Albedo *_LightColor0 * cosThetaO);

				float4 Direct = diffuse + (1 - m.Roughness)*specular;

				/* INDIRECT ILLUMINATION */
				/*
				float4 IndirectLight = float4(0, 0, 0, 0);
				for (int i = 0; i < 9; i++) {
					float3 wo = reflect(-V, Normal);
					switch (i) {
						case 0: break;
						case 1: wo = reflect(-V, normalize(Normal + float3(roughness, 0, 0))); break;
						case 2: wo = reflect(-V, normalize(Normal + float3(-roughness, 0, 0))); break;
						case 3: wo = reflect(-V, normalize(Normal + float3(0, 0, roughness))); break;
						case 4: wo = reflect(-V, normalize(Normal + float3(0, 0, -roughness))); break;
						case 5: wo = reflect(-V, normalize(Normal + float3(roughness/2, 0, roughness / 2))); break;
						case 6: wo = reflect(-V, normalize(Normal + float3(roughness/2, 0, -roughness / 2))); break;
						case 7: wo = reflect(-V, normalize(Normal + float3(-roughness / 2, 0, roughness/2))); break;
						case 8: wo = reflect(-V, normalize(Normal + float3(-roughness / 2, 0, -roughness/2))); break;
						default: break;
					} 

					IndirectLight += dot(wo, Normal) * texCUBElod(_ReflectionCubeMap, float4(wo, roughness*roughness * 7));
				}
				IndirectLight /= 9.0;
				*/

				half mip = perceptualRoughnessToMipmapLevel(m.Roughness);

				float3 wo = reflect(-V, Normal);

				float4 IndirectLight = dot(wo, Normal) * texCUBElod(_ReflectionCubeMap, float4(wo, mip));

				float4 Indirect = lerp(m.Albedo*IndirectLight, IndirectLight, (1-m.Roughness)*fresnel(dot(H, V), 1.33f));
				
				/* DIRECT REFLECTION */
				//float3 reflectDir = reflect(-V, Normal);
				//float4 Reflection = (1 - roughness)*texCUBElod(_ReflectionCubeMap, float4(reflectDir, roughness*roughness*9));
				
				return saturate(Direct + Indirect);
			}

			Material Gravel(float weight, float wetness, float height) {
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
					float lod = 9 * (sqrt(_GravelUVDetailMultiply)/10) * Depth / (_LODDistance1);

					float4 albedoD = (UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorTextures, float3(UV*_GravelUVDetailMultiply, 6+1.0), lod));
					float4 albedoDM = (UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorTextures, float3(UV*_GravelUVDetailMultiply, 6+1.0), 8+lod));

					float roughnessD = UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorTextures, float3(UV*_GravelUVDetailMultiply, 6+3.0), lod).x;

					float4 colorD = ColorTransfer(albedo, albedoD, albedoDM);

					float3 NormalD = _enableNormalMap ? UnpackNormal(UNITY_SAMPLE_TEX2DARRAY_LOD(_NormalTextures, float3(UV*_GravelUVDetailMultiply, 5), lod)) : float3(0, 0, 1);

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

				if (_enableNoiseHue) albedo = ColorPerturbation(albedo);

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

			Material Dirt(float weight, float wetness, float height) {
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
					float lod = 9 * (sqrt(_DirtUVDetailMultiply) / 10) * Depth / (_LODDistance1);

					float4 albedoD = (UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorTextures, float3(UV*_DirtUVDetailMultiply, 10 + 1.0), lod));
					float4 albedoDM = (UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorTextures, float3(UV*_DirtUVDetailMultiply, 10 + 1.0), 8 + lod));

					float roughnessD = UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorTextures, float3(UV*_DirtUVDetailMultiply, 10 + 3.0), lod).x;

					float4 colorD = ColorTransfer(albedo, albedoD, albedoDM);

					float3 NormalD = _enableNormalMap ? UnpackNormal(UNITY_SAMPLE_TEX2DARRAY_LOD(_NormalTextures, float3(UV*_DirtUVDetailMultiply, 7), lod)) : float3(0, 0, 1);

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

				if (_enableNoiseHue) albedo = ColorPerturbation(albedo);

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

			Material Rock(float weight, float wetness, float height, float height2) {
				if (_MaterialDebug == 1) {
					Material m;
					m.Albedo = _RockDebug;
					return m;
				}

				float4 albedoL = (UNITY_SAMPLE_TEX2DARRAY(_ColorTextures, float3(UV*_RockUVLargeMultiply, 0.0)));
				float4 albedoLM = (UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorTextures, float3(UV*_RockUVLargeMultiply, 0.0), 9));

				float roughness = UNITY_SAMPLE_TEX2DARRAY(_ColorTextures, float3(UV*_RockUVLargeMultiply, 2.0)).x;

				float4 albedo = ColorTransfer(satellite, albedoL, albedoLM);

				float3 N = _enableNormalMap ? UnpackNormal(UNITY_SAMPLE_TEX2DARRAY(_NormalTextures, float3(UV*_RockUVLargeMultiply, 0))) : float3(0,0,1);

				if (_enableDetails && (Depth < _LODDistance1)) {
					float lod = 9 * (sqrt(_RockUVDetailMultiply) / 10) * Depth / (_LODDistance1);

					float4 albedoD = (UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorTextures, float3(UV*_RockUVDetailMultiply, 0 + 1.0), lod));
					float4 albedoDM = (UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorTextures, float3(UV*_RockUVDetailMultiply, 0 + 1.0), 8 + lod));

					float roughnessD = UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorTextures, float3(UV*_RockUVDetailMultiply, 0 + 3.0), lod).x;

					float4 colorD = ColorTransfer(albedo, albedoD, albedoDM);

					float3 NormalD = _enableNormalMap ? UnpackNormal(UNITY_SAMPLE_TEX2DARRAY_LOD(_NormalTextures, float3(UV*_RockUVDetailMultiply, 1), lod)) : float3(0, 0, 1);
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

				if (_enableNoiseHue) albedo = ColorPerturbation(albedo);

				roughness = lerp(roughness, _RockRoughnessModifier, _RockRoughnessModifierStrength);

				float wetratio = saturate(2.0*wetness - pow(1.2*(height), 2));
				roughness = lerp(roughness, 0.1, wetratio);

				albedo *= lerp(1.0, 0.6, wetratio);

				Material m;
				m.Albedo = albedo;
				m.Normal = N;
				m.Roughness = roughness;
				return m;
			}

			Material Forest(float weight) {
				if (_MaterialDebug == 1) {
					Material m;
					m.Albedo = _ForestDebug;
					return m;
				}

				float4 albedoL = (UNITY_SAMPLE_TEX2DARRAY(_ColorTextures, float3(UV*_ForestUVLargeMultiply, 18 + 0.0)));
				float4 albedoLM = (UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorTextures, float3(UV*_ForestUVLargeMultiply, 18 + 0.0), 9));

				float roughness = UNITY_SAMPLE_TEX2DARRAY(_ColorTextures, float3(UV*_ForestUVLargeMultiply, 18 + 1.0)).x;

				float4 albedo = ColorTransfer(satellite, albedoL, albedoLM);

				float3 N = _enableNormalMap ? lerp(float3(0, 0, 1), UnpackNormal(UNITY_SAMPLE_TEX2DARRAY(_NormalTextures, float3(UV*_ForestUVLargeMultiply, 11))), _ForestNormalLargeStrength) : float3(0, 0, 1);

				if (_enableNoiseHue) albedo = ColorPerturbation(albedo);

				roughness = lerp(roughness, _ForestRoughnessModifier, _ForestRoughnessModifierStrength);

				Material m;
				m.Albedo = albedo;
				m.Normal = N;
				m.Roughness = roughness;
				return m;
			}

			Material Grass(float weight, float wetness, float height) {
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
					float lod = 9 * (sqrt(_GrassUVDetailMultiply) / 10) * Depth / (_LODDistance1);

					float4 albedoD = (UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorTextures, float3(UV*_GrassUVDetailMultiply, 14 + 1.0), lod));
					float4 albedoDM = (UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorTextures, float3(UV*_GrassUVDetailMultiply, 14 + 1.0), 8 + lod));

					float roughnessD = UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorTextures, float3(UV*_GrassUVDetailMultiply, 14 + 3.0), lod).x;

					float4 colorD = ColorTransfer(albedo, albedoD, albedoDM);

					float3 NormalD = _enableNormalMap ? UnpackNormal(UNITY_SAMPLE_TEX2DARRAY_LOD(_NormalTextures, float3(UV*_GrassUVDetailMultiply, 9), lod)) : float3(0, 0, 1);

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

				if (_enableNoiseHue) albedo = ColorPerturbation(albedo);

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
				float detailStrength = _enableDetails ? lerp(0, _SnowDetailStrength, LOD) : 0;

				float roughnessL = UNITY_SAMPLE_TEX2DARRAY(_ColorTextures, float3(UV*_SnowUVLargeMultiply, 2.0)).x;

				float roughnessD = 1.0;

				float lod = 9 * (sqrt(_SnowUVDetailMultiply) / 10) * Depth / (_LODDistance1);

				if (detailStrength > 0) {
					roughnessD = UNITY_SAMPLE_TEX2DARRAY_LOD(_ColorTextures, float3(UV*_SnowUVDetailMultiply, 3.0), lod).x;
				}
				float4 albedo = lerp(float4(1,1,1,1), satellite, 0.5);//float4(1, 1, 1, 1);

				float4 roughness = (roughnessL + roughnessD * detailStrength);

				float3 N = float3(0, 0, 1);

				if (_enableNormalMap) {
					float3 NL = UnpackNormal(UNITY_SAMPLE_TEX2DARRAY(_NormalTextures, float3(UV*_SnowUVLargeMultiply, 2)));
					NL = normalize(lerp(float3(0, 0, 1), NL, _SnowNormalLargeStrength));

					float3 ND = UnpackNormal(UNITY_SAMPLE_TEX2DARRAY_LOD(_NormalTextures, float3(UV*_SnowUVDetailMultiply, 3), lod));
					ND = normalize(lerp(float3(0, 0, 1), ND, _SnowNormalDetailStrength));

					N = blendNormal(NL, ND, detailStrength);
				}

				roughness = lerp(roughness, _SnowRoughnessModifier, _SnowRoughnessModifierStrength);

				float wetratio = saturate(1-weight);
				roughness = lerp(roughness, 0.1, wetratio);

				Material m;
				m.Albedo = albedo;
				m.Normal = N;
				m.Roughness = roughness;
				return m;
			}

			float4 Water(v2f input) {
				if (_MaterialDebug == 1) return _WaterDebug;

				float3 dPdu = input.dPdu;
				float3 dPdv = input.dPdv;
				float2 sigmaSq = input.sigmaSq;

				float iMAX = min(ceil((log2(_nyquistMax * input.lod) - _lods.z) * _lods.w), _nbWaves - 1.0);
				float iMax = floor((log2(_nyquistMin * input.lod) - _lods.z) * _lods.w);
				float iMin = max(0.0, floor((log2(_nyquistMin * input.lod / _lods.x) - _lods.z) * _lods.w));
				
				[unroll(100)]
				for (float i = iMin; i <=0* iMAX; i += 1.0) {
					float4 wt = tex2Dlod(_wavesSampler, float4((i + 0.5f) / _nbWaves, 0, 0, 0));
					float phase = wt.y * _Time.x - dot(wt.zw, input.u);
					float s = sin(phase);
					float c = cos(phase);
					float overk = 9.81f / (wt.y * wt.y);

					float wp = smoothstep(_nyquistMin, _nyquistMax, (2.0 * 3.151592) * overk / input.lod);
					float wn = smoothstep(_nyquistMin, _nyquistMax, (2.0 * 3.141592) * overk / input.lod * _lods.x);

					float3 factor = (1.0 - wp) * wn * wt.x * float3(wt.z * overk, 1.0, wt.w * overk);

					float3 dPd = factor * float3(c, -s, c);
					dPdu -= dPd * wt.z;
					dPdv -= dPd * wt.w;

					wt.zw *= overk;
					float kh = i < iMax ? wt.x / overk : 0.0;
					float wkh = (1.0 - wn) * kh;
					sigmaSq -= float2(wt.z * wt.z, wt.w * wt.w) * (sqrt(1.0 - wkh * wkh) - sqrt(1.0 - kh * kh));
				}
				
				dPdu = normalize(dPdu);
				dPdv = normalize(dPdv);

				sigmaSq = max(sigmaSq, 2e-5);

				float3 N = normalize(cross(dPdv, dPdu));

				//N = normalize(N * float3(1, -1, -1));				
				
				if (dot(V, N) < 0.0) {
					N = reflect(N, V); // reflects backfacing normals
				}			


				//return float4(N, 1.0);
				//N = float3(-N.x, -N.y, -N.z);
				//return dot(N, L);
				//return microfacet(input, _WaterColor, N, _WaterRoughness);
				//return float4(N, 1.0);
				float F = 0.02 + 0.98 * meanFresnel(V, N, sigmaSq);

				float D = GGX(N, H, 0.1);//_WaterRoughness);

				fixed4 specular = _LightColor0 * D * F * Geom(N, H, V, L) / (4.0f*dot(V, N)*dot(N, L));
				fixed4 diffuse = _WaterColor * dot(N, L) * _LightColor0;
				fixed4 ambient = _WaterColor * (float4(0.6, 0.7, 0.95, 1.0) + unity_AmbientGround + unity_AmbientEquator + unity_AmbientSky);

				/* INDIRECT ILLUMINATION */

			//	N = float3(0, 1, 0);

				half mip = perceptualRoughnessToMipmapLevel(_WaterRoughness);

				//return texCUBElod(unity_SpecCube0, float4(reflect(-V, N), mip));

				float3 wo0 = reflect(-V, N);

				return texCUBElod(_ReflectionCubeMap, float4(wo0, mip)) + specular;

				float4 IndirectLight = float4(0, 0, 0, 0);
				for (int i = 0; i < 1; i++) {
					float3 wo = reflect(-V, N);
					switch (i) {
					case 0: break;
					case 1: wo = reflect(-V, normalize(N + float3(_WaterRoughness, 0, 0))); break;
					case 2: wo = reflect(-V, normalize(N + float3(-_WaterRoughness, 0, 0))); break;
					case 3: wo = reflect(-V, normalize(N + float3(0, 0, _WaterRoughness))); break;
					case 4: wo = reflect(-V, normalize(N + float3(0, 0, -_WaterRoughness))); break;
					case 5: wo = reflect(-V, normalize(N + float3(_WaterRoughness / 2, 0, _WaterRoughness / 2))); break;
					case 6: wo = reflect(-V, normalize(N + float3(_WaterRoughness / 2, 0, -_WaterRoughness / 2))); break;
					case 7: wo = reflect(-V, normalize(N + float3(-_WaterRoughness / 2, 0, _WaterRoughness / 2))); break;
					case 8: wo = reflect(-V, normalize(N + float3(-_WaterRoughness / 2, 0, -_WaterRoughness / 2))); break;
					default: break;
					}

					IndirectLight += dot(wo, N) * texCUBElod(_ReflectionCubeMap, float4(wo, 0));
				}

				IndirectLight /= 1.0;

				float4 Indirect = IndirectLight;//lerp(_WaterColor*IndirectLight, IndirectLight, fresnel(dot(N, V), 1.33f));

				return specular + IndirectLight;
			} 

			float RockH(float weight) {

				float height = 0;			

				[unroll(3)]
				for (int i = 0; i < 2 * weight + 1; i++) {
					float heightL = (UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(UV * _RockUVLargeMultiply, 0.0 + _N_NOISE))).r;
					
					if (_enableDetails && (Depth < _LODDistance1)) {
						float heightD = (UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(UV*_RockUVDetailMultiply, 1.0 + _N_NOISE))).r * _RockDetailStrength;

						if (Depth >= _LODDistance1 * (1 - blendSize)) {
							float w = (1 - ((Depth - _LODDistance1 * (1 - blendSize)) / (_LODDistance1*blendSize)));
							height = (heightL + heightD * w) / (1 + w);
						}
						else
							height = (heightL + heightD) / 2.0;
					}					

					UV -= 0.05 * (heightL - 1) * VertexNormal.y * V.xz * weight / _RockUVLargeMultiply;

				}
				return (height * _RockHeightStrength + _RockHeightOffset) * weight;
			}

			float GrassH(float weight) {

				float height = 0;

				[unroll(3)]
				for (int i = 0; i < 2 * weight + 1; i++) {
					float heightL = UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(UV*_GrassUVDetailMultiply, 8.0 + _N_NOISE)).x *_GrassHeightStrength + _GrassHeightOffset;

					if (_enableDetails && (Depth < _LODDistance1)) {
						float heightD = (UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(UV*_GrassUVDetailMultiply, 9.0 + _N_NOISE)).x*_GrassHeightStrength + _GrassHeightOffset);

						if (Depth >= _LODDistance1 * (1 - blendSize)) {
							float w = (1 - ((Depth - _LODDistance1 * (1 - blendSize)) / (_LODDistance1*blendSize)));
							height = (heightL + heightD * w) / (1 + w);
						}
						else height = (heightL+ heightD) * 0.5;
					}

					UV -= 0.01 * (heightL - 1) * VertexNormal.y * V.xz * weight / _RockUVLargeMultiply;
				}

				return height* weight;
			}

			float ForestH(float weight) {

				float height = 0;

				[unroll(9)]
				for (int i = 0; i < 8 * weight + 1; i++) {
					height = UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(UV*_ForestUVLargeMultiply, 11.0 + _N_NOISE)).x *_ForestHeightStrength + _ForestHeightOffset;

					UV -= 0.02 * (height - 1) * VertexNormal.y * V.xz * weight / _RockUVLargeMultiply;
				}

				return height* weight;
			}

			float GravelH(float weight) {
				float height = 0;

				[unroll(3)]
				for (int i = 0; i < 2 * weight + 1; i++) {
					float heightL = (UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(UV*_GravelUVLargeMultiply, 4.0 + _N_NOISE))).r;

					if (_enableDetails && (Depth < _LODDistance1)) {
						float heightD = (UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(UV*_GravelUVDetailMultiply, 5.0 + _N_NOISE))).r * _GravelDetailStrength;

						if (Depth >= _LODDistance1 * (1 - blendSize)) {
							float w = (1 - ((Depth - _LODDistance1 * (1 - blendSize)) / (_LODDistance1*blendSize)));
							height = (heightL + heightD * w) / (1 + w);
						}
						else
							height = (heightL + heightD) / 2.0;
					}

					UV -= 0.01 * (heightL - 1) * VertexNormal.y * V.xz * weight / _RockUVLargeMultiply;
				}
				return (height*_GravelHeightStrength + _GravelHeightOffset) * weight;
			}

			float DirtH(float weight) {
				float height = 0;

				[unroll(3)]
				for (int i = 0; i < 2 * weight + 1; i++) {
					float heightL = UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(UV*_DirtUVLargeMultiply, 6.0 + _N_NOISE)).x *_DirtHeightStrength + _DirtHeightOffset;

					if (_enableDetails && (Depth < _LODDistance1)) {
						float heightD = (UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(UV*_DirtUVDetailMultiply, 7.0 + _N_NOISE)).x*_DirtHeightStrength + _DirtHeightOffset);

						if (Depth >= _LODDistance1 * (1 - blendSize)) {
							float w = (1 - ((Depth - _LODDistance1 * (1 - blendSize)) / (_LODDistance1*blendSize)));
							height = (heightL + heightD * w) / (1 + w);
						}
						else
							height = (heightL + heightD) / 2.0;
					}
					UV -= 0.01 * (heightL - 1) * VertexNormal.y * V.xz * weight / _RockUVLargeMultiply;

				}
				return height * weight;
			}

			float SnowH(float weight) {
				float height = 0;

				[unroll(3)]
				for (int i = 0; i < 2 * weight + 1; i++) {
					float heightL = (UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(UV*_SnowUVLargeMultiply, 2.0 + _N_NOISE))).r;

					if (_enableDetails && (Depth < _LODDistance1)) {
						float heightD = (UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(UV*_SnowUVDetailMultiply, 3.0 + _N_NOISE))).r * _SnowDetailStrength;

						if (Depth >= _LODDistance1 * (1 - blendSize)) {
							float w = (1 - ((Depth - _LODDistance1 * (1 - blendSize)) / (_LODDistance1*blendSize)));
							height = (heightL + heightD * w) / (1 + w);
						}
						else
							height = (heightL + heightD) / 2.0;
					}

					UV -= 0.1 * (heightL - 1) * VertexNormal.y * V.xz * weight / _RockUVLargeMultiply;

				}
				return (height*_SnowHeightStrength + _SnowHeightOffset) * weight;
			}

			float CommonH(float weight) {

				float height = 0;

				[unroll(3)]
				for (int i = 0; i < 2 * weight + 1; i++) {
					if (_enableDetails && (Depth < _LODDistance1)) {
						float heightD = ((UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(UV*_CommonUVDetailMultiply, 10 + _N_NOISE))).r*_CommonHeightDetailStrength + _CommonHeightDetailOffset);

						if (Depth >= _LODDistance1 * (1 - blendSize)) {
							height = lerp(heightD, height, (Depth - _LODDistance1 * (1 - blendSize)) / (_LODDistance1*blendSize));
						}
						else height = heightD;
					}

					UV -= 0.01 * (height - 1) * VertexNormal.y * V.xz * weight / _CommonUVDetailMultiply;

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
				if (maxH == ForestH) return Forest(WSGT.w);
				else if (maxH == snowH) return Snow(WSGT.y);
				else if (maxH == gravelH) return Gravel(DGR.y, wetness, gravelH);
				else if (maxH == grassH) return Grass(WSGT.z, wetness, grassH);
				else if (maxH == dirtH) return Dirt(DGR.x, wetness, dirtH);
				else if (maxH == rockH || maxH == commonH) return Rock(DGR.z, wetness, rockH, commonH);

				return Dirt(DGR.x, wetness, dirtH);
			}

			float4 frag(v2f _input) : COLOR
			{
				v2f input = _input;
				/*
				float3 barys;
				barys.xy = _input.barycentric;
				barys.z = 1 - barys.x - barys.y;

				float minBary = min(barys.x, min(barys.y, barys.z));
				float delta = fwidth(minBary);
				minBary = smoothstep(0, 1*delta, minBary);
				*/

				//			float4 worldSpacePosition : TEXCOORD2;

				Material defaultMat;
				defaultMat.Albedo = float4(0, 0, 0, 0);
				defaultMat.Normal = float3(0, 0, 1);
				defaultMat.Roughness = 0;

				float3 Tangent = normalize(input.tangent);
				float3 Biangent = normalize(input.bitangent);

				VertexNormal = normalize(input.normal);

				V = normalize(_WorldSpaceCameraPos - input.worldSpacePosition.xyz);
				L = normalize(input.lightDirection.xyz);
				H = normalize(V + L);
				UV = input.uv;// +(0.1*Normal.xz);//*0.1f;

				satellite = tex2D(_Sat, input.uv);

				Material satmat;
				satmat.Albedo = satellite;
				satmat.Roughness = 1;
				satmat.Normal = float3(0, 0, 1);

				float ns = 0.13;
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

					Noise = (UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(input.uv, 0)));
					Noise_Classes = (UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(input.uv, 1)));

					//return Noise_Classes.w;

					/*
					if (_enableNoise == 1) {
						input.uv += _noiseStrength * Noise.xy;
					}
					*/

					DGR = tex2D(_ClassesDGR, input.uv);// +float4(Noise_Classes.xy, 0, 0);
					WSGT = tex2D(_ClassesWSGT, input.uv);

					if (_enableNoise == 1) {
						WSGT += float4(0, 0, Noise_Classes.w, 0);
					}


					WSGT.w = (1.0 - WSGT.w);

					DGR = saturate(sin(3.141592 * (DGR - 0.5))*0.55 + 0.5);
					WSGT = saturate(sin(3.141592 * (WSGT - 0.5))*0.55 + 0.5);

					WSGT.w *= 2.0;


					if (_SlopeModifierEnabled == 1) {
						float slopeRockModifier = saturate((-Normal.y + _SlopeModifierThreshold) * _SlopeModifierStrength);

						if (_SlopeModifierDebug == 1) return slopeRockModifier;

						DGR.z += slopeRockModifier;
					}

					float sum = DGR.x + DGR.y + DGR.z + WSGT.x + WSGT.y + WSGT.z + WSGT.w;
					DGR /= sum;
					WSGT /= sum;

					float4 Color = float4(0, 0, 0, 1.0);

					//ForestH(1);
					//return microfacet(Forest(1));

					if (Depth < _LODDistance3) {
						float waterH = WSGT.x;
						float snowH = 0;
						float grassH = 0;
						float forestH = 0;
						float rockH = 0;
						float dirtH = 0;
						float gravelH = 0;
						float commonH = 0;

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
						if (DGR.y > 0)
						{
							gravelH = GravelH(DGR.y);
						}
						//Rock
						if (DGR.z > 0)
						{
							rockH = RockH(DGR.z);
						}

						if (DGR.y > 0 || DGR.x > 0 || WSGT.z > 0) {
							commonH = CommonH(max(DGR.y, max(DGR.x, WSGT.z)));
						}

						satellite = tex2D(_Sat, input.uv);

						wetness = pow(saturate(5.0*WSGT.y + 1.2*(WSGT.x > 0.5 ? 0 : WSGT.x)), 1.5);

						float4 ColorH = float4(0,0,0,0);
						float4 ColorB = float4(0, 0, 0, 0);
						Material MaterialB;

						if (Depth < _LODDistance2) {						

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

							if (waterH > maxH0) ColorH = lerp(ColorH, Water(input), sqrt(saturate(waterH - 0.5) / 0.5));
						}

						if (Depth >= _LODDistance2 * (1 - blendSize)) {
							float4 water = float4(0, 0, 0, 0);
							Material snow = defaultMat;
							Material grass = defaultMat;
							Material forest = defaultMat;
							Material dirt = defaultMat;
							Material gravel = defaultMat;
							Material rock = defaultMat;

							//Snow
							if (WSGT.y > 0)
							{
								snow = Snow(WSGT.y);
							}
							//Grass
							if (WSGT.z > 0)
							{
								grass = Grass(WSGT.z, wetness, 0);
							}
							//Forest
							if (WSGT.w > 0)
							{
								forest = Forest(WSGT.w);
							}
							//Dirt
							if (DGR.x > 0)
							{
								dirt = Dirt(DGR.x, wetness, 0);
							}
							//Gravel
							if (DGR.y > 0)
							{
								gravel = Gravel(DGR.y, wetness, 0);
							}
							//Rock
							if (DGR.z > 0)
							{
								rock = Rock(DGR.z, wetness, 1, 0);
							}

							MaterialB.Albedo = snow.Albedo * WSGT.y + grass.Albedo * WSGT.z + forest.Albedo * WSGT.w + dirt.Albedo * DGR.x + gravel.Albedo * DGR.y + rock.Albedo * DGR.z;
							MaterialB.Roughness = snow.Roughness * WSGT.y + grass.Roughness * WSGT.z + forest.Roughness * WSGT.w + dirt.Roughness * DGR.x + gravel.Roughness * DGR.y + rock.Roughness * DGR.z;
							MaterialB.Normal = normalize(snow.Normal * WSGT.y + grass.Normal * WSGT.z + forest.Normal * WSGT.w + dirt.Normal * DGR.x + gravel.Normal * DGR.y + rock.Normal * DGR.z);
							
							ColorB = microfacet(MaterialB);

							//Water
							if (WSGT.x > 0)
							{
								ColorB = ColorB + float4(Water(input).rgb * WSGT.x, 1.0);
							}
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
					else if (Depth < _LODDistance4) {
						Color = microfacet(satmat);

						if (Depth >= _LODDistance4 * (1 - blendSize)) {
							Color = lerp(Color, satellite, (Depth - _LODDistance4 * (1 - blendSize)) / (_LODDistance4*blendSize));
						}
					}
					else Color = satellite;


					return Color;
					
				}
				else
				{
					return satellite;
				}

			}
			ENDCG
		}
	}
}
