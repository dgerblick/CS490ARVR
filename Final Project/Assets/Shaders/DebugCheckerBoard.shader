Shader "Custom/DebugCheckerBoard" {
    Properties {
        _ColorA ("Color A", Color) = (0, 0, 0, 1)
        _ColorB ("Color B", Color) = (1, 1, 1, 1)
        _Scale("Scale", Float) = 1.0
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        struct Input {
            float3 worldPos;
        };

        fixed4 _ColorA;
        fixed4 _ColorB;
        float _Scale;
        half _Glossiness;
        half _Metallic;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
        // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf(Input IN, inout SurfaceOutputStandard o) {
            fixed4 c;
            float3 checkerboard = 0.5f * floor(IN.worldPos / _Scale + 0.5f);

            if (frac(checkerboard.x + checkerboard.y + checkerboard.z) == 0.0f) {
                c = _ColorA;
            } else {
                c = _ColorB;
            }

            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
