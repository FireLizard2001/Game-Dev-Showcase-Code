#region Namespaces
using System.Collections;
using System.Collections.Generic;
using InteractableObjects;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
#endregion

/// <summary>
/// Manager of in game player inventory and money UI
/// </summary>
public class UIPlayerInventory : MonoBehaviour
{
    #region Attributes
    /* Serialized Fields */
    [Header("References")]
    [Tooltip("Reference to painting UI icon.")]
    public Sprite paintingUI = null;
    [Tooltip("Reference to sculpture UI icon.")]
    public Sprite sculptureUI = null;
    [Tooltip("Reference to the player")]
    public GameObject player = null;
    [Tooltip("Max number of items a player can hold.")]
    public int maxInventoryCount = 5;

    /* Private */
    private GameObject playerInventory = null; // Reference to player inventory
    private int inventoryCount = 0; // Number of items in player inventory
    private int currentMoney = 0; // Money in inventory



    #endregion

    #region Methods

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            GetPlayerReference();
        }

        if (playerInventory != null && playerInventory.transform.childCount != inventoryCount)
        {
            UpdateInventoryIcons();
        }
    }

    /// <summary>
    /// Gets the player reference
    /// Will change with networking implementation
    /// </summary>
    private void GetPlayerReference()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            if (p.GetComponent<PhotonView>().IsMine)
            {
                player =  p;
            }
        }
        if (player == null) { return; }
        playerInventory = player.transform.Find("Held Item").gameObject;
    }

    /// <summary>
    /// Update the UI icons and money when an item is picked up or dropped
    /// </summary>
    private void UpdateInventoryIcons()
    {
        inventoryCount = playerInventory.transform.childCount <= 5 ? playerInventory.transform.childCount : maxInventoryCount;
        currentMoney = 0;
        // Loop through the player inventory and determine which icon to display based on if it is a sculpture or painting
        for (int i = 0; i < inventoryCount; ++i)
        {
            if (playerInventory.transform.GetChild(i).gameObject.name.Contains("Painting"))
            {
                gameObject.transform.GetChild(i).gameObject.GetComponent<Image>().sprite = paintingUI;
                //currentMoney += playerInventory.transform.GetChild(i).gameObject.GetComponent<Painting>().GetValue();
            }
            else
            {
                gameObject.transform.GetChild(i).gameObject.GetComponent<Image>().sprite = sculptureUI;
                //currentMoney += playerInventory.transform.GetChild(i).gameObject.GetComponent<Network_Breakable>().GetValue();
            }
        }
        // Loop through the rest of the UI icons and disable their image
        for (int i = inventoryCount; i < gameObject.transform.childCount; ++i)
        {
            gameObject.transform.GetChild(i).gameObject.GetComponent<Image>().sprite = null;
        }
        transform.parent.GetChild(1).gameObject.GetComponent<TMP_Text>().text = "$" + currentMoney.ToString();
    }

    #endregion
}
