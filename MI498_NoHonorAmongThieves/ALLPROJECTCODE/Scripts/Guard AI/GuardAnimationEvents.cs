using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardAnimationEvents : MonoBehaviour
{
    public bool doPlay = true;
    public void PlayFootstep()
    {
        if (doPlay)
        {
            AkSoundEngine.PostEvent("guardFootsteps", this.gameObject);
        }
    }
}
