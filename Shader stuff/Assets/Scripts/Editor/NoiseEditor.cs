using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NoiseGenerator))]
public class NoiseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        NoiseGenerator generator = (NoiseGenerator)target;

        if (GUILayout.Button("Generate Noise Texture"))
        {
            generator.generateNoise();
        }
        if (GUILayout.Button("Reset Noise Texture"))
        {
            generator.resetNoise();
        }

        base.OnInspectorGUI();
    }
}
