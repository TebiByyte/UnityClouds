using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class NoiseGenerator : MonoBehaviour
{
    [Range(4, 40)]
    public int numberOfPoints = 20;
    public int textureRes = 512;
    public ComputeShader noiseGen;
    public RenderTexture noiseTexture;

    private Vector3[] cells = new Vector3[] {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, 0, 1),
            new Vector3(-1, 0, 0),
            new Vector3(0, -1, 0),
            new Vector3(0, 0, -1),
            //Edges
            new Vector3(1, 1, 0),
            new Vector3(1, -1, 0),
            new Vector3(-1, 1, 0),
            new Vector3(-1, -1, 0),
            new Vector3(0, 1, 1),
            new Vector3(0, 1, -1),
            new Vector3(0, -1, 1),
            new Vector3(0, -1, -1),
            new Vector3(1, 0, 1),
            new Vector3(1, 0, -1),
            new Vector3(-1, 0, 1),
            new Vector3(-1, 0, -1),
            //Corners
            new Vector3(1, 1, 1),
            new Vector3(-1, 1, 1),
            new Vector3(1, -1, 1),
            new Vector3(1, 1, -1),
            new Vector3(-1, -1, 1),
            new Vector3(-1, 1, -1),
            new Vector3(1, -1, -1),
            new Vector3(-1, -1, -1)};

    Vector3[] generatePoints()
    {
        Vector3[] result = new Vector3[numberOfPoints * cells.Length];
        Vector3[] cell0 = new Vector3[numberOfPoints];

        for (int i = 0; i < numberOfPoints; i++)
        {
            float x = Random.Range(-0.5f, 0.5f);
            float y = Random.Range(-0.5f, 0.5f);
            float z = Random.Range(-0.5f, 0.5f);
            cell0[i] = new Vector3(x, y, z);
        }

        for (int i = 0; i < cells.Length; i++)
        {
            for (int j = 0; j < numberOfPoints; j++)
            {
                int index = i * numberOfPoints + j;
                result[index] = cell0[j] + cells[i];
            }
        }

        return result;
    }

    public void resetNoise()
    {
        noiseTexture.Release();
        noiseTexture = null;
    }

    public void generateNoise()
    {
        Vector3[] points0 = generatePoints();
        Vector3[] points1 = generatePoints();
        Vector3[] points2 = generatePoints();

        if (noiseTexture == null)
        {
            noiseTexture = createTexture(textureRes, RenderTextureFormat.ARGBFloat);
        }

        if (noiseGen == null)
        {
            Debug.Log("Doesn't exist");

        }

        ComputeBuffer pointBuffer0 = new ComputeBuffer(points1.Length, 24, ComputeBufferType.Structured);
        pointBuffer0.SetData(points0);
        ComputeBuffer pointBuffer1 = new ComputeBuffer(points1.Length, 24, ComputeBufferType.Structured);
        pointBuffer1.SetData(points1);
        ComputeBuffer pointBuffer2 = new ComputeBuffer(points1.Length, 24, ComputeBufferType.Structured);
        pointBuffer2.SetData(points2);

        noiseGen.SetInt("numPoints", points0.Length);
        noiseGen.SetTexture(0, "Result", noiseTexture);
        noiseGen.SetBuffer(0, "Data0", pointBuffer0);
        noiseGen.SetBuffer(0, "Data1", pointBuffer1);
        noiseGen.SetBuffer(0, "Data2", pointBuffer2);
        noiseGen.SetFloat("texSize", textureRes);
        noiseGen.Dispatch(0, 512 / 8, 512 / 8, 512 / 8);
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
