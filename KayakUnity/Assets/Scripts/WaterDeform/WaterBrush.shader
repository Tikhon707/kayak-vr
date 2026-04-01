Shader "Hidden/WaterBrush"
{
    SubShader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            Blend One One // Additive blending

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float2 _Center;
            float _Radius;
            float _Intensity;

            struct Attributes
            {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            Varyings vert(Attributes input)
            {
                Varyings o;
                o.pos = TransformObjectToHClip(input.position.xyz);
                o.uv = input.uv;
                return o;
            }

            float4 frag(Varyings i) : SV_Target
            {
                float d = distance(i.uv, _Center);
                float h = d < _Radius ? _Intensity * (1.0 - smoothstep(0, _Radius, d)) : 0;
                return h;
            }
            ENDHLSL
        }
    }
}