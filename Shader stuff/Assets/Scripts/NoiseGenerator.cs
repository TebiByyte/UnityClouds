using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class NoiseGenerator : MonoBehaviour
{
    [HideInInspector]
    public List<Vector3> points;
    [Range(4, 40)]
    public int numberOfPoints = 20;
    public int textureRes = 512;
    public ComputeShader noiseGen;
    public RenderTexture noiseTexture;
    public bool visualizePoints = false;
    private ComputeBuffer pointBuffer;
    public Material shader;

    private void Start()
    {
        createTexture(textureRes, RenderTextureFormat.ARGBFloat);
    }

    private void OnDrawGizmos()
    {
        if (visualizePoints)
        {
            foreach (Vector3 point in points)
            {
                Gizmos.DrawSphere(point, 0.01f);
            }
        }
    }

    public void generatePoints()
    {
        //Todo: Clone points to make worley noise
        List<Vector3> result = new List<Vector3>();

        for (int i = 0; i < numberOfPoints; i++)
        {
            float x = Random.Range(-0.5f, 0.5f);
            float y = Random.Range(-0.5f, 0.5f);
            float z = Random.Range(-0.5f, 0.5f);
            result.Add(new Vector3(x, y, z));
        }

        points = result;
    }

    public void resetNoise()
    {
        noiseTexture.Release();
        noiseTexture = null;
    }

    public void generateNoise()
    {
        if (noiseTexture == null)
        {
            noiseTexture = createTexture(textureRes, RenderTextureFormat.ARGBFloat);
        }


        pointBuffer = new ComputeBuffer(points.Count, 24, ComputeBufferType.Structured);
        pointBuffer.SetData(points.ToArray());

        int kernel = noiseGen.FindKernel("CSMain");
        noiseGen.SetInt("numPoints", points.Count);
        noiseGen.SetTexture(kernel, "Result", noiseTexture);
        noiseGen.SetBuffer(kernel, "Data", pointBuffer);
        noiseGen.SetFloat("texSize", textureRes);
        noiseGen.Dispatch(kernel, 512 / 8, 512 / 8, 512 / 8);

        shader.SetTexture("_MainTex", noiseTexture);
    }

    public RenderTexture createTexture(int size, RenderTextureFormat format)
    {
        RenderTexture result = new RenderTexture(size, size, 0, format, 10);
        result.enableRandomWrite = true;
        result.volumeDepth = size;
        result.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        result.format = format;
        result.wrapMode = TextureWrapMode.Repeat;
        result.Create();

        return result;
    }
}
