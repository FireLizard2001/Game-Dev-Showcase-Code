using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using PlayerFunctionality;

public class Pathing_Network : MonoBehaviour
{
    #region Inspector Variables
    public enum GuardStates
    {
        Patrolling,
        Investigating,
        Detecting,
        Chasing,
        Stunned
    }

    public enum GuardName
    {
        TJ,
        Adam,
        Drew
    }

    [Header("Guard Values")]

    [Tooltip("Current state of the guard")]
    [SerializeField] private GuardStates guardState = GuardStates.Patrolling;

    [Tooltip("Current name of the guard")]
    [SerializeField] private GuardName guardName = GuardName.TJ;

    [Header("Patrol Values")]

    [Tooltip("Reference to the patrol mover object")]
    [SerializeField] private Transform patrolMover;

    [Tooltip("The Y-rotation angle for guard sweeping")]
    [SerializeField] private float sweepAngle = 90f;

    [Tooltip("The speed of guard sweeping")]
    [SerializeField] private float sweepSpeed = 0.5f;

    [Header("Investigation Values")]

    [Tooltip("Speed when heading to an investigation point")]
    [SerializeField] private float investigationSpeed = 8f;

    [Tooltip("The time in which a guard waits at an investigation point")]
    [SerializeField] private float investigationTime = 8f;

    [Header("Chase Values")]

    [Tooltip("Speed during a guard chase")]
    [SerializeField] private float chaseSpeed = 5f;

    [Header("Stun Values")]

    [Tooltip("The length of a stun")]
    [SerializeField] private float stunTime = 3f;

    #endregion

    #region Patrolling Variables

    // Is the guard currently moving?
    private bool isMoving = true;

    // The current running timer for waiting
    private float currWaitTimer = 0f;

    // The time to wait at the current point (pulled from the PatrolPoint script)
    private float currWaitTime = 0f;

    // The lower rotation of a guard sweep angle
    private float lowerRotation = 0f;

    // The higher rotation of a guard sweep angle
    private float higherRotation = 0f;

    #endregion

    #region Investigation Variables

    // Reference to the target transform
    private Transform target;

    #endregion

    #region Chase Variables

    [Tooltip("Reference to the caught smoke VFX")]
    [SerializeField] private GameObject caughtVFX;

    [Tooltip("Catching VFX offset")]
    [SerializeField] private Vector3 vfxOffset;

    [Tooltip("Buffer for unreachable status")]
    [SerializeField] private float unreachableBuffer = 1.5f;

    #endregion

    #region References

    // Reference to the NavMeshAgent for movement
    private NavMeshAgent agent;

    public Animator anim;

    // Reference to the detection cone
    private VisionCone_Network detection;

    private bool isCatching = false;

    // Reference to the Guard UI
    private GuardUI UI;

    PhotonView PV;

    private bool isStunned = false;
    private float unReachableTimer = 0;

    #endregion

    void Start()
    {
        // Grab NavMeshAgent reference and set speed
        agent = GetComponent<NavMeshAgent>();
        detection = GetComponent<VisionCone_Network>();
        UI = GetComponent<GuardUI>();
        anim = GetComponentInChildren<Animator>();
        PV = GetComponent<PhotonView>();

        if (patrolMover != null)
        {
            agent.speed = patrolMover.GetComponent<PatrolMover_Network>().patrolSpeed;
            patrolMover.GetComponent<PatrolMover_Network>().SetPathing(this);
        }

        if (patrolMover == null)
        {
            ChangeState(GuardStates.Stunned);
        }
        else if (patrolMover != null)
        {
            ChangeState(GuardStates.Patrolling);
        }

        AkSoundEngine.SetSwitch("GuardName", guardName.ToString(), this.gameObject);
    }

    private void Update()
    {
        // Switch logic based on current guard state
        switch (guardState)
        {
            case GuardStates.Patrolling:

                // Have guard follow the mover full time
                if (target != null)
                {
                    agent.SetDestination(target.position);
                }
                MovementCheck();

                break;
            case GuardStates.Investigating:
                MovementCheck();
                break;
            case GuardStates.Detecting:

                if (target != null)
                {
                    transform.LookAt(target);
                }

                break;
            case GuardStates.Chasing:
                ChasePlayer();
                break;
            case GuardStates.Stunned:
                break;
        }
      
    }

    /// <summary>
    /// Getter for the current guard state
    /// </summary>
    /// <returns>The current guard state</returns>
    public GuardStates GetState()
    {
        return guardState;
    }

    /// <summary>
    /// Changes the current state of the guard
    /// </summary>
    /// <param name="newState">New guard state</param>
    public void ChangeState(GuardStates newState)
    {
        guardState = newState;

        // Switch logic based on current guard state
        switch (guardState)
        {
            case GuardStates.Patrolling:

                // Sets target to patrol mover and resets speed
                target = patrolMover;
                agent.speed = patrolMover.GetComponent<PatrolMover_Network>().patrolSpeed;
                detection.ToggleSearching(true);
                UI.ChangeAlert(GuardStates.Patrolling);

                PV.RPC("RPC_ResetTrigger", RpcTarget.All, "IdleTrigger", this.gameObject.GetPhotonView().ViewID);
                PV.RPC("RPC_SetTrigger", RpcTarget.All, "MoveTrigger", this.gameObject.GetPhotonView().ViewID);
                PV.RPC("RPC_SetBool", RpcTarget.All, "IsSprinting", this.gameObject.GetPhotonView().ViewID, false);

                break;
            case GuardStates.Investigating:

                // Sends guard to the triggered point
                if (target != null)
                {
                    agent.SetDestination(target.position);
                    agent.speed = investigationSpeed;
                    isMoving = true;
                    detection.ToggleSearching(true);
                    UI.ChangeAlert(GuardStates.Investigating);

                    PV.RPC("RPC_ResetTrigger", RpcTarget.All, "IdleTrigger", this.gameObject.GetPhotonView().ViewID);
                    PV.RPC("RPC_SetTrigger", RpcTarget.All, "MoveTrigger", this.gameObject.GetPhotonView().ViewID);
                    PV.RPC("RPC_SetBool", RpcTarget.All, "IsSprinting", this.gameObject.GetPhotonView().ViewID, true);

                    if (PlayerPrefs.GetInt("MyCharacter") == detection.GetTargetID())
                    {
                        AkSoundEngine.StopAll(this.gameObject);
                        AkSoundEngine.PostEvent("Investigate", this.gameObject);
                    }
                }

                break;
            case GuardStates.Detecting:

                // Handles stopping movement
                agent.ResetPath();
                UI.ChangeAlert(GuardStates.Detecting);

                PV.RPC("RPC_SetTrigger", RpcTarget.All, "IdleTrigger", this.gameObject.GetPhotonView().ViewID);
                PV.RPC("RPC_ResetTrigger", RpcTarget.All, "MoveTrigger", this.gameObject.GetPhotonView().ViewID);

                if (PlayerPrefs.GetInt("MyCharacter") == detection.GetTargetID()) 
                {
                    AkSoundEngine.StopAll(this.gameObject);
                    AkSoundEngine.PostEvent("EnterDetection", this.gameObject);
                }

                break;
            case GuardStates.Chasing:

                // Sets speed for chasing
                agent.speed = chaseSpeed;
                detection.ToggleSearching(false);
                UI.ChangeAlert(GuardStates.Chasing);

                PV.RPC("RPC_ResetTrigger", RpcTarget.All, "IdleTrigger", this.gameObject.GetPhotonView().ViewID);
                PV.RPC("RPC_SetTrigger", RpcTarget.All, "MoveTrigger", this.gameObject.GetPhotonView().ViewID);
                PV.RPC("RPC_SetBool", RpcTarget.All, "IsSprinting", this.gameObject.GetPhotonView().ViewID, true);

                if (PlayerPrefs.GetInt("MyCharacter") == detection.GetTargetID())
                {
                    AkSoundEngine.StopAll(this.gameObject);
                    AkSoundEngine.PostEvent("PlayerSpotted", this.gameObject);
                }

                break;
            case GuardStates.Stunned:
                if (!isStunned)
                {
                    isStunned = true;
                    agent.ResetPath();
                    StartCoroutine("StunSequence");
                }

                break;
        }

        // Update debugger UI if enabled
        if (UI.debugMenuOn)
        {
            UI.UpdateState(guardState.ToString());

            if (target != null)
            {
                UI.UpdateTarget(target.name);
            }
        }
    }

    /// <summary>
    /// Handles the waiting sequence of the patrolling state
    /// </summary>
    public void MovementCheck()
    {
        // If agent stops moving (while patrolling), start wait sequence
        if (agent.velocity.magnitude == 0)
        {
            // Resets wait timers
            if (isMoving)
            {
                isMoving = false;

                currWaitTime = investigationTime;
                currWaitTimer = 0f;

                PV.RPC("RPC_SetTrigger", RpcTarget.All, "IdleTrigger", this.gameObject.GetPhotonView().ViewID);
                PV.RPC("RPC_ResetTrigger", RpcTarget.All, "MoveTrigger", this.gameObject.GetPhotonView().ViewID);

                // Sets the angle spread for guard sweeping based on initial direction

                float initDir = guardState == GuardStates.Patrolling ? 
                    patrolMover.GetComponent<PatrolMover_Network>().GetAngle() : transform.rotation.eulerAngles.y;

                lowerRotation = initDir - sweepAngle / 2;
                higherRotation = initDir + sweepAngle / 2;
            }

            // Guard sweeping while stopped at a point/investigation area
            float rY = Mathf.SmoothStep(lowerRotation, higherRotation, Mathf.PingPong(Time.time * sweepSpeed, 1));
            transform.rotation = Quaternion.Euler(0, rY, 0);

            currWaitTimer += Time.deltaTime;

            // Decide next move after waiting is done
            if (currWaitTimer >= currWaitTime && guardState == GuardStates.Investigating)
            {
                ChangeState(GuardStates.Patrolling);

                if (PlayerPrefs.GetInt("MyCharacter") == detection.GetTargetID())
                {
                    AkSoundEngine.StopAll(this.gameObject);
                    AkSoundEngine.PostEvent("PlayerSpotted", this.gameObject);
                }
            }
        }
        else
        {
            if (!isMoving)
            {
                isMoving = true;

                PV.RPC("RPC_ResetTrigger", RpcTarget.All, "IdleTrigger", this.gameObject.GetPhotonView().ViewID);
                PV.RPC("RPC_SetTrigger", RpcTarget.All, "MoveTrigger", this.gameObject.GetPhotonView().ViewID);
                PV.RPC("RPC_SetBool", RpcTarget.All, "IsSprinting", this.gameObject.GetPhotonView().ViewID, false);
            }
        }
    }

    /// <summary>
    /// Chases a target over time
    /// </summary>
    public void ChasePlayer()
    {
        if (target == null) { return;  }

        NavMeshPath navMeshPath = new NavMeshPath();

        if (agent.CalculatePath(target.position, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete)
        {
            agent.SetDestination(target.position);
            unReachableTimer = 0;
        }
        else
        {
            unReachableTimer += Time.deltaTime;

            if (unReachableTimer >= unreachableBuffer)
            {
                ChangeState(GuardStates.Patrolling);
            }
        }
    }

    /// <summary>
    /// Triggers an investigation sequence for a guard
    /// </summary>
    /// <param name="trigger">The transform of the source trigger</param>
    public void TriggerInvestigation(Transform trigger, bool isSound)
    {
        if (guardState == GuardStates.Chasing && isSound)
        {
            return;
        }

        if (guardState == GuardStates.Stunned && isSound)
        {
            return;
        }

        target = trigger;
        ChangeState(GuardStates.Investigating);
    }

    /// <summary>
    /// Triggers a detection sequence for a guard
    /// </summary>
    /// <param name="trigger">The transform of the source trigger</param>
    public void TriggerDetect(Transform trigger)
    {
        target = trigger;
        ChangeState(GuardStates.Detecting);
    }

    /// <summary>
    /// Triggers a chase sequence on the player
    /// </summary>
    /// <param name="player">Reference to the targeted player transform</param>
    public void TriggerChase(Transform player)
    {
        target = player;
        ChangeState(GuardStates.Chasing);
    }

    public void TriggerStun()
    {
        if (guardState != GuardStates.Stunned)
        {
            ChangeState(GuardStates.Stunned);
        }
    }

    /// <summary>
    /// Checks when a guard catches a player
    /// </summary>
    /// <param name="other">The collided with GO</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && guardState != GuardStates.Stunned && Time.timeSinceLevelLoad > 2f)
        {
            // Currently Debug.logs and sets the guard back to it's normal state when caught
            //Debug.Log($"{other.name} CAUGHT by {this.transform.parent.name}");
            //Debug.Log(Time.timeSinceLevelLoad);

            if (other.GetComponent<PhotonView>().IsMine)
            {
                if (!isCatching)
                {
                    isCatching = true;
                    StartCoroutine("CaughtSequence", other);

                    AkSoundEngine.StopAll(this.gameObject);
                    AkSoundEngine.PostEvent("PlayerCaught", this.gameObject);
                }
            }
        }
    }

    /// <summary>
    /// Setter for moving state from PatrolMover
    /// </summary>
    /// <param name="move">Should the p;layer be moving?</param>
    public void SetMoving(bool move)
    {
        isMoving = move;
    }

    IEnumerator StunSequence()
    {
        detection.ToggleStun(true);

        PV.RPC("RPC_ResetTrigger", RpcTarget.All, "MoveTrigger", this.gameObject.GetPhotonView().ViewID);
        PV.RPC("RPC_SetTrigger", RpcTarget.All, "IsStunned", this.gameObject.GetPhotonView().ViewID);

        yield return new WaitForSeconds(stunTime);

        detection.ToggleStun(false);
        TriggerInvestigation(this.transform, false);

        isStunned = false;
    }

    IEnumerator CaughtSequence(Collider other)
    {
        // Drop and explode all paintings
        Player_Interact_Network pIntNet = other.GetComponent<Player_Interact_Network>();
        pIntNet.Caught();
        detection.CancelChasing();

        PV.RPC("RPC_ResetTrigger", RpcTarget.All, "MoveTrigger", this.gameObject.GetPhotonView().ViewID);
        PV.RPC("RPC_SetTrigger", RpcTarget.All, "PlayerCatch", this.gameObject.GetPhotonView().ViewID);

        if (caughtVFX != null)
        {
            Instantiate(caughtVFX, other.transform.position + (other.transform.forward * 2) + other.transform.up, Quaternion.identity);
        }

        // Send back to spawn
        other.GetComponent<CharacterController>().enabled = false;
        pIntNet.SetStun(2);
        //other.GetComponentInChildren<PlayerFlashLight>().RespawnLight();

        yield return new WaitUntil(() => pIntNet.HeldObject == null);
        yield return new WaitForSeconds(0.3f);

        other.transform.position = GameSetup.GS.GetSpawnTransform().position;
        other.transform.rotation = GameSetup.GS.GetSpawnTransform().rotation;
        other.GetComponent<CharacterController>().enabled = true;

        if (caughtVFX != null)
        {
            Instantiate(caughtVFX, other.transform.position + (other.transform.forward * 2) + other.transform.up, Quaternion.identity);
        }

        yield return new WaitForSeconds(0.3f);

        isCatching = false;
        ChangeState(GuardStates.Patrolling);
    }

    // ANIMATIONS

    [PunRPC]
    void RPC_SetTrigger(string trigger, int guardID)
    {
        if (PhotonView.Find(guardID).transform.gameObject.GetComponent<Pathing_Network>().anim == null) { return; }
        PhotonView.Find(guardID).transform.gameObject.GetComponent<Pathing_Network>().anim.SetTrigger(trigger);

    }

    [PunRPC]
    void RPC_ResetTrigger(string trigger, int guardID)
    {
        if (PhotonView.Find(guardID).transform.gameObject.GetComponent<Pathing_Network>().anim == null) { return; }
        PhotonView.Find(guardID).transform.gameObject.GetComponent<Pathing_Network>().anim.ResetTrigger(trigger);
    }

    [PunRPC]
    void RPC_SetBool(string trigger, int guardID, bool state)
    {
        if (PhotonView.Find(guardID).transform.gameObject.GetComponent<Pathing_Network>().anim == null) { return; }
        PhotonView.Find(guardID).transform.gameObject.GetComponent<Pathing_Network>().anim.SetBool(trigger, state);
    }
}
