using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MaterialManager))]
public class MaterialManagerEditor : Editor
{
    [SerializeField]
    private bool _AmbientFoldout = true;
    [SerializeField]
    private bool _RockFoldout = true;
    [SerializeField]
    private bool _SnowFoldout = true;
    [SerializeField]
    private bool _GravelFoldout = true;
    [SerializeField]
    private bool _DirtFoldout = true;
    [SerializeField]
    private bool _GrassFoldout = true;
    [SerializeField]
    private bool _GlobalFoldout = true;
    [SerializeField]
    private bool _NoiseFoldout = true;

    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();

        MaterialManager myScript = (MaterialManager)target;

        GUIStyle style = new GUIStyle();
        style.fontStyle = FontStyle.Bold;

        EditorStyles.foldout.fontStyle = FontStyle.Bold;
        EditorStyles.foldout.fontSize = 11;

        // GLOBAL PARAMETERS
        _GlobalFoldout = EditorGUILayout.Foldout(_GlobalFoldout, "Global Parameters", "Foldout");
        if (_GlobalFoldout)
        {
            EditorGUI.indentLevel++;
            myScript.LiveUpdate = EditorGUILayout.Toggle("Live Texture Update", myScript.LiveUpdate);

            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Satellite Image Proportion");
            myScript.SatelliteProportion = EditorGUILayout.Slider(myScript.SatelliteProportion, 0.0f, 1.0f);
            GUILayout.EndHorizontal();

            myScript.EnableNormalMaps = EditorGUILayout.Toggle("Enable Normal Maps", myScript.EnableNormalMaps);
            myScript.EnableDetails = EditorGUILayout.Toggle("Enable Details", myScript.EnableDetails);
            myScript.HeightBasedMix = EditorGUILayout.Toggle("Height Based Mix", myScript.HeightBasedMix);
            myScript.Tessellation = EditorGUILayout.Toggle("Tessellation", myScript.Tessellation);
            EditorGUI.indentLevel--;
        }

        // AMBIENT LIGHT OPTIONS
        _AmbientFoldout = EditorGUILayout.Foldout(_AmbientFoldout, "Ambient Light", "Foldout");
        if(_AmbientFoldout)
        {
            EditorGUI.indentLevel++;

            myScript.AmbientLightColor = EditorGUILayout.ColorField("Color", myScript.AmbientLightColor);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Strength");
            myScript.AmbientLightStrength = EditorGUILayout.FloatField(myScript.AmbientLightStrength);
            GUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }

        // NOISE OPTIONS
        _NoiseFoldout = EditorGUILayout.Foldout(_NoiseFoldout, "Noise", "Foldout");
        if (_NoiseFoldout)
        {
            EditorGUI.indentLevel++;
            myScript.EnableNoiseUV = EditorGUILayout.Toggle("Enable UV Noise", myScript.EnableNoiseUV);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("UV Scale");
            myScript.NoiseUVScale = EditorGUILayout.FloatField(myScript.NoiseUVScale);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("UV Strength");
            myScript.NoiseUVStrength = EditorGUILayout.FloatField(myScript.NoiseUVStrength);
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            myScript.EnableNoiseHue = EditorGUILayout.Toggle("Enable HL Noise", myScript.EnableNoiseHue);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Hue Scale");
            myScript.NoiseHueScale = EditorGUILayout.FloatField(myScript.NoiseHueScale);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Hue Strength");
            myScript.NoiseHueStrength = EditorGUILayout.FloatField(myScript.NoiseHueStrength);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Luminance Scale");
            myScript.NoiseLumScale = EditorGUILayout.FloatField(myScript.NoiseLumScale);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Luminance Strength");
            myScript.NoiseLumStrength = EditorGUILayout.FloatField(myScript.NoiseLumStrength);
            GUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }


        // ROCK MATERIAL OPTIONS
        _RockFoldout = EditorGUILayout.Foldout(_RockFoldout, "Rock Material", "Foldout");
        if(_RockFoldout)
        {
            EditorGUI.indentLevel++;

            // Large Textures
            EditorGUILayout.LabelField("Large Textures", style);
            {
                EditorGUI.indentLevel++;

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("UV Multiplier");
                myScript.RockLargeUVMultiply = EditorGUILayout.FloatField(myScript.RockLargeUVMultiply);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Albedo");
                myScript.RockColorLarge = (Texture2D)EditorGUILayout.ObjectField(myScript.RockColorLarge, typeof(Texture2D), false);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Roughness");
                myScript.RockRoughnessLarge = (Texture2D)EditorGUILayout.ObjectField(myScript.RockRoughnessLarge, typeof(Texture2D), false);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Height");
                myScript.RockHeightLarge = (Texture2D)EditorGUILayout.ObjectField(myScript.RockHeightLarge, typeof(Texture2D), false);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Normal");
                myScript.RockNormalLarge = (Texture2D)EditorGUILayout.ObjectField(myScript.RockNormalLarge, typeof(Texture2D), false);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel++;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Strength");
                myScript.RockNormaLargeStrength = EditorGUILayout.Slider(myScript.RockNormaLargeStrength, 0.0f, 1.0f);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;

                EditorGUI.indentLevel--;
            }

            // Detail Textures
            EditorGUILayout.LabelField("Details Textures", style);
            {
                EditorGUI.indentLevel++;

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Details Strength");
                myScript.RockDetailStrength = EditorGUILayout.Slider(myScript.RockDetailStrength, 0.0f, 1.0f);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("UV Multiplier");
                myScript.RockDetailUVMultiply = EditorGUILayout.FloatField(myScript.RockDetailUVMultiply);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Albedo");
                myScript.RockColorDetails = (Texture2D)EditorGUILayout.ObjectField(myScript.RockColorDetails, typeof(Texture2D), false);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Roughness");
                myScript.RockRoughnessDetails = (Texture2D)EditorGUILayout.ObjectField(myScript.RockRoughnessDetails, typeof(Texture2D), false);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Height");
                myScript.RockHeightDetails = (Texture2D)EditorGUILayout.ObjectField(myScript.RockHeightDetails, typeof(Texture2D), false);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Normal");
                myScript.RockNormalDetails = (Texture2D)EditorGUILayout.ObjectField(myScript.RockNormalDetails, typeof(Texture2D), false);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel++;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Strength");
                myScript.RockNormaDetailStrength = EditorGUILayout.Slider(myScript.RockNormaDetailStrength, 0.0f, 1.0f);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;


                EditorGUI.indentLevel--;
            }

            EditorGUILayout.LabelField("Roughness Override", style);
            {
                EditorGUI.indentLevel++;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Roughness");
                myScript.RockRoughnessModifier = EditorGUILayout.Slider(myScript.RockRoughnessModifier, 0.0f, 1.0f);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Strength");
                myScript.RockRoughnessModifierStrength = EditorGUILayout.Slider(myScript.RockRoughnessModifierStrength, 0.0f, 1.0f);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.LabelField("Height Control", style);
            {
                EditorGUI.indentLevel++;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Strength");
                myScript.RockHeightStrength = EditorGUILayout.Slider(myScript.RockHeightStrength, 0.0f, 2.0f);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Offset");
                myScript.RockHeightOffset = EditorGUILayout.Slider(myScript.RockHeightOffset, -1.0f, 1.0f);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
        }

        // SNOW MATERIAL OPTIONS
        _SnowFoldout = EditorGUILayout.Foldout(_SnowFoldout, "Snow Material", "Foldout");
        if (_SnowFoldout)
        {
            EditorGUI.indentLevel++;

            // Large Textures
            EditorGUILayout.LabelField("Large Textures", style);
            {
                EditorGUI.indentLevel++;

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("UV Multiplier");
                myScript.SnowLargeUVMultiply = EditorGUILayout.FloatField(myScript.SnowLargeUVMultiply);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Albedo");
                myScript.SnowColorLarge = (Texture2D)EditorGUILayout.ObjectField(myScript.SnowColorLarge, typeof(Texture2D), false);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Roughness");
                myScript.SnowRoughnessLarge = (Texture2D)EditorGUILayout.ObjectField(myScript.SnowRoughnessLarge, typeof(Texture2D), false);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Height");
                myScript.SnowHeightLarge = (Texture2D)EditorGUILayout.ObjectField(myScript.SnowHeightLarge, typeof(Texture2D), false);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Normal");
                myScript.SnowNormalLarge = (Texture2D)EditorGUILayout.ObjectField(myScript.SnowNormalLarge, typeof(Texture2D), false);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel++;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Strength");
                myScript.SnowNormaLargeStrength = EditorGUILayout.Slider(myScript.SnowNormaLargeStrength, 0.0f, 1.0f);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;

                EditorGUI.indentLevel--;
            }

            // Detail Textures
            EditorGUILayout.LabelField("Details Textures", style);
            {
                EditorGUI.indentLevel++;

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Details Strength");
                myScript.SnowDetailStrength = EditorGUILayout.Slider(myScript.SnowDetailStrength, 0.0f, 1.0f);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("UV Multiplier");
                myScript.SnowDetailUVMultiply = EditorGUILayout.FloatField(myScript.SnowDetailUVMultiply);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Albedo");
                myScript.SnowColorDetails = (Texture2D)EditorGUILayout.ObjectField(myScript.SnowColorDetails, typeof(Texture2D), false);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Roughness");
                myScript.SnowRoughnessDetails = (Texture2D)EditorGUILayout.ObjectField(myScript.SnowRoughnessDetails, typeof(Texture2D), false);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Height");
                myScript.SnowHeightDetails = (Texture2D)EditorGUILayout.ObjectField(myScript.SnowHeightDetails, typeof(Texture2D), false);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Normal");
                myScript.SnowNormalDetails = (Texture2D)EditorGUILayout.ObjectField(myScript.SnowNormalDetails, typeof(Texture2D), false);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel++;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Strength");
                myScript.SnowNormaDetailStrength = EditorGUILayout.Slider(myScript.SnowNormaDetailStrength, 0.0f, 1.0f);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;


                EditorGUI.indentLevel--;
            }

            EditorGUILayout.LabelField("Roughness Override", style);
            {
                EditorGUI.indentLevel++;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Roughness");
                myScript.SnowRoughnessModifier = EditorGUILayout.Slider(myScript.SnowRoughnessModifier, 0.0f, 1.0f);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Strength");
                myScript.SnowRoughnessModifierStrength = EditorGUILayout.Slider(myScript.SnowRoughnessModifierStrength, 0.0f, 1.0f);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.LabelField("Height Control", style);
            {
                EditorGUI.indentLevel++;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Strength");
                myScript.SnowHeightStrength = EditorGUILayout.Slider(myScript.SnowHeightStrength, 0.0f, 2.0f);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Offset");
                myScript.SnowHeightOffset = EditorGUILayout.Slider(myScript.SnowHeightOffset, -1.0f, 1.0f);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }



            EditorGUI.indentLevel--;
        }

        // GRAVEL MATERIAL OPTIONS
        _GravelFoldout = EditorGUILayout.Foldout(_GravelFoldout, "Gravel Material", "Foldout");
        if (_GravelFoldout)
        {
            EditorGUI.indentLevel++;

            // Large Textures
            EditorGUILayout.LabelField("Large Textures", style);
            {
                EditorGUI.indentLevel++;

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("UV Multiplier");
                myScript.GravelLargeUVMultiply = EditorGUILayout.FloatField(myScript.GravelLargeUVMultiply);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Albedo");
                myScript.GravelColorLarge = (Texture2D)EditorGUILayout.ObjectField(myScript.GravelColorLarge, typeof(Texture2D), false);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Roughness");
                myScript.GravelRoughnessLarge = (Texture2D)EditorGUILayout.ObjectField(myScript.GravelRoughnessLarge, typeof(Texture2D), false);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Height");
                myScript.GravelHeightLarge = (Texture2D)EditorGUILayout.ObjectField(myScript.GravelHeightLarge, typeof(Texture2D), false);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel++;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Strength");
                myScript.GravelLargeHeightStrength = EditorGUILayout.Slider(myScript.GravelLargeHeightStrength, 0.0f, 2.0f);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Offset");
                myScript.GravelLargeHeightOffset = EditorGUILayout.Slider(myScript.GravelLargeHeightOffset, -1.0f, 1.0f);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Normal");
                myScript.GravelNormalLarge = (Texture2D)EditorGUILayout.ObjectField(myScript.GravelNormalLarge, typeof(Texture2D), false);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel++;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Strength");
                myScript.GravelNormaLargeStrength = EditorGUILayout.Slider(myScript.GravelNormaLargeStrength, 0.0f, 1.0f);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;

                EditorGUI.indentLevel--;
            }

            // Detail Textures
            EditorGUILayout.LabelField("Details Textures", style);
            {
                EditorGUI.indentLevel++;

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Details Strength");
                myScript.GravelDetailStrength = EditorGUILayout.Slider(myScript.GravelDetailStrength, 0.0f, 1.0f);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("UV Multiplier");
                myScript.GravelDetailUVMultiply = EditorGUILayout.FloatField(myScript.GravelDetailUVMultiply);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Albedo");
                myScript.GravelColorDetails = (Texture2D)EditorGUILayout.ObjectField(myScript.GravelColorDetails, typeof(Texture2D), false);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Roughness");
                myScript.GravelRoughnessDetails = (Texture2D)EditorGUILayout.ObjectField(myScript.GravelRoughnessDetails, typeof(Texture2D), false);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Height");
                myScript.GravelHeightDetails = (Texture2D)EditorGUILayout.ObjectField(myScript.GravelHeightDetails, typeof(Texture2D), false);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel++;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Strength");
                myScript.GravelDetailHeightStrength = EditorGUILayout.Slider(myScript.GravelDetailHeightStrength, 0.0f, 2.0f);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Offset");
                myScript.GravelDetailHeightOffset = EditorGUILayout.Slider(myScript.GravelDetailHeightOffset, -1.0f, 1.0f);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Normal");
                myScript.GravelNormalDetails = (Texture2D)EditorGUILayout.ObjectField(myScript.GravelNormalDetails, typeof(Texture2D), false);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel++;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Strength");
                myScript.GravelNormaDetailStrength = EditorGUILayout.Slider(myScript.GravelNormaDetailStrength, 0.0f, 1.0f);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;


                EditorGUI.indentLevel--;
            }

            EditorGUILayout.LabelField("Roughness Override", style);
            {
                EditorGUI.indentLevel++;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Roughness");
                myScript.GravelRoughnessModifier = EditorGUILayout.Slider(myScript.GravelRoughnessModifier, 0.0f, 1.0f);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Strength");
                myScript.GravelRoughnessModifierStrength = EditorGUILayout.Slider(myScript.GravelRoughnessModifierStrength, 0.0f, 1.0f);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }



            EditorGUI.indentLevel--;
        }

        // DIRT MATERIAL OPTIONS
        _DirtFoldout = EditorGUILayout.Foldout(_DirtFoldout, "Dirt Material", "Foldout");
        if (_DirtFoldout)
        {
            EditorGUI.indentLevel++;

            // Large Textures
            EditorGUILayout.LabelField("Large Textures", style);
            {
                EditorGUI.indentLevel++;

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("UV Multiplier");
                myScript.DirtLargeUVMultiply = EditorGUILayout.FloatField(myScript.DirtLargeUVMultiply);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Albedo");
                myScript.DirtColorLarge = (Texture2D)EditorGUILayout.ObjectField(myScript.DirtColorLarge, typeof(Texture2D), false);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Roughness");
                myScript.DirtRoughnessLarge = (Texture2D)EditorGUILayout.ObjectField(myScript.DirtRoughnessLarge, typeof(Texture2D), false);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Height");
                myScript.DirtHeightLarge = (Texture2D)EditorGUILayout.ObjectField(myScript.DirtHeightLarge, typeof(Texture2D), false);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel++;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Strength");
                myScript.DirtLargeHeightStrength = EditorGUILayout.Slider(myScript.DirtLargeHeightStrength, 0.0f, 2.0f);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Offset");
                myScript.DirtLargeHeightOffset = EditorGUILayout.Slider(myScript.DirtLargeHeightOffset, -1.0f, 1.0f);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Normal");
                myScript.DirtNormalLarge = (Texture2D)EditorGUILayout.ObjectField(myScript.DirtNormalLarge, typeof(Texture2D), false);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel++;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Strength");
                myScript.DirtNormaLargeStrength = EditorGUILayout.Slider(myScript.DirtNormaLargeStrength, 0.0f, 1.0f);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;

                EditorGUI.indentLevel--;
            }

            // Detail Textures
            EditorGUILayout.LabelField("Details Textures", style);
            {
                EditorGUI.indentLevel++;

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Details Strength");
                myScript.DirtDetailStrength = EditorGUILayout.Slider(myScript.DirtDetailStrength, 0.0f, 1.0f);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("UV Multiplier");
                myScript.DirtDetailUVMultiply = EditorGUILayout.FloatField(myScript.DirtDetailUVMultiply);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Albedo");
                myScript.DirtColorDetails = (Texture2D)EditorGUILayout.ObjectField(myScript.DirtColorDetails, typeof(Texture2D), false);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Roughness");
                myScript.DirtRoughnessDetails = (Texture2D)EditorGUILayout.ObjectField(myScript.DirtRoughnessDetails, typeof(Texture2D), false);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Height");
                myScript.DirtHeightDetails = (Texture2D)EditorGUILayout.ObjectField(myScript.DirtHeightDetails, typeof(Texture2D), false);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel++;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Strength");
                myScript.DirtDetailHeightStrength = EditorGUILayout.Slider(myScript.DirtDetailHeightStrength, 0.0f, 2.0f);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Offset");
                myScript.DirtDetailHeightOffset = EditorGUILayout.Slider(myScript.DirtDetailHeightOffset, -1.0f, 1.0f);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Normal");
                myScript.DirtNormalDetails = (Texture2D)EditorGUILayout.ObjectField(myScript.DirtNormalDetails, typeof(Texture2D), false);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel++;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Strength");
                myScript.DirtNormaDetailStrength = EditorGUILayout.Slider(myScript.DirtNormaDetailStrength, 0.0f, 1.0f);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;


                EditorGUI.indentLevel--;
            }

            EditorGUILayout.LabelField("Roughness Override", style);
            {
                EditorGUI.indentLevel++;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Roughness");
                myScript.DirtRoughnessModifier = EditorGUILayout.Slider(myScript.DirtRoughnessModifier, 0.0f, 1.0f);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Strength");
                myScript.DirtRoughnessModifierStrength = EditorGUILayout.Slider(myScript.DirtRoughnessModifierStrength, 0.0f, 1.0f);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }



            EditorGUI.indentLevel--;
        }

        // GRASS MATERIAL OPTIONS
        _GrassFoldout = EditorGUILayout.Foldout(_GrassFoldout, "Grass Material", "Foldout");
        if (_GrassFoldout)
        {
            EditorGUI.indentLevel++;

            // Large Textures
            EditorGUILayout.LabelField("Large Textures", style);
            {
                EditorGUI.indentLevel++;

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("UV Multiplier");
                myScript.GrassLargeUVMultiply = EditorGUILayout.FloatField(myScript.GrassLargeUVMultiply);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Albedo");
                myScript.GrassColorLarge = (Texture2D)EditorGUILayout.ObjectField(myScript.GrassColorLarge, typeof(Texture2D), false);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Roughness");
                myScript.GrassRoughnessLarge = (Texture2D)EditorGUILayout.ObjectField(myScript.GrassRoughnessLarge, typeof(Texture2D), false);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Height");
                myScript.GrassHeightLarge = (Texture2D)EditorGUILayout.ObjectField(myScript.GrassHeightLarge, typeof(Texture2D), false);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel++;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Strength");
                myScript.GrassLargeHeightStrength = EditorGUILayout.Slider(myScript.GrassLargeHeightStrength, 0.0f, 2.0f);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Offset");
                myScript.GrassLargeHeightOffset = EditorGUILayout.Slider(myScript.GrassLargeHeightOffset, -1.0f, 1.0f);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Normal");
                myScript.GrassNormalLarge = (Texture2D)EditorGUILayout.ObjectField(myScript.GrassNormalLarge, typeof(Texture2D), false);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel++;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Strength");
                myScript.GrassNormaLargeStrength = EditorGUILayout.Slider(myScript.GrassNormaLargeStrength, 0.0f, 1.0f);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;

                EditorGUI.indentLevel--;
            }

            // Detail Textures
            EditorGUILayout.LabelField("Details Textures", style);
            {
                EditorGUI.indentLevel++;

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Details Strength");
                myScript.GrassDetailStrength = EditorGUILayout.Slider(myScript.GrassDetailStrength, 0.0f, 1.0f);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("UV Multiplier");
                myScript.GrassDetailUVMultiply = EditorGUILayout.FloatField(myScript.GrassDetailUVMultiply);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Albedo");
                myScript.GrassColorDetails = (Texture2D)EditorGUILayout.ObjectField(myScript.GrassColorDetails, typeof(Texture2D), false);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Roughness");
                myScript.GrassRoughnessDetails = (Texture2D)EditorGUILayout.ObjectField(myScript.GrassRoughnessDetails, typeof(Texture2D), false);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Height");
                myScript.GrassHeightDetails = (Texture2D)EditorGUILayout.ObjectField(myScript.GrassHeightDetails, typeof(Texture2D), false);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel++;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Strength");
                myScript.GrassDetailHeightStrength = EditorGUILayout.Slider(myScript.GrassDetailHeightStrength, 0.0f, 2.0f);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Offset");
                myScript.GrassDetailHeightOffset = EditorGUILayout.Slider(myScript.GrassDetailHeightOffset, -1.0f, 1.0f);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Normal");
                myScript.GrassNormalDetails = (Texture2D)EditorGUILayout.ObjectField(myScript.GrassNormalDetails, typeof(Texture2D), false);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel++;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Strength");
                myScript.GrassNormaDetailStrength = EditorGUILayout.Slider(myScript.GrassNormaDetailStrength, 0.0f, 1.0f);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;


                EditorGUI.indentLevel--;
            }

            EditorGUILayout.LabelField("Roughness Override", style);
            {
                EditorGUI.indentLevel++;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Roughness");
                myScript.GrassRoughnessModifier = EditorGUILayout.Slider(myScript.GrassRoughnessModifier, 0.0f, 1.0f);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Strength");
                myScript.GrassRoughnessModifierStrength = EditorGUILayout.Slider(myScript.GrassRoughnessModifierStrength, 0.0f, 1.0f);
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }



            EditorGUI.indentLevel--;
        }
    }
}
