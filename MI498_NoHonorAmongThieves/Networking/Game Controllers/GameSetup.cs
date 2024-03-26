using Inputs;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using PlayerFunctionality;
using UnityEngine.EventSystems;

public class GameSetup : MonoBehaviour
{
    #region Attributes
    public static GameSetup GS;

    [Header("Setup")]
    [Tooltip("Player Spawn Points")]
    public Transform[] spawnPoints;

    [Tooltip("Player Jail Points")]
    public Transform[] jailPoints;

    [Header("Leaving")]
    [Tooltip("Player Leaving Screen")]
    public GameObject playerLeaveScreen;
    [Tooltip("Pause menu reference.")]
    //public GameObject pauseScreen = null;

    //private bool paused = false;    // Inidcates if we are paused

    //GameObject[] playerList = null;


    #endregion

    #region Methods

    private void Start()
    {
        //GameInputManager.Instance.PausePressed += AttemptPause;
        //if (pauseScreen == null)
        //{
        //    Debug.Log("Need reference to pause screen UI.");
        //}
    }

    //Sets Singleton
    private void OnEnable()
    {
        if(GameSetup.GS == null)
        {
            GameSetup.GS = this;
        }

    }

    //Creates a networked player for each person that joins into multiplay scene
    private void CreatePlayer()
    {
        Debug.Log("Creating Player");
        GameObject player = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PhotonNetworkPlayer"), Vector3.zero, Quaternion.identity); //PhotonNetworkPlayer name must match prefab and folder name

        CurrentPlayerStatic.playerID = player.GetPhotonView().ViewID;
    }

    //Gets the spawn positions from players joining
    public Transform GetSpawnTransform()
    {
        return jailPoints[PlayerPrefs.GetInt("MyCharacter")];
    }

    //Calls Coroutine to leave lobby
    public void DisconnectPlayer()
    {
        Time.timeScale = 1;
        StartCoroutine(DisconnectAndLoad());
    }

    ///// <summary>
    ///// Attempts to bring up the pause menu of the player
    ///// </summary>
    //public void AttemptPause()
    //{
    //    if (GameManager.instance.gameState != GameManager.GameStates.Final)
    //    {
    //        if (playerList == null)
    //        {
    //            playerList = GameObject.FindGameObjectsWithTag("Player");
    //        }
    //        paused = !paused;
    //        pauseScreen.SetActive(paused);
    //        ChangePlayerPauseState();
    //        Cursor.visible = paused;
    //        if (paused)
    //        {
    //            Cursor.lockState = CursorLockMode.None;
    //            EventSystem.current.SetSelectedGameObject(pauseScreen.transform.GetChild(0).GetChild(0).gameObject);
    //        }
    //        else
    //        {
    //            Cursor.lockState = CursorLockMode.Confined;
    //            Cursor.lockState = CursorLockMode.Locked;
    //        }
    //    }
    //}

    ///// <summary>
    ///// Updates the player movement and interact states when paused
    ///// </summary>
    //private void ChangePlayerPauseState()
    //{
    //    foreach (GameObject p in playerList)
    //    {
    //        if (p.GetComponent<AvatarSetup>().characterValue == PlayerPrefs.GetInt("MyCharacter"))
    //        {
    //            p.transform.GetChild(p.transform.childCount - 2).gameObject.SetActive(!paused);
    //            p.GetComponent<Player_Movement>().isPaused = paused;
    //            p.GetComponent<Player_Interact>().isPaused = paused;
    //            p.transform.GetComponentInChildren<Player_Camera_Network>().isPaused = paused;
    //            break;
    //        }
    //    }
    //}

    //Dissconnects player from lobby
    IEnumerator DisconnectAndLoad()
    {
        //PV.RPC("RPC_LeaveLobby", RpcTarget.All, CurrentPlayerStatic.playerID);
        Destroy(PhotonView.Find(CurrentPlayerStatic.playerID));
        playerLeaveScreen.SetActive(true);
        PhotonNetwork.Disconnect();
        while(PhotonNetwork.IsConnected)
            yield return null;
        yield return new WaitForSeconds(1); //Lobby is left super fast so if we want a leave screen wait an extra second
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene(0);
        Debug.Log("Player has left the scene! (called from GameSetup)");
    }

/*    [PunRPC]
    void RPC_LeaveLobby(int playerID)
    {
        Destroy(PhotonView.Find(playerID));
    }*/

    #endregion
}
