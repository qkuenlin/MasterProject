// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/customTerrainShader_hlsl" {
	Properties{
		_ClassesDGR("Segmentation Image Dirt/Gravel/Rock", 2D) = "white" {}
		_ClassesWSGT("Segmentation Image Water/Snow/Grass/Trees", 2D) = "white" {}
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

			fixed4 _AmbientLightColor;
			half _AmbientLightStrength;

			UNITY_DECLARE_TEX2DARRAY(_HeightTextures);

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

			sampler2D _ClassesDGR;
			sampler2D _ClassesWSGT;

			sampler2D _Sat;

			UNITY_DECLARE_TEX2DARRAY(_Textures);

			sampler2D _RockNormalDetail;
			sampler2D _RockNormalLarge;
			half _RockNormalDetailStrength;
			half _RockRoughnessModifier;
			half _RockRoughnessModifierStrength;
			float _RockUVLargeMultiply;
			float _RockUVDetailMultiply;
			float _RockDetailStrength;
			float _RockNormalLargeStrength;
			float _RockHeightStrength;
			float _RockHeightOffset;

			sampler2D _SnowNormalDetail;
			sampler2D _SnowNormalLarge;
			half _SnowNormalDetailStrength;
			half _SnowRoughnessModifier;
			half _SnowRoughnessModifierStrength;
			float _SnowUVLargeMultiply;
			float _SnowUVDetailMultiply;
			float _SnowDetailStrength;
			float _SnowNormalLargeStrength;
			float _SnowHeightStrength;
			float _SnowHeightOffset;

			sampler2D _GravelNormalDetail;
			sampler2D _GravelNormalLarge;
			half _GravelNormalDetailStrength;
			half _GravelHeightStrength;
			half _GravelHeightOffset;

			half _GravelRoughnessModifier;
			half _GravelRoughnessModifierStrength;
			float _GravelUVLargeMultiply;
			float _GravelUVDetailMultiply;
			float _GravelDetailStrength;
			float _GravelNormalLargeStrength;

			sampler2D _DirtNormalDetail;
			//sampler2D _DirtNormalLarge;
			half _DirtNormalDetailStrength;
			half _DirtHeightStrength;
			half _DirtHeightOffset;

			half _DirtRoughnessModifier;
			half _DirtRoughnessModifierStrength;
			float _DirtUVLargeMultiply;
			float _DirtUVDetailMultiply;
			float _DirtDetailStrength;
			float _DirtNormalLargeStrength;

			sampler2D _GrassNormalDetail;
			sampler2D _GrassNormalLarge;
			half _GrassNormalDetailStrength;
			half _GrassHeightStrength;
			half _GrassHeightOffset;

			half _GrassRoughnessModifier;
			half _GrassRoughnessModifierStrength;
			float _GrassUVLargeMultiply;
			float _GrassUVDetailMultiply;
			float _GrassDetailStrength;
			float _GrassNormalLargeStrength;

			sampler2D _CommonNormalDetail;
			float _CommonUVDetailMultiply;
			float _CommonNormalDetailStrength;
			float _CommonDetailStrength;
			float _CommonHeightDetailStrength;
			float _CommonHeightDetailOffset;

			samplerCUBE _ReflectionCubeMap;

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
				
				/* OCEAN SIMULATION VERTEX PART*/
								
				//if (tex2Dlod(_ClassesWSGT, float4(o.uv, 0, 1)).x > 0)
				{
					float3 cameraDir = normalize(mul(screenToCamera, v.vertex).xyz);
					float3 worldDir = mul(cameraToWorld, float4(cameraDir, 0.0)).xyz;

					float t = (v.vertex.y - _WorldSpaceCameraPos.y);

					o.lod = t * _lods.y;//length(_WorldSpaceCameraPos - o.worldSpacePosition.xyz); // size in meters of one grid cell, projected on the sea surface

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
			float Depth;
			float LOD;
			float4 satellite;
			float4 Noise;
			float4 Noise_Classes;
			float2 UV;

			float blendSize;

			float3 tspace0;
			float3 tspace1;
			float3 tspace2;

			float DetailMaterialWeight;

			float M_PI = 3.1415f;

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

			float4 microfacet(v2f input, float4 albedo, float3 N, float roughness) {
				float3 Normal = float3(0, 0, 0);
				
				Normal.x = dot(tspace0, N.xyz);
				Normal.y = dot(tspace1, N.xyz);
				Normal.z = dot(tspace2, N.xyz);
				
				float cosThetaO = dot(L, Normal);
				float cosThetaI = dot(V, Normal);
				float cosThetaH = dot(H, Normal);

				/* DIRECT ILLUMINATION + SPECULAR HIGHLIGHT*/
				fixed4 specular = fixed4(0.0, 0, 0, 0);
				if (cosThetaH > 0 && cosThetaO > 0) {
					float F = fresnel(dot(H, V), 1.5f);
					float D = blinnPhong(cosThetaH, roughness) * 3.141592 / 4.0;
					float G = smithShadowing(cosThetaI, cosThetaO, roughness);// (cosThetaO*cosThetaI);//Geom(V, H, Normal, roughness)*Geom(L, H, Normal, roughness) / (cosThetaO*cosThetaI);
					
					specular = saturate(D * F * G) * _LightColor0;
				}

				float dotNL_ = (cosThetaO+1)/2.0;
				dotNL_ *= dotNL_;
				fixed4 diffuse = saturate(albedo *_LightColor0 * cosThetaO);// +saturate(albedo * _AmbientLightColor * _AmbientLightStrength);

				float4 Direct = diffuse + (1 - roughness)*specular;

				/* INDIRECT ILLUMINATION */
				
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

				float4 Indirect = lerp(albedo*IndirectLight, IndirectLight, (1-roughness)*fresnel(dot(Normal, V), 1.33f));
				
				/* DIRECT REFLECTION */
				//float3 reflectDir = reflect(-V, Normal);
				//float4 Reflection = (1 - roughness)*texCUBElod(_ReflectionCubeMap, float4(reflectDir, roughness*roughness*9));
				
				return saturate(Direct + Indirect);
			}

			float4 Gravel(v2f input, float weight, float wetness, float height) {

				float4 albedoL = (UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*_GravelUVLargeMultiply, 6+0.0)));
				float4 albedoLM = (UNITY_SAMPLE_TEX2DARRAY_LOD(_Textures, float3(UV*_GravelUVLargeMultiply, 6+0.0), 9));

				float roughness = UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*_GravelUVLargeMultiply, 6+2.0)).x;

				float4 albedo = ColorTransfer(satellite, albedoL, albedoLM);

				float3 N = _enableNormalMap ? lerp(float3(0,0,1), UnpackNormal(tex2D(_GravelNormalLarge, UV*_GravelUVLargeMultiply)), _GravelNormalLargeStrength) : float3(0, 0, 1);

				if (_enableDetails && (Depth < _LODDistance1)) {
					float4 albedoD = (UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*_GravelUVDetailMultiply, 6+1.0)));
					float4 albedoDM = (UNITY_SAMPLE_TEX2DARRAY_LOD(_Textures, float3(UV*_GravelUVDetailMultiply, 6+1.0), 9));

					float roughnessD = UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*_GravelUVDetailMultiply, 6+3.0)).x;

					float4 colorD = ColorTransfer(albedo, albedoD, albedoDM);

					float3 NormalD = _enableNormalMap ? UnpackNormal(tex2D(_GravelNormalDetail, UV*_GravelUVDetailMultiply)) : float3(0, 0, 1);

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

				return float4(microfacet(input, albedo, N, roughness).rgb, 1);
			}

			float4 Dirt(v2f input, float weight, float wetness, float height) {
				float4 albedoL = (UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*_GravelUVLargeMultiply, 6 + 0.0)));
				float4 albedoLM = (UNITY_SAMPLE_TEX2DARRAY_LOD(_Textures, float3(UV*_GravelUVLargeMultiply, 6 + 0.0), 9));

				float roughness = UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*_GravelUVLargeMultiply, 6 + 2.0)).x;

				float4 albedo = ColorTransfer(satellite, albedoL, albedoLM);

				float3 N = _enableNormalMap ? lerp(float3(0, 0, 1), UnpackNormal(tex2D(_GravelNormalLarge, UV*_GravelUVLargeMultiply)), _GravelNormalLargeStrength) : float3(0, 0, 1);

				if (_enableDetails && (Depth < _LODDistance1)) {
					float4 albedoD = (UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*_DirtUVDetailMultiply, 10 + 1.0)));
					float4 albedoDM = (UNITY_SAMPLE_TEX2DARRAY_LOD(_Textures, float3(UV*_DirtUVDetailMultiply, 10 + 1.0), 9));

					float roughnessD = UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*_DirtUVDetailMultiply, 10 + 3.0)).x;

					float4 colorD = ColorTransfer(albedo, albedoD, albedoDM);

					float3 NormalD = _enableNormalMap ? UnpackNormal(tex2D(_DirtNormalDetail, UV*_DirtUVDetailMultiply)) : float3(0, 0, 1);

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

				return float4(microfacet(input, albedo, N, roughness).rgb, 1);
			}

			float4 Rock(v2f input, float weight, float wetness, float height, float height2) {

				float4 albedoL = (UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*_RockUVLargeMultiply, 0.0)));
				float4 albedoLM = (UNITY_SAMPLE_TEX2DARRAY_LOD(_Textures, float3(UV*_RockUVLargeMultiply, 0.0), 9));

				float roughness = UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*_RockUVLargeMultiply, 2.0)).x;

				float4 albedo = ColorTransfer(satellite, albedoL, albedoLM);

				float3 N = _enableNormalMap ? UnpackNormal(tex2D(_RockNormalLarge, UV*_RockUVLargeMultiply)) : float3(0,0,1);

				if (_enableDetails && (Depth < _LODDistance1)) {
					float4 albedoD = (UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*_RockUVDetailMultiply, 1.0)));
					float4 albedoDM = (UNITY_SAMPLE_TEX2DARRAY_LOD(_Textures, float3(UV*_RockUVDetailMultiply, 1.0), 9));

					float roughnessD = UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*_RockUVDetailMultiply, 3.0)).x;

					float4 colorD = ColorTransfer(albedo, albedoD, albedoDM);

					float3 NormalD = _enableNormalMap ? UnpackNormal(tex2D(_RockNormalDetail, UV*_RockUVDetailMultiply)) : float3(0,0,1);

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

				return float4(microfacet(input, albedo, N, roughness).rgb, 1);
			}



			float4 Trees(v2f input, float weight) {
				float3 Normal = float3(0, 0, 1);
				float height = (input.worldSpacePosition.y)* weight;

				return float4(microfacet(input, satellite, Normal, 1).rgb, height);
			}

			float4 Grass(v2f input, float weight, float wetness, float height) {
				float4 albedoL = (UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*_GrassUVLargeMultiply, 14 + 0.0)));
				float4 albedoLM = (UNITY_SAMPLE_TEX2DARRAY_LOD(_Textures, float3(UV*_GrassUVLargeMultiply, 14 + 0.0), 9));

				float roughness = UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*_GrassUVLargeMultiply, 14 + 2.0)).x;

				float4 albedo = lerp(ColorTransfer(satellite, albedoL, albedoLM), satellite, 0.7);

				float3 N = _enableNormalMap ? lerp(float3(0, 0, 1), UnpackNormal(tex2D(_GrassNormalLarge, UV*_GrassUVLargeMultiply)), _GrassNormalLargeStrength) : float3(0, 0, 1);

				if (_enableDetails && (Depth < _LODDistance1)) {
					float4 albedoD = (UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*_GrassUVDetailMultiply, 14 + 1.0)));
					float4 albedoDM = (UNITY_SAMPLE_TEX2DARRAY_LOD(_Textures, float3(UV*_GrassUVDetailMultiply, 14 + 1.0), 9));

					float roughnessD = UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*_GrassUVDetailMultiply, 14 + 3.0)).x;

					float4 colorD = ColorTransfer(albedo, albedoD, albedoDM);

					float3 NormalD = _enableNormalMap ? UnpackNormal(tex2D(_GrassNormalDetail, UV*_GrassUVDetailMultiply)) : float3(0, 0, 1);

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

				return float4(microfacet(input, albedo, N, roughness).rgb, 1);
			}

			float4 Snow(v2f input, float weight) {
				float detailStrength = _enableDetails ? lerp(0, _SnowDetailStrength, LOD) : 0;

				float roughnessL = UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*_SnowUVLargeMultiply, 2.0)).x;

				float roughnessD = 1.0;

				if (detailStrength > 0) {
					roughnessD = UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*_SnowUVDetailMultiply, 3.0)).x;
				}
				float4 albedo = lerp(float4(1,1,1,1), satellite, 0.5);//float4(1, 1, 1, 1);

				float4 roughness = (roughnessL + roughnessD * detailStrength);

				float3 N = float3(0, 0, 1);

				if (_enableNormalMap) {
					float3 NL = UnpackNormal(tex2D(_SnowNormalLarge, UV*_SnowUVLargeMultiply));
					NL = normalize(lerp(float3(0, 0, 1), NL, _SnowNormalLargeStrength));

					float3 ND = UnpackNormal(tex2D(_SnowNormalDetail, UV*_SnowUVDetailMultiply));
					ND = normalize(lerp(float3(0, 0, 1), ND, _SnowNormalDetailStrength));

					N = blendNormal(NL, ND, detailStrength);
				}

				roughness = lerp(roughness, _SnowRoughnessModifier, _SnowRoughnessModifierStrength);

				float wetratio = saturate(1-weight);
				roughness = lerp(roughness, 0.1, wetratio);

				return float4(microfacet(input, albedo, N, roughness).rgb, 1);
			}

			float4 Water(v2f input) {

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

				float height = (UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(UV*_RockUVLargeMultiply, 0.0 + _N_NOISE))).r;

				if (_enableDetails && (Depth < _LODDistance1)) {
					float hD = (UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(UV*_RockUVDetailMultiply, 1.0 + _N_NOISE))).r * _RockDetailStrength;

					if (Depth >= _LODDistance1 * (1 - blendSize)) {
						float w = (1 - ((Depth - _LODDistance1 * (1 - blendSize)) / (_LODDistance1*blendSize)));
						height = (height + hD*w) / (1 + w);
					}
					else
						height = (height + hD)/2.0;
				}

				return (height*_RockHeightStrength + _RockHeightOffset) * weight;
			}

			float GrassH(float weight) {
				float height = UNITY_SAMPLE_TEX2DARRAY_LOD(_HeightTextures, float3(UV*_DirtUVDetailMultiply, 9.0 + _N_NOISE), 9).x *_DirtHeightStrength + _DirtHeightOffset;

				if (_enableDetails && (Depth < _LODDistance1)) {
					float heightD = (UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(UV*_GrassUVDetailMultiply, 9.0 + _N_NOISE)).x*_GrassHeightStrength + _GrassHeightOffset);

					if (Depth >= _LODDistance1 * (1 - blendSize)) {
						height = lerp(heightD, height, (Depth - _LODDistance1 * (1 - blendSize)) / (_LODDistance1*blendSize));
					}
					else height = heightD;
				}

				return height* weight;
			}

			float GravelH(float weight) {

				float height = (UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(UV*_GravelUVLargeMultiply, 4.0 + _N_NOISE))).r;

				if (_enableDetails && (Depth < _LODDistance1)) {
					float hD = (UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(UV*_GravelUVDetailMultiply, 5.0 + _N_NOISE))).r * _GravelDetailStrength;

					if (Depth >= _LODDistance1 * (1 - blendSize)) {
						float w = (1 - ((Depth - _LODDistance1 * (1 - blendSize)) / (_LODDistance1*blendSize)));
						height = (height + hD * w) / (1 + w);
					}
					else
						height = (height + hD) / 2.0;
				}

				return (height*_GravelHeightStrength + _GravelHeightOffset) * weight;
			}

			float DirtH(float weight) {

				float height = UNITY_SAMPLE_TEX2DARRAY_LOD(_HeightTextures, float3(UV*_DirtUVDetailMultiply, 7.0 + _N_NOISE), 9).x *_DirtHeightStrength + _DirtHeightOffset;

				if (_enableDetails && (Depth < _LODDistance1)) {
					float heightD = (UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(UV*_DirtUVDetailMultiply, 7.0 + _N_NOISE)).x*_DirtHeightStrength + _DirtHeightOffset);

					if (Depth >= _LODDistance1 * (1 - blendSize)) {
						height = lerp(heightD, height, (Depth - _LODDistance1 * (1 - blendSize)) / (_LODDistance1*blendSize));
					}
					else height = heightD;
				}

				return height * weight;
			}

			float SnowH(float weight) {

				float height = (UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(UV*_SnowUVLargeMultiply, 2.0 + _N_NOISE))).r;

				if (_enableDetails && (Depth < _LODDistance1)) {
					float hD = (UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(UV*_SnowUVDetailMultiply, 3.0 + _N_NOISE))).r * _SnowDetailStrength;

					if (Depth >= _LODDistance1 * (1 - blendSize)) {
						float w = (1 - ((Depth - _LODDistance1 * (1 - blendSize)) / (_LODDistance1*blendSize)));
						height = (height + hD*w) / (1 + w);
					}
					else
						height = (height + hD) / 2.0;
				}

				return (height*_SnowHeightStrength + _SnowHeightOffset) * weight;
			}

			float CommonH(float weight) {

				float height = 0;

				if (_enableDetails && (Depth < _LODDistance1)) {
					float heightD = ((UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(UV*_CommonUVDetailMultiply, 10 + _N_NOISE))).r*_CommonHeightDetailStrength + _CommonHeightDetailOffset);

					if (Depth >= _LODDistance1 * (1 - blendSize)) {
						height = lerp(heightD, height, (Depth - _LODDistance1 * (1 - blendSize)) / (_LODDistance1*blendSize));
					}
					else height = heightD;
				}

				return height * weight;
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


				float3 Tangent = normalize(input.tangent);
				float3 Biangent = normalize(input.bitangent);

				Normal = normalize(input.normal);

				V = normalize(_WorldSpaceCameraPos - input.worldSpacePosition.xyz);
				L = normalize(input.lightDirection.xyz);
				H = normalize(V + L);
				UV = input.uv;// +(0.1*Normal.xz);//*0.1f;

				satellite = tex2D(_Sat, input.uv);

				float ns = 0.13;
				if (_enableNormalMap) {
					float3 n = Texture2Normal();
					Normal = normalize(Normal + ns * float3(n.x, 0, n.y));
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

					LOD = saturate((lerp(0, 1.0, 200.0 / Depth)));

					DetailMaterialWeight = 1;//saturate((pow(LOD + 0.1, 2) - 0.15) / 0.85);

					Noise = (UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(input.uv, 0)));

					Noise_Classes = (UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(input.uv, 11)));


					float4 DGR = tex2D(_ClassesDGR, input.uv);// +float4(Noise_Classes.xy, 0, 0);
					float4 WSGT = tex2D(_ClassesWSGT, input.uv);// +float4(0, 0, Noise_Classes.w, 0);

					WSGT.a = (1.0 - WSGT.a);

					DGR = saturate(sin(3.141592 * (DGR - 0.5))*0.55 + 0.5);
					WSGT = saturate(sin(3.141592 * (WSGT - 0.5))*0.55 + 0.5);

					if (_SlopeModifierEnabled == 1) {
						float slopeRockModifier = saturate((-Normal.y + _SlopeModifierThreshold) * _SlopeModifierStrength);

						if (_SlopeModifierDebug == 1) return slopeRockModifier;

						DGR.z += slopeRockModifier;
					}

					float sum = DGR.x + DGR.y + DGR.z + WSGT.x + WSGT.y + WSGT.z + WSGT.w;
					DGR /= sum;
					WSGT /= sum;

					//return float4(Normal, 1);

					if (_enableNoise == 1) {
						input.uv += _noiseStrength * Noise.xy;
					}

					/*
					if (WSGT.r == 1) return float4(1, 0, 0, 1);
					else if (WSGT.g == 1) return float4(0, 1, 0, 1);
					else if (WSGT.b == 1) return float4(0, 0, 1, 1);
					else if (WSGT.a == 1) return float4(1, 1, 1, 1);

					else return float4(0, 0, 0, 0);
					*/
					float4 Color = float4(0, 0, 0, 1.0);

					if (Depth < _LODDistance3) {
						float wetness = pow(saturate(5.0*WSGT.y + 1.2*(WSGT.x > 0.5 ? 0 : WSGT.x)), 1.5);

						float4 ColorH = float4(0, 0, 0, 0);
						float4 ColorB = float4 (0, 0, 0, 0);

						if (Depth < _LODDistance2) {
							float waterH = WSGT.x;
							float snowH = 0;
							float grassH = 0;
							float treesH = 0;
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
							//Trees
							if (WSGT.a > 0)
							{
								treesH = 0;//HEIGHT(Trees, WSGT.y, 2);
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
								rockH = RockH(DGR.z);//HEIGHT(_Rock, DGR.z, 0)
							}

							if (DGR.y > 0 || DGR.x > 0 || WSGT.z > 0) {
								commonH = CommonH(max(DGR.y, max(DGR.x, WSGT.z)));
							}


							float maxH = max(rockH, max(gravelH, max(dirtH, max(treesH, max(grassH, max(snowH, commonH))))));
							//return maxH;
							if (maxH == treesH) ColorH = Trees(input, WSGT.a);
							else if (maxH == snowH) ColorH = Snow(input, WSGT.y);
							else if (maxH == gravelH) ColorH = Gravel(input, DGR.y, wetness, gravelH);
							else if (maxH == grassH) ColorH = Grass(input, WSGT.z, wetness, grassH);
							else if (maxH == dirtH) ColorH = Dirt(input, DGR.x, wetness, dirtH);
							else if (maxH == rockH || maxH == commonH) ColorH = Rock(input, DGR.z, wetness, rockH, commonH);

							if (waterH > maxH) ColorH = lerp(ColorH, Water(input), sqrt(saturate(waterH - 0.5) / 0.5));
						}

						if (Depth >= _LODDistance2 * (1 - blendSize)) {
							float4 water = float4(0, 0, 0, 0);
							float4 snow = float4(0, 0, 0, 0);
							float4 grass = float4(0, 0, 0, 0);
							float4 trees = float4(0, 0, 0, 0);
							float4 dirt = float4(0, 0, 0, 0);
							float4 gravel = float4(0, 0, 0, 0);
							float4 rock = float4(0, 0, 0, 0);

							//Water
							if (WSGT.x > 0)
							{
								water = Water(input);
							}
							//Snow
							if (WSGT.y > 0)
							{
								snow = Snow(input, WSGT.y);
							}
							//Grass
							if (WSGT.z > 0)
							{
								grass = Grass(input, WSGT.z, wetness, 0);
							}
							//Trees
							if (WSGT.a > 0)
							{
								trees = Trees(input, WSGT.a);
							}
							//Dirt
							if (DGR.x > 0)
							{
								dirt = Dirt(input, DGR.x, wetness, 0);
							}
							//Gravel
							if (DGR.y > 0)
							{
								gravel = Gravel(input, DGR.y, wetness, 0);
							}
							//Rock
							if (DGR.z > 0)
							{
								rock = Rock(input, DGR.z, wetness, 1, 0);
							}


							ColorB = float4(rock.rgb*DGR.z + gravel.rgb*DGR.y + dirt.rgb*DGR.x + trees.rgb*WSGT.w + grass.rgb*WSGT.z + snow.rgb*WSGT.y + water.rgb*WSGT.x, 1.0);
						}

						if (Depth < _LODDistance2 && Depth >= _LODDistance2 * (1 - blendSize))
							Color = lerp(ColorH, ColorB, (Depth - _LODDistance2 * (1 - blendSize)) / (_LODDistance2*blendSize));
						else if (Depth < _LODDistance2)
							Color = ColorH;
						else Color = ColorB;


						if (Depth >= _LODDistance3 * (1 - blendSize)) {
							Color = lerp(Color, microfacet(input, satellite, float3(0, 0, 1), 1), (Depth - _LODDistance3 * (1 - blendSize)) / (_LODDistance3*blendSize));
						}

					}
					else if (Depth < _LODDistance4) {
						Color = microfacet(input, satellite, float3(0, 0, 1), 1);

						if (Depth >= _LODDistance4 * (1 - blendSize)) {
							Color = lerp(Color, satellite, (Depth - _LODDistance4 * (1 - blendSize)) / (_LODDistance4*blendSize));
						}
					}
					else Color = satellite;


					return Color;
					
				}
				else
				{
					return microfacet(input, satellite, float3(0, 0, 1), 1);
				}

			}
			ENDCG
		}
	}
}
