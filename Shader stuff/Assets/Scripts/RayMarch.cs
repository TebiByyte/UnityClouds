using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class RayMarch : MonoBehaviour
{
    public Material effectMat;
    public GameObject bounds;
    public NoiseGenerator gen;
    public Vector3 noiseScale;
    public Vector3 noiseOffset;
    public float darknessThreshold;

    public void Start()
    {
        gen.numberOfPoints = 100;
        gen.generateNoise();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        effectMat.SetVector("VolumeBoundsMax", bounds.transform.position + bounds.transform.localScale / 2);
        effectMat.SetVector("VolumeBoundsMin", bounds.transform.position - bounds.transform.localScale / 2);
        effectMat.SetTexture("_NoiseTex", gen.noiseTexture);
        effectMat.SetVector("noiseScale", noiseScale);
        effectMat.SetVector("noiseOffset", noiseOffset);
        effectMat.SetFloat("darknessThreshold", darknessThreshold);

        Graphics.Blit(source, destination, effectMat);
    }
}
