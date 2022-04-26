// UNITY_SHADER_NO_UPGRADE

Shader "Custom/MyCubeMap" {
    Properties {
        _CubemapSW ("Southwest Cubemap", CUBE) = "grey" { }
        _CubemapNW ("Northwest Cubemap", CUBE) = "grey" { }
        _CubemapSE ("Southeast Cubemap", CUBE) = "grey" { }
        _CubemapNE ("Northeast Cubemap", CUBE) = "grey" { }
        _PosX ("X Position", Range(0.0, 1.0)) = 0.0
        _PosY ("Y Position", Range(0.0, 1.0)) = 0.0
        [Toggle] _ShowWireframe ("Show Wireframe", Int) = 0
        [Toggle] _ShowCubemap ("Show Cubemap", Int) = 1
    }
    CGINCLUDE

    #include "UnityCG.cginc"

    samplerCUBE _CubemapSW;
    samplerCUBE _CubemapNW;
    samplerCUBE _CubemapSE;
    samplerCUBE _CubemapNE;
    float _PosX;
    float _PosY;
    int _ShowWireframe;
    int _ShowCubemap;

    struct v2f {
        float4 position : POSITION;
        float3 texcoord : TEXCOORD0;
    };

    struct v2g {
        float4 position : POSITION;
        float3 texcoord : TEXCOORD0;
    };

    struct g2f {
        float4 position : POSITION;
        float3 texcoord : TEXCOORD0;
        float3 barycentric : TEXCOORD1;
        float3 t0 : TEXCOORD2;
        float3 t1 : TEXCOORD3;
        float3 t2 : TEXCOORD4;
    };


    v2g vert(appdata_base v) {
        v2g o;
        o.position = mul(UNITY_MATRIX_MVP, float4(v.vertex.xyz, 0.0));
        o.position.z = 0.0;
        o.texcoord = v.vertex.xyz;
        return o;
    }

    [maxvertexcount(3)]
    void geom(triangle v2g i[3], inout TriangleStream<g2f> triangleStream) {
        g2f o0;
        o0.position = i[0].position;
        o0.texcoord = i[0].texcoord;
        o0.barycentric = float3(1, 0, 0);
        o0.t0 = i[0].position;
        o0.t1 = i[1].position;
        o0.t2 = i[2].position;
        triangleStream.Append(o0);

        g2f o1;
        o1.position = i[1].position;
        o1.texcoord = i[1].texcoord;
        o1.barycentric = float3(0, 1, 0);
        o1.t0 = i[0].position;
        o1.t1 = i[1].position;
        o1.t2 = i[2].position;
        triangleStream.Append(o1);

        g2f o2;
        o2.position = i[2].position;
        o2.texcoord = i[2].texcoord;
        o2.barycentric = float3(0, 0, 1);
        o2.t0 = i[0].position;
        o2.t1 = i[1].position;
        o2.t2 = i[2].position;
        triangleStream.Append(o2);
    }

    half4 frag(g2f i) : COLOR {
        float4 nw = texCUBE(_CubemapNW, i.texcoord);
        float4 sw = texCUBE(_CubemapSW, i.texcoord);
        float4 ne = texCUBE(_CubemapNE, i.texcoord);
        float4 se = texCUBE(_CubemapSE, i.texcoord);

        // Add Texture
        float4 c = float4(1.0, 1.0, 1.0, 1.0);
        if (_ShowCubemap == 1) {
            c = lerp(lerp(nw, sw, _PosY), lerp(ne, se, _PosY), _PosX);
        }

        // Show wireframe
        float wireThickness = 1.0;
        if (_ShowWireframe == 1) {
            float3 b = 1.0 - i.barycentric / (wireThickness *  fwidth(i.barycentric));
            float m = clamp(max(max(b.x, b.y), b.z), 0, 1);
            c -= float4(m, m, m, 0);
        }


        return c;
    }
    
    ENDCG

    SubShader { 
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Pass {
            ZWrite Off
            Cull Off
            Fog { Mode Off }
            
            CGPROGRAM 
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            ENDCG
        }
    }
    FallBack "Skybox/Cubemap"
}
