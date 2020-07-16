using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherMapGen : MonoBehaviour
{
    public int textureRes = 512;
    public ComputeShader noiseGen;
    public RenderTexture noiseTexture;

    public void generateNoise()
    {
        if (noiseTexture == null)
        {
            noiseTexture = createTexture(textureRes, RenderTextureFormat.ARGBFloat);
        }

        noiseGen.SetFloat("texSize", textureRes);
        noiseGen.SetTexture(0, "Result", noiseTexture);
        noiseGen.Dispatch(0, 512 / 8, 512 / 8, 512 / 8);
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
