using UnityEngine.Rendering.Universal;

public class BlurRendererFeature : ScriptableRendererFeature
{
    private BlurRenderPass _renderPass;
    
    public override void Create()
    {
        _renderPass = new BlurRenderPass();
        name = "Blur";
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (_renderPass.Setup(renderer))
        {
            renderer.EnqueuePass(_renderPass);
        }
    }
}