using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[Serializable]
public class EzSS_AppleManager
{
	#region ========== Smartphones =====================================================================================
	public enum Smartphones
	{
		iphone5c,
		iphone5s,
		iphone6s,
		iphone7
	}

	public enum Iphone5cColors
	{
		blue,
		green,
		red,
		white
	}

	public enum Iphone5sColors
	{
		gold,
		silver,
		spaceGray
	}

	public enum Iphone6sColors
	{
		gold,
		silver,
		spaceGray,
		goldRose
	}

	public enum Iphone7Colors
	{
		black,
		jetBlack,
		silver,
		gold,
		goldRose
	}

	public Smartphones smartphonesApple = EzSS_AppleManager.Smartphones.iphone5c;
	public Iphone5cColors iphone5cColor = EzSS_AppleManager.Iphone5cColors.blue;
	public Iphone5sColors iphone5sColor = EzSS_AppleManager.Iphone5sColors.gold;
	public Iphone6sColors iphone6sColor = EzSS_AppleManager.Iphone6sColors.gold;
	public Iphone7Colors iphone7Color = EzSS_AppleManager.Iphone7Colors.black;

	public string SmartphoneSelector()
	{
		smartphonesApple = (Smartphones)EditorGUILayout.EnumPopup("Device:", smartphonesApple);
		return smartphonesApple.ToString();
	}

	public string SmartphoneColorSelector()
	{
		if(smartphonesApple == Smartphones.iphone5c)
		{
			iphone5cColor = (Iphone5cColors)EditorGUILayout.EnumPopup("Color:", iphone5cColor);
			return iphone5cColor.ToString();
		}
		else if(smartphonesApple == Smartphones.iphone5s)
		{
			iphone5sColor = (Iphone5sColors)EditorGUILayout.EnumPopup("Color:", iphone5sColor);
			return iphone5sColor.ToString();
		}
		else if(smartphonesApple == Smartphones.iphone6s)
		{
			iphone6sColor = (Iphone6sColors)EditorGUILayout.EnumPopup("Color:", iphone6sColor);
			return iphone6sColor.ToString();
		}
		else
		{
			iphone7Color = (Iphone7Colors)EditorGUILayout.EnumPopup("Color:", iphone7Color);
			return iphone7Color.ToString();
		}
	}
	#endregion =========================================================================================================

	#region ========== Tablets =========================================================================================
	public enum Tablets
	{
		ipadAir2
	}

	public enum IpadAir2Colors
	{
		gold,
		silver,
		spaceGray
	}

	public Tablets tablet = Tablets.ipadAir2;
	public IpadAir2Colors ipadAir2Color = IpadAir2Colors.gold;

	public string TabletSelector()
	{
		tablet = (Tablets)EditorGUILayout.EnumPopup("Device:", tablet);
		return tablet.ToString();
	}

	public string TabletColorSelector()
	{
		ipadAir2Color = (IpadAir2Colors)EditorGUILayout.EnumPopup("Color:", ipadAir2Color);
		return ipadAir2Color.ToString();
	}
	#endregion =========================================================================================================

	#region ========== Computers =======================================================================================
	public enum Computers
	{
		imac,
		macbook,
		macbookAir,
		macbookPro
	}

	public enum MacbookColors
	{
		gold,
		spaceGray
	}

	public Computers computer = Computers.imac;
	public MacbookColors mackbookColor = MacbookColors.gold;

	public string ComputerSelector()
	{
		computer = (Computers)EditorGUILayout.EnumPopup("Device:", computer);
		return computer.ToString();
	}

	public string ComputerColorSelector()
	{
		if(computer == Computers.macbook)
		{
			mackbookColor = (MacbookColors)EditorGUILayout.EnumPopup("Color:", mackbookColor);
			return mackbookColor.ToString();
		}
		else
			return "";
	}
	#endregion =========================================================================================================

	#region ========== Whatches =======================================================================================
	public enum Watches
	{
		watchSeries1
	}

	public enum WatchSeries1Colors
	{
		black,
		blue,
		green,
		red,
		spaceGray,
		white
	}

	public Watches watch = Watches.watchSeries1;
	public WatchSeries1Colors watchSeries1Colors = WatchSeries1Colors.black;

	public string WatchSelector()
	{
		watch = (Watches)EditorGUILayout.EnumPopup("Device:", watch);
		return watch.ToString();
	}

	public string WatchColorSelector()
	{
		watchSeries1Colors = (WatchSeries1Colors)EditorGUILayout.EnumPopup("Color:", watchSeries1Colors);
		return watchSeries1Colors.ToString();
	}
	#endregion =========================================================================================================

	/// <summary>
	/// Ver se é necessário
	/// </summary>
	public Dictionary<string, Vector2> devicesFixedSizes = new Dictionary<string, Vector2>()
	{
		{"iphone5c", new Vector2(774f, 1605f)},
		{"iphone5s", new Vector2(769f, 1607f)},
		{"iphone6s", new Vector2(919.885f, 1819.772f)},
		{"iphone7", new Vector2(920f, 1820f)},
		{"ipadAir2", new Vector2(1856f, 2608f)},
		{"imac", new Vector2(2000f, 1687f)},
		{"macbook", new Vector2(2000f, 1182f)},
		{"macbookAir", new Vector2(2000f, 1182f)},
		{"macbookPro", new Vector2(2000f, 1182f)},
		{"watchSeries1", new Vector2(512f, 990f)}
	};

	/// <summary>
	/// The resolutions are setted based on portrait
	/// </summary>
	public Dictionary<string, EzSS_AspectHelper.Aspects> devicesAspects = new Dictionary<string, EzSS_AspectHelper.Aspects>()
	{
		{"iphone5c", EzSS_AspectHelper.Aspects.aspect_9_16},
		{"iphone5s", EzSS_AspectHelper.Aspects.aspect_9_16},
		{"iphone6s", EzSS_AspectHelper.Aspects.aspect_9_16},
		{"iphone7", EzSS_AspectHelper.Aspects.aspect_9_16},
		{"ipadAir2", EzSS_AspectHelper.Aspects.aspect_3_4},
		{"imac", EzSS_AspectHelper.Aspects.aspect_16_9},
		{"macbook", EzSS_AspectHelper.Aspects.aspect_16_10},
		{"macbookAir", EzSS_AspectHelper.Aspects.aspect_16_10},
		{"macbookPro", EzSS_AspectHelper.Aspects.aspect_16_10},
		{"watchSeries1", EzSS_AspectHelper.Aspects.aspect_4_5}
	};
		
	// Example:
	// Size of the image W or H = 100%
	// The wanted size W or H <-> X
	public Dictionary<string, Vector2> devicesScreenPercentage = new Dictionary<string, Vector2>()
	{
		{"iphone5c", new Vector2(82.945f, 70.825f)},		
		{"iphone5s", new Vector2(83.983f, 70.918f)},
		{"iphone6s", new Vector2(81.531f, 73.457f)},
		{"iphone7", new Vector2(82.255f, 73.791f)},
		{"ipadAir2", new Vector2(82.48f, 78.37f)},
		{"imac", new Vector2(91.9f, 61.31f)},
		{"macbook", new Vector2(75.3f, 79.69f)},
		{"macbookAir", new Vector2(73.25f, 77.41f)},
		{"macbookPro", new Vector2(75.1f, 79.52f)},
		{"watchSeries1", new Vector2(60.54f, 39.19f)}
	};

	// Example:
	// Size of the image W or H <-> 100%
	// Pos of the image X or Y - Pos of the offset X or Y <-> X
	public Dictionary<string, Vector2> devicesOffsets = new Dictionary<string, Vector2>()
	{
		{"iphone5c", new Vector2(.129f, .249f)},
		{"iphone5s", new Vector2(-.075f, .155f)},
		{"iphone6s", new Vector2(.081f, .460f)},
		{"iphone7", new Vector2(-.067f, .082f)},
		{"ipadAir2", new Vector2(.04f, 0f)},
		{"imac", new Vector2(0, -12.79f)},
		{"macbook", new Vector2(0, -3.21f)},
		{"macbookAir", new Vector2(.05f, -2.79f)},
		{"macbookPro", new Vector2(0f, -3.55f)},
		{"watchSeries1", new Vector2(0f, 0f)}
	};
}