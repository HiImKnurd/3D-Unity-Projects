using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BWPass : ScriptableRenderPass
{
    Material _mat; //Black and White material
    int bwId = Shader.PropertyToID("_Temp"); // Property ID for temporary render target
    RenderTargetIdentifier src, bw;
    BWPostProcess bwPP;

    public BWPass()
    {
        if (!_mat) _mat = CoreUtils.CreateEngineMaterial("Custom Post-Processing/B&W Post-Processing");

        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        bwPP = VolumeManager.instance.stack.GetComponent<BWPostProcess>();
        RenderTextureDescriptor desc = renderingData.cameraData.cameraTargetDescriptor;
        src = renderingData.cameraData.renderer.cameraColorTargetHandle;
        cmd.GetTemporaryRT(bwId, desc, FilterMode.Bilinear);
        bw = new RenderTargetIdentifier(bwId);
    }
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (bwPP == null || !bwPP.IsActive()) return;

        CommandBuffer commandBuffer = CommandBufferPool.Get("BWRenderPassFeature");

        // Set the blend intensity in the material 
        _mat.SetFloat("_blend", (float)bwPP.blendIntensity);
        // Apply the black and white effect to the temporary render target
        Blit(commandBuffer, src, bw, _mat, 0);
        //Blit the result back to the source render target
        Blit(commandBuffer, bw, src);

        // Execute the command buffer
        context.ExecuteCommandBuffer(commandBuffer);
        // Release the command buffer
        CommandBufferPool.Release(commandBuffer);
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(bwId);
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
