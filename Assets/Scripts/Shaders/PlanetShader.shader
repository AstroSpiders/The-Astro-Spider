Shader "CustomShaders/PlanetShader"
{
    // The properties block of the Unity shader. In this example this block is empty
   // because the output color is predefined in the fragment shader code.
    Properties
    { 
        [HDR]
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)

        [MainTexture] 
        _BaseMap("Albedo", 2D) = "white" {}

        [HDR]
        _AmbientColor("Ambient color", Color) = (0.4, 0.4, 0.4, 1)

        [HDR]
        _SpecularColor("Specular Color", Color) = (0.9, 0.9, 0.9, 1)
        _Glossiness("Glossiness", Float) = 32

        [HDR]
        _RimColor("Rim Color", Color) = (1, 1, 1, 1)
        _RimAmount("Rim Amount", Range(0, 1)) = 0.716
        _RimThreshold("Rim Threshold", Range(0, 1)) = 0.1

        [HDR]
        _Disurbance("Disturbance", Float) = 0.1

        _FrequencyX("FrequencyX", Float) = 1.0
        _OctavesX("OctavesX", Int) = 1

        _FrequencyY("FrequencyY", Float) = 1.0
        _OctavesY("OctavesY", Int) = 1

        _FrequencyZ("FrequencyZ", Float) = 1.0
        _OctavesZ("OctavesZ", Int) = 1

    }

    // The SubShader block containing the Shader code. 
    SubShader
    {
        // SubShader Tags define when and under which conditions a SubShader block or
        // a pass is executed.
        Tags 
        {
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalRenderPipeline"
            "LightMode" = "UniversalForward"
            "PassFlags" = "OnlyDirectional"
        }

        Pass
        {
            HLSLPROGRAM

            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _ALPHAPREMULTIPLY_ON

            // -------------------------------------
            // Unity defined keywords

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE

            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "SimplexNoise.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // The structure definition defines which variables it contains.
            // This example uses the Attributes structure as an input structure in
            // the vertex shader.
            struct Attributes
            {
                // The positionOS variable contains the vertex positions in object
                // space.
                float4 positionOS   : POSITION;
                float3 normal       : NORMAL;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                // The positions in this struct must have the SV_POSITION semantic.
                float4 positionHCS  : SV_POSITION;
                float3 worldNormal  : NORMAL;
                float2 uv           : TEXCOORD0;
                float3 positionWS   : TEXCOORD1;
                float4 shadowCoord  : TEXCOORD2;
            };
            
            // The vertex shader definition with properties defined in the Varyings 
            // structure. The type of the vert function must match the type (struct)
            // that it returns.
            Varyings vert(Attributes IN)
            {
                // Declaring the output object (OUT) with the Varyings struct.
                Varyings OUT;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.positionOS.xyz);

                // The TransformObjectToHClip function transforms vertex positions
                // from object space to homogenous space
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS  = TransformObjectToWorld(IN.positionOS);
                OUT.worldNormal = TransformObjectToWorldNormal(IN.normal);
                OUT.shadowCoord = GetShadowCoord(vertexInput);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);

                // Returning the output.
                return OUT;
            }

            float4 _AmbientColor;
            float _Glossiness;
            float4 _SpecularColor;
            
            float4 _RimColor;
            float _RimAmount;
            float _RimThreshold;

            float _Disurbance;

            float _FrequencyX;
            float _OctavesX;

            float _FrequencyY;
            float _OctavesY;

            float _FrequencyZ;
            float _OctavesZ;

            float getNoise(float3 pos, int octavesCount)
            {
                float result = 0.0f;
                float frequency = 1.0f;
                float amplitude = 1.0f;

                float amplitudeSum = 0.0f;

                for (int i = 0; i < octavesCount; i++)
                {
                    result += (snoise(pos * frequency) * 0.5f + 0.5f) * amplitude;
                    frequency *= 2.0f;
                    amplitude /= 2.0f;

                    amplitudeSum += amplitude;
                }

                result /= amplitudeSum;

                return result * 2.0f - 1.0f;
            }


            // The fragment shader definition.            
            half4 frag(Varyings IN) : SV_Target
            {
                // Defining the color variable and returning it.
                half4 customColor;
                float3 normal = normalize(IN.worldNormal);

                float dx = getNoise(IN.positionWS * _FrequencyX, _OctavesX);
                float dy = getNoise(IN.positionWS * _FrequencyY, _OctavesY);
                float dz = getNoise(IN.positionWS * _FrequencyZ, _OctavesZ);

                normal.x += dx * _Disurbance;
                normal.y += dx * _Disurbance;
                normal.z += dx * _Disurbance;

                Light mainLight = GetMainLight(IN.shadowCoord);

                float NdotL = dot(mainLight.direction, normal);
                float shadowAttenuation = smoothstep(0, 0.01, mainLight.shadowAttenuation);
                float lightIntensity = smoothstep(0, 0.01, NdotL * shadowAttenuation);
                float4 light = lightIntensity * float4(mainLight.color, 1.0);

                half3 viewDirectionWS = SafeNormalize(GetCameraPositionWS() - IN.positionWS);
                float specularIntensity = pow(NdotL * lightIntensity, _Glossiness * _Glossiness);
                float specularIntensitySmooth = smoothstep(0.005, 0.01, specularIntensity);
                float4 specular = specularIntensitySmooth * _SpecularColor;
                
                float4 rimDot = 1 - dot(viewDirectionWS, normal);
                float rimIntensity = rimDot * pow(NdotL, _RimThreshold);
                rimIntensity = smoothstep(_RimAmount - 0.01, _RimAmount + 0.01, rimIntensity);
                float4 rim = rimIntensity * _RimColor;

                return (_BaseColor * _BaseMap.Sample(sampler_BaseMap, IN.uv)) * (_AmbientColor + light + specular + rim);
            }
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
}
