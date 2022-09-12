using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingItem : MonoBehaviour
{
    public int heal_amount;

    void OnConsume() 
    {
        Destroy(this.gameObject);
        // apply healing to the correct player
    }
}
