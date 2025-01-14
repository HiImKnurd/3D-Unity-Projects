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
           _spotlightInnerCutoff ("Spotlight inner Cutoff", Range(0,360)) = 10.0
           _quantizationCount ("Quantization Count", Integer) = 15

    }
        SubShader
    {
        Tags {"Queue" = "Geometry" "RenderType" = "Opaque"}
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            Tags {"LightMode" = "ForwardBase"}
            HLSLPROGRAM

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

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
                LIGHTING_COORDS(1,2)
                float3 worldPosition : POSITION1;
                float4 shadowCoord : POSITION2;  
            };

            uniform float4 _tint;
            uniform sampler _mainTexture;
            uniform float4 _mainTexture_ST;
            uniform float _alphaCutoff;

            uniform float3 _lightPosition1;
            uniform float4 _lightDirection1;
            uniform float4 _lightColor1;
            uniform float _specularStrength1;
            uniform float _smoothness1;

            uniform int _lightType1;
            uniform float _lightIntensity1;
            uniform float3 _attenuation1;
            uniform float _spotlightCutoff1;
            uniform float _spotlightInnerCutoff1;

            uniform int _quantizationCount;

            // Shadow Mapping
            uniform sampler2D _shadowMap;
            uniform float4x4 _lightViewProj;
            uniform float _shadowBias;

            float ShadowCalculation(float4 fragPosLightSpace)
            {

                float3 shadowCoord = fragPosLightSpace.xyz / fragPosLightSpace.w;
                shadowCoord = shadowCoord * 0.5 + 0.5;

                //Sample Shadow Map
                float shadowDepth = 1.0 - tex2D(_shadowMap, shadowCoord.xy).r;
                float shadowFactor = (shadowCoord.z - _shadowBias > shadowDepth) ? 1.0 : 0.0;
                shadowFactor = saturate(1.0 - shadowFactor);

                return shadowFactor;

            }

            vertex2Fragment MyVertexShader(vertexData vd) {
                vertex2Fragment v2f;
                v2f.position = UnityObjectToClipPos(vd.position);
                v2f.worldPosition = mul(unity_ObjectToWorld, vd.position);
                v2f.uv = TRANSFORM_TEX(vd.uv, _mainTexture);
                v2f.normal = normalize(UnityObjectToWorldNormal(vd.normal));

                TRANSFER_VERTEX_TO_FRAGMENT(v2f);

                v2f.shadowCoord = mul(_lightViewProj, float4(v2f.worldPosition, 1.0));

                return v2f;
            }

            float4 MyFragmentShader(vertex2Fragment v2f) : SV_TARGET{
                float attenuation = 1.0f;
               // Directional light
                float3 finalLightDirection;
                if (_lightType1 == 0) finalLightDirection = _lightDirection1;
                // Point and Spot light
                else {
                    finalLightDirection = normalize(v2f.worldPosition - _lightPosition1);
                    float distance = length(v2f.worldPosition - _lightPosition1);
                    attenuation = 1.0 / (_attenuation1.x + _attenuation1.y * distance + _attenuation1.z * distance * distance);

                    if(_lightType1 == 2) // Spotlight
                    {
                        float theta = dot(finalLightDirection, _lightDirection1);
                        float angle = cos(radians(_spotlightCutoff1));
                        if (theta > angle) { 
                            float epsilon = cos(radians(_spotlightInnerCutoff1)) - angle;
                            float intensity = clamp((theta - angle)/epsilon, 0.0, 1.0);
                            attenuation *= intensity;
                        }
                        else attenuation = 0.0;
                    }
                }

                v2f.normal = normalize(v2f.normal);
                float4 albedo = tex2D(_mainTexture, v2f.uv) * _tint;
                //return tex2D(_mainTexture, v2f.uv).xyzw ;

                // Add alpha Cutoff here

                float shadowFactor = ShadowCalculation(v2f.shadowCoord);

                float3 viewDirection = normalize(_WorldSpaceCameraPos - v2f.worldPosition);
                float3 halfVector = normalize((viewDirection - _lightDirection1));
                float specular = pow(float(saturate(dot(v2f.normal, halfVector))), _smoothness1 * 100);
                float3 specularColor = specular * _specularStrength1 * _lightColor1.rgb;
                float3 diffuse = albedo.rgb * _lightColor1.rgb * saturate(dot(v2f.normal, -finalLightDirection));
                float3 final = (diffuse + specularColor) * _lightIntensity1 * attenuation * shadowFactor;

                float4 output = float4(final, 0);
                output.r = floor((_quantizationCount - 1.0) * output.r + 0.5) / (_quantizationCount - 1.0f);
                output.g = floor((_quantizationCount - 1.0) * output.g + 0.5) / (_quantizationCount - 1.0f);
                output.b = floor((_quantizationCount - 1.0) * output.b + 1.0) / (_quantizationCount - 1.0f);
                output.a = albedo.a;
                return output;
            }

            ENDHLSL
        }

        Pass
        {
            Tags {"LightMode" = "ForwardAdd"}   
            HLSLPROGRAM

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

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
                LIGHTING_COORDS(1,2)
                float3 worldPosition : POSITION1;
                float4 shadowCoord : POSITION2;  
            };

            uniform float4 _tint;
            uniform sampler _mainTexture;
            uniform float4 _mainTexture_ST;
            uniform float _alphaCutoff;

            uniform float3 _lightPosition1;
            uniform float4 _lightDirection1;
            uniform float4 _lightColor1;
            uniform float _specularStrength1;
            uniform float _smoothness1;

            uniform int _lightType1;
            uniform float _lightIntensity1;
            uniform float3 _attenuation1;
            uniform float _spotlightCutoff1;
            uniform float _spotlightInnerCutoff1;

            uniform int _quantizationCount;

            // Shadow Mapping
            uniform sampler2D _shadowMap;
            uniform float4x4 _lightViewProj;
            uniform float _shadowBias;

            float ShadowCalculation(float4 fragPosLightSpace)
            {

                float3 shadowCoord = fragPosLightSpace.xyz / fragPosLightSpace.w;
                shadowCoord = shadowCoord * 0.5 + 0.5;

                //Sample Shadow Map
                float shadowDepth = 1.0 - tex2D(_shadowMap, shadowCoord.xy).r;
                float shadowFactor = (shadowCoord.z - _shadowBias > shadowDepth) ? 1.0 : 0.0;
                shadowFactor = saturate(1.0 - shadowFactor);

                return shadowFactor;

            }

            vertex2Fragment MyVertexShader(vertexData vd) {
                vertex2Fragment v2f;
                v2f.position = UnityObjectToClipPos(vd.position);
                v2f.worldPosition = mul(unity_ObjectToWorld, vd.position);
                v2f.uv = TRANSFORM_TEX(vd.uv, _mainTexture);
                v2f.normal = normalize(UnityObjectToWorldNormal(vd.normal));

                TRANSFER_VERTEX_TO_FRAGMENT(v2f);

                v2f.shadowCoord = mul(_lightViewProj, float4(v2f.worldPosition, 1.0));

                return v2f;
            }

            float4 MyFragmentShader(vertex2Fragment v2f) : SV_TARGET{
                float attenuation = 1.0f;
               // Directional light
                float3 finalLightDirection;
                if (_lightType1 == 0) finalLightDirection = _lightDirection1;
                // Point and Spot light
                else {
                    finalLightDirection = normalize(v2f.worldPosition - _lightPosition1);
                    float distance = length(v2f.worldPosition - _lightPosition1);
                    attenuation = 1.0 / (_attenuation1.x + _attenuation1.y * distance + _attenuation1.z * distance * distance);

                    if(_lightType1 == 2) // Spotlight
                    {
                        float theta = dot(finalLightDirection, _lightDirection1);
                        float angle = cos(radians(_spotlightCutoff1));
                        if (theta > angle) { 
                            float epsilon = cos(radians(_spotlightInnerCutoff1)) - angle;
                            float intensity = clamp((theta - angle)/epsilon, 0.0, 1.0);
                            attenuation *= intensity;
                        }
                        else attenuation = 0.0;
                    }
                }

                v2f.normal = normalize(v2f.normal);
                float4 albedo = tex2D(_mainTexture, v2f.uv) * _tint;
                //return tex2D(_mainTexture, v2f.uv).xyzw ;

                // Add alpha Cutoff here

                float shadowFactor = ShadowCalculation(v2f.shadowCoord);

                float3 viewDirection = normalize(_WorldSpaceCameraPos - v2f.worldPosition);
                float3 halfVector = normalize((viewDirection - _lightDirection1));
                float specular = pow(float(saturate(dot(v2f.normal, halfVector))), _smoothness1 * 100);
                float3 specularColor = specular * _specularStrength1 * _lightColor1.rgb;
                float3 diffuse = albedo.rgb * _lightColor1.rgb * saturate(dot(v2f.normal, -finalLightDirection));
                float3 final = (diffuse + specularColor) * _lightIntensity1 * attenuation * shadowFactor;

                float4 output = float4(final, 0);
                output.r = floor((_quantizationCount - 1.0) * output.r + 0.5) / (_quantizationCount - 1.0f);
                output.g = floor((_quantizationCount - 1.0) * output.g + 0.5) / (_quantizationCount - 1.0f);
                output.b = floor((_quantizationCount - 1.0) * output.b + 1.0) / (_quantizationCount - 1.0f);
                output.a = albedo.a;
                return output;
            }

            ENDHLSL
        }
    }
}