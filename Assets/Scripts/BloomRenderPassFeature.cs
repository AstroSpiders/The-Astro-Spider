using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BloomRenderPassFeature : ScriptableRendererFeature
{
    class BloomRenderPass : ScriptableRenderPass
    {
        private string                 _profilerTag;
        private Material               _bloomExtractMaterial;
        private Material               _gaussianBlurHorizontalMaterial;
        private Material               _gaussianBlurVerticalMaterial;
        private Material               _bloomCombineMaterial;
        private RenderTargetIdentifier _cameraColorTargetIdent;

        private RenderTargetHandle     _tempTexture1;
        private RenderTargetHandle     _tempTexture2;
        private RenderTargetHandle     _resultTexture;
        private RenderTexture          _renderTexture;

        private int                    _cameraWidth;
        private int                    _cameraHeight;

        private float                  _bloomThreshold;
        private float                  _blurAmount;

        private float                  _bloomIntensity;
        private float                  _baseIntensity;
        private float                  _bloomSaturation;
        private float                  _baseSaturation;

        public BloomRenderPass(string          profilerTag, 
                               RenderPassEvent renderPassEvent, 
                               Material        bloomExtractMaterial,
                               Material        gaussianBlurMaterial,
                               Material        bloomCombineMaterial,
                               float           bloomThreshold,
                               float           blurAmount,
                               float           bloomIntensity,
                               float           baseIntensity,
                               float           bloomSaturation,
                               float           baseSaturation)
        {
            _profilerTag                    = profilerTag;
            this.renderPassEvent            = renderPassEvent;
            _bloomExtractMaterial           = bloomExtractMaterial;

            _gaussianBlurHorizontalMaterial = new Material(gaussianBlurMaterial);
            _gaussianBlurVerticalMaterial   = new Material(gaussianBlurMaterial);

            _bloomCombineMaterial           = bloomCombineMaterial;
                                            
            _bloomThreshold                 = bloomThreshold;
            _blurAmount                     = blurAmount;

            _bloomIntensity                 = bloomIntensity;
            _baseIntensity                  = baseIntensity;
            _bloomSaturation                = bloomSaturation;
            _baseSaturation                 = baseSaturation;

            _tempTexture1.id                = 0;
            _tempTexture2.id                = 1;
            _resultTexture.id               = 2;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(_profilerTag);
            
            cmd.Clear();
            
            //cmd.SetGlobalTexture()

            _bloomExtractMaterial.SetFloat("_BloomThreshold", _bloomThreshold);
            cmd.Blit(_cameraColorTargetIdent, _tempTexture1.Identifier(), _bloomExtractMaterial, 0);

            SetBlurEffectParameters(1.0f / (_cameraWidth / 2.0f), 0.0f, _gaussianBlurHorizontalMaterial);
            cmd.Blit(_tempTexture1.Identifier(), _tempTexture2.Identifier(), _gaussianBlurHorizontalMaterial, 0);

            SetBlurEffectParameters(0.0f, 1.0f / (_cameraHeight / 2.0f), _gaussianBlurVerticalMaterial);
            cmd.Blit(_tempTexture2.Identifier(), _renderTexture, _gaussianBlurVerticalMaterial, 0);

            _bloomCombineMaterial.SetFloat("_BloomIntensity", _bloomIntensity);
            _bloomCombineMaterial.SetFloat("_BaseIntensity", _baseIntensity);
            _bloomCombineMaterial.SetFloat("_BloomSaturation", _bloomSaturation);
            _bloomCombineMaterial.SetFloat("_BaseSaturation", _baseSaturation);            
            _bloomCombineMaterial.SetTexture("_BloomTexture", _renderTexture);

            cmd.Blit(_cameraColorTargetIdent, _resultTexture.Identifier(), _bloomCombineMaterial);

            cmd.Blit(_resultTexture.Identifier(), _cameraColorTargetIdent);

            context.ExecuteCommandBuffer(cmd);

            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            _cameraWidth  = cameraTextureDescriptor.width;
            _cameraHeight = cameraTextureDescriptor.height;

            var newDescriptor = cameraTextureDescriptor;

            newDescriptor.width /= 2;
            newDescriptor.height /= 2;

            cmd.GetTemporaryRT(_tempTexture1.id, newDescriptor);
            cmd.GetTemporaryRT(_tempTexture2.id, newDescriptor);

            cmd.GetTemporaryRT(_resultTexture.id, cameraTextureDescriptor);

            if (_renderTexture is null)
                _renderTexture = new RenderTexture(newDescriptor);
        }
        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(_resultTexture.id);

            cmd.ReleaseTemporaryRT(_tempTexture2.id);
            cmd.ReleaseTemporaryRT(_tempTexture1.id);
        }

        public void Setup(RenderTargetIdentifier cameraColorTargetIdent) => _cameraColorTargetIdent = cameraColorTargetIdent;

        private void SetBlurEffectParameters(float dx, float dy, Material material)
        {
            var sampleCount = 15;

            var sampleWeights = new float[sampleCount];
            var sampleOffsetsX = new float[sampleCount];
            var sampleOffsetsY = new float[sampleCount];


            sampleWeights[0] = ComputeGaussian(0);
            sampleOffsetsX[0] = 0;
            sampleOffsetsY[0] = 0;

            var totalWeights = sampleWeights[0];

            for (var i = 0; i < sampleCount / 2; i++)
            {
                var weight = ComputeGaussian(i + 1);

                sampleWeights[i * 2 + 1] = weight;
                sampleWeights[i * 2 + 2] = weight;

                totalWeights += weight * 2;

                var sampleOffset = i * 2 + 1.5f;

                var delta = new Vector2(dx, dy) * sampleOffset;

                sampleOffsetsX[i * 2 + 1] = delta.x;
                sampleOffsetsY[i * 2 + 1] = delta.y;

                sampleOffsetsX[i * 2 + 2] = -delta.x;
                sampleOffsetsY[i * 2 + 2] = -delta.y;
            }

            for (var i = 0; i < sampleWeights.Length; i++) sampleWeights[i] /= totalWeights;

            material.SetFloatArray("_SampleOffsetsX", sampleOffsetsX);
            material.SetFloatArray("_SampleOffsetsY", sampleOffsetsY);

            material.SetFloatArray("_SampleWeights", sampleWeights);
        }

        private float ComputeGaussian(float n)
        {
            var theta = _blurAmount;

            return (float)(1.0 / Math.Sqrt(2 * Math.PI * theta) *
                            Math.Exp(-(n * n) / (2 * theta * theta)));
        }
    }

    [Serializable]
    public class BloomSettings
    {
        public bool            IsEnabled       = true;
        public RenderPassEvent WhenToInsert    = RenderPassEvent.AfterRendering;

        public Material        BloomExtractMaterial;
        public Material        GaussianBlurMaterial;
        public Material        BloomCombineMaterial;

        public float           BloomThreshold  = 0.5f;
        public float           BlurAmount      = 0.5f;
        public float           BloomIntensity  = 1.5f;
        public float           BaseIntensity   = 1.0f;
        public float           BloomSaturation = 1.0f;
        public float           BaseSaturation  = 1.0f;
    }

    [SerializeField]
    private BloomSettings   _settings = new BloomSettings();
    private BloomRenderPass _bloomRenderPass;

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!_settings.IsEnabled)
            return;

        var cameraColorTargetIdent = renderer.cameraColorTarget;
        _bloomRenderPass.Setup(cameraColorTargetIdent);

        renderer.EnqueuePass(_bloomRenderPass);
    }

    public override void Create() => _bloomRenderPass = new BloomRenderPass("Bloom pass", 
                                                                            _settings.WhenToInsert, 
                                                                            _settings.BloomExtractMaterial, 
                                                                            _settings.GaussianBlurMaterial, 
                                                                            _settings.BloomCombineMaterial,
                                                                            _settings.BloomThreshold, 
                                                                            _settings.BlurAmount,
                                                                            _settings.BloomIntensity,
                                                                            _settings.BaseIntensity,
                                                                            _settings.BloomSaturation,
                                                                            _settings.BaseSaturation);
}
