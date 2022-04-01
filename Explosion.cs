using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{

    public float areaOfEffect;
    public float force;
    public float explosionDamage;

    public LayerMask LayerToHit;

    public GameObject ExplosionEffect;

    // Start is called before the first frame update
    void Start()
    {
        Collider2D[] objects = Physics2D.OverlapCircleAll(transform.position, areaOfEffect, LayerToHit);

        foreach (Collider2D obj in objects)
        {
            if (obj.gameObject.tag == "Grabbed")
            {
                obj.gameObject.transform.parent.parent.gameObject.GetComponent<GrabController>().isCarrying = false;
                Destroy(obj.gameObject);
            }
            else
            {
                Vector2 direction = obj.transform.position - transform.position;
                obj.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 126 * (1 - direction.magnitude / areaOfEffect));
                obj.GetComponent<Rigidbody2D>().AddForce(direction.normalized * force * (1 - direction.magnitude / areaOfEffect));
                if (obj.gameObject.tag == "Player")
                {
                    obj.gameObject.GetComponent<Health>().TakeDamage((int)(explosionDamage * (1 - direction.magnitude / areaOfEffect)));
                }
            }

        }

        Instantiate(ExplosionEffect, transform.position, Quaternion.identity, null);
        GameObject.Find("Main Camera").GetComponent<ScreenShaker>().explode = true;
        //Destroy(ExplosionEffectIns, 10);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, areaOfEffect);
    }


}
