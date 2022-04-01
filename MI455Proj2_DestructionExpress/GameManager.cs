using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;


    [Header("General")]
    public GameObject inputManager;
    [Tooltip("This vector updates the overall physics system to change the gravity in game.")]
    public Vector3 gameGravity = new Vector3(0f, -9.8f, 0f);
    private bool gameIsStarted = false;

    [Header("On Cart")]
    public bool onCart = true;
    [Tooltip("Total shots allowed in a level")]
    public int totalShots = 300;

    [Header("In Air")]
    public bool inAir = false;
    private GameObject launchedCrewMember = null;
    private Launcher shotCrewLauncher = null;
    [Tooltip("Boolean to prevent player from shooting a crewmember prior to a rocket exploding")]
    private bool rocketIsLaunched = false;

    [Header("Launching Crew Member")]
    [Tooltip("This is the projectile for chad to launch. Should be crewmebmer with a rocket launcher.")]
    public GameObject crewMember;
    public float verticalForce = 50f;
    public float launchForce = 1000f;
    public float crewMemberGravity = -9.8f;

    [Header("Camera Handler")]
    public float transitionSpeed;
    public float sceneViewWait;
    private FirstPerson cartCam;
    private ThirdPerson shotMemberCam;
    private Vector2 lookInput = Vector2.zero;
    private CinemachineVirtualCamera sceneCamera;

    [Header("Audio")]
    [SerializeField] private GameObject launchAudio;
    [SerializeField] private GameObject fireAudio;
    [SerializeField] private GameObject gameAudio;
    [SerializeField] private GameObject distortGameAudio;
    [SerializeField] private GameObject slowAudio;
    [SerializeField] private GameObject unslowAudio;
    [SerializeField] private GameObject tickAudio;

    [Header("Time Slow")]
    [SerializeField] private GameObject colorFilter;
    private int time_charges = 3;
    private int time_stop_count = 0;
    private bool time_slowed = false;
    private float time_scale_slow = 0.5f;

    //Manager Variables
    public static float charges { get { return instance.time_charges; } }
    public static float stopped_count { get { return instance.time_stop_count; } }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    // Locks cursor so player does not see mouse on screen.
    // Also sets initial gravity and finds needd components in scene
    void Start()
    {
        Invoke("startGame", 1.0f);

        Cursor.lockState = CursorLockMode.Locked;

        Physics.gravity = gameGravity;

        // For camera handler, if not set find by name
        if (cartCam == null)
            cartCam = GameObject.Find("First Person Camera").GetComponent<FirstPerson>();

        if (sceneCamera == null)
            sceneCamera = GameObject.Find("Scene Camera").GetComponent<CinemachineVirtualCamera>();
    }

    public void rightClickDown(InputAction.CallbackContext context)
    {
        if (context.started && gameIsStarted && time_charges > 0 && inAir)
        {
            timeSlow();
        }
        if ((context.canceled || context.performed) && gameIsStarted && time_charges > 0 && time_slowed)
        {
            timeSpeed();
        }
    }

    // Slows timescale while playing audio.
    public void timeSlow()
    {
        // Change time scale for slowdown effect
        Debug.Log("Slow Started!");
        Time.timeScale = time_scale_slow;
        Time.fixedDeltaTime = time_scale_slow * 0.02f;

        // Set flags for active window of time slowdown
        colorFilter.SetActive(true);
        time_stop_count++;
        time_slowed = true;

        // Play Audio
        if (slowAudio != null && tickAudio != null && distortGameAudio != null && gameAudio != null)
        {
            slowAudio.GetComponent<AudioSource>().Play();
            tickAudio.GetComponent<AudioSource>().Play();
            distortGameAudio.GetComponent<AudioSource>().volume = 0.1f;
            gameAudio.GetComponent<AudioSource>().volume = 0f;
        }
    }

    // Increases timescale while playing audio and using a charge
    public void timeSpeed()
    {
        // Change time scale back from slowdown effect
        Debug.Log("Slow Ended!");
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        // Unset flags for active window of time slowdown
        colorFilter.SetActive(false);
        time_charges--;
        time_slowed = false;

        // Play and Stop Audio
        if (unslowAudio != null && slowAudio != null && tickAudio != null && distortGameAudio != null && gameAudio != null)
        {
            slowAudio.GetComponent<AudioSource>().Stop();
            tickAudio.GetComponent<AudioSource>().Stop();
            unslowAudio.GetComponent<AudioSource>().Play();
            distortGameAudio.GetComponent<AudioSource>().volume = 0f;
            gameAudio.GetComponent<AudioSource>().volume = 0.1f;
        }
    }

    public void leftClickDown(InputAction.CallbackContext context)
    {
        if (context.started && gameIsStarted)
        {
            if ((inAir == true && onCart == true) || (inAir == false && onCart == false))
            {
                Debug.LogError("Current State Invalid. InAir and OnCart are equal.");
            }

            else if (onCart && totalShots > 0 && rocketIsLaunched == false)
            {
                cartHandler();
            }

            else if (inAir)
            {
                airHandler();
            }
        }
    }

    // This or airHandler should be called every frame.
    // Function to handle code while on the cart.
    private void cartHandler()
    {
        Debug.Log("Player Launched");

        // Plays audio
        GameObject audio = Instantiate(launchAudio);
        Destroy(audio, 3);

        // Spawn crewmember projectile. Switch camera to this object by default as it spawns with higher priority and switch back when instance destroyed or hits ground.
        launchedCrewMember = Instantiate(crewMember, cartCam.transform.position, cartCam.transform.rotation);

        // Shoots crewmember based on axis
        launchedCrewMember.GetComponent<ConstantForce>().force = new Vector3(0f, crewMemberGravity, 0f);
        launchedCrewMember.GetComponent<Rigidbody>().AddForce(launchedCrewMember.transform.up * verticalForce);
        launchedCrewMember.GetComponent<Rigidbody>().AddForce(launchedCrewMember.transform.forward * launchForce);

        launchedCrewMember.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);

        // For adding spawned crew member to the input manager, get the spawned camera and rocket launcher.

        shotMemberCam = launchedCrewMember.GetComponentInChildren<ThirdPerson>();
        shotCrewLauncher = launchedCrewMember.GetComponentInChildren<Launcher>();


        onCart = false;
        inAir = true;

    }

    // This or cartHandler should be called every frame.
    // Function to handle code while in the air.
    private void airHandler()
    {
        Debug.Log("Rocket Shot");

        // Plays audio
        GameObject audio = Instantiate(fireAudio);
        Destroy(audio, 3);

        totalShots--;
        rocketIsLaunched = true;
        // Stops time slowdown when rocket is fired
        if (time_slowed) { timeSpeed(); }

        launchedCrewMember.GetComponentInChildren<CinemachineVirtualCamera>().Priority = 0;

        shotCrewLauncher.Shoot();
        onCart = true;
        inAir = false;

        // Toggle Crosshair
        Crosshair.instance.Disable();

        // Destroy spawned crew member object after camera moves to rocket.
        StartCoroutine("CameraTransfer");
    }

    IEnumerator CameraTransfer()
    {
        yield return new WaitForSeconds(transitionSpeed);

        Destroy(launchedCrewMember);
    }


    public void UpdateCamera(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();

        if (onCart)
        {
            cartCam.SetLookInput(lookInput);
        }
        else
        {
            shotMemberCam.SetLookInput(lookInput);
        }
    }

    public void SwitchToSceneCamera()
    {
        sceneCamera.Priority = 4;
        StartCoroutine("SceneView");
    }

    IEnumerator SceneView()
    {
        yield return new WaitForSeconds(sceneViewWait);

        sceneCamera.Priority = 0;
    }

    public void RocketExploded()
    {
        rocketIsLaunched = false;
    }

    public void HitGround()
    {
        if (inAir)
        {
            airHandler();
        }
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void startGame()
    {
        gameIsStarted = true;
    }
}
