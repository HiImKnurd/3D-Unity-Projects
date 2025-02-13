using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GaussianPass : ScriptableRenderPass
{
    private Material material;

    private GaussianPostProcess blurPostProcess;
    private RenderTargetIdentifier src;
    private RenderTargetHandle dst;

    private int texID;

    public GaussianPass()
    {
        if (!material) material = CoreUtils.CreateEngineMaterial(
            "Custom Post-Processing/Gaussian Blur");

        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        // Get the GaussianBlurPostProcess component from the volume stack
        blurPostProcess = VolumeManager.instance.stack.GetComponent<GaussianPostProcess>();
        RenderTextureDescriptor desc = renderingData.cameraData.cameraTargetDescriptor;
        src = renderingData.cameraData.renderer.cameraColorTargetHandle;
    }
    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        if(blurPostProcess == null || !blurPostProcess.IsActive())
        {
            return;
        }

        texID = Shader.PropertyToID("_MainTex");
        dst = new RenderTargetHandle();
        dst.id = texID;
        cmd.GetTemporaryRT(texID, cameraTextureDescriptor);
        base.Configure(cmd, cameraTextureDescriptor);
    }
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (blurPostProcess == null || !blurPostProcess.IsActive())
        {
            return;
        }

        CommandBuffer cmd = CommandBufferPool.Get("Custom/Gaussian Blur");

        // Blur effect properties
        int gridSize = Mathf.CeilToInt(blurPostProcess.blurIntensity.value * 6f);
        if(gridSize % 2 == 0)
        {
            gridSize++;
        }

        material.SetInteger("_GridSize", gridSize);
        material.SetFloat("_Spread", blurPostProcess.blurIntensity.value);

        // Apply using 2 passses
        cmd.Blit(src, texID,material, 0);
        cmd.Blit(texID, src, material, 1);

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }

}
