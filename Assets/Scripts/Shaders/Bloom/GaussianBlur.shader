Shader "CustomShaders/GaussianBlur"
{
    // The properties block of the Unity shader. In this example this block is empty
        // because the output color is predefined in the fragment shader code.
    Properties
    {
        [HideInInspector]
        _MainTex("Base (RGB)", 2D) = "white" {}
    }

    // The SubShader block containing the Shader code. 
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        // SubShader Tags define when and under which conditions a SubShader block or
        // a pass is executed.
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" }

        Pass
        {
            // The HLSL code block. Unity SRP uses the HLSL language.
            HLSLPROGRAM

            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            // This line defines the name of the vertex shader. 
            #pragma vertex vert
            // This line defines the name of the fragment shader. 
            #pragma fragment frag

            // The Core.hlsl file contains definitions of frequently used HLSL
            // macros and functions, and also contains #include references to other
            // HLSL files (for example, Common.hlsl, SpaceTransforms.hlsl, etc.).
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"            

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            // The structure definition defines which variables it contains.
            // This example uses the Attributes structure as an input structure in
            // the vertex shader.
            struct Attributes
            {
                // The positionOS variable contains the vertex positions in object
                // space.
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                // The positions in this struct must have the SV_POSITION semantic.
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
            };

            // The vertex shader definition with properties defined in the Varyings 
            // structure. The type of the vert function must match the type (struct)
            // that it returns.
            Varyings vert(Attributes IN)
            {
                // Declaring the output object (OUT) with the Varyings struct.
                Varyings OUT;
                // The TransformObjectToHClip function transforms vertex positions
                // from object space to homogenous space
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                // Returning the output.
                return OUT;
            }

            SamplerState linear_clamp_sampler_MainTex;

            #define SAMPLE_COUNT 15

            float _SampleOffsetsX[SAMPLE_COUNT];
            float _SampleOffsetsY[SAMPLE_COUNT];
            float _SampleWeights[SAMPLE_COUNT];

            // The fragment shader definition.            
            half4 frag(Varyings IN) : SV_Target
            {
                float4 c = 0;
                
                for (int i = 0; i < SAMPLE_COUNT; i++)
                    c += _MainTex.Sample(linear_clamp_sampler_MainTex, IN.uv + float2(_SampleOffsetsX[i], _SampleOffsetsY[i])) * _SampleWeights[i];
                
                return c;
            }

            ENDHLSL
        }
    }
}
