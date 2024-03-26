using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedDestructor : MonoBehaviour
{
    public float timeTilDestroy = 3f;

    void Start()
    {
        Destroy(this.gameObject, timeTilDestroy);
    }
}
