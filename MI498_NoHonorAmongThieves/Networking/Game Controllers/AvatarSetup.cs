using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarSetup : MonoBehaviour
{
    private PhotonView PV;
    public int characterValue;
    public GameObject myCharacter;
    public GameObject BackUpModel;

    // Start is called before the first frame update
    void Awake()
    {
        PV = GetComponent<PhotonView>();

        GameManager GM = GameObject.Find("GameManager").GetComponent<GameManager>();

        if (PV.IsMine)
        {
            if (PlayerPrefs.GetInt("MyCharacter") == 0)
            {
                GM.isMaster = true;
                GM.SetPaintingMaterials();
                GM.SelectCollectorPainting();

                GM.StartInGameMusic();
            }
            PV.RPC("RPC_AddCharacter", RpcTarget.AllBuffered, PlayerPrefs.GetInt("MyCharacter"));
        }
    }

    [PunRPC]
    void RPC_AddCharacter(int whichCharacter)
    {
        characterValue = whichCharacter;
        try
        {
            myCharacter = Instantiate(PlayerInfo.PI.allCharacters[whichCharacter], transform.position, transform.rotation, transform);
        }
        catch
        {
            myCharacter = Instantiate(BackUpModel, transform.position, transform.rotation, transform);
        }
    }
}
