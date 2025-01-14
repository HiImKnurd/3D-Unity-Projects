Shader "Unlit/MultiLightShader"
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

           _quantizationCount ("Quantization Count", Integer) = 15

    }
        SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
        Cull Off
            HLSLPROGRAM

            #include "UnityCG.cginc"

            #pragma vertex MyVertexShader
            #pragma fragment MyFragmentShader

            struct vertexData {
                float4 position: POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };

            struct vertex2Fragment {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float3 tangent : TEXCOORD1;
                float3 bitangent : TEXCOORD2;
                float3 worldPosition : POSITION1;
                float4 shadowCoord : POSITION2;
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
            uniform float _spotlightInnerCutoff;

            uniform int _quantizationCount;

            uniform int _lightCount;
            uniform int _vectorCount;
            uniform int _floatCount;
            uniform float4 _lightVectorData[32]; // Max light count = 8
            uniform float _lightFloatData[56];

            // Shadow Mapping
            uniform sampler2D _shadowMap;
            uniform float4x4 _lightViewProj;
            uniform float _shadowBias;

            float ShadowCalculation(float4 fragPosLightSpace)
            {
                //vec2 poissonDisk[4] = vec2[](
                //    vec2( -0.94201624, -0.39906216 ),
                //    vec2( 0.94558609, -0.76890725 ),
                //    vec2( -0.094184101, -0.92938870 ),
                //    vec2( 0.34495938, 0.29387760 )
                //    );

                float3 shadowCoord = fragPosLightSpace.xyz / fragPosLightSpace.w;
                shadowCoord = shadowCoord * 0.5 + 0.5;
                //if(shadowCoord.z > 1) return 0.f;
                float shadowFactor = 0.0;

                //Sample Shadow Map
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        float2 offset = float2(x, y) * 2.0f; 
                        float shadowDepth = 1.0 - tex2D(_shadowMap, shadowCoord.xy + offset);
                        shadowFactor += saturate(1.0 - (shadowCoord.z - _shadowBias > shadowDepth) ? 1.0 : 0.0);
                        //float shadowDepth = tex2D(_shadowMap, shadowCoord.xy + offset);
                        //shadowFactor += (shadowCoord.z - _shadowBias > shadowDepth) ? 1.0 : 0.0;
                    }
                }

                return shadowFactor / 9.0;

            }

            void GetLightData(int index){
                int VectorIndex = index * 4;
                int FloatIndex = index * 7;
                
                _lightPosition = _lightVectorData[index + VectorIndex];
                _lightDirection = _lightVectorData[index + 1 + VectorIndex];
                _attenuation = _lightVectorData[index + 2 + VectorIndex];
                _lightColor = _lightVectorData[index + 3 + VectorIndex];

                _lightType = _lightFloatData[index + FloatIndex];
                _smoothness = _lightFloatData[index + 1 + FloatIndex];
                _specularStrength = _lightFloatData[index + 2 + FloatIndex];
                _lightIntensity = _lightFloatData[index + 3 + FloatIndex];
                _spotlightCutoff = _lightFloatData[index + 4 + FloatIndex];
                _spotlightInnerCutoff = _lightFloatData[index + 5 + FloatIndex];
                _quantizationCount = _lightFloatData[index + 6 + FloatIndex];
            }

            vertex2Fragment MyVertexShader(vertexData vd) {
                vertex2Fragment v2f;
                v2f.position = UnityObjectToClipPos(vd.position);
                v2f.worldPosition = mul(unity_ObjectToWorld, vd.position);
                v2f.uv = TRANSFORM_TEX(vd.uv, _mainTexture);
                v2f.normal = normalize(UnityObjectToWorldNormal(vd.normal));
                v2f.tangent = normalize(UnityObjectToWorldDir(vd.tangent.xyz));
                v2f.bitangent = cross(v2f.normal, v2f.tangent) * (vd.tangent.w * unity_WorldTransformParams.w);

                v2f.shadowCoord = mul(_lightViewProj, float4(v2f.worldPosition, 1.0));

                return v2f;
            }

            float4 MyFragmentShader(vertex2Fragment v2f) : SV_TARGET{
            float4 output = float4(0,0,0, 0);
            float shadowFactor = 0; 
            for(int i = 0;i < _lightCount; i++){
                GetLightData(i);

                float attenuation = 1.0f;
                //_lightPosition.x += i * 3;
                //if(i > 0) _lightDirection.x = -_lightDirection.x;
               // Directional light
                float3 finalLightDirection;
                if (_lightType == 0) finalLightDirection = _lightDirection;
                // Point and Spot light
                else {
                    finalLightDirection = normalize(v2f.worldPosition - (_lightPosition));
                    float distance = length(v2f.worldPosition - _lightPosition);
                    attenuation = 1.0 / (_attenuation.x + _attenuation.y * distance + _attenuation.z * distance * distance);

                    if(_lightType == 2) // Spotlight
                    {
                        float theta = dot(finalLightDirection, _lightDirection);
                        float angle = cos(radians(_spotlightCutoff));
                        if (theta > angle) { 
                            float epsilon = cos(radians(_spotlightInnerCutoff)) - angle;
                            float intensity = clamp((theta - angle)/epsilon, 0.0, 1.0);
                            attenuation *= intensity;
                        }
                        else attenuation = 0.0;
                    }
                }

                v2f.normal = normalize(v2f.normal);
                float4 albedo = tex2D(_mainTexture, v2f.uv) * _tint;
                //return tex2D(_mainTexture, v2f.uv).xyzw ;


                if(i == 0) {
                shadowFactor = ShadowCalculation(v2f.shadowCoord);
                }
                else shadowFactor = 1.0f;

                float3 viewDirection = normalize(_WorldSpaceCameraPos - v2f.worldPosition);
                float3 halfVector = normalize((viewDirection - _lightDirection));
                float specular = pow(float(saturate(dot(v2f.normal, halfVector))), _smoothness * 100);
                float3 specularColor = specular * _specularStrength * _lightColor.rgb;
                float3 diffuse = albedo.rgb * _lightColor.rgb * saturate(dot(v2f.normal, -finalLightDirection));
                float3 final = (diffuse + specularColor) * _lightIntensity * attenuation * (shadowFactor / _lightCount);

                //if(shadowed) output += float4(final, 0) * 0.2f;
                output += float4(final, 0) * shadowFactor;
                if(shadowFactor > 0.5f){
                output.r = floor((_quantizationCount - 1.0) * output.r + 0.5) / (_quantizationCount - 1.0f);
                output.g = floor((_quantizationCount - 1.0) * output.g + 0.5) / (_quantizationCount - 1.0f);
                output.b = floor((_quantizationCount - 1.0) * output.b + 0.5) / (_quantizationCount - 1.0f);
                }
                output.a = albedo.a;
                }
                return output;
            }

            ENDHLSL
        }
    }
}