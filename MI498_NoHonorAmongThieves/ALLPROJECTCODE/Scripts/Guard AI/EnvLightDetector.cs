using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// The center of env light detector, return position detected to guard
/// </summary>
public class EnvLightDetector : MonoBehaviour
{
    public List<Vector3> lightDetectors = new List<Vector3>();

    public float frequency = 30.0f;  
    
    // the scan timer itself
    private float scanTimer;
    private float scanInterval ;
    // Start is called before the first frame update
    void Start()
    {
        scanInterval = 1.0f / frequency;
        
    }

    // Update is called once per frame
    void Update()
    {

        // Handles frequency timing
        scanTimer -= Time.deltaTime;
        if (scanTimer < 0)
        {
            scanTimer += scanInterval;
            GetObjectsUnderLight();
        }
        
    }

    /// <summary>
    /// Check everything is explored under light
    /// </summary>
    public void GetObjectsUnderLight()
    {
        lightDetectors.Clear();
        foreach (Transform child in transform)
        {
            lightDetectors.Add(child.position);

        }
        
    }
}
