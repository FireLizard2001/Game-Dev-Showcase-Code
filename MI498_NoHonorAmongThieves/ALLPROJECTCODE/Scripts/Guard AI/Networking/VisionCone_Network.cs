using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;
using Photon.Pun;
using PlayerFunctionality;

public class VisionCone_Network : MonoBehaviour
{

    #region Inspector Variables
    // two variables for controlling the AI vision cone

    [Header("Size Values")]

    [Tooltip("The distance of the vision cone")]
    [SerializeField] private float length = 20f;

    [Tooltip("The angle around the guard that makes up the cone")]
    [SerializeField] private float angle = 60f;

    [Tooltip("The height of the cone")]
    [SerializeField] private float height = 2.0f;
    
    [Header("Debug Values")]

    [Tooltip("The color of the cone")]
    [SerializeField] private Color coneColor = Color.green;

    [Tooltip("Should the cone be drawn in editor?")]
    [SerializeField] private bool drawConeInTesting = true;

    [Header("Detection Values")]

    [Tooltip("The time it takes to detect a player")]
    [SerializeField] private float detectionTime = 3;

    [Tooltip("The frequency in which the scan fires")]
    [SerializeField] private float frequencyScan = 30;

    [Tooltip("The layers that a player exists on")]
    [SerializeField] private LayerMask playerLayer;

    [Tooltip("The layers that will block cone detection")]
    [SerializeField] private LayerMask occlusionlayers; // the occlusion layer for like walls, barrels anything

    [Tooltip("Time the guard will follow further after broken contact")]
    [SerializeField] private float followFurtherTime = 2f;

    #endregion

    #region Detection Values

    // List that holds colliders for vision cone creation
    private Collider[] collids = new Collider[50];
    // Helper for vision cone creation
    private int count;

    // The current interval of scanning timer
    private float scanInterval;
    // the scan timer itself
    private float scanTimer;

    // The current amount of time the guard has been detecting
    private float currDetectTime = 0;
    // Is the guard currently detecting?
    private bool isDetecting = false;
    // Is the guard currently chasing?
    private bool isChasing = false;
    // A helper bool for initially setting up detecting state
    private bool startedDetecting = false;

    Mesh sensor; // the visioncone mesh obj

    // List of object to return Stored here
    private List<GameObject> objectsDetect = new List<GameObject>();

    // Is the guard currently searching?
    private bool isSearching = true;
    // Reference to Pathing script
    private Pathing_Network path;
    // Reference to GuardUI script
    private GuardUI UI;
    // Has the guard started chasing?
    private bool chaseStarted = false;
    private bool chasingFurther = false;

    // Is the guard currently stunned?
    private bool isStunned = false;

    #endregion

    private void Start()
    {
        // Sets interval based on frequency
        scanInterval = 1.0f / frequencyScan;

        // Grab script references
        path = GetComponent<Pathing_Network>();
        UI = GetComponent<GuardUI>();
        UI.SetDetectCountdown("");
    }

    private void Update()
    {
        // Handles frequency timing

        if (objectsDetect.Count == 0 || objectsDetect[0] == null)
        {
            scanTimer -= Time.deltaTime;
            if (scanTimer < 0 && isSearching)
            {
                scanTimer += scanInterval;
                Scan();
            }
        }
        else
        {
            Detecting();

            Chasing();
        }
    }
    
    /// <summary>
    /// Scan the physical bodys
    /// </summary>
    private void Scan()
    {
        // Sphere detection
        count = Physics.OverlapSphereNonAlloc(transform.position, length, collids, playerLayer,
            QueryTriggerInteraction.Collide);
        
        for (int i = 0; i < count; ++i)
        {
            // Grabs player colliders
            GameObject obj = collids[i].gameObject;
            if (CheckIfSee(obj) && !obj.GetComponent<Player_Interact_Network>().isChased)
            {
                // Adds player to list and starts detection sequence
                if (PostManager.instance.vigOn && obj.GetComponent<PhotonView>().IsMine)
                {
                    PostManager.instance.ToggleVig(false);
                }

                objectsDetect.Add(obj);
                isDetecting = true;
            }
        }
        
    }

    /// <summary>
    /// Call a Wwise Audio Event
    /// </summary>
    /// <param name="eventName">Name of the event</param>
    [PunRPC]
    void PlayEvent(string eventName)
    {
        AkSoundEngine.PostEvent(eventName, gameObject);
    }

    /// <summary>
    /// Handles the guard detecting (player in sight) sequence
    /// </summary>
    public void Detecting()
    {
        // Handles detecting logic
        if (isDetecting)
        {
            if (!startedDetecting)
            {
                // Sets initial bools and timer values
                startedDetecting = true;
                ToggleSearching(false);

                currDetectTime = detectionTime;
                path.TriggerDetect(objectsDetect[0].transform);

                // WWISE EVENTS FOR GUARD DETECTION - UNCOMMENT WHEN READY

                if (objectsDetect[0].GetComponent<PhotonView>().IsMine)
                {
                    AkSoundEngine.PostEvent("guardSee", gameObject);
                    gameObject.GetComponent<PhotonView>().RPC("PlayEvent", RpcTarget.Others, "guardSee");

                    AkSoundEngine.PostEvent("guardDetect", gameObject);
                }
            }

            // If the raycast is broken, guard will move to last known position to investigate
            if (!CheckIfSee(objectsDetect[0]) || objectsDetect[0].GetComponent<Player_Interact_Network>().isChased)
            {
                if (objectsDetect[0].GetComponent<PhotonView>().IsMine)
                {
                    AkSoundEngine.PostEvent("guardAudioStop", gameObject);
                    gameObject.GetComponent<PhotonView>().RPC("PlayEvent", RpcTarget.Others, "guardAudioStop");
                }

                CancelDetecting();
                path.TriggerInvestigation(objectsDetect[0].transform, false);
                return;
            }

            currDetectTime -= Time.deltaTime;

            // Detect UI timer
            UI.SetDetectCountdown(Mathf.Round(currDetectTime).ToString());
            UI.SetDetectFill((detectionTime - currDetectTime) / detectionTime);

            // If detect time makes it's full duration, trigger a chase
            if (currDetectTime < 0 && !isStunned)
            {
                isChasing = true;
                CancelDetecting();
                path.TriggerChase(objectsDetect[0].transform);
                objectsDetect[0].GetComponent<Player_Interact_Network>().isChased = true;

                // Stop detection sounds before alert
                if (objectsDetect[0].GetComponent<PhotonView>().IsMine)
                {
                    AkSoundEngine.PostEvent("guardAudioStop", gameObject);
                    gameObject.GetComponent<PhotonView>().RPC("PlayEvent", RpcTarget.Others, "guardAudioStop");
                }

                //Debug.Log("Should call the event");
                if (objectsDetect[0].GetComponent<PhotonView>().IsMine)
                {
                    AkSoundEngine.PostEvent("guardAlert", gameObject);
                    gameObject.GetComponent<PhotonView>().RPC("PlayEvent", RpcTarget.Others, "guardAlert");
                }
            }
        }
    }

    /// <summary>
    /// Handles the chasing (detection bar filled) sequence
    /// </summary>
    public void Chasing()
    {
        if (isChasing)
        {
            if (!chaseStarted)
            {
                chaseStarted = true;

                path.TriggerChase(objectsDetect[0].transform);
            }

            // If the raycast is broken, guard will move to last known position to investigate
            if (!CheckIfSee(objectsDetect[0]))
            {
                if (!chasingFurther)
                {
                    chasingFurther = true;
                    StartCoroutine("ChaseFurther");
                }
            }
            else
            {
                if (chasingFurther)
                {
                    StopCoroutine("ChaseFurther");
                    chasingFurther = false;
                }
            }
        }
    }

    /// <summary>
    /// Check physical body in the cone
    /// </summary>>
    public bool CheckIfSee(GameObject other)
    {
        if (other == null) { return false; }
        Vector3 origin = transform.position;
        Vector3 dest = other.transform.position;
        Vector3 direction = dest - origin;

        // Is the player on the same horizontal plane?
        if (direction.y < -(height / 2) || direction.y > height)
        {
            return false;
        }

        // Is the player inside the angle?
        direction.y = 0;
        float delteAngle = Vector3.Angle(direction, transform.forward);
        if (delteAngle > angle)
        {
            return false;
        }

        origin.y += height / 2;
        dest.y = origin.y;
        // if linecast anything in the occlusion
        if (Physics.Linecast(origin, dest, occlusionlayers, QueryTriggerInteraction.Ignore))
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// The mesh of visionCone created
    /// </summary>
    Mesh CreateSensor()
    {
        Mesh sensor = new Mesh();
        
        // create triangle based sensor range
        int segs = 10;
        int numTri = (segs*4) +2 +2;
        int numVert = numTri * 3;
        Vector3[] vertices = new Vector3[numVert];
        int[] triangles = new int[numVert];
        
        // the points of the sensor
        Vector3 botCen = Vector3.zero;
        Vector3 botLeft = Quaternion.Euler(0, -angle, 0) * Vector3.forward * length;
        Vector3 botRight = Quaternion.Euler(0, angle, 0) * Vector3.forward * length;
        Vector3 topCen = botCen + Vector3.up * height;
        Vector3 topRight = botRight +  Vector3.up * height;
        Vector3 topLeft= botLeft +  Vector3.up * height;

        int vert = 0;
        
        // create 3D mesh based on the points
        // left
        vertices[vert++] = botCen;
        vertices[vert++] = botLeft;
        vertices[vert++] = topLeft;
        
        vertices[vert++] = topLeft;
        vertices[vert++] = topCen;
        vertices[vert++] = botCen;
        // right
        vertices[vert++] = botCen;
        vertices[vert++] = topCen;
        vertices[vert++] = topRight;
        
        vertices[vert++] = topRight;
        vertices[vert++] = botRight;
        vertices[vert++] = botCen;

        float currentAngle = -angle;
        float deltaAngle = (angle * 2) / segs;
        for (int i = 0; i < segs; ++i)
        {
            botLeft = Quaternion.Euler(0, currentAngle, 0) * Vector3.forward * length;
            botRight = Quaternion.Euler(0, currentAngle+deltaAngle , 0) * Vector3.forward * length;
            
            topRight = botRight +  Vector3.up * height;
            topLeft= botLeft +  Vector3.up * height;
             
            vertices[vert++] = botLeft;
            vertices[vert++] = botRight;
            vertices[vert++] = topRight;
        
            vertices[vert++] = topRight;
            vertices[vert++] = topLeft;
            vertices[vert++] = botLeft;
            // top
            vertices[vert++] = topCen;
            vertices[vert++] = topLeft;
            vertices[vert++] = topRight;
            // bot
            vertices[vert++] = botCen;
            vertices[vert++] = botRight;
            vertices[vert++] = botLeft;
        
            currentAngle += deltaAngle;
        }
    
        for (int i = 0; i < numVert; ++i)
        {
            triangles[i] = i;
        }

        sensor.vertices = vertices;
        sensor.triangles = triangles;
        sensor.RecalculateNormals();

        return sensor;
    }

    /// <summary>
    /// Set Event when the script is on validate the visionCone
    /// </summary>
    private void OnValidate()
    {
        sensor = CreateSensor();
        scanInterval = 1.0f / frequencyScan;
    }

    /// <summary>
    /// Draw cone gizmos, if need to draw out
    /// </summary>
    private void OnDrawGizmos()
    {
        // Draw the main cones
        if (sensor && drawConeInTesting)
        {
            Gizmos.color = coneColor;
            Gizmos.DrawMesh(sensor, transform.position, transform.rotation);
        }
        
        // Draw right now if detected object
        Gizmos.DrawWireSphere(transform.position,length);
        for (int i = 0; i < count; ++i)
        {
            Gizmos.DrawSphere(collids[i].transform.position, 0.2f);
        }
        
        Gizmos.color = Color.red;
        // display in color for each object detected
        foreach (var obj in objectsDetect)
        {
            Gizmos.DrawSphere(obj.transform.position,0.2f);
        }
    }

    /// <summary>
    /// Toggles search functionality based on path state
    /// </summary>
    /// <param name="search">Should the guard search?</param>
    public void ToggleSearching(bool search)
    {
        if (!search)
        {
            isSearching = search;
            return;
        }

        if (isDetecting)
        {
            CancelDetecting();
        }

        if (isChasing)
        {
            CancelChasing();
        }


        objectsDetect.Clear();
        StartCoroutine("Cooldown");
    }

    /// <summary>
    /// Toggles stun on and off for detection
    /// </summary>
    /// <param name="stun">True if entering stun</param>
    public void ToggleStun(bool stun)
    {
        isStunned = stun;
        StopCoroutine("Cooldown");

        if (isStunned)
        {
            if (objectsDetect.Count > 0)
            {
                if (!PostManager.instance.vigOn && objectsDetect[0].GetComponent<PhotonView>().IsMine)
                {
                    PostManager.instance.ToggleVig(true);
                }
            }

            ToggleSearching(false);

            if (isDetecting)
            {
                CancelDetecting();
            }

            if (isChasing)
            {
                CancelChasing();
            }

            UI.ChangeAlert(Pathing_Network.GuardStates.Stunned);
        }
        else
        {
            ToggleSearching(true);
            UI.ChangeAlert(Pathing_Network.GuardStates.Investigating);
        }
    }

    /// <summary>
    /// Cooldown for guards being able to detect a player after catching 
    /// Will likely be changed later after catching works fully
    /// </summary>
    /// <returns></returns>
    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(2f);
        isSearching = true;
    }

    /// <summary>
    /// Cancels detecting state
    /// </summary>
    private void CancelDetecting()
    {
        isDetecting = false;
        startedDetecting = false;

        UI.SetDetectCountdown("");

        if (!isChasing && objectsDetect.Count > 0)
        {
            if (!PostManager.instance.vigOn && objectsDetect[0].GetComponent<PhotonView>().IsMine)
            {
                PostManager.instance.ToggleVig(true);
            }
        }
    }
    
    /// <summary>
    /// Cancels chasing state
    /// </summary>
    public void CancelChasing()
    {
        isChasing = false;
        chaseStarted = false;

        chasingFurther = false;
        StopCoroutine("ChaseFurther");

        if (objectsDetect.Count > 0)
        {
            objectsDetect[0].GetComponent<Player_Interact_Network>().isChased = false;

            if (!PostManager.instance.vigOn && objectsDetect[0].GetComponent<PhotonView>().IsMine)
            {
                PostManager.instance.ToggleVig(true);
            }
        }
    }

    /// <summary>
    /// Handles functionality for continued chasing after stopping
    /// </summary>
    /// <returns>Yield time</returns>
    IEnumerator ChaseFurther()
    {
        yield return new WaitForSeconds(followFurtherTime);

        chasingFurther = false;
        CancelChasing();

        path.TriggerInvestigation(objectsDetect[0].transform, false);
    }

    public int GetTargetID()
    {
        if (objectsDetect.Count > 0 && objectsDetect[0].GetComponent<AvatarSetup>() != null)
        {
            return objectsDetect[0].GetComponent<AvatarSetup>().characterValue;
        }

        return -1;
    }
}
