#region Namespaces
using UnityEngine;
using Photon.Pun;
using PlayerFunctionality;
using TMPro;
using System.Collections;
using System.Collections.Generic;
#endregion

/// <summary> Summary Here </summary>
public class UIPlayerName : MonoBehaviour
{
    PhotonView pv;
    TextMeshPro playerUI;
    List<Transform> otherPlayers;

    void Awake()
    {
        pv = GetComponentInParent<PhotonView>();
        playerUI = GetComponent<TextMeshPro>();
    }

    IEnumerator Start()
    {
        if (!pv.IsMine) yield return null;
        Player_Interact_Network pin = GetComponentInParent<Player_Interact_Network>();

        // Set this player's name when available
        yield return new WaitWhile(() => pin.nickname == "");
        playerUI.text = pin.nickname;

        // Wait 100 fixed updates to allow for player creation
        for (int i=0; i < 100; i++)
            yield return new WaitForFixedUpdate();

        // Populate player array
        UIPlayerName[] playerArray = FindObjectsOfType(typeof(UIPlayerName)) as UIPlayerName[];
        otherPlayers = new List<Transform>();

        // Find all players except current
        foreach (var player in playerArray)
        {
            if (player == this) continue;
            otherPlayers.Add(player.transform);
        }
    }

    void Update()
    {
        // Only continue if this is another player
        if (!pv.IsMine) return;
        if (otherPlayers == null || otherPlayers.Count == 0) return;

        foreach (var player in otherPlayers)
        {
            if (player != null)
                player.rotation = Quaternion.LookRotation(player.position - transform.position);
        }
    }
}
