using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
//using UnityEngine.Rendering.PostProcessing;



public class PPManager : MonoBehaviour
{
    [SerializeField] Volume globalVolume;
    private DifferenceOfGaussiansPostProcess DoG;
    private BWPostProcess bw;
    private GaussianPostProcess blur;
    [SerializeField][Range(0f, 0.5f)] float _hitstopthreshhold = 0.25f;
    [SerializeField] PlayerController playerController;
    // Start is called before the first frame update
    void Start()
    {
        if(globalVolume.profile.TryGet<DifferenceOfGaussiansPostProcess>(out DoG))
        {
            DoG.active = false;
        }
        if(globalVolume.profile.TryGet<BWPostProcess>(out bw))
        {
            bw.blendIntensity.value = 0f;
        }
        if(globalVolume.profile.TryGet<GaussianPostProcess>(out blur))
        {
            blur.active = false;
            blur.blurIntensity.value = 0f;
        }    
    }

    // Update is called once per frame
    void Update()
    {
        if(playerController._isDead) 
        {
            if (!bw.IsActive())
            {
                //bw.active = true;
                bw.blendIntensity.value = 1f;
            }
            blur.active = true;
            while(blur.blurIntensity.value < 10) blur.blurIntensity.value += 1f * Time.deltaTime;
        }
    }
    public void StartHitstop(float duration, bool impact)
    {
        StartCoroutine(HitStop(duration, impact));
    }

    IEnumerator HitStop(float duration, bool impact)
    {
        yield return new WaitForSecondsRealtime(duration * 0.2f);
        if (impact) DoG.active = true;
        Time.timeScale = 0f;
        DoG.threshhold.value = _hitstopthreshhold;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
        DoG.active = false;
    }
}
