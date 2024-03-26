using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    #region Inspector Values
    [Header("Spawn Values")]

    [Tooltip("The list of spawnpoints a player can take")]
    [SerializeField] private List<Transform> spawnPoints;
    #endregion

    #region Spawn Values

    // The amount of spawns taken
    private int spawnsTaken = 0;

    public static SpawnManager instance = null;

    #endregion

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// Grabs the next available spawn
    /// </summary>
    /// <returns>A reference to the spawn transform</returns>
    public Transform GrabSpawn()
    {
        spawnsTaken++;

        if (spawnPoints[spawnsTaken - 1] == null)
        {
            Debug.Log("Missing spawn points!");
        }

        return spawnPoints[spawnsTaken - 1];
    }

    /// <summary>
    /// Function to return a the first spawn point to use as a spectator sport for escaped players
    /// </summary>
    /// <returns></returns>
    public Transform GetSpectatorSpawn()
    {
        return spawnPoints[0];
    }
}
