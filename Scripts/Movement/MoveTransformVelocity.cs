using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTransformVelocity : MonoBehaviour, IMoveVelocity
{
    [SerializeField] private float moveMultiplier; 

    private Vector3 velocityVector;


    /// <summary>
    /// Sets the velocity vector of the object
    /// </summary>
    /// <param name="newVelocityVector"></param>
    public void SetVelocity(Vector3 newVelocityVector)
    {
        this.velocityVector = newVelocityVector; // Sets the velocity vector itself
    }

    /// <summary>
    /// Applies the new velocity vector with the multiplier to the object
    /// Uses Transform based velocity to not cause collisions 
    /// </summary>
    private void FixedUpdate()
    {
        transform.position += velocityVector * moveMultiplier * Time.deltaTime;
    }
}
