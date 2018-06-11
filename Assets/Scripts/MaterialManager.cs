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
    public Texture2D SnowColorLarge;
    [SerializeField]
    public Texture2D SnowRoughnessLarge;
    [SerializeField]
    public Texture2D SnowNormalLarge;
    [SerializeField]
    public Texture2D SnowHeightLarge;
    [SerializeField]
    public Texture2D SnowColorDetails;
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
    public float GravelLargeHeightStrength;
    [SerializeField]
    public float GravelDetailHeightStrength;
    [SerializeField]
    public float GravelLargeHeightOffset;
    [SerializeField]
    public float GravelDetailHeightOffset;

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
    public float DirtLargeHeightStrength;
    [SerializeField]
    public float DirtDetailHeightStrength;
    [SerializeField]
    public float DirtLargeHeightOffset;
    [SerializeField]
    public float DirtDetailHeightOffset;

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
    public float GrassLargeHeightStrength;
    [SerializeField]
    public float GrassDetailHeightStrength;
    [SerializeField]
    public float GrassLargeHeightOffset;
    [SerializeField]
    public float GrassDetailHeightOffset;

    private Texture2DArray Textures;

    private Color[] Noise;

    private Texture2DArray HeightTextures;
    private bool _first;
    // Use this for initialization
    void Start()
    {
        Textures = new Texture2DArray(RockColorDetails.width, RockColorDetails.height, 5 * 4, TextureFormat.ARGB32, true);
        HeightTextures = new Texture2DArray(SnowColorDetails.width, SnowColorDetails.height, 5 * 2 + 1, TextureFormat.RGBAFloat, true);
        CreateNoise(SnowColorDetails.height);

        _first = true;
        StartCoroutine("TextureUpdate");
    }

    void CreateNoise(int size)
    {
        Vector4 offsetX = new Vector4(Random.value, Random.value, Random.value, Random.value);
        Vector4 offsetY = new Vector4(Random.value, Random.value, Random.value, Random.value);


        Noise = new Color[size * size];
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
                float sampleW = (Mathf.PerlinNoise(xCoord, yCoord)+0.5f);
                Noise[y * size + x] = new Color(sampleX, sampleY, sampleZ, sampleW);
            }
        }
    }

    IEnumerator TextureUpdate()
    {
        while (true)
        {
            if (LiveUpdate || _first)
            {
                CreateNoise(SnowColorDetails.height);


                _first = false;
                Textures.SetPixels(RockColorLarge.GetPixels(), 0);
                yield return new WaitForSeconds(0.05f);

                Textures.SetPixels(RockColorDetails.GetPixels(), 1);
                yield return new WaitForSeconds(0.05f);

                Textures.SetPixels(RockRoughnessLarge.GetPixels(), 2);
                yield return new WaitForSeconds(0.05f);

                Textures.SetPixels(RockRoughnessDetails.GetPixels(), 3);
                yield return new WaitForSeconds(0.05f);

                Textures.SetPixels(SnowColorLarge.GetPixels(), 4 + 0);
                yield return new WaitForSeconds(0.05f);

                Textures.SetPixels(SnowColorDetails.GetPixels(), 4 + 1);
                yield return new WaitForSeconds(0.05f);

                Textures.SetPixels(SnowRoughnessLarge.GetPixels(), 4 + 2);
                yield return new WaitForSeconds(0.05f);

                Textures.SetPixels(SnowRoughnessDetails.GetPixels(), 4 + 3);
                yield return new WaitForSeconds(0.05f);

                Textures.SetPixels(GravelColorLarge.GetPixels(), 8 + 0);
                yield return new WaitForSeconds(0.05f);

                Textures.SetPixels(GravelColorDetails.GetPixels(), 8 + 1);
                yield return new WaitForSeconds(0.05f);

                Textures.SetPixels(GravelRoughnessLarge.GetPixels(), 8 + 2);
                yield return new WaitForSeconds(0.05f);

                Textures.SetPixels(GravelRoughnessDetails.GetPixels(), 8 + 3);
                yield return new WaitForSeconds(0.05f);

                Textures.SetPixels(DirtColorLarge.GetPixels(), 12 + 0);
                yield return new WaitForSeconds(0.05f);

                Textures.SetPixels(DirtColorDetails.GetPixels(), 12 + 1);
                yield return new WaitForSeconds(0.05f);

                Textures.SetPixels(DirtRoughnessLarge.GetPixels(), 12 + 2);
                yield return new WaitForSeconds(0.05f);

                Textures.SetPixels(DirtRoughnessDetails.GetPixels(), 12 + 3);
                yield return new WaitForSeconds(0.05f);

                Textures.SetPixels(GrassColorLarge.GetPixels(), 16 + 0);
                yield return new WaitForSeconds(0.05f);

                Textures.SetPixels(GrassColorDetails.GetPixels(), 16 + 1);
                yield return new WaitForSeconds(0.05f);

                Textures.SetPixels(GrassRoughnessLarge.GetPixels(), 16 + 2);
                yield return new WaitForSeconds(0.05f);

                Textures.SetPixels(GrassRoughnessDetails.GetPixels(), 16 + 3);
                yield return new WaitForSeconds(0.05f);
                Textures.Apply();


                HeightTextures.SetPixels(RockHeightLarge.GetPixels(), 0 + 1);
                yield return new WaitForSeconds(0.05f);

                HeightTextures.SetPixels(RockHeightDetails.GetPixels(), 1 + 1);
                yield return new WaitForSeconds(0.05f);

                HeightTextures.SetPixels(SnowHeightLarge.GetPixels(), 2 + 1);
                yield return new WaitForSeconds(0.05f);

                HeightTextures.SetPixels(SnowHeightDetails.GetPixels(), 3 + 1);
                yield return new WaitForSeconds(0.05f);

                HeightTextures.SetPixels(GravelHeightLarge.GetPixels(), 4 + 1);
                yield return new WaitForSeconds(0.05f);

                HeightTextures.SetPixels(GravelHeightDetails.GetPixels(), 5 + 1);
                yield return new WaitForSeconds(0.05f);

                HeightTextures.SetPixels(DirtHeightLarge.GetPixels(), 6 + 1);
                yield return new WaitForSeconds(0.05f);

                HeightTextures.SetPixels(DirtHeightDetails.GetPixels(), 7 + 1);
                yield return new WaitForSeconds(0.05f);

                HeightTextures.SetPixels(GrassHeightLarge.GetPixels(), 8 + 1);
                yield return new WaitForSeconds(0.05f);

                HeightTextures.SetPixels(GrassHeightDetails.GetPixels(), 9 + 1);
                yield return new WaitForSeconds(0.05f);

                HeightTextures.SetPixels(Noise, 0);
                HeightTextures.Apply();

                foreach (Material m in materials)
                {
                    m.SetTexture("_RockNormalDetail", RockNormalDetails);
                    m.SetTexture("_RockNormalLarge", RockNormalLarge);
                    m.SetTexture("_Textures", Textures);
                    yield return new WaitForSeconds(0.05f);

                    m.SetTexture("_SnowNormalDetail", SnowNormalDetails);
                    m.SetTexture("_SnowNormalLarge", SnowNormalLarge);
                    yield return new WaitForSeconds(0.05f);

                    m.SetTexture("_GravelNormalDetail", GravelNormalDetails);
                    m.SetTexture("_GravelNormalLarge", GravelNormalLarge);
                    yield return new WaitForSeconds(0.05f);

                    m.SetTexture("_DirtNormalDetail", DirtNormalDetails);
                    m.SetTexture("_DirtNormalLarge", DirtNormalLarge);
                    yield return new WaitForSeconds(0.05f);

                    m.SetTexture("_GrassNormalDetail", GrassNormalDetails);
                    m.SetTexture("_GrassNormalLarge", GrassNormalLarge);
                    yield return new WaitForSeconds(0.05f);

                    m.SetTexture("_HeightTextures", HeightTextures);
                    yield return new WaitForSeconds(0.05f);
                }
                yield return new WaitForSeconds(0.5f);

                Debug.Log("Textures updated");
            }
            yield return new WaitForSeconds(1.0f);

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
            m.SetFloat("_SatelliteProportion", SatelliteProportion);

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

            m.SetFloat("_GravelHeightDetailStrength", GravelDetailHeightStrength);
            m.SetFloat("_GravelHeightLargeStrength", GravelLargeHeightStrength);
            m.SetFloat("_GravelHeightDetailOffset", GravelDetailHeightOffset);
            m.SetFloat("_GravelHeightLargeOffset", GravelLargeHeightOffset);

            m.SetFloat("_DirtUVDetailMultiply", DirtDetailUVMultiply);
            m.SetFloat("_DirtUVLargeMultiply", DirtLargeUVMultiply);

            m.SetFloat("_DirtNormalDetailStrength", DirtNormaDetailStrength);
            m.SetFloat("_DirtNormalLargeStrength", DirtNormaLargeStrength);

            m.SetFloat("_DirtRoughnessModifier", DirtRoughnessModifier);
            m.SetFloat("_DirtRoughnessModifierStrength", DirtRoughnessModifierStrength);
            m.SetFloat("_DirtDetailStrength", DirtDetailStrength);

            m.SetFloat("_DirtHeightDetailStrength", DirtDetailHeightStrength);
            m.SetFloat("_DirtHeightLargeStrength", DirtLargeHeightStrength);
            m.SetFloat("_DirtHeightDetailOffset", DirtDetailHeightOffset);
            m.SetFloat("_DirtHeightLargeOffset", DirtLargeHeightOffset);

            m.SetFloat("_GrassUVDetailMultiply", GrassDetailUVMultiply);
            m.SetFloat("_GrassUVLargeMultiply", GrassLargeUVMultiply);

            m.SetFloat("_GrassNormalDetailStrength", GrassNormaDetailStrength);
            m.SetFloat("_GrassNormalLargeStrength", GrassNormaLargeStrength);

            m.SetFloat("_GrassRoughnessModifier", GrassRoughnessModifier);
            m.SetFloat("_GrassRoughnessModifierStrength", GrassRoughnessModifierStrength);
            m.SetFloat("_GrassDetailStrength", GrassDetailStrength);

            m.SetFloat("_GrassHeightDetailStrength", GrassDetailHeightStrength);
            m.SetFloat("_GrassHeightLargeStrength", GrassLargeHeightStrength);
            m.SetFloat("_GrassHeightDetailOffset", GrassDetailHeightOffset);
            m.SetFloat("_GrassHeightLargeOffset", GrassLargeHeightOffset);
        }
    }
}
