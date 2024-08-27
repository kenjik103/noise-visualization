using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using static Unity.Mathematics.math;


public class NoiseVisualization : Visualization
{
    static int noiseId = Shader.PropertyToID("_Noise");


    [SerializeField] int seed;

    [SerializeField] SpaceTRS domain = new SpaceTRS {
        scale = 8f
    };

    NativeArray<uint4> noise;
    
    ComputeBuffer noiseBuffer;
    

    protected override void EnableVisualization(int dataLength, MaterialPropertyBlock propertyBlock) {
        
        noise = new NativeArray<uint4>(dataLength, Allocator.Persistent);
        
        noiseBuffer = new ComputeBuffer(dataLength * 4, 4);
        
        
        propertyBlock ??= new MaterialPropertyBlock();
        propertyBlock.SetBuffer(noiseId, noiseBuffer);
    }

    protected override void DisableVisualization() {
        noise.Dispose();
        noiseBuffer.Release();
        noiseBuffer = null;
    }

    protected override void UpdateVisualization(NativeArray<float3x4>positions, int resolution, JobHandle handle) {
            
        handle.Complete();
        
       noiseBuffer.SetData(noise.Reinterpret<float>(4 * 4));
        
    }
}