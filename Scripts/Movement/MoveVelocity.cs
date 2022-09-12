using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveVelocity : MonoBehaviour, IMoveVelocity
{
    [SerializeField] private float moveMultiplier;

    private Vector3 velocityVector;
    private Rigidbody2D rb2D;

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>(); // Gets the RB component in the GO
    }


    /// <summary>
    /// Sets the velocity vector of the object
    /// </summary>
    /// <param name="newVelocityVector"></param>
    public void SetVelocity(Vector3 newVelocityVector) 
    {
        this.velocityVector = newVelocityVector; // Sets the velocity vector itself
    }

    /// <summary>
    /// Using the physics engine, it applies the velocity to the RB
    /// Allows for impact / collisions 
    /// </summary>
    private void FixedUpdate()
    {
        rb2D.velocity = velocityVector * moveMultiplier; // Sets the magnitude of the velocity vector
    }
}
