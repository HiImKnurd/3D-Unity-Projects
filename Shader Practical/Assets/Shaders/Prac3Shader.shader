Shader "Unlit/Prac3Shader"
{
    Properties{
           _tint("Tint", Color) = (0,0,0,1)
           _pixels("Pixels", int) = 100
           _mainTexture ("Texture", 2D) = "white" {}
           _secondTexture ("Texture", 2D) = "white" {}
           _thirdTexture ("Texture", 2D) = "white" {}
           _alphaCutoff ("Alpha Cutoff", Range(0,1)) = 0.5
           _subdivision ("Subdivisions", int) = 1
    }
        SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
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
            uniform sampler _thirdTexture;
            uniform float4 _mainTexture_ST;
            uniform float _alphaCutoff;
            uniform int _subdivision;

            vertex2Fragment MyVertexShader(vertexData vd) {
                vertex2Fragment v2f;
                v2f.position = UnityObjectToClipPos(vd.position);
                v2f.uv = TRANSFORM_TEX(vd.uv, _mainTexture);
                return v2f;
            }

            float4 MyFragmentShader(vertex2Fragment v2f) : SV_TARGET{
                float4 result = _tint * tex2D(_mainTexture, v2f.uv) * tex2D(_mainTexture, v2f.uv * _subdivision * _subdivision);
                float4 final;
                if(result.r >= 0.1f) 
                    final = _tint * tex2D(_secondTexture, v2f.uv); 
                else if (result.b >= 0.1f) 
                    final = _tint * tex2D(_thirdTexture, v2f.uv);
                else 
                    final = float4(0,0,0,0);
                clip(final.a - _alphaCutoff);
                return final;
            }


            ENDHLSL
        }
    }
}
