using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherMapGen : MonoBehaviour
{
    public int textureRes = 512;
    public ComputeShader noiseGen;
    public RenderTexture noiseTexture;

    private void Start()
    {
        noiseTexture = createTexture(textureRes, RenderTextureFormat.ARGBFloat);
    }

    public void generateNoise()
    {
        noiseTexture = createTexture(textureRes, RenderTextureFormat.ARGBFloat);

        if (noiseGen == null)
        {
            Debug.Log("Weather map couldn't be generated");

        }

        int kernel = noiseGen.FindKernel("CSMain");

        noiseGen.SetFloat("texSize", textureRes);
        noiseGen.SetTexture(kernel, "Result", noiseTexture);
        noiseGen.Dispatch(kernel, 512 / 8, 512 / 8, 512 / 8);
    }

    public RenderTexture createTexture(int size, RenderTextureFormat format)
    {
        RenderTexture result = new RenderTexture(size, size, 0, format, 10);
        result.enableRandomWrite = true;
        result.volumeDepth = size;
        result.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
        result.format = format;
        result.wrapMode = TextureWrapMode.Repeat;
        result.Create();

        return result;
    }
}
