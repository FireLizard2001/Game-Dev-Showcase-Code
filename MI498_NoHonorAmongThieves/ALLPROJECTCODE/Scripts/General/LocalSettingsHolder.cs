using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalSettingsHolder : MonoBehaviour
{

    public static LocalSettingsHolder instance = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            DestroyImmediate(this.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
