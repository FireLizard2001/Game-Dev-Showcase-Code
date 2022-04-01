using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBase : MonoBehaviour
{

    [Header("Bullet Info")]
    [Tooltip("Speed the bullet travels at.")]
    public float speed = 20f;
    [Tooltip("Damage this bullet does.")]
    public int damage = 10;
    [Tooltip("Distance bullet will travel before despawning.")]
    public float despawnDistance;
    [Tooltip("Particles to spawn for this bullets impact (can be blank if explosion).")]
    public GameObject impactEffect;
    [Tooltip("Sound to play when bullet impacts object.")]
    public AudioSource impactAudio;

    [Header("Explosive Bullets")]
    [Tooltip("Whether or not this explodes.")]
    public bool isExplosive = false;
    [Tooltip("This can be left empty if there isExplosive is false.")]
    public GameObject explosion = null;

    private float spawnLocation;
    private GameObject myPlayer = null;
    private Rigidbody2D rb;
    private Vector3 lastPosition;
    private bool fastCollisionFix = false;
    private Collider2D collision2;
    private RaycastHit2D rHit;
    public bool isMelee = false;

    public void SetPlayer(GameObject player)
    {
        myPlayer = player;
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = this.gameObject.GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * speed;
        spawnLocation = transform.position.x;
        lastPosition = transform.position;
    }

    private void Update()
    {
        if (despawnDistance <= Mathf.Abs(transform.position.x - spawnLocation))
        {
            EndBulletLifeNoEffect();
        }
        fastCollisionFix = Physics2D.Raycast(lastPosition, (transform.position - lastPosition).normalized, (transform.position - lastPosition).magnitude);
        if (fastCollisionFix)
        {
            rHit = Physics2D.Raycast(lastPosition, (transform.position - lastPosition).normalized, (transform.position - lastPosition).magnitude);
            collision2 = rHit.collider;
            /// This is what the bullet will collide with. Needs to have a trigger collider and a tag check
            if (collision2.CompareTag("Foreground") || ((collision2.CompareTag("Player") && collision2.gameObject != myPlayer || collision2.CompareTag("Box") || collision2.CompareTag("Grabbed")) && collision2.gameObject.GetComponent<Rigidbody2D>() != null))
            {
                if (collision2.CompareTag("Player"))
                {
                    if (isMelee)
                    {
                        collision2.gameObject.GetComponent<Rigidbody2D>().AddForce(9 * 250 * Mathf.Sin(5f * Mathf.PI / 180f) * Vector2.up);
                        collision2.gameObject.GetComponent<Rigidbody2D>().AddForce(9 * 250 * (collision2.gameObject.transform.position - myPlayer.transform.position).normalized);
                    }
                    else
                    {
                        collision2.gameObject.GetComponent<Rigidbody2D>().AddForce(damage * 15 * Mathf.Sin(5f * Mathf.PI / 180f) * Vector2.up);
                        collision2.gameObject.GetComponent<Rigidbody2D>().AddForce(damage * 15 * (collision2.gameObject.transform.position - myPlayer.transform.position).normalized);
                    }

                    collision2.gameObject.GetComponent<Health>().TakeDamage(damage);
                }
                else if (collision2.CompareTag("Box") || collision2.CompareTag("Grabbed"))
                {
                    collision2.gameObject.GetComponent<Health>().TakeDamage(damage);
                }
                EndBulletLife();
            }
        }
        lastPosition = transform.position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

    }

    private void EndBulletLife()
    {
        Instantiate(impactEffect, transform.position, transform.rotation, null);

        if (isExplosive)
        {
            Instantiate(explosion, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    private void EndBulletLifeNoEffect()
    {
        if (isExplosive)
        {
            Instantiate(explosion, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}
