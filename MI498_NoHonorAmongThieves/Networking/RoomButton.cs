using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomButton : MonoBehaviour
{
    [Tooltip("Display for room name")]
    [SerializeField] private TMP_Text nameText;
    [Tooltip("Display for room security")]
    [SerializeField] private TMP_Text securityText;
    [Tooltip("Display for room size")]
    [SerializeField] private TMP_Text sizeText;

    private CustomMatchmakingLobbyController myController;

    private string roomPassword;
    private bool roomSecurity;
    private string roomName; //String for saving room name
    private int roomSize; //String for saving room size
    private int playerCount; //How many players are in the lobby

    // Activates the ability to join a room.
    public void SelectRoomClick()
    {
        myController.SelectedRoom(roomName, roomSecurity, roomPassword);
    }

    //function to be called from lobby controller to set room variables
    public void SetRoom(string nameInput, int sizeInput, int countInput, string password, bool security, CustomMatchmakingLobbyController myControl)
    {
        myController = myControl;

        roomName = nameInput;
        roomSize = sizeInput;
        playerCount = countInput;
        roomPassword = password;
        roomSecurity = security;

        if (security)
        {
            securityText.text = "Private";
        }
        else
        {
            securityText.text = "Public";
        }

        nameText.text = nameInput;
        sizeText.text = countInput + "/" + sizeInput;
    }
}
