Shader "Instanced/SDFComposition"
{
    Properties
    {
        _MainTex ("Accumulated Density (RGB: Density, ID*Inf, 0)", 2D) = "white" {}
        _BgColor ("Background Color", Color) = (0,0,0,1)
        _Gloss ("SurfaceGloss", Range(0, 1)) = 0
    }
    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "UnityCG.cginc"
            #include "../ShaderTypes.hlsl"

            sampler2D _MainTex;
            float4 _BgColor;
            float _Gloss;
            StructuredBuffer<FluidMediumProfile> FluidMediaProfiles;
            
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v){
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //sample accumulated distance feild
                 float4 data = tex2D(_MainTex, i.uv);
                float density = data.r;

                //draw background if no fluid
                if (density < 0.001) return _BgColor;

                //recover the dominant fluid medium profile
                //_MainTex.g = id * influence so div by total influence (density) to get id
                uint winnerID = (uint) round(data.g / max(density, 0.0001));
                FluidMediumProfile flprof = FluidMediaProfiles[winnerID];
                //physics-based thresholding of fluid interface
                float saturation = density / max(flprof.targetDensity, 0.001);
                float finalThreshold = 0.5 - (flprof.viscosityStrength * 0.15) + (flprof.pressureMultiplier * 0.02);
                if (saturation > finalThreshold){
                    float3 finalRGB = lerp(float3(1,1,1), float3(0.5,0.7,1), saturation); //temp coloring
                    float edgeAlpha = smoothstep(finalThreshold, finalThreshold + 0.05, saturation); //edge antialiasing
                    return fixed4(lerp(_BgColor.rgb, finalRGB, edgeAlpha), 1);
                }
                return _BgColor;
            }
            ENDCG
        }
    }
}