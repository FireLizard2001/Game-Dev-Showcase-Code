using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class GuardUI : MonoBehaviour
{
    #region Inspector Variables
    [Header("UI References")]

    [Tooltip("Reference to the detect countdown text")]
    [SerializeField] private TextMeshProUGUI detectCountdown;

    [Tooltip("Reference to the static icon")]
    [SerializeField] private Image staticIcon;

    [Tooltip("Reference to the fill icon")]
    [SerializeField] private Image fillIcon;

    [Tooltip("Color for patrolling")]
    [SerializeField] private Color patrolColor;

    [Tooltip("Color for investigating")]
    [SerializeField] private Color investigateColor;

    [Tooltip("Color for detecting")]
    [SerializeField] private Color detectColor;

    [Tooltip("Color for chasing")]
    [SerializeField] private Color chaseColor;

    [Tooltip("Color for stun")]
    [SerializeField] private Color stunColor;

    [Header("Debugger References")]

    [Tooltip("Should the debug menu be on?")]
    public bool debugMenuOn = false;

    [Tooltip("Reference to the name text")]
    [SerializeField] private TextMeshProUGUI nameText;

    [Tooltip("Reference to the state text")]
    [SerializeField] private TextMeshProUGUI stateText;

    [Tooltip("Reference to the target text")]
    [SerializeField] private TextMeshProUGUI targetText;

    [Tooltip("Reference to PhotonView")]
    [SerializeField] PhotonView PV;
    #endregion

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    void Start()
    {
        // Sets the name of the guard
        nameText.text = transform.root.name;

        ToggleDebugger(debugMenuOn);

        fillIcon.color = detectColor;
        fillIcon.fillAmount = 0;
    }

    /// <summary>
    /// Sets detect countdown text
    /// </summary>
    /// <param name="newText">The new detect timer value</param>
    public void SetDetectCountdown(string newText)
    {
        detectCountdown.text = newText;
    }

    public void SetDetectFill(float fillValue)
    {
        fillIcon.fillAmount = fillValue;
    }

    public void ChangeAlert(Pathing_Network.GuardStates newState)
    {
        PV.RPC("RPC_ChangeAlert", RpcTarget.All, newState);
    }

    [PunRPC]
    public void RPC_ChangeAlert(Pathing_Network.GuardStates newState)
    {
        fillIcon.fillAmount = 0;

        switch (newState)
        {
            case Pathing_Network.GuardStates.Patrolling:
                staticIcon.color = patrolColor;
                break;

            case Pathing_Network.GuardStates.Investigating:
                staticIcon.color = investigateColor;
                break;

            case Pathing_Network.GuardStates.Detecting:
                staticIcon.color = investigateColor;
                break;

            case Pathing_Network.GuardStates.Chasing:
                staticIcon.color = chaseColor;
                break;

            case Pathing_Network.GuardStates.Stunned:
                staticIcon.color = stunColor;
                break;
        }
    }

    /// <summary>
    /// Toggles the debugger on and off
    /// </summary>
    /// <param name="debugOn"></param>
    public void ToggleDebugger(bool debugOn)
    {
        stateText.transform.parent.gameObject.SetActive(debugOn);
    }

    /// <summary>
    /// Setter for state text
    /// </summary>
    /// <param name="newState">new state string</param>
    public void UpdateState(string newState)
    {
        stateText.text = newState;
    }

    /// <summary>
    /// Setter for target text
    /// </summary>
    /// <param name="newTarget">new target string</param>
    public void UpdateTarget(string newTarget)
    {
        targetText.text = newTarget;
    }
}
