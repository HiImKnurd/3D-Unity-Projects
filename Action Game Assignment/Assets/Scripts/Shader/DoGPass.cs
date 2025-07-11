#pragma warning disable CS0618 // Type or member is obsolete
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DoGPass : ScriptableRenderPass
{
    private Material _material;
    //private Material _mat1, _mat2;
    private DifferenceOfGaussiansPostProcess dogPostProcess;
    int tempid = Shader.PropertyToID("_MainTex"); // Property ID for temporary render target
    RenderTargetIdentifier src, dst;

    public DoGPass()
    {
        if (!_material) _material = CoreUtils.CreateEngineMaterial(
            "Custom Post-Processing/Gaussian Blur");

        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        dogPostProcess = VolumeManager.instance.stack.GetComponent<DifferenceOfGaussiansPostProcess>();
        RenderTextureDescriptor desc = renderingData.cameraData.cameraTargetDescriptor;
        tempid = Shader.PropertyToID("_MainTex");
        cmd.GetTemporaryRT(tempid, desc);
        src = renderingData.cameraData.renderer.cameraColorTargetHandle;
        dst = new RenderTargetIdentifier(tempid);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (dogPostProcess == null || !dogPostProcess.IsActive()) return;

        CommandBuffer commandBuffer = CommandBufferPool.Get("Custom/Difference of Gaussians");

        // Blur effect properties
        int gridSize1 = Mathf.CeilToInt(dogPostProcess.blurIntensity1.value * 6f);
        if (gridSize1 % 2 == 0) gridSize1++;
        int gridSize2 = Mathf.CeilToInt(dogPostProcess.blurIntensity2.value * 6f);
        if (gridSize2 % 2 == 0) gridSize2++;

        //_material.SetInteger("_kernelSize", dogPostProcess.kernelSize.value);
        _material.SetFloat("_threshhold", dogPostProcess.threshhold.value);
        _material.SetFloat("_tau", dogPostProcess.tau.value);
        _material.SetFloat("_phi", dogPostProcess.phi.value);
        _material.SetInteger("_invert", dogPostProcess.invert.value ? 1 : 0);
        _material.SetInteger("_hyperbolic", dogPostProcess.hyperbolic.value ? 1 : 0);

        // First Gaussian
        _material.SetFloat("_Spread", dogPostProcess.blurIntensity1.value);
        _material.SetInteger("_GridSize", gridSize1);
        var gauss1 = RenderTexture.GetTemporary(Screen.width, Screen.height);
        Blit(commandBuffer, src, dst, _material, 0);
        Blit(commandBuffer, dst, gauss1, _material, 1); // Store the blurred texture

        // Second Gaussian
        _material.SetFloat("_blurIntensity", dogPostProcess.blurIntensity2.value);
        _material.SetInteger("_GridSize", gridSize2);
        var gauss2 = RenderTexture.GetTemporary(Screen.width, Screen.height);
        Blit(commandBuffer, src, dst, _material, 0);
        Blit(commandBuffer, dst, gauss2, _material, 1); // Store the blurred texture

        // Difference of Gaussians
        _material.SetTexture("_SecondTex", gauss2); // Send second blurred texture
        Blit(commandBuffer, gauss1, src, _material, 2); // Render the difference

        // Release temporary textures
        RenderTexture.ReleaseTemporary(gauss1);
        RenderTexture.ReleaseTemporary(gauss2); 

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
