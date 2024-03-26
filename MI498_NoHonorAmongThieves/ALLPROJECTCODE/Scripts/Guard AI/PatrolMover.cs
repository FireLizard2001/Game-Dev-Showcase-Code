using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrolMover : MonoBehaviour
{

    #region Inspector Variables

    [Header("Patrol Values")]

    [Tooltip("List of patrol points to traverse")]
    [SerializeField] private List<PatrolPoint> points;

    [Tooltip("The guard's patrol speed")]
    public float patrolSpeed = 6;

    [Tooltip("Should the points list wrap-around, or backtrack?")]
    [SerializeField] private bool wrapAround = true;

    #endregion

    #region Patrol Variables

    // Is the guard currently moving?
    private bool isMoving = true;

    // Current index of the patrol point
    private int currPoint = -1;

    // The current running timer for waiting
    private float currWaitTimer = 0f;

    // The time to wait at the current point (pulled from the PatrolPoint script)
    private float currWaitTime = 0f;

    // If no wraparound points, helps keep track of the current direction of the list
    private int pathDir = 1;

    private float eulerRotationY = 0;

    #endregion

    #region References

    // Reference to the NavMeshAgent for movement
    private NavMeshAgent agent;

    // Pathing script reference
    private Pathing path;

    #endregion

    private void Awake()
    {
        // Sets agent values
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed;

        if (points.Count != 0)
        {
            NextPoint();
        }
        else
        {
            Debug.Log("Mover is missing patrol points!");
        }
    }

    private void Update()
    {
        if (points.Count != 0)
        {
            MovementCheck();
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

                currWaitTime = points[currPoint].waitTime;
                currWaitTimer = 0f;
            }

            currWaitTimer += Time.deltaTime;

            // Decide next move after waiting is done
            if (currWaitTimer >= currWaitTime)
            {
                NextPoint();
            }
        }
    }

    /// <summary>
    /// Chooses the next patrol point and updates agent
    /// </summary>
    public void NextPoint()
    {
        if (wrapAround)
        {
            currPoint = currPoint >= points.Count - 1 ? 0 : currPoint + 1;
        }
        else
        {
            // Determines the direction of the current track

            if (currPoint >= points.Count - 1)
            {
                pathDir = -1;
            }
            else if (currPoint <= 0)
            {
                pathDir = 1;
            }

            currPoint += pathDir;
        }

        // Just in case a PatrolPoint is missing
        if (points[currPoint] is null)
        {
            NextPoint();
            return;
        }

        // Update agent with it's next destination
        isMoving = true;

        if (path != null)
        {
            path.SetMoving(true);
        }
        
        agent.SetDestination(points[currPoint].transform.position);
        eulerRotationY = points[currPoint].transform.rotation.eulerAngles.y;
    }

    public float GetAngle()
    {
        return eulerRotationY;
    }

    public void SetPathing(Pathing newPath)
    {
        path = newPath;
    }
}
