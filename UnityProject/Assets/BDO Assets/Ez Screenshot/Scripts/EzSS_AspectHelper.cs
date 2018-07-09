using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EzSS_AspectHelper 
{
	public enum Aspects
	{
		// Portrait
		aspect_2_3,
		aspect_3_4,
		aspect_3_5,
		aspect_4_5,
		aspect_9_16,
		aspect_10_16,

		// Landscape
		aspect_3_2,
		aspect_4_3,
		aspect_5_3,
		aspect_5_4,
		aspect_16_9,
		aspect_16_10,

		// Custom
		free
	};

	public static Dictionary<Aspects, Aspects> AspectsInverted = new Dictionary<Aspects, Aspects>()
	{
		{Aspects.aspect_2_3, Aspects.aspect_3_2},
		{Aspects.aspect_3_4, Aspects.aspect_4_3},
		{Aspects.aspect_3_5, Aspects.aspect_5_3},
		{Aspects.aspect_4_5, Aspects.aspect_5_4},
		{Aspects.aspect_9_16, Aspects.aspect_16_9},
		{Aspects.aspect_10_16, Aspects.aspect_16_10},
		{Aspects.aspect_3_2, Aspects.aspect_2_3},
		{Aspects.aspect_4_3, Aspects.aspect_3_4},
		{Aspects.aspect_5_3, Aspects.aspect_3_5},
		{Aspects.aspect_5_4, Aspects.aspect_4_5},
		{Aspects.aspect_16_9, Aspects.aspect_9_16},
		{Aspects.aspect_16_10, Aspects.aspect_10_16}
	};

	public static Dictionary<Aspects, Vector2> AspectsVectors = new Dictionary<Aspects, Vector2>()
	{
		{Aspects.aspect_2_3, new Vector2(2, 3)},
		{Aspects.aspect_3_4, new Vector2(3, 4)},
		{Aspects.aspect_3_5, new Vector2(3, 5)},
		{Aspects.aspect_4_5, new Vector2(4, 5)},
		{Aspects.aspect_9_16, new Vector2(9, 16)},
		{Aspects.aspect_10_16, new Vector2(10, 16)},
		{Aspects.aspect_3_2, new Vector2(3, 2)},
		{Aspects.aspect_4_3, new Vector2(4, 3)},
		{Aspects.aspect_5_3, new Vector2(5, 3)},
		{Aspects.aspect_5_4, new Vector2(5, 4)},
		{Aspects.aspect_16_9, new Vector2(16, 9)},
		{Aspects.aspect_16_10, new Vector2(16, 10)},
		{Aspects.free, Vector2.zero}
	};

	public static Dictionary<Aspects, float> AspectsResults = new Dictionary<Aspects, float>()
	{
		{Aspects.aspect_2_3, .666f},
		{Aspects.aspect_3_4, .75f},
		{Aspects.aspect_3_5, .6f},
		{Aspects.aspect_4_5, .8f},
		{Aspects.aspect_9_16, .5625f},
		{Aspects.aspect_10_16, .625f},
		{Aspects.aspect_3_2, 1.5f},
		{Aspects.aspect_4_3, 1.333f},
		{Aspects.aspect_5_3, 1.666f},
		{Aspects.aspect_5_4, 1.25f},
		{Aspects.aspect_16_9, 1.777f},
		{Aspects.aspect_16_10, 1.6f}
	};
}
