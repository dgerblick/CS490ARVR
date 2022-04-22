// UNITY_SHADER_NO_UPGRADE

Shader "Custom/MyCubeMap" {
    Properties {
        _Cubemap ("Fallback Cubemap", CUBE) = "grey" { }
        [Toggle] _ShowWireframe ("Show Wireframe", Int) = 0
    }
    CGINCLUDE

    #include "UnityCG.cginc"

    samplerCUBE _Cubemap;
    int _ShowWireframe;

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
    };


    v2g vert(appdata_base v) {
        v2g o;
        o.position = mul(UNITY_MATRIX_MVP, float4(v.vertex.xyz, 0.0));
        o.position.z = 0.0;
        o.texcoord = v.texcoord;
        return o;
    }

    [maxvertexcount(3)]
    void geom(triangle v2g i[3], inout TriangleStream<g2f> triangleStream) {
        g2f o0;
        o0.position = i[0].position;
        o0.texcoord = i[0].texcoord;
        o0.barycentric = float3(1, 0, 0);
        triangleStream.Append(o0);

        g2f o1;
        o1.position = i[1].position;
        o1.texcoord = i[1].texcoord;
        o1.barycentric = float3(0, 1, 0);
        triangleStream.Append(o1);

        g2f o2;
        o2.position = i[2].position;
        o2.texcoord = i[2].texcoord;
        o2.barycentric = float3(0, 0, 1);
        triangleStream.Append(o2);
    }

    half4 frag(g2f i) : COLOR {
        float4 c = texCUBE(_Cubemap, i.texcoord);

        // Show wireframe
        float wireThickness = 0.005;
        if (_ShowWireframe == 1 && (i.barycentric.x < wireThickness || i.barycentric.y < wireThickness || i.barycentric.z < wireThickness)) {
            c = float4(0.0, 0.0, 0.0, 1.0);
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
