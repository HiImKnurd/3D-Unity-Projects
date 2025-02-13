Shader "Custom Post-Processing/Retro"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _pixelSize("Pixel Size", Range(0,20)) = 0
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
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            // Uniforms
            uniform sampler2D _MainTex;
            float4 _MainTex_ST;
            uniform float _pixelSize;



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
                float2 UV = i.uv;
                UV.x *= _ScreenParams.x / _pixelSize;
                UV.y *= _ScreenParams.y / _pixelSize;
                UV.x = int(UV.x);
                UV.y = int(UV.y);
                UV.x /= _ScreenParams.x / _pixelSize;
                UV.y /= _ScreenParams.y / _pixelSize;

                float4 result = tex2D(_MainTex, UV); // Color sampler

                return result;
            }
            ENDHLSL
        }
    }
}
