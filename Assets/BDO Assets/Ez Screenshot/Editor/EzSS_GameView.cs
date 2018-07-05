using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public static class EzSS_GameView
{
	static object gameViewSizesInstance;
	static MethodInfo getGroup;

	public enum GameViewSizeType
	{
		AspectRatio, FixedResolution
	}

	static EzSS_GameView()
	{
		Type gameViewSizes = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
		Type singleType = typeof(ScriptableSingleton<>).MakeGenericType(gameViewSizes);
		PropertyInfo instanceProperty = singleType.GetProperty("instance");
		getGroup = gameViewSizes.GetMethod("GetGroup");
		gameViewSizesInstance = instanceProperty.GetValue(null, null);
	}

	static object GetGroup(GameViewSizeGroupType type)
	{
		return getGroup.Invoke(gameViewSizesInstance, new object[] { (int)type });
	}

	public static GameViewSizeGroupType GetCurrentGroupType()
	{
		PropertyInfo getCurrentGroupTypeProp = gameViewSizesInstance.GetType().GetProperty("currentGroupType");
		return (GameViewSizeGroupType)(int)getCurrentGroupTypeProp.GetValue(gameViewSizesInstance, null);
	}

	public static int FindSize(GameViewSizeGroupType sizeGroupType, string text)
	{
		object group = GetGroup(sizeGroupType);
		MethodInfo getDisplayTexts = group.GetType().GetMethod("GetDisplayTexts");
		string[] displayTexts = getDisplayTexts.Invoke(group, null) as string[];
		for(int i = 0; i < displayTexts.Length; i++)
		{
			string display = displayTexts[i];
			// the text we get is "Name (W:H)" if the size has a name, or just "W:H" e.g. 16:9
			// so if we're querying a custom size text we substring to only get the name
			// You could see the outputs by just logging
			// Debug.Log(display);
			int pren = display.IndexOf('(');
			if(pren != -1)
				display = display.Substring(0, pren-1); // -1 to remove the space that's before the prens. This is very implementation-depdenent
			if(display == text)
				return i;
		}
		return -1;
	}

	public static void SetAspectRatio(GameViewSizeType gameViewSizeType, int width, int height, string name)
	{
		int _aspectIndex = 0;
		if(!string.IsNullOrEmpty(name))
			_aspectIndex= FindSize(GetCurrentGroupType(), name);

		// Verify if the aspect exists. If the aspect is -1, it means that it doesn't exists
		if(_aspectIndex <= -1)
		{
			object group = GetGroup(GetCurrentGroupType());
			MethodInfo addCustomSize = getGroup.ReturnType.GetMethod("AddCustomSize");
			var gvsType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
			var ctor = gvsType.GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int), typeof(string) });
			var newSize = ctor.Invoke(new object[] { (int)gameViewSizeType, width, height, name });
			addCustomSize.Invoke(group, new object[] { newSize });
			return;
		}

		Type gvWndType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
		PropertyInfo selectedSizeIndexProp = gvWndType.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		EditorWindow gvWnd = EditorWindow.GetWindow(gvWndType);
		MethodInfo SizeSelectionCallback = gvWndType.GetMethod("SizeSelectionCallback", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		selectedSizeIndexProp.SetValue(gvWnd, _aspectIndex, null);
		EzSS_Editor.currentAspectName = name;

		// All aspect sizes must be reset to prevent a zoom problem
		for (int i = 0; i < _aspectIndex + 1; i++) 
		{
			SizeSelectionCallback.Invoke(gvWnd, new object[] {i, null});	
		}
	}
}