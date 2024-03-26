using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using QFSW.QC;
using Photon.Pun;
using PlayerFunctionality;
using InteractableObjects;
using UnityEngine.UI;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
    // Static reference for objects to access GameManager
    public static GameManager instance = null;

    public static int scoreCap { set;  get; } = 3;

    static System.Random random = new System.Random();
    static System.Random randomMat = new System.Random();

    public List<Material> paintingmats;

    public bool isMaster = false;

    PhotonView PV;

    #region Game Variables

    public enum GameStates
    {
        Planning,
        Stealing,
        Alarm,
        Pause,
        Final
    }

    [Header("Game Values")]

    [Tooltip("Stealing phase timer length in seconds.")]
    public float stealingPhaseTimerLength = 60f;

    [Tooltip("Alarm phase timer length in seconds.")]
    public float alarmPhaseTimerLength = 60f;

    [Tooltip("Bool for if we want a round timer or not.")]
    public bool useTimer = true;

    /* Current time remaining in the phase. */
    [HideInInspector] public float timeRemaining = 60f;

    /* Signal for if the timer should be running or not. */
    [HideInInspector] public bool timerRunning = false;

    [Tooltip("The current game state")]
    [SerializeField] public GameStates gameState = GameStates.Planning;

    [Tooltip("Unity event for alarm phase trigger.")]
    [SerializeField] private UnityEvent alarmTrigger;

    [Tooltip("Unity event for the round end trigger.")]
    [SerializeField] private UnityEvent roundOverTrigger;

    [Tooltip("Reference to UI element to indicate who is holding the painting")]
    public GameObject paintingHolderUI = null;

    [Tooltip("Red Player Icon Color.")]
    public Color32 redColor;
    [Tooltip("Blue Icon Color.")]
    public Color32 blueColor;
    [Tooltip("Green Icon Color.")]
    public Color32 greenColor;
    [Tooltip("Yellow Icon Color.")]
    public Color32 yellowColor;

    #endregion
    private void Awake()
    {
        PV = GetComponent<PhotonView>();

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            DestroyImmediate(this.gameObject);
        }
    }

    private void Start()
    {
        ChangeState(GameStates.Stealing);
    }

    private void Update()
    {
        if (useTimer) HandleTimer();
    }

    /// <summary> Get the current game state </summary>
    public GameStates GetGameState() => gameState;

    /// <summary>
    /// Function that acts as a timer for the current phase and switches state when time has run out
    /// </summary>
    private void HandleTimer()
    {
        if (timerRunning)
        {
            if (timeRemaining > 0f)
            {
                timeRemaining -= Time.deltaTime;
            }
            else
            {
                EndTimer();
            }
        }

    }

    /// <summary>
    /// Function to prepare the timer with the correct timer length
    /// </summary>
    /// <param name="timerLength"></param> The length of time the timer should be counting down for
    private void StartTimer(float timerLength)
    {
        timeRemaining = timerLength;
        timerRunning = true;
    }

    /// <summary>
    /// Function to disable player movement at the end of rounds and evaluate loser scores
    /// </summary>
    public void HandlePlayersAfterRound()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            player.transform.GetComponentInChildren<Player_Camera_Network>().isPaused = true;
            player.transform.GetChild(player.transform.childCount - 2).gameObject.SetActive(false);

            if (!player.GetComponent<Player_Interact_Network>().escaped)
            {
                player.GetComponent<Player_Interact_Network>().ResetPoints();
            }
        }
    }

    /// <summary>
    /// Function to set this escaped player into spectator mode.
    /// </summary>
    /// <param name="escapedPlayer"></param> The player that just escaped
    public void PlayerEscaped(GameObject escapedPlayer)
    {
        // Prevents guards from tracking
        escapedPlayer.layer = LayerMask.NameToLayer("Default");

        // Prevent interaction / movement
        escapedPlayer.GetComponent<Player_Movement_Network>().enabled = false;
        escapedPlayer.GetComponent<Player_Interact_Network>().enabled = false;
        
        foreach (Transform child in escapedPlayer.transform)
        {
            // Disable every child except camera
            if (!child.CompareTag("MainCamera"))
                child.gameObject.SetActive(false);

            // Disable spotlight
            else
                child.GetChild(0).gameObject.SetActive(false);
        }

        Transform spectatorPoint = GameObject.FindObjectOfType<SpawnManager>().GetSpectatorSpawn();
        escapedPlayer.transform.position = new Vector3(spectatorPoint.position.x, spectatorPoint.position.y, spectatorPoint.position.z);

    }

    public void SetPaintingMaterials()
    {
        // If I am the master client
        if (isMaster)
        {
            PV.RPC("RPC_SetPaintingMaterials", RpcTarget.All);

        }
    }

    public void StartInGameMusic()
    {
        if (isMaster)
        {
            LeaderboardManager.Instance.StartGameMusic();
        }
    }

    #region Commands

    /// <summary>
    /// Function to determine which painting to set as the collectors.
    /// </summary>
    [Command]
    public void SelectCollectorPainting()
    {
        if (isMaster)
        {
            PV.RPC("RPC_SelectCollectorPainting", RpcTarget.All);
        }
    }

    /// <summary>
    /// Function to set an associated van to glow or stop glowing.
    /// </summary>
    [Command]
    public void MakeVanGlow(int whichVan, bool shouldGlow)
    {

        // For quick playtesting purposes, this boolean switches whether everyone sees a van glow or just the player who has the collector item.

        if (true) // Not networked
        {
            GameObject[] vansnewarray;
            vansnewarray = GameObject.FindGameObjectsWithTag("ExitZone");

            List<GameObject> vans = new List<GameObject>(vansnewarray);

            foreach (GameObject van in vans)
            {
                if (van.GetComponent<DropZone>().playerNum == whichVan)
                {
                    if (shouldGlow)
                    {
                        van.GetComponent<DropZone>().openVan.layer = LayerMask.NameToLayer("Held Non-Interactable");

                        // chevron handler
                        van.GetComponent<DropZone>().openVan.gameObject.GetComponent<Target>().enabled = true;
                    }
                    else
                    {
                        van.GetComponent<DropZone>().openVan.layer = LayerMask.NameToLayer("Default");

                        // chevron handler
                        van.GetComponent<DropZone>().openVan.GetComponent<Target>().enabled = false;
                    }
                }
            }
        }
        else // Networked
        {
            //Debug.Log("Call to SetVanGlow: Shouldglow? " + shouldGlow + " vanID? " + whichVan);
            PV.RPC("RPC_MakeVanGlow", RpcTarget.All, whichVan, shouldGlow);
        }
    }

    /// <summary>
    /// Function to set an associated van to glow or stop glowing.
    /// </summary>
    [Command]
    public void UpdatePaintingHolderUI(int whichPlayer, string name, bool beingHeld)
    {
        PV.RPC("RPC_UpdatePaintingHolderUI", RpcTarget.All, whichPlayer, name, beingHeld);
    }

    /// <summary>
    /// Helper function for resetting the scene
    /// </summary>
    [Command]
    public void ResetScene()
    {
        PV.RPC("RPC_ResetScene", RpcTarget.All);
    }

    /// <summary>
    /// Changes the current game state and calls associated functions
    /// </summary>
    /// <param name="newState">The new game state</param>
    [Command]
    public void ChangeState(GameStates newState)
    {
        gameState = newState;

        //Debug.Log("Switching to " + gameState);

        PV.RPC("RPC_ChangeState", RpcTarget.All, gameState);
    }

    /// <summary>
    /// Handler Function for ending the round timer.
    /// </summary>
    [Command]
    private void EndTimer()
    {
        PV.RPC("RPC_EndTimer", RpcTarget.All);
        switch (gameState)
        {
            case GameStates.Stealing:
                ChangeState(GameStates.Alarm);
                break;
            case GameStates.Alarm:
                ChangeState(GameStates.Final);
                break;
        }
    }

    #endregion

    #region Networking
    [PunRPC]
    void RPC_SetPaintingMaterials()
    {
        /// This should be simpler but it checks if a list of paintings has been found, if not then it finds one and sets it for all players.
        GameObject[] paintingsnewarray;
        paintingsnewarray = GameObject.FindGameObjectsWithTag("Painting");

        List<GameObject> paintings = new List<GameObject>(paintingsnewarray);

        foreach (GameObject painting in paintings)
        {
            int index = randomMat.Next(paintingmats.Count);
            painting.transform.GetChild(0).transform.GetChild(0).GetComponent<Renderer>().material = paintingmats[index];
        }
    }

    [PunRPC]
    void RPC_SelectCollectorPainting()
    {
        //Debug.Log("Selecting Painting: am master? : " + isMaster);
        // If I am the master client
        if (isMaster)
        {

            /// This should be simpler but it checks if a list of paintings has been found, if not then it finds one and sets it for all players.
            GameObject[] paintingsnewarray;
            paintingsnewarray = GameObject.FindGameObjectsWithTag("Painting");

            List<GameObject> paintings = new List<GameObject>(paintingsnewarray);

            if (paintings.Count == 0)
            {
                Debug.Log("No game objects are tagged with 'Painting'");
            }

            // Choose Random Painting in List
            int index = random.Next(paintings.Count);
            paintings[index].GetComponent<Interactable>().SetToCollectorsItem();
        }
    }

    [PunRPC]
    void RPC_MakeVanGlow(int whichVan, bool shouldGlow)
    {
        /// This should be simpler but it checks if a list of paintings has been found, if not then it finds one and sets it for all players.
        GameObject[] vansnewarray;
        vansnewarray = GameObject.FindGameObjectsWithTag("ExitZone");

        List<GameObject> vans = new List<GameObject>(vansnewarray);

        foreach (GameObject van in vans)
        {
            if (van.GetComponent<DropZone>().playerNum == whichVan)
            {
                if (shouldGlow)
                {
                    van.GetComponent<DropZone>().openVan.layer = LayerMask.NameToLayer("Held Non-Interactable");

                    // chevron handler
                    van.GetComponent<DropZone>().openVan.gameObject.GetComponent<Target>().enabled = true;
                }
                else
                {
                    van.GetComponent<DropZone>().openVan.layer = LayerMask.NameToLayer("Default");

                    // chevron handler
                    van.GetComponent<DropZone>().openVan.GetComponent<Target>().enabled = false;
                }
            }
        }
    }

    [PunRPC]
    void RPC_UpdatePaintingHolderUI(int whichPlayer, string name, bool beingHeld)
    {
        if (paintingHolderUI != null)
        {
            if (beingHeld)
            {
                paintingHolderUI.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text = "Painting Held By:";
                paintingHolderUI.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = name;
                paintingHolderUI.transform.GetChild(1).gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-45f, paintingHolderUI.transform.GetChild(1).gameObject.GetComponent<RectTransform>().anchoredPosition.y);
                switch (whichPlayer)
                {
                    case 0:
                        paintingHolderUI.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().color = redColor;
                        break;
                    case 1:
                        paintingHolderUI.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().color = blueColor;
                        break;
                    case 2:
                        paintingHolderUI.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().color = greenColor;
                        break;
                    case 3:
                        paintingHolderUI.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().color = yellowColor;
                        break;
                    default:
                        Debug.Log("Not a valid character ID");
                        break;
                }
            }
            else
            {
                paintingHolderUI.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = "";
                paintingHolderUI.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text = "The painting is up for grabs!";
                paintingHolderUI.transform.GetChild(1).gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, paintingHolderUI.transform.GetChild(1).gameObject.GetComponent<RectTransform>().anchoredPosition.y);
            }
        }
        else
        {
            Debug.Log("Need reference to painting holder UI element.");
        }
    }

    [PunRPC]
    void RPC_EndTimer()
    {
        timeRemaining = 0;
        timerRunning = false;
    }

    [PunRPC]
    void RPC_ResetScene()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    [PunRPC]
    void RPC_ChangeState(GameStates gameState)
    {
        switch (gameState)
        {
            case GameStates.Planning:
                // Planning phase functions here
                break;
            case GameStates.Stealing:
                // Stealing phase functions here
                StartTimer(stealingPhaseTimerLength);
                break;
            case GameStates.Alarm:
                // Alarm phase functions here
                StartTimer(alarmPhaseTimerLength);
                alarmTrigger.Invoke();
                //HandleAlarmPhase();
                break;
            case GameStates.Pause:
                // Pause functions here
                break;
            case GameStates.Final:
                // Final (game over) functions here
                roundOverTrigger.Invoke();
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                Time.timeScale = 0;
                LeaderboardManager.Instance.UpdateAllLeaderboards(true);
                break;
        }
    }

    #endregion
}
