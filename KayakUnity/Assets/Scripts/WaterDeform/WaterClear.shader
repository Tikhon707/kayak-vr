Shader "Hidden/WaterClear"
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
            float4 vert(float4 pos : POSITION) : SV_POSITION { return pos; }
            float4 frag() : SV_Target { return 0; }
            ENDHLSL
        }
    }
}