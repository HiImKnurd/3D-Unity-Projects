using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenuForRenderPipeline("Custom/Difference of Gaussians"
    , typeof(UniversalRenderPipeline))]
public class DifferenceOfGaussiansPostProcess : VolumeComponent, IPostProcessComponent
{
    [Tooltip("Blur factor for first Gaussian")]
    public FloatParameter blurIntensity1 = new ClampedFloatParameter(0f, 0f, 100f);
    [Tooltip("Blur factor for second Gaussian, only set this > 0 if you want blur")]
    public FloatParameter blurIntensity2 = new ClampedFloatParameter(0f, 0f, 100f);
    //[Tooltip("Gaussian kernel size")]
    //public IntParameter kernelSize = new ClampedIntParameter(0, 0, 100);
    [Tooltip("Greater than threshhold is white")]
    public FloatParameter threshhold = new ClampedFloatParameter(0.005f, -1f, 1f);
    [Header("Values for DoG operator")]
    [Tooltip("Precision of edge detection")]
    public FloatParameter tau = new ClampedFloatParameter(1f, 0.01f, 5f);
    [Tooltip("Contrast between threshholds")]
    public FloatParameter phi = new ClampedFloatParameter(1f, 0.1f, 100f);
    public BoolParameter invert = new BoolParameter(false);
    [Tooltip("When off, will render in strictly 2 colors, unaffected by phi")]
    public BoolParameter hyperbolic = new BoolParameter(true);
    public bool IsActive()
    {
        return (blurIntensity1.value > 0) && active;
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
