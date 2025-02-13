#pragma warning disable CS0618 // Type or member is obsolete
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RetroPass : ScriptableRenderPass
{
    private Material _material;
    private RetroPostProcess retroPostProcess;
    int tempid = Shader.PropertyToID("_MainTex"); // Property ID for temporary render target
    RenderTargetIdentifier src, dst;

    public RetroPass()
    {
        if (!_material) _material = CoreUtils.CreateEngineMaterial(
            "Custom Post-Processing/Retro");

        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        retroPostProcess = VolumeManager.instance.stack.GetComponent<RetroPostProcess>();
        RenderTextureDescriptor desc = renderingData.cameraData.cameraTargetDescriptor;
        tempid = Shader.PropertyToID("_MainTex");
        cmd.GetTemporaryRT(tempid, desc);
        src = renderingData.cameraData.renderer.cameraColorTargetHandle;
        dst = new RenderTargetIdentifier(tempid);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (retroPostProcess == null || !retroPostProcess.IsActive()) return;

        CommandBuffer commandBuffer = CommandBufferPool.Get("Custom/Retro");

        // Set the parameters in the material 
        _material.SetFloat("_pixelSize", (float)retroPostProcess.pixelSize);
        _material.SetInteger("_redCount", retroPostProcess.redColourCount.value);
        _material.SetInteger("_greenCount", retroPostProcess.greenColourCount.value);
        _material.SetInteger("_blueCount", retroPostProcess.blueColourCount.value);
        _material.SetInteger("_bayerLevel", retroPostProcess.bayerLevel.value);
        // Apply the black and white effect to the temporary render target
        Blit(commandBuffer, src, dst, _material, 0);
        //Blit the result back to the source render target
        Blit(commandBuffer, dst, src);

        // Execute the command buffer
        context.ExecuteCommandBuffer(commandBuffer);
        // Release the command buffer
        CommandBufferPool.Release(commandBuffer);
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(tempid);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
#pragma warning restore CS0618 // Type or member is obsolete
