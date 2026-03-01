using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

/// <summary>
/// FREQUENCY 프로젝트 — Render Graph 기반 커스텀 포스트프로세싱 Feature 템플릿.
/// 각 주파수(20Hz / 7.83Hz / 18.98Hz / 528Hz)별로 이 클래스를 상속하거나 복제해 Material만 교체한다.
/// </summary>
public class FrequencyPostProcessFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public Material material;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        [Range(0f, 1f)]
        public float intensity = 1.0f;
    }

    public Settings settings = new Settings();
    private FrequencyPostProcessPass m_Pass;

    public override void Create()
    {
        m_Pass = new FrequencyPostProcessPass(settings);
        m_Pass.renderPassEvent = settings.renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.material == null) return;
        renderer.EnqueuePass(m_Pass);
    }

    // ─────────────────────────────────────────────────────────
    // Render Graph 기반 패스
    // ─────────────────────────────────────────────────────────
    class FrequencyPostProcessPass : ScriptableRenderPass
    {
        private Settings m_Settings;

        private class PassData
        {
            public TextureHandle source;
            public Material material;
            public float intensity;
        }

        public FrequencyPostProcessPass(Settings settings)
        {
            m_Settings = settings;
        }

        // Unity 6 Render Graph API
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameContext)
        {
            var resourceData = frameContext.Get<UniversalResourceData>();
            TextureHandle source = resourceData.activeColorTexture;

            var descriptor = renderGraph.GetTextureDesc(source);
            descriptor.name = "FrequencyPostProcess_Temp";
            TextureHandle destination = renderGraph.CreateTexture(descriptor);

            // Pass 1: source → temp
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(
                "Frequency Post Process", out var passData))
            {
                passData.source    = source;
                passData.material  = m_Settings.material;
                passData.intensity = m_Settings.intensity;

                builder.UseTexture(source, AccessFlags.Read);
                builder.SetRenderAttachment(destination, 0, AccessFlags.Write);

                builder.SetRenderFunc(static (PassData data, RasterGraphContext context) =>
                {
                    data.material.SetFloat("_Intensity", data.intensity);
                    Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), data.material, 0);
                });
            }

            // Pass 2: temp → camera color
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(
                "Frequency Post Process Copy Back", out var passData2))
            {
                passData2.source = destination;

                builder.UseTexture(destination, AccessFlags.Read);
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);

                builder.SetRenderFunc(static (PassData data, RasterGraphContext context) =>
                {
                    // 수정: RasterGraphContext에서는 직접적인 Blit 대신 
                    // RasterCommandBuffer의 메서드를 사용하거나, 올바른 Blitter 인자를 전달해야 합니다.
                    // Unity 6에서는 아래 방식이 가장 간결하고 정확합니다.

                    Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), 0, false);
                });
            }
        }
    }
}
