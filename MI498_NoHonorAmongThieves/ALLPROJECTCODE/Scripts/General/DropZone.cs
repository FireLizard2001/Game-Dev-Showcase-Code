#region Namespaces
using UnityEngine;
using InteractableObjects;
using PlayerFunctionality;
using Photon.Pun;
using System.Collections;
#endregion

/// <summary> A designated place for players to cash in objects </summary>
public class DropZone : MonoBehaviour
{

    [Header("Settings")]
    [Tooltip("The player associated with this dropzone.")]
    public int playerNum = -1;

    [Tooltip("The openVanGameobject. Used for setting glow.")]
    public GameObject openVan = null;

    // Reference to GM
    GameManager GM;

    private GameObject ourPlayer = null;
    private bool isCollecting = false;

    void Start()
    {
        GM = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Player_Interact_Network>(out Player_Interact_Network curPlayer))
            if (curPlayer.IsHoldingItem() && curPlayer.GetHeldItem().Value == 1)
            {
                curPlayer.GetHeldItem().DestroySelf();
                curPlayer.AttemptThrow();
                curPlayer.ResetReticle();
                HandlePaintingScore();
                return;
            }
            else if (curPlayer.IsHoldingItem())
            {
                return;
            }

        if (!PhotonNetwork.IsMasterClient)
            return;

        // Stop here if not a painting
        if (!other.TryGetComponent<Painting>(out Painting curObject))
            return;

        if (other.gameObject.GetComponent<Interactable>().Value == 0)
            return;

        Debug.Log("Player Has Scored. Adding Points");

        HandlePaintingScore();
        other.gameObject.GetComponent<Interactable>().DestroySelf();

    }

    //Coroutine to make sure players are spawned in before grabbing them
    public void SetPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.GetComponent<AvatarSetup>().characterValue == playerNum)
            {
                ourPlayer = player;
                break;
            }
        }
    }

    /// <summary>
    /// Handles the adding of points to player score and checks if the game is over
    /// </summary>
    private void HandlePaintingScore()
    {
        if (ourPlayer.GetComponent<Player_Leaderboard_Network>() != null)
        {
            // Add points
            ourPlayer.GetComponent<Player_Interact_Network>().AddPoint();

            // To fix dupe bug, add check for which character throws here
            ourPlayer.GetComponent<Player_Leaderboard_Network>().AddScore();
        }
        else
        {
            // Add points
            ourPlayer.GetComponent<Player_Interact>().AddPoint();

            LeaderboardManager.Instance.AddScore(ourPlayer.GetComponent<AvatarSetup>().characterValue);
        }

        // Check if the game continues or ends.
        if (GM.GetGameState() == GameManager.GameStates.Final)
        {
            return;
        }
        else if (ourPlayer.GetComponent<Player_Interact>().Points >= GameManager.scoreCap)
        {
            GM.ChangeState(GameManager.GameStates.Final);
        }
        else
        {
            GM.SelectCollectorPainting();
        }
    }
}
