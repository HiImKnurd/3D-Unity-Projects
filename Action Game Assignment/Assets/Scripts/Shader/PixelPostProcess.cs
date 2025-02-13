using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenuForRenderPipeline("Custom/Pixelization"
    , typeof(UniversalRenderPipeline))]
public class PixelPostProcess : VolumeComponent, IPostProcessComponent
{
    [Tooltip("Affects the pixel count")]
    public FloatParameter pixelSize = new ClampedFloatParameter(0f, 0f, 20f);
    public bool IsActive()
    {
        return (pixelSize.value > 0) && active;
    }

    public bool IsTileCompatible() => true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
