using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostManager : MonoBehaviour
{
    public static PostManager instance = null;

    [Header("Guard Variables")]
    public float maxVignetteIntensity = 0.6f;
    public float maxLensIntensity = -0.2f;
    public float maxChromaticIntensity = 0.7f;
    public float lerpTime = 0.5f;

    [Header("Color Values")]
    public Color normalColor = Color.black;
    public Color depositingColor;

    #region Post Variables

    private Volume volume;
    private Vignette vig;
    private LensDistortion lens;
    private ChromaticAberration aber;
    private SplitToning split;

    #endregion

    public bool vigOn = true;
    public bool stunOn = false;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

            volume = GetComponent<Volume>();

            if (volume != null)
            {
                volume.profile.TryGet(out vig);
                volume.profile.TryGet(out lens);
                volume.profile.TryGet(out aber);
                volume.profile.TryGet(out split);
            }
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void ToggleVig(bool on)
    {
        vigOn = on;

        StopAllCoroutines();
        
        if (on)
        {
            StartCoroutine("SlideOn");
        }
        else if (!on)
        {
            StartCoroutine("SlideOff");
        }
    }

    public void ToggleStun(bool on)
    {
        stunOn = on;

        StopAllCoroutines();

        if (on)
        {
            StartCoroutine("StunOn");
        }
        else if (!on)
        {
            StartCoroutine("StunOff");
        }
    }

    public void VignetteColor(bool isDepositing)
    {
        vig.color.value = isDepositing ? depositingColor : normalColor;
    }

    IEnumerator SlideOff()
    {
        float elapsedTime = 0;

        while (elapsedTime < lerpTime)
        {
            vig.intensity.value = Mathf.Lerp(vig.intensity.value, 0, elapsedTime / lerpTime);
            lens.intensity.value = Mathf.Lerp(lens.intensity.value, maxLensIntensity, elapsedTime / lerpTime);
            aber.intensity.value = Mathf.Lerp(aber.intensity.value, maxChromaticIntensity, elapsedTime / lerpTime);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        yield return new WaitForSeconds(2f);
    }

    IEnumerator SlideOn()
    {
        float elapsedTime = 0;

        while (elapsedTime < lerpTime)
        {
            vig.intensity.value = Mathf.Lerp(vig.intensity.value, maxVignetteIntensity, elapsedTime / lerpTime);
            lens.intensity.value = Mathf.Lerp(lens.intensity.value, 0, elapsedTime / lerpTime);
            aber.intensity.value = Mathf.Lerp(aber.intensity.value, 0, elapsedTime / lerpTime);

            elapsedTime += Time.deltaTime;

            yield return null;
        }
    }

    IEnumerator StunOff()
    {
        float elapsedTime = 0;

        while (elapsedTime < lerpTime)
        {
            vig.intensity.value = Mathf.Lerp(vig.intensity.value, maxVignetteIntensity, elapsedTime / lerpTime);
            aber.intensity.value = Mathf.Lerp(aber.intensity.value, 0, elapsedTime / lerpTime);

            elapsedTime += Time.deltaTime;

            yield return null;
        }
    }

    IEnumerator StunOn()
    {
        float elapsedTime = 0;

        while (elapsedTime < lerpTime)
        {
            vig.intensity.value = Mathf.Lerp(vig.intensity.value, 1, elapsedTime / lerpTime);
            aber.intensity.value = Mathf.Lerp(aber.intensity.value, 0.7f, elapsedTime / lerpTime);

            elapsedTime += Time.deltaTime;

            yield return null;
        }
    }
}
