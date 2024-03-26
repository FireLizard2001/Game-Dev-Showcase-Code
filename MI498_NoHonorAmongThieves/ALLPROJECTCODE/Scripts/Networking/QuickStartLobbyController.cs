using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class QuickStartLobbyController : MonoBehaviourPunCallbacks
{
    public static QuickStartLobbyController room;

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
        gameObject.transform.parent = null;
        DontDestroyOnLoad(this.gameObject);
    }

    /// <summary>
    /// Function to connect to master server
    /// </summary>
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        //Sets the player to a random team
        PlayerPrefs.SetInt("MyCharacter", Random.Range(0, 3));

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
        PhotonNetwork.JoinRandomRoom();    
    }

    /// <summary>
    /// Function to try creating a room after joining fails
    /// </summary>
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to join a room");
        CreateRoom();
    }

    /// <summary>
    /// Function to handle room creation
    /// </summary>
    void CreateRoom()
    {
        Debug.Log("Creating new room...");
        int randomRoomNumber = Random.Range(0, 10000); //creating a random name for the room
        RoomOptions roomOps = new RoomOptions()
        {
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = (byte)4,
            //CustomRoomProperties = table
        };
        PhotonNetwork.CreateRoom("Room joined with code: " + randomRoomNumber, roomOps); //Attempts to make a new room
        Debug.Log(randomRoomNumber);
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

    /// <summary>
    /// Function that is called after joining a room
    /// </summary>
    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Starting Game");
            PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().name);
        }
        
    }


    //Creates a networked player for each person that joins into multiplay scene
    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Creating Player");
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PhotonNetworkPlayer"), Vector3.zero, Quaternion.identity); //PhotonNetworkPlayer name must match prefab and folder name
        }
    }
}
