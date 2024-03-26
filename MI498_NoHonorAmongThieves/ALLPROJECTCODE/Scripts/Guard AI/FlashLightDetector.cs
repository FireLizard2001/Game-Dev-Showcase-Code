using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashLightDetector : MonoBehaviour
{
    
    public float radius = 2.0f;  // this will be a sphere radius by ray cast
    public float distance = 10.0f;  // will set to flashlight range
    public List<GameObject> objectDetected = new List<GameObject>();

    public Light flashLightRef = null;
    // Start is called before the first frame update
    void Start()
    {
        if (flashLightRef != null)
        {
            distance = flashLightRef.range;
        }
        
    }

    // Raycast and update the objects hit
    void Update()
    {
        
        var forwardRay = new Ray(transform.position, transform.forward);
        var hitChecks = Physics.SphereCastAll(forwardRay, radius, distance);

        objectDetected.Clear();
        Debug.DrawRay(forwardRay.origin, forwardRay.direction * distance, Color.yellow);
        //Gizmos.DrawWireSphere(forwardRay.origin+ forwardRay.direction *distance, radius);
        
        foreach (var ray in hitChecks)
        {
            if (ray.collider.tag == "Player")
            {
                objectDetected.Add(ray.transform.gameObject);
            }
            
            
        }
        
    }


}
