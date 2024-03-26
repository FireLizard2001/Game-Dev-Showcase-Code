using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomMatchmakingLobbyController : MonoBehaviourPunCallbacks
{
    #region General Lobby Variables
    [Header("Panels")]
    [Tooltip("Panel for displaying the create lobby menu.")]
    [SerializeField] private GameObject createLobbyPanel;
    [Tooltip("Panel for displaying the join lobby menu")]
    [SerializeField] private GameObject joinLobbyPanel;
    [Tooltip("Panel for displaying the room you joined")]
    [SerializeField] private GameObject roomPanel;

    [Header("Loading Buttons")]
    [Tooltip("Button used for playing game")]
    [SerializeField] private GameObject playButton;
    [Tooltip("Button used for displaying when loading.")]
    [SerializeField] private GameObject loadingButton;
    [Tooltip("Host and Join game buttons.")]
    [SerializeField] private GameObject lobbyButtons;

    [Tooltip("Developer variable for setting number of players")]
    private int roomSize = 4; // Developer variable for capping number of players

    [Header("Rooms")]
    [Tooltip("Container for holding all the room listings")]
    [SerializeField] private Transform roomsContainer;
    [Tooltip("Prefab for displaying each room in the lobby")]
    [SerializeField] private GameObject roomListingPrefab;
    [Tooltip("List of all current rooms")]
    private List<RoomInfo> roomListings;
    #endregion

    #region Create Lobby Variables
    [Header("Create Lobby Inputs")]
    [Tooltip("Button used for creating lobby")]
    [SerializeField] private GameObject lobbyCreateButton;
    [Tooltip("Input field so player can change their nickname")]
    public TMP_InputField playerNameInputCreate;
    [Tooltip("String for saving room name")]
    private string roomName;
    [Tooltip("Toggle for public")]
    [SerializeField] private Toggle publicToggle;
    [Tooltip("Toggle for private")]
    [SerializeField] private Toggle privateToggle;
    [Tooltip("Password Input Bundle")]
    [SerializeField] private GameObject roomPasswordInput;
    [Tooltip("String for tracking security state")]
    private bool isPrivate = true;
    [Tooltip("String for saving password")]
    private string roomPassword = "";
    #endregion

    #region Join Lobby Variables
    [Header("Join Lobby Inputs")]
    [Tooltip("Input field so player can change their nickname")]
    public TMP_InputField playerNameInputJoin;
    [Tooltip("Button used for joining lobby")]
    [SerializeField] private GameObject lobbyJoinButton;
    [Tooltip("Input field so player can enter room password")]
    public GameObject joinRoomPasswordInput;
    private string joinRoomPassword = "";
    private string rmName;
    private bool rmSecurity;
    private string rmPass;
    #endregion

    #region General Methods

    public void Awake()
    {
        roomListings = new List<RoomInfo>(); //init roomListing
    }

    /// <summary>
    /// Callback function for when first connection is made
    /// </summary>
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.AutomaticallySyncScene = true; //Makes it so whatever scene the master is in, others join
        loadingButton.SetActive(false); // Deactivate loading button when ready to connect
        playButton.SetActive(true); //Activate button for connecting to lobby
        lobbyButtons.SetActive(true); //Activate sub buttons for joining and hosting games

        //Check for player name saved to player prefs
        if (PlayerPrefs.HasKey("NickName"))
        {
            if (PlayerPrefs.GetString("NickName") == "")
            {
                PhotonNetwork.NickName = "Player " + Random.Range(0, 1000); //Random playername when empty
            }
            else
            {
                PhotonNetwork.NickName = PlayerPrefs.GetString("NickName"); //Get saved playername
            }
        }
        else
        {
            PhotonNetwork.NickName = "Player " + Random.Range(0, 1000); //Random playername when empty, this is the default that the player sees
        }
        playerNameInputCreate.text = PhotonNetwork.NickName; //Updates input field with playername
        playerNameInputJoin.text = PhotonNetwork.NickName; //Updates input field with playername
    }

    public void OnClickJoinLobby() // This must be called after we join the server. We connect to the server on game launch. This is tied to both the Host and Join game buttons.
    {
        PhotonNetwork.JoinLobby(); //First tries to join an existing room
    }

    public void OnHostLobby() // This is to clean the UI, and set the values for security to public by default if you navigate out of a room while still in the main menu.
    {
        OnSecurityTogglePressed(true);
    }


    /// <summary>
    /// Input function for playername
    /// </summary>
    /// <param name="nameInput"> The string that represents what to update the player name to</param>
    public void PlayerNameUpdate(string nameInput)
    {
        int length = Mathf.Min(nameInput.Length, 10);
        nameInput = nameInput.Substring(0, length);
        if (nameInput == "" || nameInput == null)
            nameInput = "Player " + Random.Range(0, 10000);
        PhotonNetwork.NickName = nameInput;
        PlayerPrefs.SetString("NickName", nameInput);
    }

    #endregion

    #region Room Listing

    /// <summary>
    /// Updates the lobby listing
    /// </summary>
    /// <param name="roomList">List of rooms to use to update the lobby listing</param>
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        int tempIndex;
        if (joinLobbyPanel.activeSelf == false)
        {
            joinRoomPasswordInput.SetActive(false);
        }
        foreach (RoomInfo room in roomList) //Loop through each room in roomlist
        {
            if (roomListings != null) //Try to find exisiting room listings
            {
                tempIndex = roomListings.FindIndex(ByName(room.Name));
            }
            else
            {
                tempIndex = -1;
            }
            if (tempIndex != -1) //Remove listing because it has been closed
            {
                roomListings.RemoveAt(tempIndex);

                // As a room listing has been removed, if it is the one the player had selected, deactivate the joinroom button as well as the password input for join room
                if (room.Name == rmName)
                {
                    lobbyJoinButton.SetActive(false);
                    joinRoomPasswordInput.SetActive(false);
                }
                Destroy(roomsContainer.GetChild(tempIndex).gameObject);
            }
            if (room.PlayerCount > 0 && !roomListings.Contains(room)) //Add a new room listing
            {
                roomListings.Add(room);
                ListRoom(room);
            }
        }
    }


    /// <summary>
    /// Predicate Function for searching through room names
    /// </summary>
    /// <param name="name">the room name we are searching for</param>
    /// <returns></returns>
    static System.Predicate<RoomInfo> ByName(string name)
    {
        return delegate (RoomInfo room)
        {
            return room.Name == name;
        };
    }

    /// <summary>
    /// Displays new room listing for the current room
    /// </summary>
    /// <param name="room">The room we are creating a listing for</param>
    void ListRoom(RoomInfo room)
    {
        if (room.IsOpen && room.IsVisible)
        {
            GameObject tempListing = Instantiate(roomListingPrefab, roomsContainer);
            RoomButton tempButton = tempListing.GetComponent<RoomButton>();

            tempButton.SetRoom(room.Name, room.MaxPlayers, room.PlayerCount, (string)room.CustomProperties["password"], (bool)room.CustomProperties["security"], this);
        }
    }

    #endregion

    #region Room Making

    /// <summary>
    /// Input function for changing room name
    /// </summary>
    /// <param name="nameIn">The input name to change the room name to</param>
    public void OnRoomNameChanged(string nameIn)
    {
        int length = Mathf.Min(nameIn.Length, 16);
        nameIn = nameIn.Substring(0, length);
        roomName = nameIn;
    }

    /// <summary>
    /// A function to enable and disable the public / private security setting
    /// </summary>
    /// <param name="which">A boolean to tell which was pressed where True is Public and False is Private</param>
    public void OnSecurityTogglePressed(bool which)
    {
        // Here we disable and enable UI as needed for the Create Lobby Menu for the security section.
        // We also notify the system whether the game has a password by setting isPrivate.

        // We are setting it to public.
        if (which)
        {
            roomPasswordInput.SetActive(false);
            isPrivate = false;

            publicToggle.interactable = false;
            privateToggle.interactable = true;
        }
        // We are setting it to private.
        else
        {
            roomPasswordInput.SetActive(true);
            isPrivate = true;

            publicToggle.interactable = true;
            privateToggle.interactable = false;
        }
    }

    /// <summary>
    /// Input function for changing room password
    /// </summary>
    /// <param name="passIn">The input to change the room password to</param>
    public void OnRoomPasswordChanged(string passIn)
    {
        roomPassword = passIn;
    }

    /// <summary>
    /// Function to create a new room
    /// </summary>
    public void CreateRoom()
    {
        // Debug.Log("Creating room now");

        #region Room Customization
        if(roomName == "" || roomName == null)
        {
            roomName = "Room " + Random.Range(0, 10000); // If the room name is blank, make a default name for it
        }

        //Debug.Log(roomName);

        if(roomPassword is null)
        {
            roomPassword = "";
        }

        ExitGames.Client.Photon.Hashtable table = new ExitGames.Client.Photon.Hashtable()
        {
            {"security", isPrivate},
            {"password", roomPassword}
        };

        #endregion

        RoomOptions roomOps = new RoomOptions()
        {
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = (byte)roomSize,
            CustomRoomProperties = table
    };

        roomOps.CustomRoomPropertiesForLobby = new string[] { "security", "password" };

        PhotonNetwork.CreateRoom(roomName, roomOps, TypedLobby.Default); //Attempts to make a new room
    }

    /// <summary>
    /// Function to handle if the room creation failed
    /// </summary>
    /// <param name="returnCode"></param>
    /// <param name="message"></param>
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Tried to create a new room but failed, there likely is already a room with the same name");

        // Tell this to the player
    }

    #endregion

    #region Room Joining
    // Attempts to join a room
    public void JoinRoomClick()
    {
        // Check to see if the room has a password.
        if (rmSecurity)
        {      
            // Check to see if the password the player put in is correct
            if(rmPass == joinRoomPassword)
            {
                PhotonNetwork.JoinRoom(rmName);
            }
            else
            {

                // Add notification to player that password is incorrect.
                Debug.LogWarning("Password entered is not correct. Expected password is " + rmPass + " but you have entered " + joinRoomPassword);

            }
        }
        else
        {
            PhotonNetwork.JoinRoom(rmName);
        }
    }

    /// <summary>
    /// Called from a given room button. Sets the controller values to know which room we want to try and join.
    /// Activates the join room button if not already active.
    /// Sets the password input to be active/deactive depending on the selected rooms security.
    /// </summary>
    /// <param name="roomName"></param>
    /// <param name="roomSecurity"></param>
    /// <param name="roomPass"></param>
    public void SelectedRoom(string roomName, bool roomSecurity, string roomPass)
    {
        if (roomSecurity)
        {
            joinRoomPasswordInput.SetActive(true);
        }
        else
        {
            joinRoomPasswordInput.SetActive(false);
        }
        rmName = roomName;
        rmSecurity = roomSecurity;
        rmPass = roomPass;
        lobbyJoinButton.SetActive(true);
    }

    /// <summary>
    /// Input function for changing room password
    /// </summary>
    /// <param name="passIn">The input to change the room password to</param>
    public void OnJoinPasswordChanged(string passIn)
    {
        joinRoomPassword = passIn;
    }

    #endregion
}
