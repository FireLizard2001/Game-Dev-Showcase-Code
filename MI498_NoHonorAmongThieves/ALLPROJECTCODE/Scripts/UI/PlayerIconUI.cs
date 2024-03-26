#region Namespace
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using PlayerFunctionality;
using UnityEngine;
using UnityEngine.UI;
using InteractableObjects;
#endregion

/// <summary>
/// Handle player movement and interaction UI elements
/// </summary>
public class PlayerIconUI : MonoBehaviour
{
    #region Attributes
    [Header("Sprite References")]
    [Tooltip("List of icon sprites.")]
    public List<Sprite> sprites;
    [Tooltip("List of player sprites.")]
    public List<Sprite> playerSprites;
    [Tooltip("List of control sprites.")]
    public List<Sprite> controlSpites;
    [Tooltip("Enabled Icon Color.")]
    public Color32 enabledColor;
    [Tooltip("Disabled Icon Color.")]
    public Color32 disabledColor;

    // Private attributes
    private GameObject ourPlayer = null; //Our player
    private Slider pushSlider = null; //Slider for our push cool down animation



    #endregion

    #region Methods
    // Start is called before the first frame update
    protected virtual void Awake()
    {
        pushSlider = transform.GetChild(transform.childCount - 3).gameObject.GetComponent<Slider>();
        StartCoroutine(GetPlayer(0.05f));
    }

    // Update is called once per frame
    void Update()
    {
        if (ourPlayer != null && pushSlider != null)
        {
            CheckMovement();
            CheckPushCoolDown();
            CheckHoldingItem();
        }
    }

    /// <summary>
    /// Check the current movement status of the player and update the icon
    /// </summary>
    private void CheckMovement()
    {
        // If we are crouching set the crouch icon
        if (ourPlayer.GetComponent<Player_Movement_Network>().IsCrouching())
        {
            transform.GetChild(transform.childCount - 1).gameObject.GetComponent<Image>().color = enabledColor;
        }
        else if(ourPlayer.GetComponent<Player_Movement_Network>().IsJumping())
        {
            transform.GetChild(transform.childCount - 2).gameObject.GetComponent<Image>().sprite = sprites[1];
            transform.GetChild(transform.childCount - 2).gameObject.GetComponent<Image>().color = enabledColor;
        }
        else if(ourPlayer.GetComponent<Player_Movement_Network>().IsSprinting())
        {
            transform.GetChild(transform.childCount - 2).gameObject.GetComponent<Image>().sprite = sprites[2];
            transform.GetChild(transform.childCount - 2).gameObject.GetComponent<Image>().color = enabledColor;
        }
        else
        {
            transform.GetChild(transform.childCount - 1).gameObject.GetComponent<Image>().color = disabledColor;
            transform.GetChild(transform.childCount - 2).gameObject.GetComponent<Image>().sprite = sprites[2];
            transform.GetChild(transform.childCount - 2).gameObject.GetComponent<Image>().color = disabledColor;

        }
    }

    /// <summary>
    /// Check the current coold own status of our push and update the icon
    /// </summary>
    private void CheckPushCoolDown()
    {
        pushSlider.value = ourPlayer.GetComponent<Player_Interact_Network>().GetPushCooldown();
    }

    /// <summary>
    /// Check to see if player is holding an item
    /// </summary>
    private void CheckHoldingItem()
    {
        if (ourPlayer.GetComponent<Player_Interact_Network>().GetViewItem() != null)
        {
            transform.GetChild(transform.childCount - 5).gameObject.SetActive(true);
            transform.GetChild(transform.childCount - 6).gameObject.SetActive(true);
            transform.GetChild(transform.childCount - 6).gameObject.GetComponent<Image>().sprite = controlSpites[4];
            Interactable ourObj = ourPlayer.GetComponent<Player_Interact_Network>().GetViewItem();
            if (ourObj is Painting)
                transform.GetChild(transform.childCount - 5).gameObject.GetComponent<Image>().sprite = controlSpites[0];
            else
                transform.GetChild(transform.childCount - 5).gameObject.GetComponent<Image>().sprite = controlSpites[1];

        }
        else if (ourPlayer.GetComponent<Player_Interact_Network>().GetHeldItem() != null)
        {
            transform.GetChild(transform.childCount - 5).gameObject.SetActive(true);
            transform.GetChild(transform.childCount - 6).gameObject.SetActive(true);
            transform.GetChild(transform.childCount - 6).gameObject.GetComponent<Image>().sprite = controlSpites[5];
            Interactable ourObj = ourPlayer.GetComponent<Player_Interact_Network>().GetHeldItem();
            if (ourObj is Painting)
                transform.GetChild(transform.childCount - 5).gameObject.GetComponent<Image>().sprite = controlSpites[2];
            else
                transform.GetChild(transform.childCount - 5).gameObject.GetComponent<Image>().sprite = controlSpites[3];
        }
        else
        {
            transform.GetChild(transform.childCount - 5).gameObject.SetActive(false);
            transform.GetChild(transform.childCount - 6).gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Enumerator to get our current player
    /// </summary>
    /// <param name="waitTime"></param> Time to wait after start to run
    /// <returns></returns>
    IEnumerator GetPlayer(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.GetComponent<PhotonView>().IsMine)
            {
                ourPlayer = player;
                transform.GetChild(transform.childCount - 4).gameObject.GetComponent<Image>().enabled = true;
                switch (player.GetComponent<AvatarSetup>().characterValue)
                {
                    case 0:
                        transform.GetChild(transform.childCount - 4).gameObject.GetComponent<Image>().sprite = playerSprites[0];
                        break;
                    case 1:
                        transform.GetChild(transform.childCount - 4).gameObject.GetComponent<Image>().sprite = playerSprites[1];
                        break;
                    case 2:
                        transform.GetChild(transform.childCount - 4).gameObject.GetComponent<Image>().sprite = playerSprites[2];
                        break;
                    case 3:
                        transform.GetChild(transform.childCount - 4).gameObject.GetComponent<Image>().sprite = playerSprites[3];
                        break;
                    default:
                        Debug.Log("Not a valid character ID");
                        break;
                }

                break;
            }
        }
    }

    #endregion
}
