using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Pathing : MonoBehaviour
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

    [Header("Guard Values")]

    [Tooltip("Current state of the guard")]
    [SerializeField] private GuardStates guardState = GuardStates.Patrolling;

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

    [Tooltip("The time the guard continues to check the player position after initial investigation")]
    [SerializeField] private float furtherTime = 1f;

    [Header("Chase Values")]

    [Tooltip("Speed during a guard chase")]
    [SerializeField] private float chaseSpeed = 5f;

    [Tooltip("Time the guard will follow further after broken contact")]
    [SerializeField] private float followFurtherTime = 2f;

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
    #endregion

    #region References

    // Reference to the NavMeshAgent for movement
    private NavMeshAgent agent;

    // Reference to the detection cone
    private VisionCone detection;

    // Reference to the Guard UI
    private GuardUI UI;

    #endregion

    void Start()
    {
        // Grab NavMeshAgent reference and set speed
        agent = GetComponent<NavMeshAgent>();
        detection = GetComponent<VisionCone>();
        UI = GetComponent<GuardUI>();

        if (patrolMover != null)
        {
            agent.speed = patrolMover.GetComponent<PatrolMover>().patrolSpeed;
            patrolMover.GetComponent<PatrolMover>().SetPathing(this);
        }

        if (patrolMover == null)
        {
            ChangeState(GuardStates.Stunned);
        }
        else if (patrolMover != null)
        {
            ChangeState(GuardStates.Patrolling);
        }
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
        StopCoroutine("FollowFurther");

        // Switch logic based on current guard state
        switch (guardState)
        {
            case GuardStates.Patrolling:

                // Sets target to patrol mover and resets speed
                target = patrolMover;
                agent.speed = patrolMover.GetComponent<PatrolMover>().patrolSpeed;
                detection.ToggleSearching(true);

                break;
            case GuardStates.Investigating:

                // Sends guard to the triggered point
                if (target != null)
                {
                    agent.SetDestination(target.position);
                    StartCoroutine("FollowFurther");
                    agent.speed = investigationSpeed;
                    isMoving = true;
                    detection.ToggleSearching(true);
                }

                break;
            case GuardStates.Detecting:

                // Handles stopping movement
                agent.ResetPath();

                break;
            case GuardStates.Chasing:

                // Sets speed for chasing
                agent.speed = chaseSpeed;
                detection.ToggleSearching(false);

                break;
            case GuardStates.Stunned:
                agent.ResetPath();
                StartCoroutine("StunSequence");

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

                // Sets the angle spread for guard sweeping based on initial direction

                float initDir = guardState == GuardStates.Patrolling ? 
                    patrolMover.GetComponent<PatrolMover>().GetAngle() : transform.rotation.eulerAngles.y;

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
            }
        }
    }

    /// <summary>
    /// Chases a target over time
    /// </summary>
    public void ChasePlayer()
    {
        agent.SetDestination(target.position);
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
        if (other.CompareTag("Player") && guardState != GuardStates.Stunned)
        {
            // Currently Debug.logs and sets the guard back to it's normal state when caught
            Debug.Log($"{other.name} CAUGHT!");

            if (other.GetComponent<PlayerSpawn>())
            {
                other.GetComponent<PlayerSpawn>().Respawn();
            }

            ChangeState(GuardStates.Patrolling);
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

    IEnumerator FollowFurther()
    {
        yield return new WaitForSeconds(followFurtherTime);

        if (target != null && isMoving)
        {
            agent.SetDestination(target.position);
        }
    }

    IEnumerator StunSequence()
    {
        detection.ToggleStun(true);

        yield return new WaitForSeconds(stunTime);

        detection.ToggleStun(false);
        TriggerInvestigation(this.transform, false);
    }
}
