Shader "EcoClean/Sea Surface"
{
    // Transparent, double-sided version of Beach Water so the sea can be seen from underwater.
    Properties
    {
        _BaseColor ("Water color", Color) = (0.03, 0.52, 0.65, 1)
        _Opacity ("Opacity from above", Range(0, 1)) = 0.78
        _UnderColor ("Surface seen from below", Color) = (0.45, 0.8, 0.85, 1)
        _UnderOpacity ("Opacity from below", Range(0, 1)) = 0.55
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            half _Opacity;
            half4 _UnderColor;
            half _UnderOpacity;
            CBUFFER_END

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float fogFactor : TEXCOORD1;
            };

            Varyings Vert(float4 positionOS : POSITION)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input, bool frontFace : SV_IsFrontFace) : SV_Target
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
                float3 cameraPos = GetCameraPositionWS();
                float nearDetail = 1 - smoothstep(27, 84, distance(cameraPos, input.positionWS));

                if (cameraPos.y < input.positionWS.y)
                {
                    // Looking up from underwater: bright, rippling surface.
                    half3 under = _UnderColor.rgb * (.9 + .15 * swell) + crest * half3(.2,.25,.25);
                    return half4(MixFog(under, input.fogFactor), _UnderOpacity);
                }

                float3 view = SafeNormalize(cameraPos - input.positionWS);
                float grazing = pow(1 - saturate(view.y), 4);
                half3 color = _BaseColor.rgb * (.93 + .09 * swell * nearDetail);
                color = lerp(color, half3(.37,.68,.81), grazing * .21);
                color += crest * nearDetail * half3(.15,.21,.21);
                // Grazing views reflect more, so the far sea becomes opaque.
                half alpha = lerp(_Opacity, 1, grazing);
                return half4(MixFog(color, input.fogFactor), alpha);
            }
            ENDHLSL
        }
    }
}
