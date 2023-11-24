Shader "PostProcessing/GaussianBlurFullScreen"
{
    Properties
    {
        [HideInInspector]
        _MainTex ("Texture", 2D) = "white" {}

        [HideInInspector]
        _Spread ("Spread", Float) = 0

        [HideInInspector]
        _GridSize ("Grid Size", Integer) = 0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        #define E 2.71828f

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_TexelSize;
            uint _Grid;
            float _Spread;
        CBUFFER_END

        struct appdata
        {
            float4 positionOS : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct interpolators
        {
            float2 uv : TEXCOORD0;
            float4 positionCS : SV_POSITION;
        };

        float gaussian(const int param)
        {
            const float sigma_sqr = _Spread * _Spread;
            return (1 / sqrt(TWO_PI * sigma_sqr) * pow(E, -(param * param) / (2 * sigma_sqr)));
        }

        interpolators vert(appdata v)
        {
            interpolators o;
            o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
            o.uv = v.uv;
            return o;
        }

        float4 frag_horizontal(const interpolators input) : SV_Target
        {
            float3 col = 0;
            float grid_sum = 0.0f;

            // how many pixels are there left / right of this fragment
            const int upper = (_Grid - 1) / 2;
            const int lower = -upper;

            for (int x = lower; x <= upper; ++x)
            {
                const float g = gaussian(x);
                const float2 uv_offset_x = float2(input.uv.x + _MainTex_TexelSize.x * x, input.uv.y);
                grid_sum += g;
                col += g * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv_offset_x).xyz;
            }
            col /= grid_sum;
            return float4(col, 1);
        }

        float4 frag_vertical(const interpolators input) : SV_Target
        {
            float3 col = 0;
            float grid_sum = 0.0f;

            // how many pixels are there up / down of this fragment
            const int upper = (_Grid - 1) / 2;
            const int lower = -upper;

            for (int y = lower; y <= upper; ++y)
            {
                const float g = gaussian(y);
                const float2 uv_offset_y = float2(input.uv.x, input.uv.y + _MainTex_TexelSize.y * y);
                grid_sum += g;
                col += g * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv_offset_y).xyz;
            }
            col /= grid_sum;
            return float4(col, 1);
        }
        ENDHLSL

        Pass
        {
            Name "Horizontal"

            HLSLPROGRAM
            #pragma vertex vert;
            #pragma fragment frag_horizontal;
            ENDHLSL
        }

        Pass
        {
            Name "Vertical"

            HLSLPROGRAM
            #pragma vertex vert;
            #pragma fragment frag_vertical;
            ENDHLSL
        }
    }
}