using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RetroRenderPassFeature : ScriptableRendererFeature
{
    private RetroPass retroPass;
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(retroPass);
    }

    public override void Create()
    {
        retroPass = new RetroPass();
        name = "Retro";
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
