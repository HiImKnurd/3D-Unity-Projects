Shader "Unlit/AdditiveBlend"
{
    Properties{
           _tint("Tint", Color) = (0,0,0,1)
           _mainTexture ("Texture", 2D) = "white" {}
           _secondTexture ("Texture", 2D) = "white" {}

           [Enum(UnityEngine.Rendering.BlendMode)]
           _srcFactor("source factor", float) = 5
           [Enum(UnityEngine.Rendering.BlendMode)]
           _dstFactor("destination Factor", float) = 10
           [Enum(UnityEngine.Rendering.BlendOp)]
           _Opp("Operation", float) = 0
    }
        SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Opaque"}
        Blend [_srcFactor] [_dstFactor]
        BlendOp [_Opp]
        Pass
        {
            HLSLPROGRAM

            #include "UnityCG.cginc"

            #pragma vertex MyVertexShader
            #pragma fragment MyFragmentShader

            struct vertexData {
                float4 position: POSITION;
                float2 uv : TEXCOORD0;
            };

            struct vertex2Fragment {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            uniform float4 _tint;
            uniform sampler _mainTexture;
            uniform sampler _secondTexture;
            uniform float4 _mainTexture_ST;

            vertex2Fragment MyVertexShader(vertexData vd) {
                vertex2Fragment v2f;
                v2f.position = UnityObjectToClipPos(vd.position);
                v2f.uv = TRANSFORM_TEX(vd.uv, _mainTexture);
                return v2f;
            }

            float4 MyFragmentShader(vertex2Fragment v2f) : SV_TARGET{
                float4 result = tex2D(_mainTexture, v2f.uv) * 1;
                result += tex2D(_secondTexture, v2f.uv) * 0.5;

                return result;
            }


            ENDHLSL
        }
    }
}
