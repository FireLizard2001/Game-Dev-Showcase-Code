using System.Collections;
using System.Collections.Generic;
using PlayerFunctionality;
using UnityEngine;
using Photon.Pun;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    // Dictionary to hold player scores
    private SortedDictionary<string, int> playerDict = null;
    private SortedDictionary<string, int> nicknameToPlayerID = null;
    private GameObject[] playerList = null;
    private GameObject[] leaderboards = null;

    private int highestScore = 0;

    private PhotonView PV;

    private void Awake()
    {
        PV = this.GetComponent<PhotonView>();
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void StartGameMusic()
    {
        highestScore = 0;

        if (GameManager.instance.isMaster)
        {
            gameObject.GetComponent<PhotonView>().RPC("SetMusicState", RpcTarget.All, true);
        }
    }

    // Re-intializes the leaderboards in the game
    private void Init()
    {
        if (playerDict == null)
        {
            playerDict = new SortedDictionary<string, int>();
            nicknameToPlayerID = new SortedDictionary<string, int>();
            playerList = new GameObject[4];
        }

        leaderboards = GameObject.FindGameObjectsWithTag("Leaderboard");
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (!playerDict.ContainsKey(player.GetComponent<Player_Interact>().nickname))
            {
                playerDict[player.GetComponent<Player_Interact>().nickname] = 0;
                playerList[player.GetComponent<AvatarSetup>().characterValue] = player;
                if (!nicknameToPlayerID.ContainsKey(player.GetComponent<Player_Interact>().nickname))
                {
                    nicknameToPlayerID[player.GetComponent<Player_Interact>().nickname] = player.GetComponent<AvatarSetup>().characterValue;
                }
            }
        }
    }

    // Handles setting the proper trucks up
    public void UpdateAllLeaderboardsAndVans()
    {
        UpdateAllLeaderboards(false);
        GameObject[] vans = GameObject.FindGameObjectsWithTag("DropZone");
        foreach (GameObject van in vans)
        {
            int current_player = van.transform.GetChild(2).transform.GetComponent<DropZone>().playerNum;
            van.transform.GetChild(2).transform.GetComponent<DropZone>().SetPlayer();
            if (playerList[current_player] != null)
            {
                van.transform.GetChild(0).gameObject.SetActive(false);
                van.transform.GetChild(1).gameObject.SetActive(true);
            }
        }
/*        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC("RPC_SetMaxPaintings", RpcTarget.All, PlayerPrefs.GetInt("PaintingsToWin"));
        }*/
    }

/*    [PunRPC]
    void RPC_SetMaxPaintings(int maxPaintings)
    {
        GameManager.scoreCap = maxPaintings;
    }*/

    // Updates all active leaderboards in the game
    public void UpdateAllLeaderboards(bool scored)
    {
        Init();
        foreach (GameObject leaderboard in leaderboards)
        {
            leaderboard.GetComponent<LeaderboardUI>().PopulateLeaderboard();
            if (scored)
            {
                leaderboard.GetComponent<LeaderboardUI>().SpawnScoreParticle();
            }
        }
    }

    public SortedDictionary<string, int> GetScores() { return playerDict; }

    public SortedDictionary<string, int> GetNameToID() { return nicknameToPlayerID; }


    public void AddScore(int playerId)
    {
        playerDict[playerList[playerId].GetComponent<Player_Interact>().nickname] += 1;
        UpdateAllLeaderboards(true);

        if (playerDict[playerList[playerId].GetComponent<Player_Interact>().nickname] > highestScore)
        {
            highestScore = playerDict[playerList[playerId].GetComponent<Player_Interact>().nickname];


            if (GameManager.instance.isMaster)
            {
                gameObject.GetComponent<PhotonView>().RPC("SetMusicState", RpcTarget.All, false);
            }
        }
    }

    [PunRPC]
    public void SetMusicState(bool initial)
    {
        if (initial)
        {
            AkSoundEngine.PostEvent("inGameMusic", this.gameObject);
        }

        AkSoundEngine.SetState("inGameMusic", $"Lvl{highestScore}");
    }
}
