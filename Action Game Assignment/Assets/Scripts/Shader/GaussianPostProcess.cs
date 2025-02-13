using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenuForRenderPipeline("Custom/Gaussian Blur"
    , typeof(UniversalRenderPipeline))]
public class GaussianPostProcess : VolumeComponent, IPostProcessComponent
{
    [Tooltip("Standard deviation of the gaussian blur")]
    public FloatParameter blurIntensity = new ClampedFloatParameter(0f, 0f, 100f);
    public bool IsActive()
    {
        return (blurIntensity.value > 0f) && active;
    }

    public bool IsTileCompatible() => true;
}
