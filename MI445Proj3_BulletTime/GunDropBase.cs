using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunDropBase : MonoBehaviour
{

    private bool canInteract = false;

    [Header("Weapon Info")]
    [Tooltip("The ammo this weapon has when new.")]
    public int startingAmmo;
    [Tooltip("Weapon reload time.")]
    public float shotDelay;
    [Tooltip("The recoil the player shooting this gun takes.")]
    public float shotRecoil;
    [Tooltip("The sound played when this gun is shot.")]
    public AudioSource fireBulletAudio;
    [Tooltip("The bullet this gun will shoot.")]
    public GameObject bullet;
    [Tooltip("What this weapon is.")]
    public PlayerWeapon.Weapon weapon;
    private PlayerInteract interactor;
    private PlayerWeapon weaponScript;
    public GameObject gunPickupSound;

    // Update is called once per frame
    void Update()
    {
        IdleMovement();
        if (canInteract)
        {
            if (interactor.InteractStarted && interactor.gameObject.GetComponent<PlayerControllerPlus>().isAlive)
            {
                WeaponAttributes();
            }
        }
    }

    /// <summary>
    /// Description:
    /// Function to lerp the gun object up and down to add juice.
    /// Input: 
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    private void IdleMovement()
    {

    }

    /// <summary>
    /// Description:
    /// Function to set the attrbutes within this players PlayerWeapon script
    /// </summary>
    private void WeaponAttributes()
    {
        weaponScript = interactor.gameObject.GetComponent<PlayerWeapon>();
        weaponScript.SetCurrentBullent(bullet);
        weaponScript.SetWeaponAmmo(startingAmmo);
        weaponScript.SetShotDelay(shotDelay);
        weaponScript.SetRecoil(shotRecoil);
        weaponScript.isMelee = false;
        GivePlayerGun(interactor, weaponScript);
    }

    /// <summary>
    /// Description:
    /// Virtual Function to give the player a given gun.
    /// Override in given gun class
    /// </summary>
    public virtual void GivePlayerGun(PlayerInteract playerInteract, PlayerWeapon playerWeapon)
    {
        playerWeapon.SetWeapon(weapon);
        // Make pickup effect
        GameObject.Instantiate(gunPickupSound, transform.position, Quaternion.identity, null);
        StuffSpawner.weaponBoxBuckets[gameObject.GetComponent<SinusoidalMover>().initialPosition] = false;
        Destroy(this.gameObject);
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

}
