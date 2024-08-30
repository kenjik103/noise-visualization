Shader "Custom/Noise Shader"
{
    SubShader
    {
        CGPROGRAM
        #pragma surface ConfigureSurface Standard fullforwardshadows
        #pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural
        #pragma editor_sync_compilation
        
        #pragma target 4.5

        #include "NoiseGPU.hlsl"
        
        struct Input
        {
            float2 world_pos;
        };

        void ConfigureSurface(Input input, inout SurfaceOutputStandard surface){
            surface.Albedo = GetNoiseColor();
        }

        ENDCG
    }
    FallBack "Diffuse"
}