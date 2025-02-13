Shader "Custom Post-Processing/Chromatic Aberration"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // Uniforms
            uniform sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float2 _focalOffset, _radius;
            float _hardness, _intensity, _redOffset, _greenOffset, _blueOffset;

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
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 pos = i.uv - 0.5f;
                pos -= _focalOffset;
                pos *= _radius;

                float2 d = pos;
                float intensity = saturate(pow(abs(length(pos)), _hardness)) * _intensity;

                float2 redUV = i.uv + (d * _redOffset * _MainTex_TexelSize.xy) * intensity;
                float2 blueUV = i.uv + (d * _greenOffset * _MainTex_TexelSize.xy) * intensity;
                float2 greenUV = i.uv + (d * _blueOffset * _MainTex_TexelSize.xy) * intensity;

                float4 final = 1.0f;
                final.r = tex2D(_MainTex, redUV).r;
                final.g = tex2D(_MainTex, blueUV).g;
                final.b = tex2D(_MainTex, greenUV).b;

                return col;
            }
            ENDHLSL
        }
    }
}
