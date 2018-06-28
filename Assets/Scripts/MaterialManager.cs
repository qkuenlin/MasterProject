using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class MaterialManager : MonoBehaviour
{
    [SerializeField]
    public List<Material> materials;

    [SerializeField]
    public bool LiveUpdate;

    [SerializeField]
    public bool EnableNoiseUV;
    [SerializeField]
    public float NoiseUVScale;
    [SerializeField]
    public float NoiseUVStrength;

    [SerializeField]
    public bool EnableNoiseHue;
    [SerializeField]
    public float NoiseHueScale;
    [SerializeField]
    public float NoiseHueStrength;

    [SerializeField]
    public bool EnableNoiseLum;
    [SerializeField]
    public float NoiseLumScale;
    [SerializeField]
    public float NoiseLumStrength;

    [SerializeField]
    public bool Tessellation;
    [SerializeField]
    public bool EnableDetails;
    [SerializeField]
    public bool EnableNormalMaps;
    [SerializeField]
    public bool HeightBasedMix;
    [SerializeField]
    public Color AmbientLightColor;
    [SerializeField]
    public float AmbientLightStrength;
    [SerializeField]
    public float SatelliteProportion;

    [SerializeField]
    public float RockNormaLargeStrength;
    [SerializeField]
    public float RockNormaDetailStrength;
    [SerializeField]
    public float RockRoughnessModifier;
    [SerializeField]
    public float RockRoughnessModifierStrength;
    [SerializeField]
    public Texture2D RockColorLarge;
    [SerializeField]
    public Texture2D RockRoughnessLarge;
    [SerializeField]
    public Texture2D RockNormalLarge;
    [SerializeField]
    public Texture2D RockHeightLarge;
    [SerializeField]
    public Texture2D RockColorDetails;
    [SerializeField]
    public Texture2D RockRoughnessDetails;
    [SerializeField]
    public Texture2D RockNormalDetails;
    [SerializeField]
    public Texture2D RockHeightDetails;
    [SerializeField]
    public float RockDetailUVMultiply;
    [SerializeField]
    public float RockLargeUVMultiply;
    [SerializeField]
    public float RockDetailStrength;
    [SerializeField]
    public float RockHeightStrength;
    [SerializeField]
    public float RockHeightOffset;

    [SerializeField]
    public float SnowNormaLargeStrength;
    [SerializeField]
    public float SnowNormaDetailStrength;
    [SerializeField]
    public float SnowRoughnessModifier;
    [SerializeField]
    public float SnowRoughnessModifierStrength;
    [SerializeField]
    public Texture2D SnowRoughnessLarge;
    [SerializeField]
    public Texture2D SnowNormalLarge;
    [SerializeField]
    public Texture2D SnowHeightLarge;
    [SerializeField]
    public Texture2D SnowRoughnessDetails;
    [SerializeField]
    public Texture2D SnowNormalDetails;
    [SerializeField]
    public Texture2D SnowHeightDetails;
    [SerializeField]
    public float SnowDetailUVMultiply;
    [SerializeField]
    public float SnowLargeUVMultiply;
    [SerializeField]
    public float SnowDetailStrength;
    [SerializeField]
    public float SnowHeightStrength;
    [SerializeField]
    public float SnowHeightOffset;

    [SerializeField]
    public float GravelNormaLargeStrength;
    [SerializeField]
    public float GravelNormaDetailStrength;
    [SerializeField]
    public float GravelRoughnessModifier;
    [SerializeField]
    public float GravelRoughnessModifierStrength;
    [SerializeField]
    public Texture2D GravelColorLarge;
    [SerializeField]
    public Texture2D GravelRoughnessLarge;
    [SerializeField]
    public Texture2D GravelNormalLarge;
    [SerializeField]
    public Texture2D GravelHeightLarge;
    [SerializeField]
    public Texture2D GravelColorDetails;
    [SerializeField]
    public Texture2D GravelRoughnessDetails;
    [SerializeField]
    public Texture2D GravelNormalDetails;
    [SerializeField]
    public Texture2D GravelHeightDetails;
    [SerializeField]
    public float GravelDetailUVMultiply;
    [SerializeField]
    public float GravelLargeUVMultiply;
    [SerializeField]
    public float GravelDetailStrength;
    [SerializeField]
    public float GravelHeightStrength;
    [SerializeField]
    public float GravelHeightOffset;

    [SerializeField]
    public float DirtNormaLargeStrength;
    [SerializeField]
    public float DirtNormaDetailStrength;
    [SerializeField]
    public float DirtRoughnessModifier;
    [SerializeField]
    public float DirtRoughnessModifierStrength;
    [SerializeField]
    public Texture2D DirtColorLarge;
    [SerializeField]
    public Texture2D DirtRoughnessLarge;
    [SerializeField]
    public Texture2D DirtNormalLarge;
    [SerializeField]
    public Texture2D DirtHeightLarge;
    [SerializeField]
    public Texture2D DirtColorDetails;
    [SerializeField]
    public Texture2D DirtRoughnessDetails;
    [SerializeField]
    public Texture2D DirtNormalDetails;
    [SerializeField]
    public Texture2D DirtHeightDetails;
    [SerializeField]
    public float DirtDetailUVMultiply;
    [SerializeField]
    public float DirtLargeUVMultiply;
    [SerializeField]
    public float DirtDetailStrength;
    [SerializeField]
    public float DirtHeightStrength;
    [SerializeField]
    public float DirtHeightOffset;

    [SerializeField]
    public float GrassNormaLargeStrength;
    [SerializeField]
    public float GrassNormaDetailStrength;
    [SerializeField]
    public float GrassRoughnessModifier;
    [SerializeField]
    public float GrassRoughnessModifierStrength;
    [SerializeField]
    public Texture2D GrassColorLarge;
    [SerializeField]
    public Texture2D GrassRoughnessLarge;
    [SerializeField]
    public Texture2D GrassNormalLarge;
    [SerializeField]
    public Texture2D GrassHeightLarge;
    [SerializeField]
    public Texture2D GrassColorDetails;
    [SerializeField]
    public Texture2D GrassRoughnessDetails;
    [SerializeField]
    public Texture2D GrassNormalDetails;
    [SerializeField]
    public Texture2D GrassHeightDetails;
    [SerializeField]
    public float GrassDetailUVMultiply;
    [SerializeField]
    public float GrassLargeUVMultiply;
    [SerializeField]
    public float GrassDetailStrength;
    [SerializeField]
    public float GrassHeightStrength;
    [SerializeField]
    public float GrassHeightOffset;

    [SerializeField]
    public float CommonNormaDetailStrength;
    [SerializeField]
    public Texture2D CommonNormalDetails;
    [SerializeField]
    public Texture2D CommonHeightDetails;
    [SerializeField]
    public float CommonDetailUVMultiply;
    [SerializeField]
    public float CommonDetailStrength;
    [SerializeField]
    public float CommonDetailHeightStrength;
    [SerializeField]
    public float CommonDetailHeightOffset;

    [SerializeField]
    public bool SlopeModifierEnabled;
    [SerializeField]
    public float SlopeModifierThreshold;
    [SerializeField]
    public float SlopeModifierStrength;
    [SerializeField]
    public bool SlopeModifierDebug;

    [SerializeField]
    public float lodDistance0;
    [SerializeField]
    public float lodDistance1;
    [SerializeField]
    public float lodDistance2;
    [SerializeField]
    public float lodDistance3;
    [SerializeField]
    public float lodDistance4;

    [SerializeField]
    public bool lodDebug;

    [SerializeField]
    public bool materialDebug;
    [SerializeField]
    public Color GravelDebug;
    [SerializeField]
    public Color GrassDebug;
    [SerializeField]
    public Color DirtDebug;
    [SerializeField]
    public Color RockDebug;
    [SerializeField]
    public Color SnowDebug;
    [SerializeField]
    public Color WaterDebug;
    [SerializeField]
    public Color TreesDebug;

    [SerializeField]
    public ReflectionProbe reflectionProbe;

    private Texture2DArray Textures;

    private Texture reflectionCubeMap;

    private Color[] Noise0;
    private Color[] Noise1;
    private int N_NOISE = 2;

    private Texture2DArray HeightTextures;
    private bool _first;
    // Use this for initialization
    void Start()
    {
        Textures = new Texture2DArray(RockColorDetails.width, RockColorDetails.height, 18, TextureFormat.ARGB32, true);
        HeightTextures = new Texture2DArray(RockColorDetails.width, RockColorDetails.height, 5 * 2 + 1 + N_NOISE, TextureFormat.RGBAFloat, true);
        // reflectionCubeMap = new Cubemap(128, TextureFormat.RGB24, true);        

        foreach (Material m in materials)
        {
            m.SetInt("_N_NOISE", N_NOISE);
        }

        _first = true;
        StartCoroutine("TextureUpdate");
        StartCoroutine("RenderReflection");
    }

    void CreateNoise(int size)
    {
        Vector4 offsetX = new Vector4(Random.value, Random.value, Random.value, Random.value);
        Vector4 offsetY = new Vector4(Random.value, Random.value, Random.value, Random.value);

        Noise0 = new Color[size * size];
        Noise1 = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                //UV perturbation
                float xCoord = (offsetX.x + (1.0f * x) / (1.0f * size)) * NoiseUVScale;
                float yCoord = (offsetY.x + (1.0f * y) / (1.0f * size)) * NoiseUVScale;
                float sampleX = 2 * (Mathf.PerlinNoise(xCoord, yCoord) - 0.5f);
                xCoord = (offsetX.y + (1.0f * x) / (1.0f * size)) * NoiseUVScale;
                yCoord = (offsetY.y + (1.0f * y) / (1.0f * size)) * NoiseUVScale;
                float sampleY = 2 * (Mathf.PerlinNoise(xCoord, yCoord) - 0.5f);

                //Hue perturbation
                xCoord = (offsetX.w + (1.0f * x) / (1.0f * size)) * NoiseHueScale;
                yCoord = (offsetY.w + (1.0f * y) / (1.0f * size)) * NoiseHueScale;
                float sampleZ = 2 * (Mathf.PerlinNoise(xCoord, yCoord) - 0.5f);

                //Luminance perturbation
                xCoord = (offsetX.z + (1.0f * x) / (1.0f * size)) * NoiseLumScale;
                yCoord = (offsetY.z + (1.0f * y) / (1.0f * size)) * NoiseLumScale;
                float sampleW = (Mathf.PerlinNoise(xCoord, yCoord) + 0.5f);

                Noise0[y * size + x] = new Color(sampleX, sampleY, sampleZ, sampleW);

                float Weight0 = 1.0f;
                float Weight1 = 1.0f;
                float Weight2 = 1.0f;
                float Weight3 = 1.0f;

                float Frequency = 20.0f;
                float Lacunarity = 2.0f;
                float Gain = 1.0f;
                float sample0 = 0;
                float sample1 = 0;
                float sample2 = 0;
                float sample3 = 0;

                for (int i = 0; i < 8; i++)
                {
                    xCoord = (offsetX.x + (1.0f * x) / (1.0f * size)) * Frequency;
                    yCoord = (offsetY.x + (1.0f * y) / (1.0f * size)) * Frequency;
                    float tmp = Weight0 * Mathf.Pow(Mathf.PerlinNoise(xCoord, yCoord), 2);
                    Weight0 = tmp * Gain;
                    sample0 += tmp;

                    xCoord = (offsetX.y + (1.0f * x) / (1.0f * size)) * Frequency;
                    yCoord = (offsetY.y + (1.0f * y) / (1.0f * size)) * Frequency;
                    tmp = Weight1 * Mathf.Pow(Mathf.PerlinNoise(xCoord, yCoord), 2);
                    Weight1 = tmp * Gain;
                    sample1 += tmp;

                    xCoord = (offsetX.z + (1.0f * x) / (1.0f * size)) * Frequency;
                    yCoord = (offsetY.z + (1.0f * y) / (1.0f * size)) * Frequency;
                    tmp = Weight2 * Mathf.Pow(Mathf.PerlinNoise(xCoord, yCoord), 2);
                    Weight2 = tmp * Gain;
                    sample2 += tmp;

                    xCoord = (offsetX.w + (1.0f * x) / (1.0f * size)) * Frequency;
                    yCoord = (offsetY.w + (1.0f * y) / (1.0f * size)) * Frequency;
                    tmp = Weight3 * Mathf.Pow(Mathf.PerlinNoise(xCoord, yCoord), 2);
                    Weight3 = tmp * Gain;
                    sample3 += tmp;

                    Frequency *= Lacunarity;
                }

                Noise1[y * size + x] = new Color(sample0, sample1, sample2, sample3);

            }
        }
    }

    IEnumerator TextureUpdate()
    {
        while (true)
        {
            if (LiveUpdate || _first)
            {
                _first = false;
                CreateNoise(RockColorDetails.height);

                // _first = false;
                if (RockColorLarge != null) Textures.SetPixels(RockColorLarge.GetPixels(), 0);
                yield return new WaitForSeconds(0.05f);

                if (RockColorDetails != null) Textures.SetPixels(RockColorDetails.GetPixels(), 1);
                yield return new WaitForSeconds(0.05f);

                if (RockRoughnessLarge != null) Textures.SetPixels(RockRoughnessLarge.GetPixels(), 2);
                yield return new WaitForSeconds(0.05f);

                if (RockRoughnessDetails != null) Textures.SetPixels(RockRoughnessDetails.GetPixels(), 3);
                yield return new WaitForSeconds(0.05f);

                if (SnowRoughnessLarge != null) Textures.SetPixels(SnowRoughnessLarge.GetPixels(), 4);
                yield return new WaitForSeconds(0.05f);

                if (SnowRoughnessDetails != null) Textures.SetPixels(SnowRoughnessDetails.GetPixels(), 4 + 1);
                yield return new WaitForSeconds(0.05f);

                if (GravelColorLarge != null) Textures.SetPixels(GravelColorLarge.GetPixels(), 6 + 0);
                yield return new WaitForSeconds(0.05f);

                if (GravelColorDetails != null) Textures.SetPixels(GravelColorDetails.GetPixels(), 6 + 1);
                yield return new WaitForSeconds(0.05f);

                if (GravelRoughnessLarge != null) Textures.SetPixels(GravelRoughnessLarge.GetPixels(), 6 + 2);
                yield return new WaitForSeconds(0.05f);

                if (GravelRoughnessDetails != null) Textures.SetPixels(GravelRoughnessDetails.GetPixels(), 6 + 3);
                yield return new WaitForSeconds(0.05f);

                if (DirtColorLarge != null) Textures.SetPixels(DirtColorLarge.GetPixels(), 10 + 0);
                yield return new WaitForSeconds(0.05f);

                if (DirtColorDetails != null) Textures.SetPixels(DirtColorDetails.GetPixels(), 10 + 1);
                yield return new WaitForSeconds(0.05f);

                if (DirtRoughnessLarge != null) Textures.SetPixels(DirtRoughnessLarge.GetPixels(), 10 + 2);
                yield return new WaitForSeconds(0.05f);

                if (DirtRoughnessDetails != null) Textures.SetPixels(DirtRoughnessDetails.GetPixels(), 10 + 3);
                yield return new WaitForSeconds(0.05f);

                if (GrassColorLarge != null) Textures.SetPixels(GrassColorLarge.GetPixels(), 14 + 0);
                yield return new WaitForSeconds(0.05f);

                if (GrassColorDetails != null) Textures.SetPixels(GrassColorDetails.GetPixels(), 14 + 1);
                yield return new WaitForSeconds(0.05f);

                if (GrassRoughnessLarge != null) Textures.SetPixels(GrassRoughnessLarge.GetPixels(), 14 + 2);
                yield return new WaitForSeconds(0.05f);

                if (GrassRoughnessDetails != null) Textures.SetPixels(GrassRoughnessDetails.GetPixels(), 14 + 3);
                yield return new WaitForSeconds(0.05f);
                Textures.Apply();

                if (RockHeightLarge != null) HeightTextures.SetPixels(RockHeightLarge.GetPixels(), 0 + N_NOISE);
                yield return new WaitForSeconds(0.05f);

                if (RockHeightDetails != null) HeightTextures.SetPixels(RockHeightDetails.GetPixels(), 1 + N_NOISE);
                yield return new WaitForSeconds(0.05f);

                if (SnowHeightLarge != null) HeightTextures.SetPixels(SnowHeightLarge.GetPixels(), 2 + N_NOISE);
                yield return new WaitForSeconds(0.05f);

                if (SnowHeightDetails != null) HeightTextures.SetPixels(SnowHeightDetails.GetPixels(), 3 + N_NOISE);
                yield return new WaitForSeconds(0.05f);

                if (GravelHeightLarge != null) HeightTextures.SetPixels(GravelHeightLarge.GetPixels(), 4 + N_NOISE);
                yield return new WaitForSeconds(0.05f);

                if (GravelHeightDetails != null) HeightTextures.SetPixels(GravelHeightDetails.GetPixels(), 5 + N_NOISE);
                yield return new WaitForSeconds(0.05f);

                if (DirtHeightLarge != null) HeightTextures.SetPixels(DirtHeightLarge.GetPixels(), 6 + N_NOISE);
                yield return new WaitForSeconds(0.05f);

                if (DirtHeightDetails != null) HeightTextures.SetPixels(DirtHeightDetails.GetPixels(), 7 + N_NOISE);
                yield return new WaitForSeconds(0.05f);

                if (GrassHeightLarge != null) HeightTextures.SetPixels(GrassHeightLarge.GetPixels(), 8 + N_NOISE);
                yield return new WaitForSeconds(0.05f);

                if (GrassHeightDetails != null) HeightTextures.SetPixels(GrassHeightDetails.GetPixels(), 9 + N_NOISE);
                yield return new WaitForSeconds(0.05f);

                if (CommonHeightDetails != null) HeightTextures.SetPixels(CommonHeightDetails.GetPixels(), 10 + N_NOISE);
                yield return new WaitForSeconds(0.05f);

                HeightTextures.SetPixels(Noise0, 0);
                HeightTextures.SetPixels(Noise1, 1);

                HeightTextures.Apply();

                foreach (Material m in materials)
                {
                    if (RockNormalDetails != null) m.SetTexture("_RockNormalDetail", RockNormalDetails);
                    if (RockNormalLarge != null) m.SetTexture("_RockNormalLarge", RockNormalLarge);
                    m.SetTexture("_Textures", Textures);
                    yield return new WaitForSeconds(0.05f);

                    if (SnowNormalDetails != null) m.SetTexture("_SnowNormalDetail", SnowNormalDetails);
                    if (SnowNormalLarge != null) m.SetTexture("_SnowNormalLarge", SnowNormalLarge);
                    yield return new WaitForSeconds(0.05f);

                    if (GravelNormalDetails != null) m.SetTexture("_GravelNormalDetail", GravelNormalDetails);
                    if (GravelNormalLarge != null) m.SetTexture("_GravelNormalLarge", GravelNormalLarge);
                    yield return new WaitForSeconds(0.05f);

                    if (DirtNormalDetails != null) m.SetTexture("_DirtNormalDetail", DirtNormalDetails);
                    if (DirtNormalLarge != null) m.SetTexture("_DirtNormalLarge", DirtNormalLarge);
                    yield return new WaitForSeconds(0.05f);

                    if (GrassNormalDetails != null) m.SetTexture("_GrassNormalDetail", GrassNormalDetails);
                    if (GrassNormalLarge != null) m.SetTexture("_GrassNormalLarge", GrassNormalLarge);
                    yield return new WaitForSeconds(0.05f);

                    if (CommonNormalDetails != null) m.SetTexture("_CommonNormalDetail", CommonNormalDetails);

                    m.SetTexture("_HeightTextures", HeightTextures);
                    yield return new WaitForSeconds(0.05f);
                }
                yield return new WaitForSeconds(0.5f);

                Debug.Log("Textures updated");
            }
            yield return new WaitForSeconds(1.0f);

        }
    }

    IEnumerator RenderReflection()
    {
        while (true)
        {
            int i = reflectionProbe.RenderProbe();

            yield return new WaitWhile(() => reflectionProbe.IsFinishedRendering(i));
            reflectionCubeMap = reflectionProbe.texture;

            foreach (Material m in materials)
            {
                if (m != null)
                    m.SetTexture("_ReflectionCubeMap", reflectionCubeMap);
            }

            yield return new WaitForSeconds(1f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Material m in materials)
        {
            m.SetInt("_tesselation", Tessellation ? 1 : 0);
            m.SetInt("_enableNormalMap", EnableNormalMaps ? 1 : 0);
            m.SetInt("_enableDetails", EnableDetails ? 1 : 0);
            m.SetInt("_heightBasedMix", HeightBasedMix ? 1 : 0);
            if(Camera.current != null) m.SetFloat("_SatelliteProportion", SatelliteProportion * Camera.current.pixelWidth);

            m.SetFloat("_noiseStrength", NoiseUVStrength);
            m.SetInt("_enableNoise", EnableNoiseUV ? 1 : 0);

            m.SetFloat("_noiseHueStrength", NoiseHueStrength);
            m.SetInt("_enableNoiseHue", EnableNoiseHue ? 1 : 0);

            m.SetFloat("_noiseLumStrength", NoiseLumStrength);
            m.SetInt("_enableNoiseLum", EnableNoiseLum ? 1 : 0);

            m.SetColor("_AmbientLightColor", AmbientLightColor);
            m.SetFloat("_AmbientLightStrength", AmbientLightStrength);

            m.SetFloat("_RockUVDetailMultiply", RockDetailUVMultiply);
            m.SetFloat("_RockUVLargeMultiply", RockLargeUVMultiply);

            m.SetFloat("_RockNormalDetailStrength", RockNormaDetailStrength);
            m.SetFloat("_RockNormalLargeStrength", RockNormaLargeStrength);

            m.SetFloat("_RockRoughnessModifier", RockRoughnessModifier);
            m.SetFloat("_RockRoughnessModifierStrength", RockRoughnessModifierStrength);
            m.SetFloat("_RockDetailStrength", RockDetailStrength);

            m.SetFloat("_RockHeightStrength", RockHeightStrength);
            m.SetFloat("_RockHeightOffset", RockHeightOffset);

            m.SetFloat("_SnowUVDetailMultiply", SnowDetailUVMultiply);
            m.SetFloat("_SnowUVLargeMultiply", SnowLargeUVMultiply);

            m.SetFloat("_SnowNormalDetailStrength", SnowNormaDetailStrength);
            m.SetFloat("_SnowNormalLargeStrength", SnowNormaLargeStrength);

            m.SetFloat("_SnowRoughnessModifier", SnowRoughnessModifier);
            m.SetFloat("_SnowRoughnessModifierStrength", SnowRoughnessModifierStrength);
            m.SetFloat("_SnowDetailStrength", SnowDetailStrength);

            m.SetFloat("_SnowHeightStrength", SnowHeightStrength);
            m.SetFloat("_SnowHeightOffset", SnowHeightOffset);

            m.SetFloat("_GravelUVDetailMultiply", GravelDetailUVMultiply);
            m.SetFloat("_GravelUVLargeMultiply", GravelLargeUVMultiply);

            m.SetFloat("_GravelNormalDetailStrength", GravelNormaDetailStrength);
            m.SetFloat("_GravelNormalLargeStrength", GravelNormaLargeStrength);

            m.SetFloat("_GravelRoughnessModifier", GravelRoughnessModifier);
            m.SetFloat("_GravelRoughnessModifierStrength", GravelRoughnessModifierStrength);
            m.SetFloat("_GravelDetailStrength", GravelDetailStrength);

            m.SetFloat("_GravelHeightStrength", GravelHeightStrength);
            m.SetFloat("_GravelHeightOffset", GravelHeightOffset);

            m.SetFloat("_DirtUVDetailMultiply", DirtDetailUVMultiply);
            m.SetFloat("_DirtUVLargeMultiply", DirtLargeUVMultiply);

            m.SetFloat("_DirtNormalDetailStrength", DirtNormaDetailStrength);
            m.SetFloat("_DirtNormalLargeStrength", DirtNormaLargeStrength);

            m.SetFloat("_DirtRoughnessModifier", DirtRoughnessModifier);
            m.SetFloat("_DirtRoughnessModifierStrength", DirtRoughnessModifierStrength);
            m.SetFloat("_DirtDetailStrength", DirtDetailStrength);

            m.SetFloat("_DirtHeightStrength", DirtDetailStrength);
            m.SetFloat("_DirtHeightOffset", DirtHeightOffset);

            m.SetFloat("_GrassUVDetailMultiply", GrassDetailUVMultiply);
            m.SetFloat("_GrassUVLargeMultiply", GrassLargeUVMultiply);

            m.SetFloat("_GrassNormalDetailStrength", GrassNormaDetailStrength);
            m.SetFloat("_GrassNormalLargeStrength", GrassNormaLargeStrength);

            m.SetFloat("_GrassRoughnessModifier", GrassRoughnessModifier);
            m.SetFloat("_GrassRoughnessModifierStrength", GrassRoughnessModifierStrength);
            m.SetFloat("_GrassDetailStrength", GrassDetailStrength);

            m.SetFloat("_GrassHeightStrength", GrassHeightStrength);
            m.SetFloat("_GrassHeightOffset", GrassHeightOffset);

            m.SetFloat("_CommonUVDetailMultiply", CommonDetailUVMultiply);
            m.SetFloat("_CommonNormalDetailStrength", CommonNormaDetailStrength);
            m.SetFloat("_CommonDetailStrength", CommonDetailStrength);
            m.SetFloat("_CommonHeightDetailStrength", CommonDetailHeightStrength);
            m.SetFloat("_CommonHeightDetailOffset", CommonDetailHeightOffset);

            m.SetFloat("_LODDistance0", lodDistance0);
            m.SetFloat("_LODDistance1", lodDistance1);
            m.SetFloat("_LODDistance2", lodDistance2);
            m.SetFloat("_LODDistance3", lodDistance3);
            m.SetFloat("_LODDistance4", lodDistance4);
            m.SetInt("_LODDebug", lodDebug ? 1 : 0);

            m.SetInt("_SlopeModifierDebug", SlopeModifierDebug ? 1 : 0);
            m.SetInt("_SlopeModifierEnabled", SlopeModifierEnabled ? 1 : 0);
            m.SetFloat("_SlopeModifierThreshold", SlopeModifierThreshold);
            m.SetFloat("_SlopeModifierStrength", SlopeModifierStrength);

            m.SetInt("_MaterialDebug", materialDebug ? 1 : 0);
            m.SetColor("_GravelDebug", GravelDebug);
            m.SetColor("_GrassDebug", GrassDebug);
            m.SetColor("_DirtDebug", DirtDebug);
            m.SetColor("_RockDebug", RockDebug);
            m.SetColor("_WaterlDebug", WaterDebug);
            m.SetColor("_TreesDebug", TreesDebug);
            m.SetColor("_SnowDebug", SnowDebug);



        }

        RaycastHit info;
        if(Physics.Raycast(Camera.main.transform.position, new Vector3(0,-1,0), out info, 500))
        {
            reflectionProbe.transform.position = transform.position - new Vector3(0, 2*(transform.position.y-info.point.y), 0);
        }
        else
        {
            reflectionProbe.transform.position = transform.position;
        }

    }
}
