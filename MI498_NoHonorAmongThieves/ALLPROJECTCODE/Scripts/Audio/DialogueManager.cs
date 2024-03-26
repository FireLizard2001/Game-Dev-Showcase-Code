using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DialogueManager : MonoBehaviour
{

    public static DialogueManager instance;

    [Header("UI References")]

    [Tooltip("Reference to the parent of the collector dialogue")]
    [SerializeField] private GameObject collectorUI;

    [Tooltip("The amount of time the dialogue card stays active")]
    [SerializeField] private float dialogueUILength = 2f;

    [Tooltip("The target location of the dialogue UI")]
    [SerializeField] private Vector3 onScreenLocation;

    private Vector3 target;
    private Vector3 initialLocation;
    private bool isDialogue = false;
    private float dialogueTimer = 0f;
    private bool isMoving = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

            initialLocation = collectorUI.transform.localPosition;
            target = initialLocation;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Update()
    {
        if (isDialogue)
        {
            dialogueTimer += Time.deltaTime;

            if (dialogueTimer >= dialogueUILength)
            {
                dialogueTimer = 0;
                isDialogue = false;
                target = initialLocation;
                isMoving = true;
            }
        }

        if (isMoving)
        {
            MoveCollectorUI();
        }
    }

    public void MoveCollectorUI()
    {
        if (Vector3.Distance(collectorUI.transform.localPosition, target) >= 5f)
        {
            collectorUI.transform.localPosition = Vector3.Lerp(collectorUI.transform.localPosition, target, 10 * Time.deltaTime);
        }
        else
        {
            isMoving = false;
        }
    }

    public void PlayLocalDialogue(string eventName)
    {
        isDialogue = true;
        target = onScreenLocation;
        isMoving = true;

        AkSoundEngine.PostEvent(eventName, this.gameObject);
    }

    public void PlayNetworkDialogue(string eventName, int callerID)
    {
        if (PlayerPrefs.GetInt("MyCharacter") == callerID)
        {
            gameObject.GetComponent<PhotonView>().RPC("PlayDialogueNetwork", RpcTarget.Others, eventName);
        }
    }

    [PunRPC]
    private void PlayDialogueNetwork(string eventName)
    {
        isDialogue = true;
        target = onScreenLocation;
        isMoving = true;

        AkSoundEngine.PostEvent(eventName, this.gameObject);
    }
}
