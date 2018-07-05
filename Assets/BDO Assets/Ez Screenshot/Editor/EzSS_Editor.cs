using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public class EzSS_Editor : EditorWindow 
{
	#region ========== Editor ==========================================================================================
	private Vector2 scrollBar; // Controls the scrollbar position
	private Rect rect; // Used to draw a box
	ScriptableObject target;
	SerializedObject serializedObject;
	private SerializedProperty property;
	public static EditorWindow editorWindow;

	[MenuItem("Window/BDO Assets/Ez Screenshot")]
	public static void ShowWindow()
	{
		editorWindow = EditorWindow.GetWindow(typeof(EzSS_Editor));
		GUIContent _titleContent = new GUIContent("Ez Screenshot");

		editorWindow.autoRepaintOnSceneChange = true;
		editorWindow.titleContent = _titleContent;
		editorWindow.Show();
	}

	private void OnGUI()
	{
		scrollBar = EditorGUILayout.BeginScrollView(scrollBar);

		BasicSetupGUI();
		GUILayout.Space(10);
		MockupGUI();
		GUILayout.Space(10);
		BackgroundGUI();
		GUILayout.Space(10);
		AspectGUI();
		GUILayout.Space(10);
		ResolutionsGUI();
		GUILayout.Space(10);
		ScreenshotGUI();
		GUILayout.Space(10);

		EZ_Style.DisplayInformation("https://forum.unity3d.com/threads/ez-screenshot.457345/");
		EditorGUILayout.EndScrollView();
	}
	#endregion ========== Editor =======================================================================================

	#region ========== Basic Setup =====================================================================================
	private enum CameraType
	{
		single,
		multiple
	}

	private enum EncodeType 
	{
		PNG,
		JPG
	};

	private CameraType cameraType = CameraType.single;
	private EncodeType encodeType = EncodeType.PNG;
	private int jpgQuality = 75;
	public List<Camera> cameras = new List<Camera>();
	private Camera camera; // The camera that is used to take the screenshot
	private string fileNamePrefix = "screenshot"; // The prefix of the name
	private bool useDate = false;
	private bool useTime = false;
	private string finalFileName = "";
	private string saveAtPath = ""; // Save the path that the picture will be saved

	private void BasicSetupGUI()
	{
		// Camera
		EZ_Style.Header("Camera", false, false);
		cameraType = (CameraType)EditorGUILayout.EnumPopup("Camera Type:", cameraType);
		// Camera selection for each type
		if(cameraType == CameraType.single)
			camera = EditorGUILayout.ObjectField("Camera:", camera, typeof(Camera), true,null) as Camera;
		else
		{
			EditorGUILayout.HelpBox(EzSS_Messages.INFO_03, MessageType.Info);
			target = this;
			serializedObject = new SerializedObject(target);
			property = serializedObject.FindProperty("cameras");
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(property, true);
			if(EditorGUI.EndChangeCheck())
				serializedObject.ApplyModifiedProperties();
		}
		EZ_Style.Footer();

		// Encode type
		EZ_Style.Header("Encode", false, false);
		encodeType = (EncodeType)EditorGUILayout.EnumPopup("Encode Type:", encodeType);
		if(encodeType == EncodeType.JPG)
			jpgQuality = EditorGUILayout.IntSlider("Quality", jpgQuality, 1, 100);
		EZ_Style.Footer();

		// Name prefix and configuration
		EZ_Style.Header("Name", false, false);
		fileNamePrefix = EditorGUILayout.TextField("Screenshot Prefix:", fileNamePrefix);
		useDate = EditorGUILayout.Toggle("Use Date:", useDate);
		useTime = EditorGUILayout.Toggle("Use Time:", useTime);

		// Cofigurates the name and display it
		finalFileName = fileNamePrefix;
		if(useDate)
			finalFileName = finalFileName + "_" + DateTime.Now.ToString("yy-MMM-dd");
		if(useTime)
			finalFileName = finalFileName + "-" + DateTime.Now.ToString("HH-mm-ss");
		EditorGUILayout.LabelField("File Name:");
		EZ_Style.BeginHorizontalRectBox();
		EditorGUILayout.LabelField(finalFileName);
		EZ_Style.EndHorizontalRectBox();
		EZ_Style.Footer();

		// Save at
		EZ_Style.Header("Save at Destination", false, false);
		// Set the path to save the image
		EditorGUILayout.LabelField("Save at:");
		EZ_Style.rect = EditorGUILayout.BeginVertical();
		GUI.Box(EZ_Style.rect, GUIContent.none);
		if(string.IsNullOrEmpty(saveAtPath))
			EditorGUILayout.LabelField("Browse location using the button bellow");
		else
			EditorGUILayout.LabelField(saveAtPath);
		EditorGUILayout.EndVertical();

		// Path buttons
		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button("Browse"))
			saveAtPath = EditorUtility.SaveFolderPanel("Save At", saveAtPath, Application.dataPath);
		if(GUILayout.Button("Open Folder"))
			Application.OpenURL("file://" + saveAtPath);
		EditorGUILayout.EndHorizontal();
		EZ_Style.Footer();
	}
	#endregion ========== Basic Setup ==================================================================================

	#region ========== Mockup ==========================================================================================
	EzSS_AppleManager appleManager;

	private enum MockupBrands
	{
		apple
	}

	private enum MockupCategories
	{
		smartphone,
		tablet,
		computer,
		watch
	}

	private enum MockupOrientation
	{
		portrait,
		landscapeLeft,
		landscapeRight
	}

	private MockupBrands mockupBrand = MockupBrands.apple;
	private MockupCategories mockupCategory = MockupCategories.smartphone;
	private MockupOrientation mockupOrientation = MockupOrientation.portrait;

	private bool useMockup = false;
	private bool setMockupOrientation = false; // Mockup has more than one orientation?
	private Vector2 mockupFixedSize = Vector2.zero;
	private Vector2 screenPercentage;
	private Vector2 deviceOffset = Vector2.zero;
	private EzSS_AspectHelper.Aspects mockupAspect = EzSS_AspectHelper.Aspects.free; // this aspect is related to the screen of the mockup

	private Texture2D mockupImage;
	private readonly string mockupsPath = "Assets/BDO Assets/Ez Screenshot/Images/";
	private string selectedBrand = "";
	private string selectedCategory = "";
	private string selectedMockup = "";
	private string selectedColor = "";
	private string selectedOrientation = "";

	private Dictionary<MockupCategories, Vector2> mockupPreviewPreviewOffset = new Dictionary<MockupCategories, Vector2>()
	{
		{ MockupCategories.smartphone, new Vector2(100, 200) },
		{ MockupCategories.tablet, new Vector2(150, 200) },
		{ MockupCategories.computer, new Vector2(200, 200) },
		{ MockupCategories.watch, new Vector2(100, 200) },
	};

	private void MockupGUI()
	{
		if(useMockup = EZ_Style.Header("Mockup", true, useMockup))
		{
			if(appleManager == null)
				appleManager = new EzSS_AppleManager();
			
			if(encodeType == EncodeType.JPG)
			{
				encodeType = EncodeType.PNG; 
				EditorUtility.DisplayDialog(EzSS_Messages.TITLE_00, EzSS_Messages.WARNING_02, EzSS_Messages.BUTTON_00);
			}

			// Select the brand
			mockupBrand = (MockupBrands)EditorGUILayout.EnumPopup("Brand:", mockupBrand);
			// Select the category
			mockupCategory = (MockupCategories)EditorGUILayout.EnumPopup("Category:", mockupCategory);

			// Select the device
			if(mockupCategory == MockupCategories.smartphone)
			{
				if(mockupBrand == MockupBrands.apple)
				{
					setMockupOrientation = true;
					// Mockup configurations
					selectedMockup = appleManager.SmartphoneSelector();
					selectedColor = appleManager.SmartphoneColorSelector();
					mockupFixedSize = appleManager.devicesFixedSizes[appleManager.smartphonesApple.ToString()];
					mockupAspect = appleManager.devicesAspects[appleManager.smartphonesApple.ToString()];
					screenPercentage = appleManager.devicesScreenPercentage[appleManager.smartphonesApple.ToString()];
					deviceOffset = appleManager.devicesOffsets[appleManager.smartphonesApple.ToString()];
				}
			}
			else if(mockupCategory == MockupCategories.tablet)
			{
				if(mockupBrand == MockupBrands.apple)
				{
					setMockupOrientation = true;
					// Mockup configurations
					selectedMockup = appleManager.TabletSelector();
					selectedColor = appleManager.TabletColorSelector();
					mockupFixedSize = appleManager.devicesFixedSizes[appleManager.tablet.ToString()];
					mockupAspect = appleManager.devicesAspects[appleManager.tablet.ToString()];
					screenPercentage = appleManager.devicesScreenPercentage[appleManager.tablet.ToString()];
					deviceOffset = appleManager.devicesOffsets[appleManager.tablet.ToString()];
				}
			}
			else if(mockupCategory == MockupCategories.computer)
			{
				if(mockupBrand == MockupBrands.apple)
				{
					// Computers has wide aspect ratio, but the orientation is portrait because landcape will invert x and y width and height
					setMockupOrientation = false;
					mockupOrientation = MockupOrientation.portrait;
					// Mockup configurations
					selectedMockup = appleManager.ComputerSelector();
					selectedColor = appleManager.ComputerColorSelector();
					mockupFixedSize = appleManager.devicesFixedSizes[appleManager.computer.ToString()];
					mockupAspect = appleManager.devicesAspects[appleManager.computer.ToString()];
					screenPercentage = appleManager.devicesScreenPercentage[appleManager.computer.ToString()];
					deviceOffset = appleManager.devicesOffsets[appleManager.computer.ToString()];
				}
			}
			else
			{
				if(mockupBrand == MockupBrands.apple)
				{
					// Watches has just one orientation
					setMockupOrientation = false;
					mockupOrientation = MockupOrientation.portrait;
					// Mockup configurations
					selectedMockup = appleManager.WatchSelector();
					selectedColor = appleManager.WatchColorSelector();
					mockupFixedSize = appleManager.devicesFixedSizes[appleManager.watch.ToString()];
					mockupAspect = appleManager.devicesAspects[appleManager.watch.ToString()];
					screenPercentage = appleManager.devicesScreenPercentage[appleManager.watch.ToString()];
					deviceOffset = appleManager.devicesOffsets[appleManager.watch.ToString()];
				}
			}

			if(setMockupOrientation)
			{
				mockupOrientation = (MockupOrientation)EditorGUILayout.EnumPopup("Orientation:", mockupOrientation);
				selectedOrientation = mockupOrientation.ToString();

				if(EzSS_AspectHelper.AspectsResults[mockupAspect] < 1 && (mockupOrientation == MockupOrientation.landscapeLeft || mockupOrientation == MockupOrientation.landscapeRight))
					mockupAspect = EzSS_AspectHelper.AspectsInverted[appleManager.devicesAspects[selectedMockup]];
			}
			else
				selectedOrientation = "";

			// Mockup image path
			selectedBrand = mockupBrand.ToString();
			selectedCategory = mockupCategory.ToString();
			if(!string.IsNullOrEmpty(selectedColor))
				selectedColor = "-" + selectedColor;
			if(!string.IsNullOrEmpty(selectedOrientation))
				selectedOrientation = "-" + selectedOrientation;
			string _finalPath = mockupsPath + selectedBrand + "/" + selectedCategory + "/" + selectedMockup + selectedColor + selectedOrientation + ".png";
			// Load the image
			mockupImage = (Texture2D)AssetDatabase.LoadAssetAtPath(_finalPath, typeof(Texture2D));

			// Display the image
			float _offsetX = mockupPreviewPreviewOffset[mockupCategory].x;
			float _offsetY = mockupPreviewPreviewOffset[mockupCategory].y;
			EditorGUILayout.LabelField("Mockup Preview:");
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if(mockupOrientation == MockupOrientation.portrait)
				GUILayout.Label(mockupImage, GUILayout.Width(_offsetX), GUILayout.Height(_offsetY));
			else
				GUILayout.Label(mockupImage, GUILayout.Width(_offsetY), GUILayout.Height(_offsetX));
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}

		EZ_Style.Footer();
	}
	#endregion ========== Mockup =======================================================================================

	#region ========== Background ======================================================================================
	private enum BackgroundColorsOptions
	{
		black,
		transparent,
		white,
		custom
	}

	private BackgroundColorsOptions backgroundColorOption = BackgroundColorsOptions.transparent;
	private Color backgroundColor = new Color(1, 1, 1, 1);
	private bool useBackground;

	private void BackgroundGUI()
	{
		if(useBackground = EZ_Style.Header("Background", true, useBackground))
		{
			backgroundColorOption = (BackgroundColorsOptions)EditorGUILayout.EnumPopup("Background Color:", backgroundColorOption);

			if(backgroundColorOption == BackgroundColorsOptions.black)
				backgroundColor = Color.black;
			else if(backgroundColorOption == BackgroundColorsOptions.transparent)
				backgroundColor = Color.clear;
			else if(backgroundColorOption == BackgroundColorsOptions.white)
				backgroundColor = Color.white;
			else
			{
				if(backgroundColor.a <= 0)
					backgroundColor = Color.white;
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Color:", GUILayout.Width(148));
				backgroundColor = EditorGUILayout.ColorField(GUIContent.none, backgroundColor, false, false, false, GUILayout.MinWidth(10));
				EditorGUILayout.EndHorizontal();
			}
				

			// If alpha is 0 display the warning
			if(encodeType == EncodeType.JPG && backgroundColor.a <= 0)
				EditorGUILayout.HelpBox(EzSS_Messages.WARNING_01, MessageType.Warning);
			EditorGUILayout.HelpBox(EzSS_Messages.INFO_00, MessageType.Info);
		}
		EZ_Style.Footer();
	}
	#endregion =========================================================================================================

	#region ========== Aspect ==========================================================================================
	private EzSS_AspectHelper.Aspects screenshotAspect = EzSS_AspectHelper.Aspects.free;
	private EzSS_AspectHelper.Aspects backgroundAspect = EzSS_AspectHelper.Aspects.free;

	private float aspectResult = 0; // X / Y = RESULT
	private float gameAspectResult = 0; // The aspect ratio of the game view

	private Vector2 aspectVector = Vector2.zero;

	// If mockup is enabled the name will be the mockup aspect otherwise it will be the screenshot aspect
	public static string currentAspectName;

	private void AspectGUI()
	{
		EZ_Style.Header("Aspect", false, false);

		if(useBackground)
			backgroundAspect = (EzSS_AspectHelper.Aspects)EditorGUILayout.EnumPopup("Background Aspect:", backgroundAspect);

		if(useMockup)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Mockup Aspect:", GUILayout.Width(148));
			EZ_Style.BeginHorizontalRectBox();
			EditorGUILayout.LabelField(currentAspectName);
			EZ_Style.EndHorizontalRectBox();
			EditorGUILayout.EndHorizontal();
			VerifyAspect(mockupAspect);
		}
		else
		{
			screenshotAspect = (EzSS_AspectHelper.Aspects)EditorGUILayout.EnumPopup("Screenshot Aspect:", screenshotAspect);
			VerifyAspect(screenshotAspect);
		}
			
		EZ_Style.Footer();
	}

	private void VerifyAspect(EzSS_AspectHelper.Aspects aspect)
	{
		string _newAspectName = "";
		aspectVector = EzSS_AspectHelper.AspectsVectors[aspect];

		// Set the name of the new aspect
		if(Vector2.Equals(aspectVector, Vector2.zero))
			_newAspectName = "";
		else
			_newAspectName = aspectVector.x + ":" + aspectVector.y;

		// If the current name and the new one are different, change the aspect of the game view
		if(!string.Equals(currentAspectName, _newAspectName))
		{
			EzSS_GameView.SetAspectRatio(EzSS_GameView.GameViewSizeType.AspectRatio, (int)aspectVector.x, (int)aspectVector.y, _newAspectName);

			// Reset all the cameras to update the aspect
			Camera[] _tempCameras = GameObject.FindObjectsOfType(typeof(Camera)) as Camera[];
			for (int i = 0; i < _tempCameras.Length; i++) {
				_tempCameras[i].enabled = false;
				_tempCameras[i].enabled = true;
			}
		}
	}
	#endregion ========== Aspect =======================================================================================

	#region ========== Resolutions =====================================================================================
	private int maxSize = 4096;
	private int minSize = 32;

	private int backgroundWidth = 1280;
	private int backgroundHeight = 720;

	private int mockupWidth = 1280;
	private int mockupHeight = 720;

	private int gameViewWidth = 1280;
	private int gameViewHeight = 720;

	private void ResolutionsGUI()
	{
		EZ_Style.Header("Resolutions", false, false);

		if(useBackground)
		{
			SetBackgroundSize();
			GUILayout.Space(10);
		}
		if(useMockup)
			SetMockupSize();
		else
			SetScreenshotSize();

		EZ_Style.Footer();
	}

	private void SetBackgroundSize()
	{
		EditorGUILayout.LabelField("Background Size:");
		Vector2 _size = SizeConstructor(backgroundAspect, backgroundWidth, backgroundHeight);
		backgroundWidth = (int)_size.x;
		backgroundHeight = (int)_size.y;
		EditorGUILayout.HelpBox(EzSS_Messages.INFO_01, MessageType.Info);
	}

	private void SetMockupSize()
	{
		EditorGUILayout.LabelField("Mockup Size:");
		Vector2 _size = SizeConstructor(mockupAspect, mockupWidth, mockupHeight);
		mockupWidth = (int)_size.x;
		mockupHeight = (int)_size.y;

		if(!useBackground)
			EditorGUILayout.HelpBox(EzSS_Messages.INFO_01, MessageType.Info);	
		else
		{
			if(mockupWidth > backgroundWidth)
				mockupWidth = backgroundWidth;
			if(mockupHeight > backgroundHeight)
				mockupHeight = backgroundHeight;

			EditorGUILayout.HelpBox(EzSS_Messages.INFO_02, MessageType.Info);
		}

		// Set the gameview size based on the mockup size
		// If the device is on landscape, the width is the height and height is width
		if(mockupOrientation == MockupOrientation.landscapeLeft || mockupOrientation == MockupOrientation.landscapeRight)
		{
			gameViewWidth =  Mathf.RoundToInt((float)(mockupWidth * screenPercentage.y) / 100);
			gameViewHeight = Mathf.RoundToInt((float)(mockupHeight * screenPercentage.x) / 100);
		}
		else
		{
			gameViewWidth = Mathf.RoundToInt(((float)mockupWidth * screenPercentage.x) / 100);
			gameViewHeight = Mathf.RoundToInt(((float)mockupHeight * screenPercentage.y) / 100);
		}
	}

	private void SetScreenshotSize()
	{
		EditorGUILayout.LabelField("Image Size:");
		if(!useBackground)
			EditorGUILayout.HelpBox(EzSS_Messages.INFO_01, MessageType.Info);
		else
		{
			if(gameViewWidth > backgroundWidth)
				gameViewWidth = backgroundWidth;
			if(gameViewHeight > backgroundHeight)
				gameViewHeight = backgroundHeight;

			EditorGUILayout.HelpBox(EzSS_Messages.INFO_02, MessageType.Info);
		}

		Vector2 _size = SizeConstructor(screenshotAspect, gameViewWidth, gameViewHeight);
		gameViewWidth = (int)_size.x;
		gameViewHeight = (int)_size.y;
	}

	private Vector2 SizeConstructor(EzSS_AspectHelper.Aspects aspect, int width, int height)
	{
		int _width = width;
		int _height = height;

		if(aspect == EzSS_AspectHelper.Aspects.aspect_2_3 ||
			aspect == EzSS_AspectHelper.Aspects.aspect_3_4 ||
			aspect == EzSS_AspectHelper.Aspects.aspect_3_5 ||
			aspect == EzSS_AspectHelper.Aspects.aspect_4_5 ||
			aspect == EzSS_AspectHelper.Aspects.aspect_9_16 ||
			aspect == EzSS_AspectHelper.Aspects.aspect_10_16)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Width:", GUILayout.Width(45));
			rect = EditorGUILayout.BeginHorizontal();
			GUI.Box(rect, GUIContent.none);
			EditorGUILayout.LabelField(_width.ToString());
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Height:", GUILayout.Width(45));
			_height = EditorGUILayout.IntSlider(_height, minSize, maxSize);
			EditorGUILayout.EndHorizontal();

			if(useMockup && mockupImage != null)
				_width = Mathf.RoundToInt(_height / (mockupFixedSize.y / mockupFixedSize.x));
			else
				_width = Mathf.RoundToInt((float)_height * EzSS_AspectHelper.AspectsResults[aspect]);
		}
		else if(aspect == EzSS_AspectHelper.Aspects.aspect_3_2 ||
			aspect == EzSS_AspectHelper.Aspects.aspect_4_3 ||
			aspect == EzSS_AspectHelper.Aspects.aspect_5_3 ||
			aspect == EzSS_AspectHelper.Aspects.aspect_5_4 ||
			aspect == EzSS_AspectHelper.Aspects.aspect_16_9 ||
			aspect == EzSS_AspectHelper.Aspects.aspect_16_10)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Width:", GUILayout.Width(45));
			_width = EditorGUILayout.IntSlider(_width, minSize, maxSize);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Height:", GUILayout.Width(45));
			rect = EditorGUILayout.BeginHorizontal();
			GUI.Box(rect, GUIContent.none);
			EditorGUILayout.LabelField(_height.ToString());
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndHorizontal();

			if(useMockup && mockupImage != null)
			{
				if(mockupCategory == MockupCategories.computer)
					_height = Mathf.RoundToInt(_width / (mockupFixedSize.x / mockupFixedSize.y));
				else
					_height = Mathf.RoundToInt(_width / (mockupFixedSize.y / mockupFixedSize.x));
			}
			else
				_height = Mathf.RoundToInt(_width / EzSS_AspectHelper.AspectsResults[aspect]);
		}
		else
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Width:", GUILayout.Width(45));
			_width = EditorGUILayout.IntSlider(_width, minSize, maxSize);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Height:", GUILayout.Width(45));
			_height = EditorGUILayout.IntSlider(_height, minSize, maxSize);
			EditorGUILayout.EndHorizontal();
		}

		return new Vector2(_width, _height);
	}
	#endregion ========== Resolutions ==================================================================================

	#region ========== Screenshot ======================================================================================
	private readonly Color32 gameImagePlaceColor = new Color32(24, 255, 109, 255);
	private Color testColor;// = gameImagePlaceColor;

	TextureFormat textureFormat = TextureFormat.RGBA32;
	private Texture2D screenshotTexture, backgroundTexture;

	private Texture2D gameViewTexture; // The texture that will receive the image from the camera
	private Texture2D mockupTexture; // the texture that will receive the image from the current mockup
	private Texture2D finalTexture; // This is the texture that will be encoded

	private int screenshotWidth = 0;
	private int screenshotHeight = 0;

	RenderTexture renderTexture;// = new RenderTexture(imageWidth, imageHeight, 24);

	private void ScreenshotGUI()
	{
		EZ_Style.Header("Screenshot", false, false);
		if(GUILayout.Button("Take Screenshot", GUILayout.Height(30)))
		{
			if(string.IsNullOrEmpty(saveAtPath))
			{
				EditorUtility.DisplayDialog(EzSS_Messages.TITLE_00, EzSS_Messages.ERROR_01, EzSS_Messages.BUTTON_00);
				return;
			}
			TakeScreenshot();
		}
		EZ_Style.Footer();
	}

	private void TakeScreenshot()
	{
		if(CheckForErros())
			return;
		EditorUtility.DisplayProgressBar(EzSS_Messages.TITLE_01, EzSS_Messages.INFO_04, 0);

		testColor = gameImagePlaceColor;
		// Configurates the renderTexture and gameViewTexture 
		renderTexture = new RenderTexture(gameViewWidth, gameViewHeight, 24);
		gameViewTexture = new Texture2D(gameViewWidth, gameViewHeight, textureFormat, false);
		GenerateGameViewTexture();

		if(useMockup)
		{	
			GenerateMockupTexture();
			// Scale the clone to match the wanted size
			if(mockupTexture.width != mockupWidth || mockupTexture.height != mockupHeight)
				TextureScale.Bilinear(mockupTexture, mockupWidth, mockupHeight);

			if(useBackground)
			{
				// Creates the final texture
				Texture2D tempFinalTexture = new Texture2D(mockupWidth, mockupHeight, textureFormat, false);
				finalTexture = new Texture2D(backgroundWidth, backgroundHeight, textureFormat, false);
				GenerateBackgroundTexture();
				tempFinalTexture = CombineTextures(tempFinalTexture, mockupTexture, gameViewTexture);
				finalTexture = CombineTextures(finalTexture, backgroundTexture, tempFinalTexture);
			}
			else
			{
				finalTexture = new Texture2D(mockupWidth, mockupHeight, textureFormat, false);
				GenerateGameViewTexture();
				finalTexture = CombineTextures(finalTexture, mockupTexture, gameViewTexture);
			}
		}
		else
		{
			if(useBackground)
			{
				finalTexture = new Texture2D(backgroundWidth, backgroundHeight, textureFormat, false);
				GenerateBackgroundTexture();
				finalTexture = CombineTextures(finalTexture, backgroundTexture, gameViewTexture);
			}
			else
				finalTexture = gameViewTexture;
		}

		Encode(finalTexture);
		EditorUtility.ClearProgressBar();
	}

	private bool CheckForErros()
	{
		if((cameraType == CameraType.single && camera == null) || (cameraType == CameraType.multiple && cameras.Count <= 0))
		{
			EditorUtility.DisplayDialog(EzSS_Messages.TITLE_00, EzSS_Messages.ERROR_00, EzSS_Messages.BUTTON_00);
			return true;
		}

		// Verifies if there's null values inside the list
		if(cameraType == CameraType.multiple)
		{
			// Verifies if there's a null value inside the list and remove it
			int _i = 0;
			while(_i < cameras.Count)
			{
				if(cameras[_i] == null)
				{
					cameras.RemoveAt(_i);
					_i = 0;
				}
				else
					_i++;
			}
			// If the list has no camera return an error
			if(cameras.Count <= 0)
			{
				EditorUtility.DisplayDialog(EzSS_Messages.TITLE_00, EzSS_Messages.ERROR_00, EzSS_Messages.BUTTON_00);
				return true;
			}
			// If there's only one camera change the camera type to single and set the camera at position 0 as the main camera
			else if(cameras.Count == 1)
			{
				cameraType = CameraType.single;
				camera = cameras[0];
				GenerateGameViewTexture();
				return false;
			}
			else
				return false;
		}
		else
			return false;
	}

	private void GenerateGameViewTexture()
	{
		if(gameViewTexture != null)
			gameViewTexture = null;

		// Generates the screenshot based on the camera type
		if(cameraType == CameraType.multiple)
		{
			// Creates three temp textures that will be the base to create the final texture
			Texture2D _tempFinalTexture = new Texture2D(gameViewWidth, gameViewHeight, textureFormat, false);                 
			Texture2D _tempBackgroundTexture = new Texture2D(gameViewWidth, gameViewHeight, textureFormat, false);
			Texture2D _tempForegroundTexture = new Texture2D(gameViewWidth, gameViewHeight, textureFormat, false);
			// Creates an array with the cameras list
			Camera[] _cameras = cameras.ToArray();
			// Sort the array based on the depth of each camera
			System.Array.Sort(_cameras, delegate(Camera camera0, Camera camera1) {
				return EditorUtility.NaturalCompare(camera0.depth.ToString(), camera1.depth.ToString());
			});
			// Combines the render texture of each camera in just one texture
			for (int i = 0; i < _cameras.Length; i++)
			{
				if(i == 0)
					_tempFinalTexture = CaptureRenderTexture(_cameras[i]);
				else
				{
					_tempBackgroundTexture = _tempFinalTexture;
					_tempForegroundTexture = CaptureRenderTexture(_cameras[i]);
					_tempFinalTexture = CombineTextures(_tempFinalTexture, _tempBackgroundTexture, _tempForegroundTexture);
				}
			}
			// Set the result as the gameViewTexture
			gameViewTexture = _tempFinalTexture;
		}
		else
			gameViewTexture = CaptureRenderTexture(camera);
	}

	private Texture2D CaptureRenderTexture(Camera cam)
	{
		// Creates a temp texture
		Texture2D _texture = new Texture2D(gameViewWidth, gameViewHeight, textureFormat, false);
		RenderTexture.active = renderTexture;
		cam.targetTexture = renderTexture;
		cam.Render();
		_texture.ReadPixels(new Rect(0, 0, gameViewWidth, gameViewHeight), 0, 0);
		_texture.Apply();
		cam.targetTexture = null;
		RenderTexture.active = null; 
		return _texture;
	}

	private void GenerateMockupTexture()
	{
		if(mockupTexture != null)
			mockupTexture = null;
		mockupTexture = new Texture2D(mockupImage.width, mockupImage.height, textureFormat, false);
		mockupTexture.SetPixels(mockupImage.GetPixels());
		// Scale the clone to match the wanted size
		if(mockupTexture.width != mockupWidth || mockupTexture.height != mockupHeight)
			TextureScale.Bilinear(mockupTexture, mockupWidth, mockupHeight);
		mockupTexture.Apply();
	}

	private void GenerateBackgroundTexture()
	{
		if(backgroundTexture != null)
			backgroundTexture = null;
		backgroundTexture = new Texture2D(backgroundWidth, backgroundHeight, textureFormat, false);
		Color[] _backgroundPixels = backgroundTexture.GetPixels();
		for (int i = 0; i < _backgroundPixels.Length; i++)
			_backgroundPixels[i] = backgroundColor;

		backgroundTexture.SetPixels(_backgroundPixels);
		backgroundTexture.Apply();
	}

	/// <summary>
	/// Combine the mockup image with the game view image;
	/// All images will be placed on center, so it's necessary to calculate an offset for each mockup;
	/// Save the outTexture and encode it to PNG.
	/// </summary>
	private Texture2D CombineTextures(Texture2D outTexture, Texture2D background, Texture2D foreground, bool combinePixels = false)
	{
		Vector2 offset = Vector2.zero;
		int _offsetX = 0;
		int _offsetY = 0;

		if(useMockup && background == mockupTexture)
		{
			// * -1 because the image need to be corrected on X
			if(mockupOrientation == MockupOrientation.landscapeLeft)
			{
				_offsetX = Mathf.RoundToInt((mockupTexture.height * (deviceOffset.x * -1) ) / 100f);
				_offsetY = Mathf.RoundToInt((mockupTexture.width * deviceOffset.y) / 100f);
				offset = new Vector2((((mockupTexture.width - foreground.width) / 2)) + _offsetY, (((mockupTexture.height - foreground.height) / 2)) - _offsetX);
			}
			// * -1 because the image need to be corrected on Y
			else if(mockupOrientation == MockupOrientation.landscapeRight)
			{
				_offsetX = Mathf.RoundToInt((mockupTexture.height * deviceOffset.x) / 100f);
				_offsetY = Mathf.RoundToInt((mockupTexture.width * (deviceOffset.y * -1)) / 100f);
				offset = new Vector2((((mockupTexture.width - foreground.width) / 2)) + _offsetY, (((mockupTexture.height - foreground.height) / 2)) - _offsetX);
			}
			else
			{
				_offsetX = Mathf.RoundToInt((mockupTexture.width * deviceOffset.x) / 100f);
				_offsetY = Mathf.RoundToInt((mockupTexture.height * deviceOffset.y) / 100f);
				offset = new Vector2((((mockupTexture.width - foreground.width) / 2)) + _offsetX, (((mockupTexture.height - foreground.height) / 2)) - _offsetY);
			}
		}
		else
			offset = new Vector2((((outTexture.width - foreground.width) / 2)), (((outTexture.height - foreground.height) / 2)));

		outTexture.SetPixels(background.GetPixels());

		for (int y = 0; y < foreground.height; y++) 
		{
			for (int x = 0; x < foreground.width; x++) 
			{
				if(x < foreground.width && y < foreground.height)
				{
					Color _bgColor, _fgColor, _finalColor;

					if(background == mockupTexture && background.GetPixel(x, y).a <= 0)
						_bgColor = foreground.GetPixel(x, y);
					else
						_bgColor = outTexture.GetPixel((int)(x + offset.x), (int)(y + offset.y));

					if(combinePixels)
					{
						_fgColor = foreground.GetPixel(x, y) * foreground.GetPixel(x, y).a;
						_finalColor = _bgColor + _fgColor;
					}
					else
					{
						if(background == backgroundTexture && foreground.GetPixel(x, y).a <= 0)
						{
							_fgColor = background.GetPixel(x, y);
						}
						else
							_fgColor = foreground.GetPixel(x, y);

						_finalColor = Color.Lerp(_bgColor, _fgColor, 1f);
					}
						
					outTexture.SetPixel((int)(x + offset.x), (int)(y + offset.y), _finalColor);
				}
				else
					outTexture.SetPixel(x, y, background.GetPixel(x, y));
			}
		}

		outTexture.Apply();
		return outTexture;
	}

	/// <summary>
	/// Get the bytes and encode the texture to PNG
	/// </summary>
	private void Encode(Texture2D image)
	{
		Byte[] bytes;
		if(encodeType == EncodeType.PNG)
			bytes = image.EncodeToPNG();
		else
			bytes = image.EncodeToJPG(jpgQuality);

		string _imageName = GenerateScreenshotName();
		File.WriteAllBytes(_imageName, bytes);
	}

	/// <summary>
	/// Generates a name to the picture
	/// </summary>
	private string GenerateScreenshotName()
	{
		int _i = 0;
		string _number = "";
		bool _nameAvailable = false;

		// Verifies if the name exists and if so add a number after it
		while(!_nameAvailable)
		{
			if(_i > 0)
				_number = _i.ToString();
			if(File.Exists(saveAtPath + "/"+ finalFileName + "_" + _number + ".png"))
				_i++;
			else
				_nameAvailable = true;
		}
			
		return string.Format("{0}/{1}_{2}.png", saveAtPath, finalFileName, _number);
	}
	#endregion ========== Screenshot ===================================================================================

	private void OnDestroy() 
	{ 
//		editorWindow.Close();
		currentAspectName = "null"; 
//		if(appleManager)
//			DestroyImmediate(appleManager);
	}
}