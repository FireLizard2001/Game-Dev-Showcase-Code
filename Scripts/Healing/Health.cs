using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    /// <summary>
    /// This class is supposed to be called when it has the object integer to heal / damage etc
    /// </summary>
    public int health;
    public int max_health;



    void TakeDamage(int damage_to_take) 
    {
        health -= damage_to_take;
    }

    void Heal(int heal_amount)
    {
        health += heal_amount;
/*        //Discuss methods of handling overhealing(if the object is consumed or not)
        if (heal_amount < (max_health - health))  
        {
            //Partially consume object? Discuss
            health = max_health; // this is a temporary point until we determine healing items and a method to extract heal 
        }

        //Fully consume object? Discuss
        if (health > max_health) 
        {
            health = max_health;
        }*/
    }

    public int GET_CURRENT_HEALTH() 
    {
        return health;
    }

    public void SET_CURRENT_HEALTH(int set_amount) 
    {
        health = set_amount;
    }

    void Death() // temporary function to later be edited to handle respawining
    {
        Destroy(this.gameObject);
    }

}
