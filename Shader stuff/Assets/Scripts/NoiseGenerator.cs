using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class NoiseGenerator : MonoBehaviour
{
    [HideInInspector]
    public Vector3[] points;
    [Range(4, 40)]
    public int numberOfPoints = 20;
    public int textureRes = 512;
    public ComputeShader noiseGen;
    public RenderTexture noiseTexture;
    private ComputeBuffer pointBuffer;
    public Material shader;

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

    private void Start()
    {
        createTexture(textureRes, RenderTextureFormat.ARGBFloat);
        generateNoise();
    }

    void generatePoints()
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

        points = result;
    }

    public void resetNoise()
    {
        noiseTexture.Release();
        noiseTexture = null;
    }

    public void generateNoise()
    {
        generatePoints();

        if (noiseTexture == null)
        {
            noiseTexture = createTexture(textureRes, RenderTextureFormat.ARGBFloat);
        }


        pointBuffer = new ComputeBuffer(points.Length, 24, ComputeBufferType.Structured);
        pointBuffer.SetData(points);

        int kernel = noiseGen.FindKernel("CSMain");
        noiseGen.SetInt("numPoints", points.Length);
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
