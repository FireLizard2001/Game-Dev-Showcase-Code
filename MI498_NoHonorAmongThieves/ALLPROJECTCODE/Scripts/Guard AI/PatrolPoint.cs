using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolPoint : MonoBehaviour
{
    [Header("Point Values")]

    [Tooltip("The time a guard will spend at this point")]
    public float waitTime = 3f;

    [Header("Testing Values")]

    [Tooltip("Should the sphere be visible while playing?")]
    [SerializeField] private bool visibleInGame = false;

    private void Start()
    {
        GetComponent<Renderer>().enabled = visibleInGame;
    }
}
