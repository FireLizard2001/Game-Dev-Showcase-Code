#region Namespace
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#endregion

/// <summary>
/// Display the current time left in phase
/// </summary>
public class UITimer : MonoBehaviour
{
    #region Attributes
    private TMP_Text timerText;
    #endregion

    #region Methods
    // Start is called before the first frame update
    void Start()
    {
        timerText = gameObject.GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        DisplayTimer();
    }

    /// <summary>
    /// Function to display the current time left in the phase
    /// </summary>
    public void DisplayTimer()
    {
        if (GameManager.instance.timerRunning)
        {
            timerText.text = Mathf.Round(GameManager.instance.timeRemaining).ToString();
        }
    }
    #endregion
}
