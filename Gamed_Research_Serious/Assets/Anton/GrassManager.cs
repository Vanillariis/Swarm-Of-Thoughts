using UnityEngine;

public class GrassManager : MonoBehaviour
{
    // Instead of a prefab, just use the built-in Quad
    private Mesh grassMesh;

    public Material grassMaterial;
    public ComputeShader computeShader;

    [Header("Settings")]
    public int instanceCount = 100000;
    public float bladeWidth = 0.1f;
    public float bladeHeight = 0.5f;

    private ComputeBuffer dataBuffer;
    private ComputeBuffer argsBuffer;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

    void Start()
    {
        // Create a standard Quad Mesh if one isn't assigned
        GameObject tempQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        grassMesh = tempQuad.GetComponent<MeshFilter>().sharedMesh;
        Destroy(tempQuad); // We only needed the mesh data

        InitializeBuffers();
        UpdateGrassPositions();
    }

    void InitializeBuffers()
    {
        // Clear if already exists
        if (dataBuffer != null) dataBuffer.Release();
        if (argsBuffer != null) argsBuffer.Release();

        dataBuffer = new ComputeBuffer(instanceCount, sizeof(float) * 4);
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

        args[0] = (uint)grassMesh.GetIndexCount(0);
        args[1] = (uint)instanceCount;
        argsBuffer.SetData(args);

        grassMaterial.SetBuffer("_GrassDataBuffer", dataBuffer);
    }

    void UpdateGrassPositions()
    {
        int kernel = computeShader.FindKernel("CSMain");
        computeShader.SetMatrix("_LocalToWorld", transform.localToWorldMatrix);
        computeShader.SetBuffer(kernel, "_GrassDataBuffer", dataBuffer);
        computeShader.SetInt("_GrassCount", instanceCount);
        computeShader.Dispatch(kernel, Mathf.CeilToInt(instanceCount / 64f), 1, 1);
    }

    void Update()
    {
        // Update shader sizing properties
        grassMaterial.SetFloat("_Width", bladeWidth);
        grassMaterial.SetFloat("_Height", bladeHeight);

        // Render
        Bounds renderBounds = new Bounds(transform.position, transform.lossyScale * 10.0f);
        Graphics.DrawMeshInstancedIndirect(grassMesh, 0, grassMaterial, renderBounds, argsBuffer);
    }

    void OnDisable()
    {
        dataBuffer?.Release();
        argsBuffer?.Release();
    }
}