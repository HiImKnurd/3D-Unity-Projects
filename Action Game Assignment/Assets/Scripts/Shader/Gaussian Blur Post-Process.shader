Shader "Custom Post-Processing/Gaussian Blur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Spread ("Standard Deviation", Float) = 0
        _GridSize("Grid Size", Integer) = 1
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalRenderPipeline"}
        LOD 100

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #define E 2.71828f

        // Declare Properties for Blur
        sampler2D _MainTex;
        float4 _MainTex_TexelSize;
        uint _GridSize;
        float _Spread;

        //Properties for Difference of Gaussians
        float _threshhold, _tau, _phi;
        sampler2D _SecondTex;
        int _invert, _hyperbolic;

        // Gaussian function
        float gaussian(int x)
        {
            float sigmaSqu = _Spread * _Spread;
            return (1.0 / sqrt(TWO_PI * sigmaSqu)) * pow(E, -(x * x) / (2 * sigmaSqu));
        }

        struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

        v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }
        ENDHLSL

        Pass
        {
            Name "Horizontal"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag_horizontal

            // Horizontal Blur
            float4 frag_horizontal(v2f i) : SV_Target
            {
                float3 col = float3(0.f,0.f,0.f);
                float gridSum = 0.f;

                int upper = ((_GridSize - 1) / 2);
                int lower = -upper;

                for(int x = lower; x <= upper; x++)
                {
                    float gauss = gaussian(x);
                    gridSum += gauss;
                    float2 uv = i.uv + float2(_MainTex_TexelSize.x * x, 0.f);
                    col += gauss * tex2D(_MainTex,uv).xyz;
                }

                col /= gridSum;
                return float4(col,1.f);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Vertical"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag_vertical

            // Vertical Blur
            float4 frag_vertical(v2f i) : SV_Target
            {
                float3 row = float3(0.f,0.f,0.f);
                float gridSum = 0.f;

                int upper = ((_GridSize - 1) / 2);
                int lower = -upper;

                for(int y = lower; y <= upper; y++)
                {
                    float gauss = gaussian(y);
                    gridSum += gauss;
                    float2 uv = i.uv + float2(0.f,_MainTex_TexelSize.y * y);
                    row += gauss * tex2D(_MainTex,uv).xyz;
                }

                row /= gridSum;
                return float4(row,1.f);
            }
            ENDHLSL
        }

        Pass{
            Name "Difference of Gaussians"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4 frag(v2f i) : SV_Target 
            {
                float2 G = tex2D(_SecondTex, i.uv).rg;

                float4 diff = ((1+_tau) * G.r) - (_tau * G.g);

                if (_hyperbolic) diff = diff >= _threshhold ? 1 : 1 + tanh(_phi * (diff - _threshhold));
                else diff = diff >= _threshhold ? 1 : 0;

                if(_invert) diff = 1 - diff;

                return diff;
            }

            ENDHLSL
        }
    }
}
