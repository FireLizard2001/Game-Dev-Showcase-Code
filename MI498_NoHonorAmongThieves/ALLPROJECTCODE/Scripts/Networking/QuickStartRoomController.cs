using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using UnityEngine.SceneManagement;

public class QuickStartRoomController : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private int multiplayerSceneIndex; //Number for the build index to the multiplayer scene
    public int currentScene;
    public static QuickStartRoomController room;

    private void Awake()
    {
        //var objects = FindObjectsOfType<RoomController>();
        //foreach (var obj in objects) { Destroy(obj); }
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

/*    public override void OnEnable()
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
        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        //Called when multiplayer scene is loaded
        currentScene = scene.buildIndex;
        if (currentScene == multiplayerSceneIndex)
        {
            CreatePlayer();
        }
    }*/

    public override void OnJoinedRoom() //Callback function for getting int a room
    {
        Debug.Log("Joined Room");
        StartGame();
    }

    private void StartGame() //Function for starting the game
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Starting Game");
            PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().name);
        }
    }

    //Creates a networked player for each person that joins into multiplay scene
    private void CreatePlayer()
    {
        //Debug.Log("Creating Player");
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PhotonNetworkPlayer"), Vector3.zero, Quaternion.identity); //PhotonNetworkPlayer name must match prefab and folder name
    }
}
