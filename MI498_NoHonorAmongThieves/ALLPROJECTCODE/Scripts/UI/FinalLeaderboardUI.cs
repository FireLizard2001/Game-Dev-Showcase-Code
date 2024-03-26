//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;

//public class FinalLeaderboardUI : LeaderboardUI
//{

//    // The in game leaderboard
//    public LeaderboardUI leaderboard;

//    // Start is called before the first frame update
//    protected override void Awake()
//    {
//        scores = leaderboard.scores;

//        int count = 0;
//        foreach ((string, int) score in scores)
//        {
//            // Update leaderboard score
//            transform.GetChild(0).GetChild(count + 1).gameObject.SetActive(true);
//            transform.GetChild(0).GetChild(count + 1).gameObject.GetComponent<TMP_Text>().text
//                = score.Item1 + ": "
//                + score.Item2;
//            count++;
//        }
//    }
//}
