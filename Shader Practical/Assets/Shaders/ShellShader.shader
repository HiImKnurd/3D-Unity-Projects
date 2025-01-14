Shader "Unlit/ShellShader"
{
    Properties{
            //_tint("Tint", Color) = (0,0,0,1)
            //_pixels("Pixels", int) = 100
            //_density("Density", int) = 100
            //_shellHeight("Shell Height", float) = 0.01
            _shellCount("Shell Count", int) = 0
            _shellIndex("Shell Index", int) = 0
            _shellLength("Shell Length", float) = 0
            _shellHeight("Shell Height", float) = 0
            _density("Density", int) = 0
            _thickness("Thickness", float) = 0
            _shellColor("Shell Color", Color) = (1,1,1,1)
    }

    SubShader{
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent"}
        Pass{
        Cull Off
            HLSLPROGRAM

            #include "UnityCG.cginc"

            #pragma vertex MyVertexShader
            #pragma fragment MyFragmentShader

            int _shellCount;
            int _shellIndex;
            float _shellLength;
            float _shellHeight;
            int _density;
            float _thickness;
            float4 _shellColor;


            struct vertexData {
                float4 position: POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };

            struct fragmentVertex {
                float2 uv : TEXCOORD0;
                float4 position : SV_POSITION;
                float3 normal : TEXCOORD1;
            };

            float hash(uint n) {
				// integer hashing function
				n = (n << 13U) ^ n;
				n = n * (n * n * 15731U + 0x789221U) + 0x1376312589U;
				return float(n & uint(0x7fffffffU)) / float(0x7fffffff);
			}

            fragmentVertex MyVertexShader(vertexData vd) {
                fragmentVertex fv;
                _shellHeight = (float)_shellIndex / (float)_shellCount;
                vd.position.xyz += vd.normal.xyz * _shellLength * _shellHeight;
                fv.normal = normalize(UnityObjectToWorldNormal(vd.normal));
                fv.position = UnityObjectToClipPos(vd.position);
                fv.uv = vd.uv;
                return fv;
            }

            float4 MyFragmentShader(fragmentVertex fv) : SV_TARGET{

                float2 newUV = fv.uv * _density;
				float2 localUV = frac(newUV) * 2 - 1;
				float localDistance = length(localUV);

                uint2 intUV = newUV;
				uint seed = (uint)newUV.x + 100 * (uint)newUV.y + 100 * 10;
                float h = (float)_shellIndex / (float)_shellCount;

                // checking that the pixel is within the thickness of the center, which narrows as it reaches the tip
                // creates round strands that taper off at the tip
				int outsideThickness = (localDistance) > (_thickness * (hash(seed) - h));
				if (outsideThickness && _shellIndex > 0) discard;
                
                // Darker near the base, brighter at the tip
                return _shellColor * h;
            }

            ENDHLSL
        }
    }
}

