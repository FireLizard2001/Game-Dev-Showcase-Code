using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{

    // List of available weapons
    public enum Weapon
    {
        Melee, Pistol, RocketLauncher, SniperRifle, Magnum, Shotgun, AutomaticRifle
        //, Laser, AssaultRifle, Flamethrower
    };

    private Weapon activeWeapon = Weapon.Melee;

    public AudioSource meleeSound;

    public GameObject meleeBullet;

    private int weaponAmmo;
    private float shotDelay;
    private float elapsedTime;
    private float recoilImpact;
    private GameObject currentBullet;
    private AudioSource fireBulletAudio;
    [Header("Dice Effects -- DO NOT SET MANUALLY")]
    public float shotDelayMultiplier;
    public float weaponAmmoMultiplier;
    public float recoilMultiplier;
    private PlayerInteract interactor;
    private Rigidbody2D rb;

    public bool isMelee = false;
    [Tooltip("Order is melee pistol magnum shotgun auto sniper RL")]
    public List<GameObject> shootSounds;
    public Animator m_animator;

    private void Start()
    {
        shotDelayMultiplier = 1;
        weaponAmmoMultiplier = 1;
        recoilMultiplier = 1;
        interactor = this.gameObject.GetComponent<PlayerInteract>();
        rb = this.gameObject.GetComponent<Rigidbody2D>();
        //Get the Animator attached to the GameObject you are intending to animate.
        m_animator = gameObject.transform.Find("PlayerRep/CharAnim").GetComponent<Animator>();
        SetToMelee();
    }

    //Getters and Setters
    public void SetWeapon(Weapon weapon)
    {
        activeWeapon = weapon;
        elapsedTime = shotDelay;

        GameManagerPlus.UpdateUIElements();
    }
    public Weapon GetWeapon()
    {
        return activeWeapon;
    }
    public void SetWeaponAmmo(int ammo)
    {
        weaponAmmo = Mathf.RoundToInt(ammo * weaponAmmoMultiplier);
    }
    public void SetMelee()
    {
        SetToMelee();
    }
    public int GetWeaponAmmo()
    {
        return weaponAmmo;
    }
    public void SetShotDelay(float seconds)
    {
        shotDelay = seconds;

    }
    public float GetShotDelay()
    {
        return shotDelay;
    }
    public void SetBulletFireAudio(AudioSource fireBullet)
    {
        fireBulletAudio = fireBullet;
    }
    public void SetCurrentBullent(GameObject bullet)
    {
        currentBullet = bullet;
    }
    public void SetRecoil(float recoil)
    {
        recoilImpact = recoil * recoilMultiplier;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!interactor.gameObject.GetComponent<GrabController>().isCarrying && interactor.gameObject.GetComponent<PlayerControllerPlus>().isAlive && (interactor.shootStarted || interactor.shootHeld))
        {
            WeaponHandler();
        }

        /// Add code to check if weapon thrown here

        elapsedTime += Time.deltaTime;
    }

    /// <summary>
    /// Function to handle the player attacks based on equiped weapon
    /// </summary>
    private void WeaponHandler()
    {
        if ((weaponAmmo > 0 || isMelee) && elapsedTime >= shotDelay * shotDelayMultiplier)
        {
            elapsedTime = 0;
            // Add Recoil
            rb.AddForce(Vector2.down * -recoilImpact * Mathf.Sin(5f * Mathf.PI / 180f));
            rb.AddForce(transform.right * -recoilImpact);

            GunShoot();


            if (GetWeapon() != Weapon.Melee)
            {
                weaponAmmo--;
                GameManagerPlus.UpdateUIElements();
            }

        }
        else if (weaponAmmo == 0 && GetWeapon() != Weapon.Melee)
        {

            // Throw gun
            // Conditional if gun not empty, throw on ground
            // Conditional if gun empty despawn after x seconds, disallow pickup

            // Set this to the melee attack
            SetWeapon(Weapon.Melee);
            StartCoroutine(CORoutine());

        }
    }
    IEnumerator CORoutine()
    {
        yield return new WaitForSeconds(0.5f);
        SetToMelee();
    }

    private void SetToMelee()
    {
        fireBulletAudio = meleeSound;
        SetCurrentBullent(meleeBullet);
        SetShotDelay(0.5f);
        SetRecoil(0);
        isMelee = true;
        SetWeapon(Weapon.Melee);
    }

    private void GunShoot()
    {
        // Trigger fireBulletAudio
        switch (activeWeapon)
        {
            case (Weapon.Melee):
                GameObject.Instantiate(shootSounds[0], transform.position, Quaternion.identity, null);
                m_animator.SetTrigger("Punch");
                break;
            case (Weapon.Pistol):
                GameObject.Instantiate(shootSounds[1], transform.position, Quaternion.identity, null);
                break;
            case (Weapon.Magnum):
                GameObject.Instantiate(shootSounds[2], transform.position, Quaternion.identity, null);
                break;
            case (Weapon.Shotgun):
                GameObject.Instantiate(shootSounds[3], transform.position, Quaternion.identity, null);
                break;
            case (Weapon.AutomaticRifle):
                GameObject.Instantiate(shootSounds[4], transform.position, Quaternion.identity, null);
                break;
            case (Weapon.SniperRifle):
                GameObject.Instantiate(shootSounds[5], transform.position, Quaternion.identity, null);
                break;
            case (Weapon.RocketLauncher):
                GameObject.Instantiate(shootSounds[6], transform.position, Quaternion.identity, null);
                break;
        }
        if (activeWeapon == Weapon.Pistol || activeWeapon == Weapon.AutomaticRifle)
        {
            bool facingR = this.gameObject.GetComponent<PlayerControllerPlus>().facingRight;
            int adder = facingR ? 0 : 180;
            float randSpread = Random.Range(-3f, 3f);
            var thisBullet = Instantiate(currentBullet, transform.position + (transform.forward * 2), Quaternion.Euler(0, 0, randSpread + adder));
            thisBullet.GetComponent<BulletBase>().SetPlayer(this.transform.gameObject);
        }
        else if (activeWeapon != Weapon.Shotgun)
        {
            var thisBullet = Instantiate(currentBullet, transform.position + (transform.forward * 2), transform.rotation);
            thisBullet.GetComponent<BulletBase>().SetPlayer(this.transform.gameObject);
        }
        else
        {
            bool facingR = this.gameObject.GetComponent<PlayerControllerPlus>().facingRight;
            int adder = facingR ? 0 : 180;
            var thisBullet = Instantiate(currentBullet, transform.position + (transform.forward * 2), transform.rotation);
            thisBullet.GetComponent<BulletBase>().SetPlayer(this.transform.gameObject);

            var thisBullet2 = Instantiate(currentBullet, transform.position + (transform.forward * 2), transform.rotation);
            thisBullet2.GetComponent<BulletBase>().SetPlayer(this.transform.gameObject);

            float randSpread = Random.Range(-3f, 3f);
            var thisBullet3 = Instantiate(currentBullet, transform.position + (transform.forward * 2), Quaternion.Euler(0, 0, randSpread + adder));
            thisBullet3.GetComponent<BulletBase>().SetPlayer(this.transform.gameObject);

            randSpread = Random.Range(-3f, 3f);
            var thisBullet4 = Instantiate(currentBullet, transform.position + (transform.forward * 2), Quaternion.Euler(0, 0, randSpread + adder));
            thisBullet4.GetComponent<BulletBase>().SetPlayer(this.transform.gameObject);

            var thisBullet5 = Instantiate(currentBullet, transform.position + (transform.forward * 2), Quaternion.Euler(0, 0, 11.31f + adder));
            thisBullet5.GetComponent<BulletBase>().SetPlayer(this.transform.gameObject);

            var thisBullet6 = Instantiate(currentBullet, transform.position + (transform.forward * 2), Quaternion.Euler(0, 0, -11.31f + adder));
            thisBullet6.GetComponent<BulletBase>().SetPlayer(this.transform.gameObject);

            randSpread = Random.Range(-3f, 3f);
            var thisBullet7 = Instantiate(currentBullet, transform.position + (transform.forward * 2), Quaternion.Euler(0, 0, 11.31f + randSpread + adder));
            thisBullet7.GetComponent<BulletBase>().SetPlayer(this.transform.gameObject);

            randSpread = Random.Range(-3f, 3f);
            var thisBullet8 = Instantiate(currentBullet, transform.position + (transform.forward * 2), Quaternion.Euler(0, 0, 11.31f + randSpread + adder));
            thisBullet8.GetComponent<BulletBase>().SetPlayer(this.transform.gameObject);

            randSpread = Random.Range(-3f, 3f);
            var thisBullet9 = Instantiate(currentBullet, transform.position + (transform.forward * 2), Quaternion.Euler(0, 0, -11.31f + randSpread + adder));
            thisBullet9.GetComponent<BulletBase>().SetPlayer(this.transform.gameObject);

            randSpread = Random.Range(-3f, 3f);
            var thisBullet10 = Instantiate(currentBullet, transform.position + (transform.forward * 2), Quaternion.Euler(0, 0, -11.31f + randSpread + adder));
            thisBullet10.GetComponent<BulletBase>().SetPlayer(this.transform.gameObject);
        }
    }
}
