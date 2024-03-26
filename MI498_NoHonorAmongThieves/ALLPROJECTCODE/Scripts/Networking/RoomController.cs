using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.IO;
using TMPro;
using System;

public class RoomController : MonoBehaviourPunCallbacks, IInRoomCallbacks
{
    #region Inspector Variables
    [Header("Scene Indexing")]
    [Tooltip("Scene index for loading multiplayer scene")]
    [SerializeField] private int multiplayerSceneIndex;
    [Tooltip("Current scene index")]
    [SerializeField] public int currentScene;

    [Header("Canvas Panels")]
    [Tooltip("Display for mainMenu")]
    [SerializeField] private GameObject mainMenuPanel;
    [Tooltip("Display for when in makeLobby")]
    [SerializeField] private GameObject createLobbyPanel;
    [Tooltip("Display for when in joinLobby")]
    [SerializeField] private GameObject joinLobbyPanel;
    [Tooltip("Display for when in room")]
    [SerializeField] private GameObject roomPanel;

    [Header("Canvas Objects")]
    [Tooltip("Used by master client to start game")]
    [SerializeField] private GameObject startButton;
    [Tooltip("Used by master client to change paintings")]
    [SerializeField] private GameObject PaintingsToWinInput;

    [Tooltip("Displays all players in current room")]
    [SerializeField] Transform playersContainer;
    [Tooltip("Instantiate to display each player in room")]
    [SerializeField] GameObject playersListingPrefab;

    [Tooltip("Displays the name of the room")]
    [SerializeField] private TMP_Text roomNameDisplay;

    [Header("RoomInfo")]
    public bool isGameLoaded;

    public static RoomController room;
    
    #endregion

    private void Awake()
    {
        if (room == null)
        {
            room = this;
        }
        else
        {
            if (room != this)
            {
                Destroy(room.gameObject);
                room = this;
            }
        }
        DontDestroyOnLoad(this.gameObject);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
        SceneManager.sceneLoaded += OnSceneFinishedLoading;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.RemoveCallbackTarget(this);
        SceneManager.sceneLoaded -= OnSceneFinishedLoading;
    }

    void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        //Called when multiplayer scene is loaded
        currentScene = scene.buildIndex;
        if (currentScene == multiplayerSceneIndex)
        {
            CreatePlayer();
        }
    }

    #region Lobby Listings
    //Clears out all players
    void ClearPlayerListings()
    {
        //joinLobbyPanel.SetActive(true);
        for (int i = playersContainer.childCount - 1; i >= 0; i--) //Loop through all children of container
        {
            Destroy(playersContainer.GetChild(i).gameObject);
        }
        //joinLobbyPanel.SetActive(false);
    }

    //Lists all players
    void ListPlayers()
    {
        int i = 0;

        List<String> existingNames = new List<String>();
        int dupNameHandler = 1;
        foreach (Player player in PhotonNetwork.PlayerList) //Loop through all players and create a playerlisting
        {
            if (existingNames.Contains(player.NickName))
            {
                string adjustedName = player.NickName + dupNameHandler.ToString();
                player.NickName = adjustedName;
                if (player == PhotonNetwork.LocalPlayer)
                {
                    PhotonNetwork.NickName = adjustedName;
                    PlayerPrefs.SetString("NickName", adjustedName);
                }
                dupNameHandler += 1;
            }
            existingNames.Add(player.NickName);
            GameObject tempListing = Instantiate(playersListingPrefab, playersContainer);
            TMP_Text tempText = tempListing.transform.GetChild(0).GetComponent<TMP_Text>();
            tempText.text = player.NickName;

            if (player == PhotonNetwork.LocalPlayer)
            {
                PlayerInfo.PI.mySelectedCharacter = i;
                PlayerPrefs.SetInt("MyCharacter", i);
            }
            i += 1;
        }
    }
    #endregion

    #region Room Handling
    //Called when local player joins a room
    public override void OnJoinedRoom()
    {
        // Right here we should navigate to the lobby menu.
        EventSystem.current.SetSelectedGameObject(null);
        roomPanel.SetActive(true); //Activate the display for being in a room
        createLobbyPanel.SetActive(false); //Hide the display for being in a room
        joinLobbyPanel.SetActive(false); //Hide the display for being in a room
        roomNameDisplay.text = PhotonNetwork.CurrentRoom.Name; //Update room name display
        if (PhotonNetwork.IsMasterClient) //If master client then activate start button
        {
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        }
        //photonPlayers = PhotonNetwork.PlayerList;

        // See Lobby Listings Region
        ClearPlayerListings(); //Remove all old player listings
        ListPlayers(); //Relist all current player listings
    }

    //Called whenever a player enters a room
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // See Lobby Listings Region
        ClearPlayerListings(); //Remove all old player listings
        ListPlayers(); //Relist all current player listings

        int numOfPlayers = PhotonNetwork.PlayerList.Length;

        switch (numOfPlayers)
        {
            case 2:
                AkSoundEngine.PostEvent("playerJoin", this.gameObject);
                break;
            case 3:
                AkSoundEngine.PostEvent("playerJoin_01", this.gameObject);
                break;
            case 4:
                AkSoundEngine.PostEvent("playerJoin_02", this.gameObject);
                break;
        }
    }

    //Called whenever a player leaves a room
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (playersContainer == null) 
        {
            GameSetup.GS.DisconnectPlayer();
        }
        else
        {

            ClearPlayerListings(); //Remove all old player listings
            ListPlayers(); //Relist all current player listings

            AkSoundEngine.PostEvent("playerLeave", this.gameObject);

            if (PhotonNetwork.IsMasterClient) //Reallocate for new master client
            {
                startButton.SetActive(true);
            }
        }
    }
    #endregion

    #region Game State Transition (Start, Leave, etc.)
    //Paired to the start button
    //Will load players into a multiplayer session
    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {

            if (PaintingsToWinInput != null && PaintingsToWinInput.activeSelf)
            {
                if (Int32.TryParse(PaintingsToWinInput.GetComponent<TMP_InputField>().text, out int j))
                {
                    PlayerPrefs.SetInt("PaintingsToWin", j);
                }
                else
                {
                    PlayerPrefs.SetInt("PaintingsToWin", 3);
                }
            }

            // This makes it so that once a game is started, it will no longer be listed in joinable lobbys
            PhotonNetwork.CurrentRoom.IsOpen = false; //Comment out if you want player to join after game starts
            PhotonNetwork.CurrentRoom.IsVisible = false; //Comment out if you want player to join after game starts

            AkSoundEngine.StopAll();

            PhotonNetwork.LoadLevel(multiplayerSceneIndex);
        }
    }

    IEnumerator rejoinLobby()
    {
        yield return new WaitForSeconds(1);
        PhotonNetwork.JoinLobby();
    }

    //Returns player to lobby
    public void BackOnClick()
    {
        // See Lobby Listings Region
        ClearPlayerListings(); //Remove all old player listings

        // Right here we should navigate back to the main menu
        roomPanel.SetActive(false);
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LeaveLobby();
        StartCoroutine(rejoinLobby());
    }
    #endregion

    //Creates a networked player for each person that joins into multiplay scene
    private void CreatePlayer()
    {
        //Debug.Log("Creating Player");
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PhotonNetworkPlayer"), Vector3.zero, Quaternion.identity); //PhotonNetworkPlayer name must match prefab and folder name
    }

    private void GeneratePlayerList()
    {
        foreach (GameObject player in playersContainer)
        {

        }
    }
}
