using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class to initilize the networking
public class NetworkController : MonoBehaviourPunCallbacks
{
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings(); //Connects to Photon master servers

        // Note: There are other ways to connect, this is simply the easiest. Other ways can be found on the photonengine docs.
    }

    // Returns connect region after successful connection
    public override void OnConnectedToMaster()
    {
        Debug.Log("We are now connected to the " + PhotonNetwork.CloudRegion + " server!");
    }
}
