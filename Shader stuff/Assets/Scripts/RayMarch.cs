using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
[RequireComponent(typeof(NoiseGenerator))]
[RequireComponent(typeof(WeatherMapGen))]
public class RayMarch : MonoBehaviour
{
    public Material effectMat;
    public GameObject bounds;
    public NoiseGenerator gen;//These should be components
    public WeatherMapGen weatherGen;
    public Vector3 noiseScale;
    public Vector3 noiseOffset;
    public float darknessThreshold;
    public float densityThreshold;

    public void Start()
    {
        weatherGen.generateNoise();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        effectMat.SetVector("VolumeBoundsMax", bounds.transform.position + bounds.transform.localScale / 2);
        effectMat.SetVector("VolumeBoundsMin", bounds.transform.position - bounds.transform.localScale / 2);
        effectMat.SetTexture("_NoiseTex", gen.noiseTexture);
        effectMat.SetVector("noiseScale", noiseScale);
        effectMat.SetVector("noiseOffset", noiseOffset);
        effectMat.SetFloat("darknessThreshold", darknessThreshold);
        effectMat.SetFloat("densityThreshold", densityThreshold);
        effectMat.SetTexture("_WeatherMap", weatherGen.noiseTexture);

        Graphics.Blit(source, destination, effectMat);
    }
}
