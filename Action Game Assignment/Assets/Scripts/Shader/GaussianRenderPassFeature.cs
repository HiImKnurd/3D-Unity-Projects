using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GaussianRenderPassFeature : ScriptableRendererFeature
{
    private GaussianPass gaussianPass;

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(gaussianPass);
    }

    public override void Create()
    {
        gaussianPass = new GaussianPass();
        name = "Gaussian Blur";
    }
}
