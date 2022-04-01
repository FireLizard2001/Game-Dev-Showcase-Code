using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Rocket : MonoBehaviour
{
    [Header("Rocket Values")]

    [Tooltip("Explosion Prefab")]
    [SerializeField] private GameObject explosion;

    [Tooltip("Rocket Speed")]
    [SerializeField] private float rocketSpeed;

    [Tooltip("Rocket Audio")]
    [SerializeField] private GameObject rocketAudio;

    public GameObject rocketCamPrefab;

    private GameObject rocketCamera;

    // Ref to rigidbody
    Rigidbody body;
    // Is the rocket exploded?
    bool isExploded = false;

    private void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!isExploded)
        {
            Explode();
        }
    }

    IEnumerator CameraTransfer()
    {
        Debug.Log("Rocket Exploded");
        GameManager.instance.RocketExploded();
        GameManager.instance.SwitchToSceneCamera();

        yield return new WaitForSeconds(GameManager.instance.transitionSpeed);

        Debug.Log("Rocket Destroyed");
        Destroy(this.gameObject);
    }

    /// <summary>
    /// Prepares rocket for movement
    /// </summary>
    public void Fire()
    {
        gameObject.GetComponent<BoxCollider>().enabled = true;
        transform.parent.parent.parent.GetComponent<CapsuleCollider>().enabled = false;
        transform.parent = null;
        this.GetComponent<Rigidbody>().isKinematic = false;
        // Shoots rocket forward based on axis
        body.velocity = -transform.right * rocketSpeed;


        rocketCamera = Instantiate(rocketCamPrefab, this.transform.position, this.transform.rotation);
        rocketCamera.transform.parent = gameObject.transform;

        //Explode rocket if it misses after " " seconds
        StartCoroutine(SelfDestruct(3));

        IEnumerator SelfDestruct(int secs)
        {
            yield return new WaitForSeconds(secs);
            Explode();
        }
    }

    private void Explode()
    {
        isExploded = true;

        GameObject newExp = Instantiate(explosion);
        GameObject audio = Instantiate(rocketAudio);
        Destroy(audio, 1);
        newExp.transform.position = this.transform.position;

        Crosshair.instance.Enabled();

        /*
        NOTE: Need to place a coroutine here to turn off model for rocket,
        then wait for camera to transition before destroying the rocket.
        */
        GetComponent<BoxCollider>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;
        rocketCamera.GetComponentInChildren<CinemachineVirtualCamera>().Priority = 0;
        StartCoroutine("CameraTransfer");
    }

    public void SelfDestruct()
    {
        Fire();
        Explode();
        GameManager.instance.HitGround();
    }
}
