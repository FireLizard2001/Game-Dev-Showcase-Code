using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundTrigger : MonoBehaviour
{
    #region Inspector Values
    [Header("Trigger Values")]

    [Tooltip("Should the sound be triggered on start?")]
    [SerializeField] private bool triggerOnStart = true;

    [Tooltip("Time in which the sound should remain triggered")]
    [SerializeField] private float echoTime = 0.5f;

    [Tooltip("Should the object be destroyed after echo?")]
    [SerializeField] private bool destroyOnEnd = true;

    #endregion

    // Timer for resetting the trigger
    private float resetTime = 0f;

    // Trigger logic bool for echo time
    private bool trigger = false;

    private void Start()
    {
        if (triggerOnStart) { TriggerSound(); }
    }

    private void Update()
    {
        // Echo time for the trigger set
        if (trigger)
        {
            resetTime += Time.deltaTime;

            if (resetTime > echoTime)
            {
                trigger = false;
                resetTime = 0;

                if (destroyOnEnd)
                {
                    Destroy(this.gameObject);
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // If guard is inside the radius and sound is triggered, make the guard come
        if (other.CompareTag("Guard") && trigger)
        {
            other.GetComponent<Pathing_Network>().TriggerInvestigation(this.transform, true);
            trigger = false;
            Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// Plays the trigger sound
    /// </summary>
    public void TriggerSound()
    {
        trigger = true;
    }
}
