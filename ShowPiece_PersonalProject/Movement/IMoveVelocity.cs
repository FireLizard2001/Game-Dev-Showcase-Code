using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Interface to handle differnet types of movement
/// Manages transfom based movement and physics/rb velocity movement
/// </summary>
public interface IMoveVelocity 
{
    void SetVelocity(Vector3 velocityVector);
}
