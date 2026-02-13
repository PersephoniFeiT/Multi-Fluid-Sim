Shader "Instanced/SDFBlobs" {
    Properties {
        _InfluenceScale ("Influence Scale", Float) = 1.0
    }
    SubShader {
        Tags {"RenderType" = " Transparent"  " Queue" = "Transparent"}
        Blend One One
        ZWrite Off
        Cull Off

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "UnityCG.cginc"
            #include "../ShaderTypes.hlsl"

            StructuredBuffer<float2> Positions2D;
            StructuredBuffer<float2> DensityData;
            StructuredBuffer<FluidMediumProfile> FluidMediaProfiles;
            StructuredBuffer<uint> FluidMediaIndices;

            float _GlobalScale;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                uint mediumIndex : TEXCOORD1;
            }

            v2f vert (appdata_full v, uint instanceID : SV_InstanceID)
            {
                uint mIdx = FluidMediaIndices[instanceID];

                float3 worldPos = float3(Positions2D[instanceID], 0);

                float3 vPos = worldPos + mul(unity_ObjectToWorld, v.vertex * _GlobalScale);

                v2f o;
                o.pos = UnityObjectToClipPos(float4(vPos, 1));
                o.uv = v.textcood;
                o.mediumIndex = mIdx;
                return 0;
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 uv = (i.uv - 0.5) * 2;
                float sqrDst = dot(uv, uv);
                if (sqrDst > 1) discard;

                float influence = 1.0 - sqrDist;
                influence *= influence;
                return float4(influence, (float) i.mediumIndex * influence, 0, 1);
            }
            ENDCG
        }
    }
}