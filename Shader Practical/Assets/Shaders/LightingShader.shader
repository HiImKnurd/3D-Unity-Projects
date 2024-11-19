Shader "Unlit/LightingShader"
{
    Properties{
           _tint("Tint", Color) = (0,0,0,1)
           _mainTexture ("Albedo", 2D) = "white" {}
           _alphaCutoff ("Alpha Cutoff", Range(0,1)) = 0.5

           _lightPosition ("Light Position", Vector) = (0,0,0)
           _lightDirection ("Light Direction", Vector) = (0,-1,0)
           _lightColor ("Light Color", Color) = (1,1,1,1)
           _smoothness ("smoothness", Range(0,1)) = 0.5
           _specularStrength ("Specular Strength", Range(0,1)) = 0.5

           _lightType ("Light Type", Integer) = 1
           _lightIntensity ("Light Intensity", float) = 1
           _attenuation ("attenuation", Vector) = (1.0,0.09,0.032)
           _spotlightCutoff ("Spotlight Cutoff", Range(0,360)) = 20.0
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
                float3 normal : NORMAL;
            };

            struct vertex2Fragment {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float3 worldPosition : POSITION1;
            };

            uniform float4 _tint;
            uniform sampler _mainTexture;
            uniform float4 _mainTexture_ST;
            uniform float _alphaCutoff;

            uniform float3 _lightPosition;
            uniform float4 _lightDirection;
            uniform float4 _lightColor;
            uniform float _specularStrength;
            uniform float _smoothness;

            uniform int _lightType;
            uniform float _lightIntensity;
            uniform float3 _attenuation;
            uniform float _spotlightCutoff;

            vertex2Fragment MyVertexShader(vertexData vd) {
                vertex2Fragment v2f;
                v2f.position = UnityObjectToClipPos(vd.position);
                v2f.worldPosition = mul(unity_ObjectToWorld, vd.position);
                v2f.uv = TRANSFORM_TEX(vd.uv, _mainTexture);
                v2f.normal = normalize(UnityObjectToWorldNormal(vd.normal));
                return v2f;
            }

            float4 MyFragmentShader(vertex2Fragment v2f) : SV_TARGET{
                // Directional light
                float3 finalLightDirection;
                if (_lightType == 0) finalLightDirection = _lightDirection;
                // Point and Spot light
                else {
                    finalLightDirection = normalize(v2f.worldPosition - _lightPosition);
                    float distance = length(v2f.worldPosition - _lightPosition);
                    _attenuation = 1.0 / (_attenuation.x + _attenuation.y * distance + _attenuation.z * distance * distance);

                    if(_lightType == 2) // Spotlight
                    {
                        float theta = dot(finalLightDirection, _lightDirection);
                        if (theta > cos(radians(_spotlightCutoff))) {  }
                        else _attenuation = 0.0;
                    }
                }

                v2f.normal = normalize(v2f.normal);
                float4 albedo = tex2D(_mainTexture, v2f.uv) * _tint;
                float3 viewDirection = normalize(_WorldSpaceCameraPos - v2f.worldPosition);
                float3 halfVector = normalize((viewDirection - _lightDirection));
                float specular = pow(float(saturate(dot(v2f.normal, halfVector))), _smoothness * 100);
                float3 specularColor = specular * _specularStrength * _lightColor.rgb;
                float3 diffuse = albedo.rgb * _lightColor.rgb * saturate(dot(v2f.normal, -finalLightDirection));
                float3 final = (diffuse + specularColor) * _lightIntensity * _attenuation;

                float4 output = float4(final, albedo.a);
                int _colorCount = 15;
                output.r = floor((_colorCount - 1.0) * output.r + 0.5) / (_colorCount - 1.0f);
                output.g = floor((_colorCount - 1.0) * output.g + 0.5) / (_colorCount - 1.0f);
                output.b = floor((_colorCount - 1.0) * output.b + 0.5) / (_colorCount - 1.0f);
                return output;
            }


            ENDHLSL
        }
    }
}