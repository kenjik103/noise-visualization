using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using static Unity.Mathematics.math;


public abstract class Visualization : MonoBehaviour
{
    public enum Shape
    {
        Plane, Sphere, Torus
    }

    Shapes.ScheduleDelegate[] shapesJobs =
    {
        Shapes.Job<Shapes.Plane>.ScheduleParallel,
        Shapes.Job<Shapes.Sphere>.ScheduleParallel,
        Shapes.Job<Shapes.Torus>.ScheduleParallel,
    };

    [SerializeField]
    Shape shape;
    
    static int
        positionsId = Shader.PropertyToID("_Positions"),
        configId = Shader.PropertyToID("_Config"),
        normalsId = Shader.PropertyToID("_Normals");

    [SerializeField] Material material;
    [SerializeField] Mesh mesh;

    [SerializeField, Range(1, 512)] int resolution = 16;
    
    [SerializeField, Range(0.1f, 10f)]
	float instanceScale = 2f;

    [SerializeField, Range(-0.5f, 0.5f)] float displacement;

    NativeArray<float3x4> positions, normals;

    ComputeBuffer positionsBuffer, normalsBuffer;
    
    MaterialPropertyBlock propertyBlock;
    
    bool isDirty;
    Bounds bounds;

    protected abstract void EnableVisualization(int dataLength, MaterialPropertyBlock propertyBlock);
    protected abstract void DisableVisualization();
    protected abstract void UpdateVisualization(NativeArray<float3x4>positions, int resolution, JobHandle handle);

    public void OnEnable() {
        isDirty = true;
        int length = resolution * resolution;
        length = length / 4 + (length & 1);
        
        positions = new NativeArray<float3x4>(length, Allocator.Persistent);
        normals = new NativeArray<float3x4>(length, Allocator.Persistent);
        
        positionsBuffer = new ComputeBuffer(length * 4, 4 * 3);
        normalsBuffer = new ComputeBuffer(length * 4, 4 * 3);
        
        
        propertyBlock ??= new MaterialPropertyBlock();
        EnableVisualization(length, propertyBlock);
        propertyBlock.SetBuffer(positionsId, positionsBuffer);
        propertyBlock.SetBuffer(normalsId, normalsBuffer);
        propertyBlock.SetVector(configId, new Vector4(resolution, instanceScale / resolution, displacement));
    }

    public void OnDisable() {
        DisableVisualization();
        
        positions.Dispose();
        normals.Dispose();
        
        positionsBuffer.Release();
        normalsBuffer.Release();
        
        positionsBuffer = null;
        normalsBuffer = null;
    }

    public void OnValidate() {
        if (positionsBuffer != null && enabled) {
            OnDisable();
            OnEnable();
        }
    }

    public void Update() {
        if (isDirty || transform.hasChanged) {
            isDirty = false;
            transform.hasChanged = false;
            
            UpdateVisualization(
                positions, resolution,
                shapesJobs[(int)shape]
                    (positions, normals, resolution, transform.localToWorldMatrix,default));
            
            positionsBuffer.SetData(positions.Reinterpret<float3>(3 * 4 * 4));
            normalsBuffer.SetData(normals.Reinterpret<float3>(3 * 4 * 4));

            bounds = new Bounds(
                transform.position,
                float3(2f * cmax(abs(transform.lossyScale)) + displacement)
                );

        }
            
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds,
            resolution * resolution, propertyBlock);
    }
}