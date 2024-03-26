#region Namespace
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayerFunctionality;
using TMPro;
using System.Linq;
#endregion

public class LeaderboardUI : MonoBehaviour
{
    #region Attributes

    [Header("Colors for players")]
    [Tooltip("Player One Color")]
    public Color32 playerOneColor;
    [Tooltip("Player Two Color")]
    public Color32 playerTwoColor;
    [Tooltip("Player Three Color")]
    public Color32 playerThreeColor;
    [Tooltip("Player Four Color")]
    public Color32 playerFourColor;

    // Dictionary to hold player scores
    private SortedDictionary<string, int> playerDict = null;
    // Dictionary to hold mapping of player name to id
    private SortedDictionary<string, int> nicknameToPlayerID = null;

    [SerializeField] GameObject _scoreBurst;

    // The leaderboard UI transform
    Transform leaderboard = null;
    #endregion

    #region Methods

    public void PopulateLeaderboard()
    {
        if (leaderboard == null) leaderboard = transform.GetChild(0);
        playerDict = LeaderboardManager.Instance.GetScores();
        nicknameToPlayerID = LeaderboardManager.Instance.GetNameToID();
        InitializeMaxScore();
        UpdateValues();
    }

    public void SpawnScoreParticle()
    {
        Quaternion spawnRotation = Quaternion.Euler(-90f, 0f, 0f);
        Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z);
        if (_scoreBurst != null)
        {
            GameObject newScoreEffect = Instantiate(_scoreBurst, spawnPosition, spawnRotation);

            Destroy(newScoreEffect, 1f);
        }
    }

    /// <summary>
    /// Function to update all the values in the leaderboard
    /// </summary>
    public void UpdateValues()
    {
        int count = 0;
        var sortedDict = from entry in playerDict orderby entry.Value descending select entry;
        foreach (KeyValuePair<string, int> playerData in sortedDict)
        {
            if (playerData.Key == "") { continue; }
            // Update leaderboard score
            leaderboard.GetChild(count + 1).gameObject.SetActive(true);
            leaderboard.GetChild(count + 1).gameObject.GetComponent<TMP_Text>().text = playerData.Key;
            leaderboard.GetChild(count + 1).GetChild(0).gameObject.GetComponent<Slider>().value = playerData.Value;
            leaderboard.GetChild(count + 1).GetChild(1).gameObject.GetComponent<TMP_Text>().text = playerData.Value.ToString();
            UpdateColor(playerData.Key, count);
            count++;
        }
    }

    /// <summary>
    /// Sets all max slider values to the max score and displays the text to indicate max score
    /// </summary>
    private void InitializeMaxScore()
    {
        for (int i = 1; i < transform.GetChild(0).childCount - 1; i++)
        {
            leaderboard.GetChild(i).GetChild(0).gameObject.GetComponent<Slider>().maxValue = GameManager.scoreCap;
        }

        leaderboard.GetChild(transform.GetChild(0).childCount - 1).gameObject.GetComponent<TMP_Text>().text =
            "First To " + GameManager.scoreCap + " Paintings Wins!";
    }

    /// <summary>
    /// Updates the leaderboard elements to that players color
    /// </summary>
    /// <param name="name"></param> The name of the player we want to change the color to
    private void UpdateColor(string name, int count)
    {
        switch (nicknameToPlayerID[name])
        {
            case 0:
                leaderboard.GetChild(count + 1).gameObject.GetComponent<TMP_Text>().color = playerOneColor;
                leaderboard.GetChild(count + 1).GetChild(0).GetChild(1).gameObject.GetComponent<Image>().color = playerOneColor;
                leaderboard.GetChild(count + 1).GetChild(1).gameObject.GetComponent<TMP_Text>().color = playerOneColor;
                break;
            case 1:
                leaderboard.GetChild(count + 1).gameObject.GetComponent<TMP_Text>().color = playerTwoColor;
                leaderboard.GetChild(count + 1).GetChild(0).GetChild(1).gameObject.GetComponent<Image>().color = playerTwoColor;
                leaderboard.GetChild(count + 1).GetChild(1).gameObject.GetComponent<TMP_Text>().color = playerTwoColor;
                break;
            case 2:
                leaderboard.GetChild(count + 1).gameObject.GetComponent<TMP_Text>().color = playerThreeColor;
                leaderboard.GetChild(count + 1).GetChild(0).GetChild(1).gameObject.GetComponent<Image>().color = playerThreeColor;
                leaderboard.GetChild(count + 1).GetChild(1).gameObject.GetComponent<TMP_Text>().color = playerThreeColor;
                break;
            case 3:
                leaderboard.GetChild(count + 1).gameObject.GetComponent<TMP_Text>().color = playerFourColor;
                leaderboard.GetChild(count + 1).GetChild(0).GetChild(1).gameObject.GetComponent<Image>().color = playerFourColor;
                leaderboard.GetChild(count + 1).GetChild(1).gameObject.GetComponent<TMP_Text>().color = playerFourColor;
                break;
            default:
                Debug.Log("Invalid player ID");
                break;
        }
    }
    #endregion
}
