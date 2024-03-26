using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Player_Leaderboard_Network : MonoBehaviour
{
    private PhotonView PV = null;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    void Start()
    {
        PV.RPC("RPC_UpdateLeaderboards", RpcTarget.All);
    }

    void UpdateLeaderboards()
    {
        LeaderboardManager.Instance.UpdateAllLeaderboardsAndVans();
    }

    public void AddScore()
    {
        //if (PhotonNetwork.IsMasterClient)
        PV.RPC("RPC_AddScore", RpcTarget.All, GetComponent<AvatarSetup>().characterValue);
    }

    [PunRPC]
    void RPC_UpdateLeaderboards()
    {
        UpdateLeaderboards();
    }

    [PunRPC]
    void RPC_AddScore(int playerID)
    {
        LeaderboardManager.Instance.AddScore(playerID);
    }
}
