#region Namespace
using System.Collections;
using System.Collections.Generic;
using Inputs;
using PlayerFunctionality;
using UnityEngine;
using UnityEngine.UI;
#endregion

/// <summary>
/// Gets player input to toggle leaderboard UI
/// </summary>
public class LeaderboardToggleUI : MonoBehaviour
{
    #region Attributes
    [Header("References")]
    [Tooltip("Toggleable UI")]
    public GameObject leaderboard = null;

    bool _isToggling = false;       // Is the player currently sprinting?
    bool _isHeld = false;        // Is the sprint button being held?
    bool paused = false;

    GameObject[] playerList = null;
    #endregion


    #region Methods
    // Start is called before the first frame update
    void Start()
    {
        GameInputManager.Instance.LeaderboardDown += LeaderboardDown;
        GameInputManager.Instance.LeaderboardUp += LeaderboardUp;
        GameInputManager.Instance.PausePressed += AttemptPause;
    }

    // Update is called once per frame
    void Update()
    {
        CheckToggle();
    }

    /// <summary> Update is held when presed
    void LeaderboardDown() => _isHeld = true;
    /// <summary> Update is held when released
    void LeaderboardUp() => _isHeld = false;
    /// <summary> Update paused button
    public void AttemptPause() => paused = !paused;

    /// <summary>
    /// Check if we need to toggle leaderboard
    /// </summary>
    private void CheckToggle()
    {
        if (leaderboard != null)
        {
            if (_isHeld && !leaderboard.transform.GetChild(0).gameObject.activeSelf
                && GameManager.instance.gameState != GameManager.GameStates.Final && !paused)
            {
                ToggleLeaderboard();
            }
            else if (!_isHeld && leaderboard.transform.GetChild(0).gameObject.activeSelf)
            {
                ToggleLeaderboard();
            }
            else if((leaderboard.transform.GetChild(0).gameObject.activeSelf &&
                GameManager.instance.gameState == GameManager.GameStates.Final) || paused)
            {
                _isHeld = false;
                ToggleLeaderboard();
            }

        }
        else
        {
            Debug.Log("Need reference to toggleable leaderboard UI.");
        }
    }

    /// <summary>
    /// Toggle leaderboard
    /// </summary>
    private void ToggleLeaderboard()
    {
        leaderboard.GetComponent<Image>().enabled = _isHeld;
        leaderboard.transform.GetChild(0).gameObject.SetActive(_isHeld);
        if (playerList == null)
        {
            playerList = GameObject.FindGameObjectsWithTag("Player");
        }
        foreach (GameObject p in playerList)
        {
            if (p != null && p.GetComponent<AvatarSetup>().characterValue == PlayerPrefs.GetInt("MyCharacter"))
            {
                bool toggle = paused ? paused : _isHeld;
                p.transform.GetChild(p.transform.childCount - 2).gameObject.SetActive(!toggle);
                break;
            }
        }
    }
    #endregion
}
