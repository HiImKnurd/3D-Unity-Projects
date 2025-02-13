using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenuForRenderPipeline("Custom/Chromatic Aberration"
    , typeof(UniversalRenderPipeline))]
public class ChromaticAberrationPostProcess : VolumeComponent, IPostProcessComponent
{
    public Vector2Parameter focalOffset = new Vector2Parameter(new Vector2(0f, 0f));
    public Vector2Parameter radius = new Vector2Parameter(new Vector2(1f, 1f));
    public FloatParameter intensity = new ClampedFloatParameter(1.0f, 0f, 5f);
    public FloatParameter redOffset = new ClampedFloatParameter(0f, -10f, 10f);
    public FloatParameter blueOffset = new ClampedFloatParameter(0f, -10f, 10f);
    public FloatParameter greenOffset = new ClampedFloatParameter(0f, -10f, 10f);
    public bool IsActive()
    {
        return (intensity.value > 0f) && active;
    }
    public bool IsTileCompatible() => true;

}
