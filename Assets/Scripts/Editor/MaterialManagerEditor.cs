using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MaterialManager))]
public class MaterialManagerEditor : Editor
{
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
    private bool _ForestFoldout = true;
    [SerializeField]
    private bool _GlobalFoldout = true;
    [SerializeField]
    private bool _NoiseFoldout = true;
    [SerializeField]
    private bool _LODFoldout = true;

    [SerializeField]
    private bool _DefaultFoldout = false;

    public override void OnInspectorGUI()
    {
        _DefaultFoldout = EditorGUILayout.Foldout(_DefaultFoldout, "Default Editor", "Foldout");
        if (_DefaultFoldout)
            DrawDefaultInspector();
        else
        {

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

                myScript.debug = EditorGUILayout.Toggle("Debug Mode", myScript.debug);

                myScript.EnableNormalMaps = EditorGUILayout.Toggle("Enable Normal Maps", myScript.EnableNormalMaps);
                myScript.EnableDetails = EditorGUILayout.Toggle("Enable Details", myScript.EnableDetails);
                myScript.HeightBasedMix = EditorGUILayout.Toggle("Height Based Mix", myScript.HeightBasedMix);
                myScript.Tessellation = EditorGUILayout.Toggle("Tessellation", myScript.Tessellation);

                myScript.Parallax = EditorGUILayout.Toggle("Parallax", myScript.Parallax);

                myScript.materialDebug = EditorGUILayout.Toggle("Debug Material", myScript.materialDebug);
                if (myScript.materialDebug)
                {
                    myScript.RockDebug = EditorGUILayout.ColorField("Rock", myScript.RockDebug);
                    myScript.SnowDebug = EditorGUILayout.ColorField("Snow", myScript.SnowDebug);
                    myScript.GravelDebug = EditorGUILayout.ColorField("Gravel", myScript.GravelDebug);
                    myScript.GrassDebug = EditorGUILayout.ColorField("Grass", myScript.GrassDebug);
                    myScript.DirtDebug = EditorGUILayout.ColorField("Dirt", myScript.DirtDebug);
                    myScript.WaterDebug = EditorGUILayout.ColorField("Water", myScript.WaterDebug);
                    myScript.ForestDebug = EditorGUILayout.ColorField("Forest", myScript.ForestDebug);

                }


                EditorGUI.indentLevel--;
            }

            // LOD OPTIONS
            _LODFoldout = EditorGUILayout.Foldout(_LODFoldout, "Level Of Details", "Foldout");
            if (_LODFoldout)
            {
                EditorGUI.indentLevel++;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("LOD Distance 0 [m] (Details Full)");
                myScript.lodDistance0 = EditorGUILayout.FloatField(myScript.lodDistance0);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("LOD Distance 1 [m] (Detail Textures)");
                myScript.lodDistance1 = EditorGUILayout.FloatField(myScript.lodDistance1);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("LOD Distance 2 [m] (Height/Alpha Blend)");
                myScript.lodDistance2 = EditorGUILayout.FloatField(myScript.lodDistance2);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("LOD Distance 3 [m] (Large Texture)");
                myScript.lodDistance3 = EditorGUILayout.FloatField(myScript.lodDistance3);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("LOD Distance 4 [m] (Basic Rendering)");
                myScript.lodDistance4 = EditorGUILayout.FloatField(myScript.lodDistance4);
                GUILayout.EndHorizontal();

                myScript.lodDebug = EditorGUILayout.Toggle("See LODs (Debug)", myScript.lodDebug);


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
            if (_RockFoldout)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.LabelField("Slope Modifier", style);
                {
                    EditorGUI.indentLevel++;
                    myScript.SlopeModifierEnabled = EditorGUILayout.Toggle("Enable Slope Modifier", myScript.SlopeModifierEnabled);
                    if (myScript.SlopeModifierEnabled)
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Slope Threshold");
                        myScript.SlopeModifierThreshold = EditorGUILayout.Slider(myScript.SlopeModifierThreshold, 0.0f, 1.0f);
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Modifier Stength");
                        myScript.SlopeModifierStrength = EditorGUILayout.Slider(myScript.SlopeModifierStrength, 0.0f, 10.0f);
                        GUILayout.EndHorizontal();

                        myScript.SlopeModifierDebug = EditorGUILayout.Toggle("Debug View", myScript.SlopeModifierDebug);
                    }
                    EditorGUI.indentLevel--;

                }

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

                // Common Detail Textures
                EditorGUILayout.LabelField("Common Rock Details", style);
                {
                    EditorGUI.indentLevel++;

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Details Strength");
                    myScript.CommonDetailStrength = EditorGUILayout.Slider(myScript.CommonDetailStrength, 0.0f, 1.0f);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("UV Multiplier");
                    myScript.CommonDetailUVMultiply = EditorGUILayout.FloatField(myScript.CommonDetailUVMultiply);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Color");
                    myScript.CommonColorDetails = (Texture2D)EditorGUILayout.ObjectField(myScript.CommonColorDetails, typeof(Texture2D), false);
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Height");
                    myScript.CommonHeightDetails = (Texture2D)EditorGUILayout.ObjectField(myScript.CommonHeightDetails, typeof(Texture2D), false);
                    GUILayout.EndHorizontal();
                    EditorGUI.indentLevel++;
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Strength");
                    myScript.CommonDetailHeightStrength = EditorGUILayout.Slider(myScript.CommonDetailHeightStrength, 0.0f, 2.0f);
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Offset");
                    myScript.CommonDetailHeightOffset = EditorGUILayout.Slider(myScript.CommonDetailHeightOffset, -1.0f, 1.0f);
                    GUILayout.EndHorizontal();
                    EditorGUI.indentLevel--;

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Normal");
                    myScript.CommonNormalDetails = (Texture2D)EditorGUILayout.ObjectField(myScript.CommonNormalDetails, typeof(Texture2D), false);
                    GUILayout.EndHorizontal();
                    EditorGUI.indentLevel++;
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Strength");
                    myScript.CommonNormaDetailStrength = EditorGUILayout.Slider(myScript.CommonNormaDetailStrength, 0.0f, 1.0f);
                    GUILayout.EndHorizontal();
                    EditorGUI.indentLevel--;


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

                EditorGUILayout.LabelField("Height Control", style);
                {
                    EditorGUI.indentLevel++;
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Strength");
                    myScript.GravelHeightStrength = EditorGUILayout.Slider(myScript.GravelHeightStrength, 0.0f, 2.0f);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Offset");
                    myScript.GravelHeightOffset = EditorGUILayout.Slider(myScript.GravelHeightOffset, -1.0f, 1.0f);
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

                EditorGUILayout.LabelField("Height Control", style);
                {
                    EditorGUI.indentLevel++;
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Strength");
                    myScript.DirtHeightStrength = EditorGUILayout.Slider(myScript.DirtHeightStrength, 0.0f, 2.0f);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Offset");
                    myScript.DirtHeightOffset = EditorGUILayout.Slider(myScript.DirtHeightOffset, -1.0f, 1.0f);
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

                EditorGUILayout.LabelField("Height Control", style);
                {
                    EditorGUI.indentLevel++;
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Strength");
                    myScript.GrassHeightStrength = EditorGUILayout.Slider(myScript.GrassHeightStrength, 0.0f, 2.0f);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Offset");
                    myScript.GrassHeightOffset = EditorGUILayout.Slider(myScript.GrassHeightOffset, -1.0f, 1.0f);
                    GUILayout.EndHorizontal();
                    EditorGUI.indentLevel--;
                }



                EditorGUI.indentLevel--;
            }

            // Forest MATERIAL OPTIONS
            _ForestFoldout = EditorGUILayout.Foldout(_ForestFoldout, "Forest Material", "Foldout");
            if (_ForestFoldout)
            {
                EditorGUI.indentLevel++;

                // Large Textures
                EditorGUILayout.LabelField("Large Textures", style);
                {
                    EditorGUI.indentLevel++;

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("UV Multiplier");
                    myScript.ForestLargeUVMultiply = EditorGUILayout.FloatField(myScript.ForestLargeUVMultiply);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Albedo");
                    myScript.ForestColorLarge = (Texture2D)EditorGUILayout.ObjectField(myScript.ForestColorLarge, typeof(Texture2D), false);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Roughness");
                    myScript.ForestRoughnessLarge = (Texture2D)EditorGUILayout.ObjectField(myScript.ForestRoughnessLarge, typeof(Texture2D), false);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Height");
                    myScript.ForestHeightLarge = (Texture2D)EditorGUILayout.ObjectField(myScript.ForestHeightLarge, typeof(Texture2D), false);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Normal");
                    myScript.ForestNormalLarge = (Texture2D)EditorGUILayout.ObjectField(myScript.ForestNormalLarge, typeof(Texture2D), false);
                    GUILayout.EndHorizontal();
                    EditorGUI.indentLevel++;
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Strength");
                    myScript.ForestNormaLargeStrength = EditorGUILayout.Slider(myScript.ForestNormaLargeStrength, 0.0f, 1.0f);
                    GUILayout.EndHorizontal();
                    EditorGUI.indentLevel--;

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.LabelField("Roughness Override", style);
                {
                    EditorGUI.indentLevel++;
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Roughness");
                    myScript.ForestRoughnessModifier = EditorGUILayout.Slider(myScript.ForestRoughnessModifier, 0.0f, 1.0f);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Strength");
                    myScript.ForestRoughnessModifierStrength = EditorGUILayout.Slider(myScript.ForestRoughnessModifierStrength, 0.0f, 1.0f);
                    GUILayout.EndHorizontal();
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.LabelField("Height Control", style);
                {
                    EditorGUI.indentLevel++;
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Strength");
                    myScript.ForestHeightStrength = EditorGUILayout.Slider(myScript.ForestHeightStrength, 0.0f, 2.0f);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Offset");
                    myScript.ForestHeightOffset = EditorGUILayout.Slider(myScript.ForestHeightOffset, -1.0f, 1.0f);
                    GUILayout.EndHorizontal();
                    EditorGUI.indentLevel--;
                }



                EditorGUI.indentLevel--;
            }
        }
    }
}
