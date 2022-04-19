Shader "CustomShaders/BloomCombine"
{
    // The properties block of the Unity shader. In this example this block is empty
    // because the output color is predefined in the fragment shader code.
    Properties
    {
        [HideInInspector]
        _MainTex("Base (RGB)", 2D) = "white" {}

        _BloomTexture("Bloom Texture", 2D) = "white" {}

        _BloomIntensity("Bloom Intensity", Float) = 1.5
        _BaseIntensity("Base Intensity", Float) = 1
        _BloomSaturation("Bloom Intensity", Float) = 1
        _BaseSaturation("Bloom Intensity", Float) = 1
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

            Texture2D    _BloomTexture;
            SamplerState linear_clamp_sampler_BloomTexture;

            float        _BloomIntensity;
            float        _BaseIntensity;
                         
            float        _BloomSaturation;
            float        _BaseSaturation;

            // Helper for modifying the saturation of a color.
            float4 AdjustSaturation(float4 color, float saturation)
            {
                // The constants 0.3, 0.59, and 0.11 are chosen because the
                // human eye is more sensitive to green light, and less to blue.
                float grey = dot(color, float3(0.3, 0.59, 0.11));

                return lerp(grey, color, saturation);
            }

            // The fragment shader definition.            
            half4 frag(Varyings IN) : SV_Target
            {
                // Look up the bloom and original base image colors.
                float4 base = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                float4 bloom = _BloomTexture.Sample(linear_clamp_sampler_BloomTexture, IN.uv);

                // Adjust color saturation and intensity.
                bloom = AdjustSaturation(bloom, _BloomSaturation) * _BloomIntensity;
                base = AdjustSaturation(base, _BaseSaturation) * _BaseIntensity;

                // Darken down the base image in areas where there is a lot of bloom,
                // to prevent things looking excessively burned-out.
                base *= (1 - saturate(bloom));

                // Combine the two images.
                return base + bloom;
            }
            ENDHLSL
        }
    }
}
