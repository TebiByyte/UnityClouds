using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class RayMarch : MonoBehaviour
{
    public Material effectMat;
    public GameObject bounds;
    public NoiseGenerator gen;

    public void Start()
    {
        gen.numberOfPoints = 40;
        gen.generateNoise();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        effectMat.SetVector("VolumeBoundsMax", bounds.transform.position + bounds.transform.localScale / 2);
        effectMat.SetVector("VolumeBoundsMin", bounds.transform.position - bounds.transform.localScale / 2);
        effectMat.SetTexture("_NoiseTex", gen.noiseTexture);

        Graphics.Blit(source, destination, effectMat);
    }
}
