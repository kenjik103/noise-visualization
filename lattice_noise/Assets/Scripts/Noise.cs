using System.Net.Mime;
using TMPro;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;

using static Unity.Mathematics.math;

public static partial class Noise 
{
    public interface INoise
    {
        float4 GetNoise4(float4x3 postions, SmallXXHash4 hash);
    }

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct Job<N> : IJobFor where N : struct, INoise
    {
        [ReadOnly] NativeArray<float3x4> positions;
        [WriteOnly] NativeArray<float4> noise;
        SmallXXHash4 hash;
        float3x4 domainTRS;

        public void Execute(int i) {
            noise[i] = default(N).GetNoise4(domainTRS.TransformVectors(transpose(positions[i])), hash);
        }

        public static JobHandle ScheduleParallel(NativeArray<float3x4> positions, NativeArray<float4> noise, int seed, SpaceTRS domainTRS, int resolution,
            JobHandle dependency) => new Job<N>
        {
            positions = positions,
            noise = noise,
            hash = SmallXXHash4.Seed(seed),
            domainTRS = domainTRS.Matrix
        }.ScheduleParallel(positions.Length, resolution, dependency);
    }
    public delegate JobHandle ScheduleDelegate(
            NativeArray<float3x4> positions, NativeArray<float4> noise, int seed, float3x4 domainTRS, int resolution, JobHandle dependency
            );
    
}