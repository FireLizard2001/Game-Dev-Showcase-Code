using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceParent : MonoBehaviour

{

    [SerializeField] private ResourceSO resourceSO;
    /// <summary>
    /// Parent/base class for the resources.
    /// </summary>
    /// <returns></returns>
    public string GET_RESOURCE_NAME() 
    {
        return resourceSO.NAME;
    }

    public int GET_MAX_STACK() 
    {
        return resourceSO.MAX_STACK_SIZE;
    }

    public void SET_SO(ResourceSO new_SO) 
    {
        resourceSO = new_SO;
    }

}
