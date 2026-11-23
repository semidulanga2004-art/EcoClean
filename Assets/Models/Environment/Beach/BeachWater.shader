Shader "EcoClean/Beach Water"
{
    Properties
    {
        _BaseColor ("Water color", Color) = (0.03, 0.52, 0.65, 1)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            CBUFFER_END

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            Varyings Vert(float4 positionOS : POSITION)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 p = input.positionWS.xz;
                float t = _Time.y;
                float swellPhase = p.x * 1.65 + sin(p.y * .24 + t * .38) * .63 + t * 1.7;
                float ripplePhase = p.x * 2.7 - p.y * 1.12 + t * 2.1;
                float crossPhase = p.x * .84 + p.y * 2.04 - t * 1.25;
                // Fade ripples before their phase changes too quickly across a pixel.
                float swell = sin(swellPhase) * (1 - smoothstep(.65, 2.4, fwidth(swellPhase)));
                float ripple = sin(ripplePhase) * (1 - smoothstep(.65, 2.4, fwidth(ripplePhase)));
                float crossRipple = sin(crossPhase) * (1 - smoothstep(.65, 2.4, fwidth(crossPhase)));
                float crest = smoothstep(.56, 1.0, swell * .68 + ripple * .20 + crossRipple * .12);
                float3 view = SafeNormalize(GetCameraPositionWS() - input.positionWS);
                float grazing = pow(1 - saturate(view.y), 4);
                float nearDetail = 1 - smoothstep(27, 84, distance(GetCameraPositionWS(), input.positionWS));
                half3 color = _BaseColor.rgb * (.93 + .09 * swell * nearDetail);
                color = lerp(color, half3(.37,.68,.81), grazing * .21);
                color += crest * nearDetail * half3(.15,.21,.21);
                return half4(color,1);
            }
            ENDHLSL
        }
    }
}
