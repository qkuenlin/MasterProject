using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class QualitySetup : MonoBehaviour
{
    public MaterialManager mm;

    public float verylowQualityModifier = 1;
    public float lowQualityModifier = 1;
    public float mediumQualityModifier = 1;
    public float highQualityModifier = 1;
    public float veryhighQualityModifier = 1;
    public float ultraQualityModifier = 1;
    public float cinematicQualityModifier = 1;


    // Use this for initialization
    void Update()
    {
        PostProcessLayer ppl = GetComponent<PostProcessLayer>();

        if (ppl != null)
        {
            switch (QualitySettings.GetQualityLevel())
            {
                case 0:
                    ppl.antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
                    ppl.fastApproximateAntialiasing.fastMode = true;
                    mm.QualityModifer = verylowQualityModifier;
                    break;
                case 1:
                    ppl.antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
                    ppl.fastApproximateAntialiasing.fastMode = false;
                    mm.QualityModifer = lowQualityModifier;

                    break;
                case 2:
                    ppl.antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
                    ppl.subpixelMorphologicalAntialiasing.quality = SubpixelMorphologicalAntialiasing.Quality.Low;
                    mm.QualityModifer = mediumQualityModifier;

                    break;
                case 3:
                    ppl.antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
                    ppl.subpixelMorphologicalAntialiasing.quality = SubpixelMorphologicalAntialiasing.Quality.Medium;
                    mm.QualityModifer = highQualityModifier;

                    break;
                case 4:
                    ppl.antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
                    ppl.subpixelMorphologicalAntialiasing.quality = SubpixelMorphologicalAntialiasing.Quality.High;
                    mm.QualityModifer = veryhighQualityModifier;

                    break;
                case 5:
                    ppl.antialiasingMode = PostProcessLayer.Antialiasing.TemporalAntialiasing;
                    ppl.temporalAntialiasing.jitterSpread = 0.21f;
                    ppl.temporalAntialiasing.stationaryBlending = 0.95f;
                    ppl.temporalAntialiasing.motionBlending = 0.75f;
                    ppl.temporalAntialiasing.sharpness = 0.25f;

                    mm.QualityModifer = ultraQualityModifier;

                    break;
                case 6:
                    ppl.antialiasingMode = PostProcessLayer.Antialiasing.TemporalAntialiasing;
                    ppl.temporalAntialiasing.jitterSpread = 0.21f;
                    ppl.temporalAntialiasing.stationaryBlending = 0.95f;
                    ppl.temporalAntialiasing.motionBlending = 0.75f;
                    ppl.temporalAntialiasing.sharpness = 0.25f;

                    mm.QualityModifer = cinematicQualityModifier;

                    break;
                default: break;
            }
        }
    }
}
