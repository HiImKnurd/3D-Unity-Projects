using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DoGRenderPassFeature : ScriptableRendererFeature
{
    private DoGPass dogPass;
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(dogPass);
    }

    public override void Create()
    {
        dogPass = new DoGPass();
        name = "Difference of Gaussians";
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
