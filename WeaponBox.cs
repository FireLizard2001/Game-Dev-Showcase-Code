using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class which manages the weapon box objects in game
/// </summary>
public class WeaponBox : MonoBehaviour
{

    [Header("Gun")]
    [Tooltip("The possible guns inside the box.")]
    public List<GameObject> gunPrefabs;

    private bool canInteract = false;

    public PlayerInteract interactor;

    public int timeBeforeWeaponAppears = 2;
    public float timeBeforeWeaponAvailable = 0.62f;
    public GameObject smokeEffect;
    private bool smokeEffected = false;

    private void Update()
    {
        if (canInteract)
        {
            if (interactor.InteractStarted && interactor.gameObject.GetComponent<PlayerControllerPlus>().isAlive)
            {
                /// Choose a gun to spawn
                Destroy(gameObject.GetComponent<TimedObjectDestroyer>());
                if (!smokeEffected)
                    GameObject.Instantiate(smokeEffect, transform.position, Quaternion.identity, null);
                smokeEffected = true;
                StartCoroutine(CountdownToStart());
            }
        }
    }

    /// <summary>
    /// Description:
    /// Function to spawn the weapon that is currently inside this given box
    /// and then destroy this gameobject.
    /// Input: 
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    public void SpawnGun(GameObject gunPrefab)
    {
        StartCoroutine(CountdownToGrab(gunPrefab));

        // Destruction/Opening of weapon box effect

        // Replace this destroy call with a deactivate call and call the timer to replenish the box
    }

    /// <summary>
    /// Description:
    /// Standard Unity function called when a Collider2D hits another Collider2D (non-triggers)
    /// Input:
    /// Collision2D collision
    /// Return:
    /// void (no return)
    /// </summary>
    /// <param name="collision">The Collider2D that has hit this Collider2D</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {

            interactor = collision.gameObject.GetComponent<PlayerInteract>();
            canInteract = true;
        }
    }

    /// <summary>
    /// Description:
    /// Standard Unity function called when a Collider2D exits another Collider2D (non-triggers)
    /// Input:
    /// Collision2D collision
    /// Return:
    /// void (no return)
    /// </summary>
    /// <param name="collision">The Collider2D that has hit this Collider2D</param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            interactor = null;
            canInteract = false;
        }
    }

    IEnumerator CountdownToStart()
    {
        while (timeBeforeWeaponAppears > 0)
        {
            //countdownDisplay.text = countdownTime.ToString();
            //GameObject.Instantiate(countdownSound, Vector3.zero, Quaternion.identity, null);

            yield return new WaitForSeconds(1f);

            timeBeforeWeaponAppears--;
        }

        //countdownDisplay.text = "GO!";
        //GameObject.Instantiate(goSound, Vector3.zero, Quaternion.identity, null);
        SpawnGun(gunPrefabs[Random.Range(0, gunPrefabs.Count)]);
        yield return new WaitForSeconds(1f);

        //countdownDisplay.gameObject.SetActive(false);
    }
    IEnumerator CountdownToGrab(GameObject gunP)
    {
        while (timeBeforeWeaponAvailable > 0)
        {
            //countdownDisplay.text = countdownTime.ToString();
            //GameObject.Instantiate(countdownSound, Vector3.zero, Quaternion.identity, null);

            yield return new WaitForSeconds(timeBeforeWeaponAvailable);

            timeBeforeWeaponAvailable = 0;
        }

        //countdownDisplay.text = "GO!";
        //GameObject.Instantiate(goSound, Vector3.zero, Quaternion.identity, null);
        Instantiate(gunP, this.transform.position, this.transform.rotation);
        //ObjectSpawner.spawnCoords.Add(this.transform.parent.gameObject.transform);
        Destroy(gameObject);

        //countdownDisplay.gameObject.SetActive(false);
    }
}
