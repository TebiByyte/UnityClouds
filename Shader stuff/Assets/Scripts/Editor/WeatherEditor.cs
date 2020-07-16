using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WeatherMapGen))]
public class WeatherEditor : Editor
{
    public override void OnInspectorGUI()
    {
        WeatherMapGen generator = (WeatherMapGen)target;

        if (GUILayout.Button("Generate Noise Texture"))
        {
            generator.generateNoise();
        }

        base.OnInspectorGUI();
    }
}
