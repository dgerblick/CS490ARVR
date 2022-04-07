Shader "Hidden/Blend" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }
    SubShader {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass {
            CGPROGRAM

            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;
            uniform sampler2D _CameraDepthTexture;

            half4 frag(v2f_img i) : SV_Target {
                half4 color = tex2D(_MainTex, i.uv);
                float depth = LinearEyeDepth(tex2D(_CameraDepthTexture, i.uv).r);
                // half3 c = DecodeHDR(color, _MainTex_HDR);
                return color;
            }

            ENDCG
        }
    }
}
