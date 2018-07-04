using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI : MonoBehaviour {

    public bool ShowUI = true;

    float deltaTime = 0.0f;

    float lowest = 1000;
    float highest = 0;
    float avg = 0;

    int counter = 0;

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        if (ShowUI)
        {
            if (Time.frameCount > 120)
            {
                int w = Screen.width, h = Screen.height;

                GUIStyle style = new GUIStyle();

                Rect rect = new Rect(5, 5, w, h * 2 / 100);
                style.alignment = TextAnchor.UpperLeft;
                style.fontSize = h * 2 / 100;
                style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
                float msec = deltaTime * 1000.0f;

                float fps = 1.0f / deltaTime;

                if (fps < lowest) lowest = fps;
                else if (fps > highest) highest = fps;

                avg = avg * counter + fps;
                counter++;
                avg /= counter;

                string text = string.Format("Current {0:0.0} ms ({1:0.} fps)\n" + "Average: {2:0.} fps\n" + "Lowest: {3:0.} fps\n" + "Highest: {4:0.} fps\n", msec, fps, avg, lowest, highest);
                GUI.Label(rect, text, style);
            }
        }
    }
}
