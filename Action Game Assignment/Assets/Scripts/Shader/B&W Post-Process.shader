Shader "Custom Post-Processing/B&W Post-Processing"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _blend("B & W blend", Range(0,1)) = 0 // Blend intensity
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
            uniform float _blend;

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
                float4 c = tex2D(_MainTex, i.uv); // Color sampler

                // Calculate luminance
                float lum = c.r * .3 + c.g * .59 + c.b * .11;
                // Create a black and white color
                float3 bwc = float3(lum,lum,lum);

                float4 result = c;
                // lerp between original color and BW based on blend intensity
                result.rgb = lerp(c.rgb, bwc, _blend);
                return result;
            }
            ENDHLSL
        }
    }
}
