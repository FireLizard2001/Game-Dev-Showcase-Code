using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    #region Spawn Variables

    // Reference to this player's spawn point
    private Transform spawn;

    #endregion

    private void Start()
    {
        if (SpawnManager.instance != null)
        {
            spawn = SpawnManager.instance.GrabSpawn();
        }
    }

    /// <summary>
    /// Transports the player to their spawn position
    /// </summary>
    public void Respawn()
    {
        GetComponent<CharacterController>().enabled = false;
        transform.position = spawn.position;
        GetComponent<CharacterController>().enabled = true;
    }


}
