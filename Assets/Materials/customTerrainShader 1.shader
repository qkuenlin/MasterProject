Shader "Custom/customTerrainShader_surf" {
	Properties{
		_Proportion("Satellite Image Proportion", Range(0,1)) = 0.5

		_Classes("Segmentation Image", 2D) = "white" {}

		_Sat("Satellite Image", 2D) = "white" {}

		_SnowTex("Snow Base Color", 2D) = "white" {}
		_SnowRoughness("Snow Roughness", Range(0,1)) = 0.5
		_SnowNormalMap("Snow Normal", 2D) = "normal" {}

		//_WaterTex("Water Base Color", 2D) = "white" {}
		_WaterRoughness("Water Roughness", Range(0,1)) = 0.5
		//_WaterNormalMap("Water Normal", 2D) = "normal" {}

		_ForestTex("Forest Base Color", 2D) = "white" {}
		_ForestRoughness("Forest Roughness", Range(0,1)) = 0.5
		_ForestNormalMap("Forest Normal", 2D) = "normal" {}

		_GrassTex("Grass Base Color", 2D) = "white" {}
		_GrassRoughness("Grass Roughness", Range(0,1)) = 0.5
		_GrassNormalMap("Grass Normal", 2D) = "normal" {}

		_GravelTex("Gravel Base Color", 2D) = "white" {}
		_GravelRoughness("Gravel Roughness", Range(0,1)) = 0.5
		_GravelNormalMap("Gravel Normal", 2D) = "normal" {}

		_RockTex("Rock Base Color", 2D) = "white" {}
		_RockRoughness("Rock Roughness", Range(0,1)) = 0.5
		_RockNormalMap("Rock Normal", 2D) = "normal" {}

		_DirtTex("Dirt Base Color", 2D) = "white" {}
		_DirtRoughness("Dirt Roughness", Range(0,1)) = 0.5
		_DirtNormalMap("Dirt Normal", 2D) = "normal" {}

	}
		SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 5.0

		sampler2D _Classes;
		sampler2D _Sat;

		sampler2D _SnowTex;
		half _SnowRoughness;
		sampler2D _SnowNormalMap;

		half _WaterRoughness;

		//sampler2D _WaterNormalMap;

		sampler2D _ForestTex;
		half _ForestRoughness;
		sampler2D _ForestNormalMap;

		sampler2D _GrassTex;
		half _GrassRoughness;
		sampler2D _GrassNormalMap;

		sampler2D _GravelTex;
		half _GravelRoughness;
		sampler2D _GravelNormalMap;

		sampler2D _RockTex;
		half _RockRoughness;
		sampler2D _RockNormalMap;

		sampler2D _DirtTex;
		half _DirtRoughness;
		sampler2D _DirtNormalMap;		

		half _Proportion;

		struct Input {
			float2 uv_SnowTex;
			float2 uv_WaterTex;
			float2 uv_ForestTex;
			float2 uv_GrassTex;
			float2 uv_GravelTex;
			float2 uv_RockTex;
			float2 uv_DirtTex;
			float2 uv_Classes;

			float3 worldNormal;
		};

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf(Input IN, inout SurfaceOutputStandard o) {
			fixed4 segColor = tex2D(_Classes, IN.uv_Classes);

			//Snow
			if (segColor.r > 0.5 && segColor.g > 0.5 && segColor.b > 0.5) {
				o.Albedo = tex2D(_SnowTex, IN.uv_SnowTex).rgb;
				o.Smoothness = 1 - _SnowRoughness;
				o.Normal = UnpackNormal(tex2D(_SnowNormalMap, IN.uv_SnowTex));
			}

			//Water
			else if (segColor.r < 0.5 && segColor.g < 0.5 && segColor.b > 0.5) {
				o.Albedo = fixed3(0.05, 0.45, 1.0);

				o.Smoothness = 1 - _WaterRoughness;
				//o.Normal = normalize(float3(1, 1, 1));//UnpackNormal(tex2D(_WaterNormalMap, IN.uv_WaterTex));

				//IN.worldNormal = float3(0, 0, 1);
			}

			//Forest
			else if (segColor.r > 0.5 && segColor.g < 0.5 && segColor.b < 0.5) {
				o.Albedo = tex2D(_ForestTex, IN.uv_ForestTex).rgb;
				o.Smoothness = 1 - _ForestRoughness;
				o.Normal = UnpackNormal(tex2D(_ForestNormalMap, IN.uv_ForestTex));
			}

			//Grass
			else if (segColor.r < 0.5 && segColor.g > 0.5 && segColor.b < 0.5) {
				o.Albedo = tex2D(_GrassTex, IN.uv_GrassTex).rgb;
				o.Smoothness = 1 - _GrassRoughness;
				o.Normal = UnpackNormal(tex2D(_GrassNormalMap, IN.uv_GrassTex));
			}

			//Gravel
			else if (segColor.r > 0.5 && segColor.g > 0.5 && segColor.b < 0.5) {
				o.Albedo = tex2D(_GravelTex, IN.uv_GravelTex).rgb;
				o.Smoothness = 1 - _GravelRoughness;
				o.Normal = UnpackNormal(tex2D(_GravelNormalMap, IN.uv_GravelTex));
			}

			//Rock
			else if (segColor.r < 0.5 && segColor.g < 0.5 && segColor.b < 0.5) {
				o.Albedo = tex2D(_RockTex, IN.uv_RockTex).rgb;
				o.Smoothness = 1 - _RockRoughness;
				o.Normal = UnpackNormal(tex2D(_RockNormalMap, IN.uv_RockTex));
			}

			//Dirt
			else {
				o.Albedo = tex2D(_DirtTex, IN.uv_DirtTex).rgb;
				o.Smoothness = 1 - _DirtRoughness;
				o.Normal = UnpackNormal(tex2D(_DirtNormalMap, IN.uv_DirtTex));
			} 

			o.Albedo = lerp(o.Albedo, tex2D(_Sat, IN.uv_Classes).rgb, _Proportion);
			o.Normal = normalize(lerp(o.Normal, UnpackNormal(tex2D(_ForestNormalMap, IN.uv_ForestTex)), 0.5));

			o.Metallic = 0;
			o.Alpha = 1;
			//o.Albedo = o.Normal;//tex2D(_ForestNormalMap, IN.uv_ForestTex);
		}
		ENDCG
	}
		//FallBack "Diffuse"
}
