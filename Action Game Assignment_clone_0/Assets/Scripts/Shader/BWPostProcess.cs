using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenuForRenderPipeline("Custom/B&W Post-Porcessing"
    , typeof(UniversalRenderPipeline))]
public class BWPostProcess : VolumeComponent, IPostProcessComponent
{
    public FloatParameter blendIntensity = new FloatParameter(1.0f);
    public bool IsActive()
    {
        return (blendIntensity.value > 0f) && active;
    }
    public bool IsTileCompatible() => true;

}
