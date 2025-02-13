using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ChromaticPass : ScriptableRenderPass
{
    Material _mat; 
    int bwId = Shader.PropertyToID("_Temp"); // Property ID for temporary render target
    RenderTargetIdentifier src, bw;
    ChromaticAberrationPostProcess ca;

    public ChromaticPass()
    {
        if (!_mat) _mat = CoreUtils.CreateEngineMaterial("Custom Post-Processing/Chromatic Aberration");

        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        ca = VolumeManager.instance.stack.GetComponent<ChromaticAberrationPostProcess>();
        RenderTextureDescriptor desc = renderingData.cameraData.cameraTargetDescriptor;
        src = renderingData.cameraData.renderer.cameraColorTargetHandle;
        cmd.GetTemporaryRT(bwId, desc, FilterMode.Bilinear);
        bw = new RenderTargetIdentifier(bwId);
    }
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (ca == null || !ca.IsActive()) return;

        CommandBuffer commandBuffer = CommandBufferPool.Get("Custom/Chromatic Aberration");

        // Set the blend intensity in the material 
        _mat.SetVector("_focalOffset", ca.focalOffset.value);
        _mat.SetVector("_radius", ca.radius.value);
        _mat.SetFloat("_intensity", ca.intensity.value);
        _mat.SetFloat("_redOffset", ca.redOffset.value);
        _mat.SetFloat("_greenOffset", ca.greenOffset.value);
        _mat.SetFloat("_blueOffset", ca.blueOffset.value);
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
