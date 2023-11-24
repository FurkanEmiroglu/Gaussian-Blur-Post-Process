using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlurRenderPass : ScriptableRenderPass
{
    private Material _material;
    private BlurPostProcessSettings _blurPostProcessSettings;
        
    private RenderTargetIdentifier _source;
    private RenderTargetHandle _blurTexture;
    private int _blurTextureID;
    
    private static readonly int GridSize = Shader.PropertyToID("_Grid");
    private static readonly int Spread = Shader.PropertyToID("_Spread");

    public bool Setup(ScriptableRenderer renderer)
    {
        _source = renderer.cameraColorTarget;
        _blurPostProcessSettings = VolumeManager.instance.stack.GetComponent<BlurPostProcessSettings>();
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        if (_blurPostProcessSettings != null && _blurPostProcessSettings.IsActive())
        {
            _material = new Material(Shader.Find("PostProcessing/Blur"));
            return true;
        }

        return false;
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTexDesc)
    {
        if (_blurPostProcessSettings == null || !_blurPostProcessSettings.IsActive()) return;

        _blurTextureID = Shader.PropertyToID("_TemporaryBlurTextureID");
        _blurTexture = new RenderTargetHandle();
        _blurTexture.id = _blurTextureID;
        cmd.GetTemporaryRT(_blurTexture.id, cameraTexDesc);
            
        base.Configure(cmd, cameraTexDesc);
    } 
        
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (_blurPostProcessSettings == null || !_blurPostProcessSettings.IsActive()) return;

        CommandBuffer cmd = CommandBufferPool.Get("Blur Post Process");
        int gridSize = Mathf.CeilToInt(_blurPostProcessSettings.GetBlurValue());
            
        if (gridSize % 2 == 0)
        {
            gridSize++;
        }
        _material.SetInteger(GridSize, gridSize);
        _material.SetFloat(Spread, _blurPostProcessSettings.GetBlurValue());
           
        cmd.Blit(_source, _blurTexture.id, _material, 0);
        cmd.Blit(_blurTexture.id, _source, _material, 1);
        
        context.ExecuteCommandBuffer(cmd);
        
        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(_blurTextureID);
        base.FrameCleanup(cmd);
    }
}