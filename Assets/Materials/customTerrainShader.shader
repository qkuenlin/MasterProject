// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/customTerrainShader_hlsl" {
	Properties{
		_ClassesDGR("Segmentation Image Dirt/Gravel/Rock", 2D) = "white" {}
		_ClassesWSGT("Segmentation Image Water/Snow/Grass/Trees", 2D) = "white" {}

		_Sat("Satellite Image", 2D) = "white" {}

		_WaterRoughness("Water Roughness", Range(0,1)) = 0.5
		_WaterColor("Water Color", Color) = (0.26,0.19,0.16,1.0)
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

			#pragma vertex vertex //vert
			#pragma fragment frag
			//#pragma hull hull
			//#pragma domain domain
			//#pragma geometry geometry
			#pragma require 2darray
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight


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
			float2x2 _worldToWind;
			float2x2 _windToWorld;
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
			//sampler2D _GravelNormalLarge;
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
			//sampler2D _GrassNormalLarge;
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
					rockH = RockH(o, DGR.z)+2.0*DGR.z;
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
								
				if (tex2Dlod(_ClassesWSGT, float4(o.uv, 0, 0)).x >= 0.9)
				{
					float3 cameraDir = normalize(mul(screenToCamera, v.vertex).xyz);
					float3 worldDir = mul(cameraToWorld, float4(cameraDir, 0.0)).xyz;

					float t = (v.vertex.y - _WorldSpaceCameraPos.y) / worldDir.y;

					o.lod = length(_WorldSpaceCameraPos - v.vertex.xyz)/worldDir.y * _lods.y; // size in meters of one grid cell, projected on the sea surface

					o.u = 2445*o.uv;//mul(_worldToWind, _WorldSpaceCameraPos.xz + t * worldDir.xy);

					float3 dPdu = float3(1.0, 0.0, 0.0);
					float3 dPdv = float3(0.0, 0.0, 1.0);
					float2 sigmaSq = _sigmaSqTotal;

					float3 dP = float3(0.0, 0.0, 0.0);
					float iMin = max(0.0, floor((log2(_nyquistMin * o.lod) - _lods.z) * _lods.w));

					for (float i = iMin; i < _nbWaves; i += 1.0f)
					{
						float4 wt = tex2Dlod(_wavesSampler, float4((i + 0.5) / _nbWaves, 0,0,0));
						float phase = 10*wt.y * _Time.x - dot(wt.zw, o.u);
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
					
					if (t > 0.0) {
						o.pos = UnityObjectToClipPos(float4(o.P.x, o.P.y, o.P.z, 1.0));
						o.worldSpacePosition = mul(unity_ObjectToWorld, float4(o.P.x, o.P.y, o.P.z, 1.0));
					}					

					o.dPdu = dPdu;
					o.dPdv = dPdv;
					o.sigmaSq = sigmaSq;

				}

				
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
				return clamp(pow(16.0*edgeLength /viewDistance, 2), 1, 246);
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



			float4 ColorTransfer(float4 c1, float4 c2, float4 mc2, float strength) {
				if (strength == 0) return c1;
				/*
				float3 hsl1 = RGB2HSL(c1);
				float3 hsl2 = RGB2HSL(c2);
				float3 hslm2 = RGB2HSL(mc2);

				float4 c = float4(HSL2RGB(hsl2 + (hsl1 - hslm2)), 1.0);
				*/
				float3 lab1 = RGB2LAB(c1);
				float3 lab2 = RGB2LAB(c2);
				float3 labm2 = RGB2LAB(mc2);

				float4 c = float4(LAB2RGB(lab2 + (lab1 - labm2)), 1.0);

				//float4 c = c2 + (c1 - mc2);

				return lerp(c1, c, strength);
			}

			float4 ColorTransfer(float4 c1, float4 c2, float strength) {
				return ColorTransfer(c1, c2, float4(0, 0, 0, 0), strength);
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

					IndirectLight += /*dot(wo, Normal) */ texCUBElod(_ReflectionCubeMap, float4(wo, roughness*roughness * 9));
				}

				IndirectLight /= 9.0;

				float4 Indirect = lerp(albedo+0.25*IndirectLight, IndirectLight, (1-roughness)*fresnel(dot(Normal, V), 1.33f));
				
				/* DIRECT REFLECTION */
				//float3 reflectDir = reflect(-V, Normal);
				//float4 Reflection = (1 - roughness)*texCUBElod(_ReflectionCubeMap, float4(reflectDir, roughness*roughness*9));
				
				return saturate(Direct + Indirect);
			}

			float4 Gravel(v2f input, float weight, float wetness, float height) {

				float detailStrength = _enableDetails ? lerp(0, _GravelDetailStrength, LOD) : 0;

				float4 albedoL = (UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*_GravelUVLargeMultiply, 6)));
				float roughnessL = UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*_GravelUVLargeMultiply, 6 + 2)).x;

				float4 albedoD = float4(0, 0, 0, 0);
				float4 albedoDM = float4(0, 0, 0, 0);

				float roughnessD = 1.0;

				if (detailStrength > 0) {
					albedoD = (UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*_GravelUVDetailMultiply, 6 + 1)));
					albedoDM = (UNITY_SAMPLE_TEX2DARRAY_LOD(_Textures, float3(UV*_GravelUVDetailMultiply, 6 + 1), 4));

					roughnessD = UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*_GravelUVDetailMultiply, 6 + 3)).x;
				}

				float4 albedo = ColorTransfer(albedoL, albedoD, albedoDM, detailStrength);

				if (_enableNoiseHue) albedo = ColorPerturbation(albedo);

				float4 roughness = (roughnessL + roughnessD * detailStrength);

				float3 N = float3(0, 0, 1);

				if (_enableNormalMap) {
					//float3 NL = UnpackNormal(tex2D(_GravelNormalLarge, UV*_GravelUVLargeMultiply));
					//NL = normalize(lerp(float3(0, 0, 1), NL, _GravelNormalLargeStrength));

					float3 ND = UnpackNormal(tex2D(_GravelNormalDetail, UV*_GravelUVDetailMultiply));
					ND = normalize(lerp(float3(0, 0, 1), ND, _GravelNormalDetailStrength));

					N = ND;//blendNormal(NL, ND, detailStrength);
				}

				roughness = lerp(roughness, _GravelRoughnessModifier, _GravelRoughnessModifierStrength);

				float wetratio = saturate(2.0*wetness - pow(1.2*(height), 2));
				roughness = lerp(roughness, 0.1, wetratio);

				albedo = ColorTransfer(satellite, albedo, 1- _SatelliteProportion);
				albedo *= lerp(1.0, 0.6, wetratio);

				return float4(microfacet(input, albedo, N, roughness).rgb, 1);
			}

			float4 Dirt(v2f input, float weight, float wetness, float height) {
				float detailStrength = _enableDetails ? lerp(0, _DirtDetailStrength, LOD) : 0;

				float4 albedoL = (UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*_DirtUVLargeMultiply, 10)));
				float roughnessL = UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*_DirtUVLargeMultiply, 10 + 2)).x;

				float4 albedoD = float4(0, 0, 0, 0);
				float4 albedoDM = float4(0, 0, 0, 0);

				float roughnessD = 1.0;

				if (detailStrength > 0) {
					albedoD = (UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*_DirtUVDetailMultiply, 10 + 1)));
					albedoDM = (UNITY_SAMPLE_TEX2DARRAY_LOD(_Textures, float3(UV*_DirtUVDetailMultiply, 10 + 1), 4));

					roughnessD = UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*_DirtUVDetailMultiply, 10 + 3)).x;
				}

				float4 albedo = ColorTransfer(albedoL, albedoD, albedoDM, detailStrength);

				if (_enableNoiseHue) albedo = ColorPerturbation(albedo);

				float4 roughness = (roughnessL + roughnessD * detailStrength);

				float3 N = float3(0, 0, 1);

				if (_enableNormalMap) {
					//float3 NL = UnpackNormal(tex2D(_DirtNormalLarge, UV*_DirtUVLargeMultiply));
					//NL = normalize(lerp(float3(0, 0, 1), NL, _DirtNormalLargeStrength));

					float3 ND = UnpackNormal(tex2D(_DirtNormalDetail, UV*_DirtUVDetailMultiply));
					ND = normalize(lerp(float3(0, 0, 1), ND, _DirtNormalDetailStrength));

					N = ND;//blendNormal(NL, ND, detailStrength);
				}

				roughness = lerp(roughness, _DirtRoughnessModifier, _DirtRoughnessModifierStrength);

				float wetratio = saturate(2.0*wetness - pow(1.2*(height), 2));
				roughness = lerp(roughness, 0.1, wetratio);

				albedo = ColorTransfer(satellite, albedo, 1- _SatelliteProportion);
				albedo *= lerp(1.0, 0.6, wetratio);

				return float4(microfacet(input, albedo, N, roughness).rgb, 1);
			}

			float4 Rock(v2f input, float weight, float wetness, float height, float height2) {
				float detailStrength = _enableDetails ? lerp(0, _RockDetailStrength, LOD) : 0;
				float commonDetailStrength = _enableDetails ? lerp(0, _CommonDetailStrength, LOD) : 0;

				float4 albedoL = (UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*_RockUVLargeMultiply, 0.0)));
				float4 albedoLM = (UNITY_SAMPLE_TEX2DARRAY_LOD(_Textures, float3(UV*_RockUVLargeMultiply, 0.0), 8));

				float roughnessL = UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*_RockUVLargeMultiply, 2.0)).x;

				float4 albedo = ColorTransfer(satellite, albedoL, albedoLM, 1 - _SatelliteProportion);

				float4 albedoD = float4(0, 0, 0, 0);
				float4 albedoDM = float4(0, 0, 0, 0);

				float roughnessD = 1.0;

				if (detailStrength > 0) {
					albedoD = (UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*_RockUVDetailMultiply, 1.0)));
					albedoDM = (UNITY_SAMPLE_TEX2DARRAY_LOD(_Textures, float3(UV*_RockUVDetailMultiply, 1.0), 8));

					roughnessD = UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*_RockUVDetailMultiply, 3.0)).x;
				}

				albedo = ColorTransfer(albedo, albedoD, albedoDM, (1 - _SatelliteProportion)*(detailStrength));

				if (_enableNoiseHue) albedo = ColorPerturbation(albedo);

				float4 roughness = (roughnessL + roughnessD*detailStrength);

				float3 N = float3(0, 0, 1);

				if (_enableNormalMap) {
					float3 NL = float3(0, 0, 1);
					float3 ND = float3(0, 0, 0);
					if (height >= height2) {
						NL = UnpackNormal(tex2D(_RockNormalLarge, UV*_RockUVLargeMultiply));
						NL = normalize(lerp(float3(0, 0, 1), NL, _RockNormalLargeStrength));
					}
					else if (commonDetailStrength>0) {
						NL = UnpackNormal(tex2D(_CommonNormalDetail, UV*_CommonUVDetailMultiply));
						NL = normalize(lerp(float3(0, 0, 1), NL, _CommonNormalDetailStrength));
					}

					if (detailStrength > 0) {
						ND = UnpackNormal(tex2D(_RockNormalDetail, UV*_RockUVDetailMultiply));
						ND = normalize(lerp(float3(0, 0, 1), ND, _RockNormalDetailStrength));

						N = blendNormal(NL, ND, detailStrength);
					}
					else {
						N = NL;
					}
				}

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
				float detailStrength = _enableDetails ? lerp(0, _GrassDetailStrength, LOD) : 0;

				float4 albedoL = (UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*_GrassUVLargeMultiply, 14)));
				float roughnessL = UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*_GrassUVLargeMultiply, 14+2)).x;

				float4 albedoD = float4(0, 0, 0, 0);
				float4 albedoDM = float4(0, 0, 0, 0);

				float roughnessD = 1.0;

				if (detailStrength > 0) {
					albedoD = (UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*_GrassUVDetailMultiply, 14+1)));
					albedoDM = (UNITY_SAMPLE_TEX2DARRAY_LOD(_Textures, float3(UV*_GrassUVDetailMultiply, 14 + 1), 4));

					roughnessD = UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*_GrassUVDetailMultiply, 14+3)).x;
				}

				float4 albedo = ColorTransfer(albedoL, albedoD, detailStrength);

				if (_enableNoiseHue) albedo = ColorPerturbation(albedo);

				float4 roughness = (roughnessL + roughnessD * detailStrength);

				float3 N = float3(0, 0, 1);

				if (_enableNormalMap) {
					//float3 NL = UnpackNormal(tex2D(_GrassNormalLarge, UV*_GrassUVLargeMultiply));
					//NL = normalize(lerp(float3(0, 0, 1), NL, _GrassNormalLargeStrength));

					float3 ND = UnpackNormal(tex2D(_GrassNormalDetail, UV*_GrassUVDetailMultiply));
					ND = normalize(lerp(float3(0, 0, 1), ND, _GrassNormalDetailStrength));

					N = ND;//blendNormal(NL, ND, detailStrength);
				}

				roughness = lerp(roughness, _GrassRoughnessModifier, _GrassRoughnessModifierStrength);

				float wetratio = saturate(2.0*wetness - pow(1.2*(height), 2));
				roughness = lerp(roughness, 0.1, wetratio);

				albedo = ColorTransfer(satellite, albedo, 1- _SatelliteProportion);
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
				float4 albedo = float4(1, 1, 1, 1);
				albedo = ColorTransfer(satellite, albedo, 1-_SatelliteProportion);

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
				for (float i = iMin; i <= iMAX; i += 1.0) {
					float4 wt = tex2D(_wavesSampler, float2((i + 0.5f) / _nbWaves, 0));
					float phase = wt.y *10* _Time.x - dot(wt.zw, input.u);
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

				float3 N = normalize(cross(dPdu, dPdv));
				
				if (dot(V, N) < 0.0) {
					N = reflect(N, V); // reflects backfacing normals
				}
				
				float F = 0.02 + 0.98 * meanFresnel(V, N, sigmaSq);

				float D = GGX(N, H, _WaterRoughness);

				fixed4 specular = _LightColor0 * D * F * Geom(N, H, V, L) / (4.0f*dot(V, N)*dot(N, L));
				fixed4 diffuse = _WaterColor * dot(N, L) * _LightColor0;
				fixed4 ambient = _WaterColor * (float4(0.6, 0.7, 0.95, 1.0) + unity_AmbientGround + unity_AmbientEquator + unity_AmbientSky);

				float3 Ty = normalize(float3(0, N.z, -N.y));
				float3 Tx = cross(Ty, N);

				return /*reflectedSunRadiance(N, Tx, Ty, sigmaSq);*/_WaterRoughness * (diffuse)+specular + ambient;
			}

			float RockH(float weight) {
				float detailStrength = _enableDetails ? lerp(0, _RockDetailStrength, LOD) : 0;
				float hL = (UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(UV*_RockUVLargeMultiply, 0.0 + _N_NOISE))).r;
				float hD = (UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(UV*_RockUVDetailMultiply, 1.0 + _N_NOISE))).r;

				float height = ((hL + detailStrength * hD)*_RockHeightStrength / (1.0 + detailStrength) + _RockHeightOffset) * weight;

				return height;
			}

			float GravelH(float weight) {
				float detailStrength = _enableDetails ? lerp(0, _GravelDetailStrength, LOD) : 0;

				float hL = (UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(UV*_GravelUVLargeMultiply, 4.0 + _N_NOISE))).r*_GravelHeightStrength + _GravelHeightOffset;
				float hD = detailStrength * (UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(UV*_GravelUVDetailMultiply, 10 + _N_NOISE))).r*_CommonHeightDetailStrength + _CommonHeightDetailOffset;
				
				float height;

				if (hL > hD || !_enableDetails) {
					height = hL * weight;
				}
				else {
					height = hD * weight;
				}

				return height;
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
				
				float3 Tangent = normalize(input.tangent);
				float3 Biangent = normalize(input.bitangent);

				Normal = normalize(input.normal);

				tspace0 = float3(Tangent.x, Biangent.x, Normal.x);
				tspace1 = float3(Tangent.y, Biangent.y, Normal.y);
				tspace2 = float3(Tangent.z, Biangent.z, Normal.z);

				V = normalize(_WorldSpaceCameraPos - input.worldSpacePosition.xyz);
				L = normalize(input.lightDirection.xyz);
				H = normalize(V + L);
				UV = input.uv;// +(0.1*Normal.xz);//*0.1f;

				float4 satelliteD = tex2D(_Sat, input.uv);
				float4 satelliteM = tex2Dlod(_Sat, float4(input.uv, 0, 4));

				float4 albedoL = (UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(UV*10, 0.0)));
				float4 albedoLMean = (UNITY_SAMPLE_TEX2DARRAY_LOD(_Textures, float3(UV*10, 0.0), 4));


				//return float4(microfacet(input, float4(1, 1, 1, 1), float3(0,0,1), _RockRoughnessModifier).rgb, 1.0);

				Depth = length(_WorldSpaceCameraPos - input.worldSpacePosition.xyz);
				Depth = Depth / 200.0f;

				LOD = saturate((lerp(0, 1.0, 1.0 / Depth)));

				DetailMaterialWeight = 1;//saturate((pow(LOD + 0.2, 2.5) - 0.2) / 0.8);

				Noise = (UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(input.uv, 0)));

				Noise_Classes = (UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(input.uv, 11)));

				//return Noise_Classes.x;

				satellite = tex2D(_Sat, input.uv);


				if (_enableNoise == 1) {
					input.uv += _noiseStrength * Noise.xy;
				}

				float4 DGR = tex2D(_ClassesDGR, input.uv);//+float4(Noise_Classes.xy, 0, 0);
				float4 WSGT = tex2D(_ClassesWSGT, input.uv);// +float4(0, 0, Noise_Classes.w, 0);
				//return WSGT;

				//return Rock(input, 1, 0, 1, 0);
				//return float4(UV.xy, 0.0, 1.0);
				//return float4(2*(Normal-0.5), 1.0);
				
				//return float4(Grass(input, 1.0).rgb, 1.0);

				float4 Color = float4(0, 0, 0, 1.0);

				if (DetailMaterialWeight > 0) {
					float wetness = pow(saturate(10.0*WSGT.y), 1.5);

					float heightMixWeight = _heightBasedMix == 1 ? saturate((pow(LOD+0.1, 3)-0.3)/0.7) : 0;

					float4 ColorH = float4(0, 0, 0, 0);
					float4 ColorB = float4 (0, 0, 0, 0);

					if (heightMixWeight > 0) {
						float waterH = WSGT.x;
						float snowH = 0;
						float grassH = 0;
						float treesH = 0;
						float rockH = 0;
						float dirtH = 0;
						float gravelH = 0;
						float commonH = 0;

#define HEIGHT(TERRAIN, WEIGHT, OFFSET) (((UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(UV* TERRAIN##UVLargeMultiply, OFFSET + _N_NOISE))).r + (_enableDetails ? lerp(0, TERRAIN##DetailStrength, LOD) * (UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(UV* TERRAIN##UVDetailMultiply, OFFSET + 1 + _N_NOISE))).r : 0)) * TERRAIN##HeightStrength + TERRAIN##HeightOffset) * WEIGHT;

						//Snow
						if (WSGT.y > 0)
						{
							snowH = HEIGHT(_Snow, WSGT.y, 2.0)
						}
						//Grass
						if (WSGT.z > 0)
						{
							grassH = HEIGHT(_Grass, WSGT.z, 8.0)
						}
						//Trees
						if ((1 - WSGT.w) > 0)
						{
							treesH = 0;//HEIGHT(Trees, WSGT.y, 2);
						}
						//Dirt
						if (DGR.x > 0)
						{
							dirtH = HEIGHT(_Dirt, DGR.x, 6.0)
						}
						//Gravel
						if (DGR.y > 0)
						{
							gravelH = HEIGHT(_Gravel, DGR.y, 4.0);
						}
						//Rock
						if (DGR.z > 0)
						{
							rockH = RockH(DGR.z);//HEIGHT(_Rock, DGR.z, 0)
						}

						if (DGR.y > 0 || DGR.x > 0 || WSGT.z > 0) {
							float detailStrength = _enableDetails ? lerp(0, 1, LOD) : 0;
							if (detailStrength > 0) {
								float hD = detailStrength * (UNITY_SAMPLE_TEX2DARRAY(_HeightTextures, float3(UV*_CommonUVDetailMultiply, 10 + _N_NOISE))).r*_CommonHeightDetailStrength + _CommonHeightDetailOffset;
								commonH = saturate(DGR.y + DGR.x + WSGT.z) * hD;
							}

						}


						float maxH = max(rockH, max(gravelH, max(dirtH, max(treesH, max(grassH, max(snowH, max(commonH, waterH)))))));
						//return maxH;
						if (maxH == waterH) ColorH = Water(input);
						else if (maxH == treesH) ColorH = Trees(input, (1 - WSGT.w));
						else if (maxH == snowH) ColorH = Snow(input, WSGT.y);
						else if (maxH == gravelH) ColorH = Gravel(input, DGR.y, wetness, gravelH);
						else if (maxH == grassH) ColorH = Grass(input, WSGT.z, wetness, grassH);
						else if (maxH == dirtH) ColorH = Dirt(input, DGR.x, wetness, dirtH);
						else if (maxH == rockH || maxH == commonH) ColorH = Rock(input, DGR.z, wetness, rockH, commonH);
					}
					if(heightMixWeight < 1){
						float4 water = float4(0, 0, 0, 0);
						float4 snow = float4(0, 0, 0, 0);
						float4 grass = float4(0, 0, 0, 0);
						float4 trees = float4(0, 0, 0, 0);
						float4 dirt = float4(0, 0, 0, 0);
						float4 gravel = float4(0, 0, 0, 0);
						float4 rock = float4(0, 0, 0, 0);

						//return wetness;
						//Water
						if (WSGT.x > 0)
						{
							water = Water(input);
							water.w *= WSGT.x;
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
						if ((1 - WSGT.w) > 0)
						{
							trees = Trees(input, (1 - WSGT.w));
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
						float sum = DGR.x + DGR.y + DGR.z + WSGT.x + WSGT.y + WSGT.z + (1 - WSGT.w);
						DGR /= sum;
						WSGT /= sum;

						ColorB = float4(rock.rgb*DGR.z + gravel.rgb*DGR.y + dirt.rgb*DGR.x + trees.rgb*WSGT.w + grass.rgb*WSGT.z + snow.rgb*WSGT.y + water.rgb*WSGT.x, 1.0);
					}
					Color = lerp(ColorB, ColorH, heightMixWeight);

					//Color = lerp(satellite, Color, DetailMaterialWeight);

				}
				else {
					Color = satellite;
				}


				return Color;

			}
			ENDCG
		}
	}
}
