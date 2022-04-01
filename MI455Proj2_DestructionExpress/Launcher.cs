using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    [Header("Launcher Values")]

    [Tooltip("Rocket Prefab")]
    [SerializeField] private GameObject rocket;

    [Tooltip("Is the rocket reloadable?")]
    [SerializeField] private bool isReloadable;

    [Tooltip("Rocket reload rate")]
    [SerializeField] private float reloadRate;

    // Reference to the personal rocket, not prefab
    private GameObject personalRocket;

    private void Start()
    {
        // Grabs initial personal rocket
        personalRocket = transform.GetChild(1).gameObject;
    }

    public void Shoot()
    {
        // Only shoots when rocket is loaded
        if (personalRocket != null)
        {
            personalRocket.GetComponent<Rocket>().Fire();
            personalRocket = null;

            if (isReloadable)
            {
                StartCoroutine("Reload");
            }
        }
    }

    IEnumerator Reload()
    {
        yield return new WaitForSeconds(reloadRate);

        personalRocket = Instantiate(rocket, transform);
    }
}
