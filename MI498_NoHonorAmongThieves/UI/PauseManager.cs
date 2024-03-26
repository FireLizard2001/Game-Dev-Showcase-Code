#region Namespace
using System.Collections;
using System.Collections.Generic;
using Inputs;
using PlayerFunctionality;
using UnityEngine;
using UnityEngine.EventSystems;
#endregion

/// <summary>
/// Handle player pause input and menu
/// </summary>
public class PauseManager : MonoBehaviour
{
    #region attributes
    [Tooltip("Pause menu reference.")]
    public GameObject pauseScreen = null;

    private bool paused = false;    // Inidcates if we are paused

    GameObject[] playerList = null;

    #endregion

    #region method

    // Start is called before the first frame update
    void Start()
    {
        GameInputManager.Instance.PausePressed += AttemptPause;
        if (pauseScreen == null)
        {
            Debug.Log("Need reference to pause screen UI.");
        }
    }

    /// <summary>
    /// Attempts to bring up the pause menu of the player
    /// </summary>
    public void AttemptPause()
    {
        if (GameManager.instance.gameState != GameManager.GameStates.Final)
        {
            if (playerList == null)
            {
                playerList = GameObject.FindGameObjectsWithTag("Player");
            }
            paused = !paused;
            pauseScreen.SetActive(paused);
            ChangePlayerPauseState();
            Cursor.visible = paused;
            if (paused)
            {
                Cursor.lockState = CursorLockMode.None;
                EventSystem.current.SetSelectedGameObject(pauseScreen.transform.GetChild(0).GetChild(0).gameObject);
            }
            else
            {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    /// <summary>
    /// Updates the player movement and interact states when paused
    /// </summary>
    private void ChangePlayerPauseState()
    {
        foreach (GameObject p in playerList)
        {
            if (p.GetComponent<AvatarSetup>().characterValue == PlayerPrefs.GetInt("MyCharacter"))
            {
                p.transform.GetChild(p.transform.childCount - 2).gameObject.SetActive(!paused);
                p.GetComponent<Player_Movement>().isPaused = paused;
                p.GetComponent<Player_Interact>().isPaused = paused;
                p.transform.GetComponentInChildren<Player_Camera_Network>().isPaused = paused;
                break;
            }
        }
    }

    #endregion
}
