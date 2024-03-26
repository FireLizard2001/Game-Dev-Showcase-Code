using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFlashLight : MonoBehaviour
{
    public float swingSpeed = 5.0f;
    public float maxSwingAngle = 10.0f;
    public float toggleDelayTime = 2f;

    private Quaternion initialRotation;
    private Transform playerTransform;
    private Light flashLight;
    
    void Start()
    {
        initialRotation = transform.localRotation;
        playerTransform = gameObject.transform.parent.parent.gameObject.GetComponent<Transform>();
        flashLight = GetComponent<Light>();
            // GetComponentInParent<Transform>().GetComponentInParent<>();
    }

    void Update()
    {
        
        if (playerTransform != null)
        {
            float xInput = Input.GetAxis("Vertical");
            float zInput = Input.GetAxis("Horizontal");
            //initialRotation = transform.localRotation;

            Vector3 inputVector = new Vector3(xInput, 0, zInput);

            if (inputVector.magnitude > 0.1f)
            {
                float angle = Mathf.Sin(Time.time * swingSpeed/2f) * maxSwingAngle;
                // transform.localRotation = initialRotation * Quaternion.Euler(angle, 0, angle);
                
                // float angleX = Mathf.Sin(Time.time * swingSpeed) * maxSwingAngle * Mathf.Abs(zInput);
                // float angleZ = Mathf.Sin(Time.time * swingSpeed) * maxSwingAngle * Mathf.Abs(xInput);
                // transform.localRotation = initialRotation * Quaternion.Euler(0, 0, angleX) * Quaternion.Euler(angleZ, 0, 0);
                
                float angleY = Mathf.Sin(Time.time * swingSpeed) * maxSwingAngle;
                transform.localRotation = initialRotation * Quaternion.Euler(angle, angleY, 0);

            }
            else
            {
                transform.localRotation = Quaternion.Lerp(transform.localRotation, initialRotation, Time.deltaTime * swingSpeed);
            }
        }
    }

    public void RespawnLight()
    {
        StartCoroutine("ToggleDelay");
    }

    IEnumerator ToggleDelay()
    {
        flashLight.enabled = false;

        yield return new WaitForSeconds(toggleDelayTime);

        flashLight.enabled = true;
    }
}