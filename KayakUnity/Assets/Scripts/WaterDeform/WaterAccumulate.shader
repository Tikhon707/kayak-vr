Shader "Hidden/WaterAccumulate"
{
    SubShader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            Blend Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            float _Decay;

            float4 vert(float4 pos : POSITION) : SV_POSITION { return pos; }

            float4 frag(float2 uv : TEXCOORD0) : SV_Target
            {
                float h = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).r;
                return h * _Decay;
            }
            ENDHLSL
        }
    }
}