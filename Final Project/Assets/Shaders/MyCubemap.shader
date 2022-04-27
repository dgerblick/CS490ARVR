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
        [Toggle] _Morph ("Morph", Int) = 1
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
    int _Morph;

    struct appdata_in {
        float4 position : POSITION;
        float3 posSW : TEXCOORD0;
        float3 posNW : TEXCOORD1;
        float3 posSE : TEXCOORD2;
        float3 posNE : TEXCOORD3;
    };

    struct v2g {
        float4 position : POSITION;
        float3 posSW : TEXCOORD0;
        float3 posNW : TEXCOORD1;
        float3 posSE : TEXCOORD2;
        float3 posNE : TEXCOORD3;
    };

    struct g2f {
        float4 position : POSITION;
        float3 posSW : TEXCOORD0;
        float3 posNW : TEXCOORD1;
        float3 posSE : TEXCOORD2;
        float3 posNE : TEXCOORD3;
        float3 barycentric : TEXCOORD4;
    };


    v2g vert(appdata_in v) {
        v2g o;
        float3 pos = v.position.xyz;
        if (_Morph == 1) {
            pos = lerp(lerp(v.posNW, v.posSW, _PosY), lerp(v.posNE, v.posNE, _PosY), _PosX);
        }
        o.position = mul(UNITY_MATRIX_MVP, float4(pos, 0.0));
        o.position.z = 0.0;
        o.posSW = v.posSW;
        o.posNW = v.posNW;
        o.posSE = v.posSE;
        o.posNE = v.posNE;
        return o;
    }

    [maxvertexcount(3)]
    void geom(triangle v2g i[3], inout TriangleStream<g2f> triangleStream) {
        g2f o0;
        o0.position = i[0].position;
        o0.posSW = i[0].posSW;
        o0.posNW = i[0].posNW;
        o0.posSE = i[0].posSE;
        o0.posNE = i[0].posNE;
        o0.barycentric = float3(1, 0, 0);
        // o0.t0 = i[0].position;
        // o0.t1 = i[1].position;
        // o0.t2 = i[2].position;
        triangleStream.Append(o0);

        g2f o1;
        o1.position = i[1].position;
        o1.posSW = i[1].posSW;
        o1.posNW = i[1].posNW;
        o1.posSE = i[1].posSE;
        o1.posNE = i[1].posNE;
        o1.barycentric = float3(0, 1, 0);
        // o1.t0 = i[0].position;
        // o1.t1 = i[1].position;
        // o1.t2 = i[2].position;
        triangleStream.Append(o1);

        g2f o2;
        o2.position = i[2].position;
        o2.posSW = i[2].posSW;
        o2.posNW = i[2].posNW;
        o2.posSE = i[2].posSE;
        o2.posNE = i[2].posNE;
        o2.barycentric = float3(0, 0, 1);
        // o2.t0 = i[0].position;
        // o2.t1 = i[1].position;
        // o2.t2 = i[2].position;
        triangleStream.Append(o2);
    }

    half4 frag(g2f i) : COLOR {
        float4 sw = texCUBE(_CubemapSW, i.posSW);
        float4 nw = texCUBE(_CubemapNW, i.posNW);
        float4 se = texCUBE(_CubemapSE, i.posSE);
        float4 ne = texCUBE(_CubemapNE, i.posNE);

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
