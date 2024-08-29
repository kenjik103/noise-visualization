using Unity.Mathematics;

using static Unity.Mathematics.math;

public static partial class Noise
{
    public struct Lattice1D : INoise
    {
        public float4 GetNoise4(float4x3 postions, SmallXXHash4 hash) {
            int4 p0 = (int4)floor(postions.c0);
            int4 p1 = p0 + 1;
            float4 t = postions.c0 - p0;
            
            return lerp(hash.Eat(p0).Floats01A, hash.Eat(p1).Floats01A, t) * 2f - 1f;
        }
    }
    
}