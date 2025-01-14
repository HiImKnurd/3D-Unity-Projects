Shader "Unlit/FirstShader"
{
    Properties{
            _tint("Tint", Color) = (0,0,0,1)
            _pixels("Pixels", int) = 100
    }

    SubShader{
        Pass{
            HLSLPROGRAM

            #include "UnityCG.cginc"

            #pragma vertex MyVertexShader
            #pragma fragment MyFragmentShader

            float4 _tint;
            int _pixels;

            struct fragmentVertex {
                float2 uv : TEXCOORD0;
                float4 position : SV_POSITION;
            };

            fragmentVertex MyVertexShader(float4 position : POSITION, float2 uv : TEXCOORD0) {
                fragmentVertex fv;
                fv.position = UnityObjectToClipPos(position);
                fv.uv = uv;
                return fv;
            }

            float4 MyFragmentShader(fragmentVertex fv) : SV_TARGET{
                if (floor(fv.uv.x * _pixels) % 2 != 0) return _tint * float4(0.f,0.f,0.f,1.f);
                if (floor(fv.uv.y * _pixels) % 2 != 0) return _tint * float4(0.f,0.f,0.f,1.f);
                else return _tint * float4(fv.uv,0.f,1.f);
            }

            ENDHLSL
        }
    }
}
