#region Namespaces
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
#endregion

/// <summary>
/// Camera naviagtion for Main Menu UI
/// </summary>
public class UICameraNavigation : MonoBehaviour
{
    #region Variables
    /* Serialized Fields */
    [Header("References")]
    [Tooltip("Reference list of waypoints our camera will travel to Network location.")]
    public List<Transform> networkPainting;
    [Tooltip("Reference list of waypoints our camera will travel to tutorial location.")]
    public List<Transform> tutorialPainting;
    [Tooltip("Reference list of waypoints our camera will travel to settings location.")]
    public List<Transform> settingsPainting;
    [Tooltip("Reference list of waypoints our camera will travel to credits location.")]
    public List<Transform> creditsPainting;
    [Tooltip("Reference to the default location for the camera.")]
    public Transform defaultLocation;
    [Tooltip("The camera that will be moving throughout the scene")]
    public Camera theCamera = null;

    [Header("Settings")]
    [Tooltip("Delay time for camera movement coroutine.")]
    public float delayTime = 0.1f;
    [Tooltip("Time it takes for camera to move between locations")]
    public float moveTime = 2f;
    [Tooltip("Time to rotate the camera towards target.")]
    public float rotateTime = 1f;

    

    /* Private */
    private int waypointCount = 0; // Count to keep track of how many points we've moved through
    private bool nextPoint = true; // Bool to see if it is time to move to the next point
    private bool camRotated = false; // Bool indicating when we finished rotating

    private int screen = -1; // int indicating the current screen we are looking at
    private int prevScreen = 0; // int indicating the screen we are backing up from
    private bool hostScreen = true; // bool to indicate which network screen should be shown

    

    /* Coroutines */
    private IEnumerator moveCamera;
    private IEnumerator rotateCamera;

    #endregion

    #region Methods
    // Start is called before the first frame update
    void Start()
    {
        if (theCamera == null)
        {
            theCamera = Camera.main;
        }

        AkSoundEngine.StopAll();
        AkSoundEngine.PostEvent("mainMenuMusic", this.gameObject);

    }

    // Update is called once per frame
    void Update()
    {
        switch(screen)
        {
            case 0:
                // Main menu screen
                MoveToDefault();
                break;
            case 1:
                // Network screen
                InitiateMove(networkPainting, 1, false);
                prevScreen = 1;
                break;
            case 2:
                // Tutorial screen
                InitiateMove(tutorialPainting, 2, false);
                prevScreen = 2;
                break;
            case 3:
                // Settings screen
                InitiateMove(settingsPainting, 3, false);
                prevScreen = 3;
                break;
            case 4:
                // Credits screen
                InitiateMove(creditsPainting, 4, false);
                prevScreen = 4;
                break;
            case -1:
                break;
            default:
                Debug.LogError("Invalid screen index.");
                break;
        }
    }

    /// <summary>
    /// Function to be called when the host button is pressed in the Main Menu
    /// This function disables the main screen, starts the rotation coroutine
    /// and sets the varibales needed to begin movement
    /// </summary>
    public void HostButtton()
    {
        hostScreen = true;
        ButtonFunctionality(networkPainting, 1);
    }

    /// <summary>
    /// Function to be called when the join button is pressed in the Main Menu
    /// This function disables the main screen, starts the rotation coroutine
    /// and sets the varibales needed to begin movement
    /// </summary>
    public void JoinButtton()
    {
        hostScreen = false;
        ButtonFunctionality(networkPainting, 1);
    }

    /// <summary>
    /// Function to be called when the tutorial button is pressed in the Main Menu
    /// This function disables the main screen, starts the rotation coroutine
    /// and sets the varibales needed to begin movement
    /// </summary>
    public void TutorialButtton()
    {
        ButtonFunctionality(tutorialPainting, 2);
    }

    /// <summary>
    /// Function to be called when the settings button is pressed in the Main Menu
    /// This function disables the main screen, starts the rotation coroutine
    /// and sets the varibales needed to begin movement
    /// </summary>
    public void SettingsButtton()
    {
        ButtonFunctionality(settingsPainting, 3);
    }

    /// <summary>
    /// Function to be called when the credits button is pressed in the Main Menu
    /// This function disables the main screen, starts the rotation coroutine
    /// and sets the varibales needed to begin movement
    /// </summary>
    public void CreditsButtton()
    {
        ButtonFunctionality(creditsPainting, 4);
    }

    /// <summary>
    /// Function to be called when the back button for any screen is pressed in the Main Menu
    /// /// This function disables the main screen, starts the rotation coroutine
    /// and sets the varibales needed to begin movement
    /// </summary>
    public void BackButtton()
    {
        ButtonFunctionality(new List<Transform>(), 0);
    }

    /// <summary>
    /// Function to handle when a menu button is pressed
    /// </summary>
    /// <param name="path"></param> The list of waypoints for the camera to move through
    /// <param name="screenNum"></param> The screen number to move to
    private void ButtonFunctionality(List<Transform> path, int screenNum)
    {
        EventSystem.current.SetSelectedGameObject(null);
        if (screenNum == 0) rotateCamera = RotateCamera(Quaternion.Euler(defaultLocation.eulerAngles));
        else rotateCamera = RotateCamera(Quaternion.Euler(path[^1].eulerAngles));
        StartCoroutine(rotateCamera);
        waypointCount = 0;
        nextPoint = true;
        screen = screenNum;
    }

    /// <summary>
    /// This function delegates moving back to the default position for the menu from
    /// any other screen
    /// </summary>
    private void MoveToDefault()
    {
        switch (prevScreen)
        {
            case 1:
                // backup from tutorial screen
                //transform.GetChild(prevScreen).gameObject.SetActive(false);
                InitiateMove(networkPainting, 0, true);
                break;
            case 2:
                // backup from settings screen
                transform.GetChild(prevScreen).gameObject.SetActive(false);
                InitiateMove(tutorialPainting, 0, true);
                break;
            case 3:
                // backup from credits screen
                transform.GetChild(prevScreen).gameObject.SetActive(false);
                InitiateMove(settingsPainting, 0, true);
                break;
            case 4:
                // backup from credits screen
                transform.GetChild(prevScreen).gameObject.SetActive(false);
                InitiateMove(creditsPainting, 0, true);
                break;
            default:
                Debug.LogError("Invalid screen index.");
                break;
        }
    }

    /// <summary>
    /// This function loops through each of the waypoints and moves along the path
    /// </summary>
    /// <param name="path"></param> the path the camera will take
    /// <param name="curScreen"></param> the current screen we are moving too
    /// <param name="backwards"></param> indicator if we are moving back to default position
    private void InitiateMove(List<Transform> path, int curScreen, bool backwards)
    {
        // while there are more points to move through and we are ready to move
        while (nextPoint && waypointCount < path.Count)
        {
            // if this is the first point skip it as it is the point the camera is already at
            // this is needed to move backwards along the path
            if (waypointCount == 0)
            {
                waypointCount += 1;
                nextPoint = true;
                continue;
            }
            else
            {
                nextPoint = false;
                // if we are moving backwards iterate from the end of the list of points
                if (backwards)
                {
                    Vector3 endLocation = new(path[^(waypointCount + 1)].position.x, path[^(waypointCount + 1)].position.y, path[^(waypointCount + 1)].position.z);
                    moveCamera = MoveCamera(endLocation, moveTime / (path.Count - 1));
                    StartCoroutine(moveCamera);
                }
                // if not iterate forward through the list
                else
                {
                    Vector3 endLocation = new(path[waypointCount].position.x, path[waypointCount].position.y, path[waypointCount].position.z);
                    moveCamera = MoveCamera(endLocation, moveTime / (path.Count - 1));
                    StartCoroutine(moveCamera);
                }
            }
        }
        // only when we have iterate through all of the points and finished rotating can we display the new screen
        if (waypointCount >= path.Count && camRotated)
        {
            if (curScreen == 1)
            {
                if (hostScreen) transform.GetChild(curScreen).GetChild(0).gameObject.SetActive(true);
                else transform.GetChild(curScreen).GetChild(1).gameObject.SetActive(true);
            }
            else
            {
                transform.GetChild(curScreen).gameObject.SetActive(true);
            }
            screen = -1;
        }
    }


    /// <summary>
    /// Enumerator to move the camera over time through its path
    /// </summary>
    /// <param name="destination"></param> the destination of the camera
    /// <param name="duration"></param> the duration of movement time
    /// <returns></returns>
    IEnumerator MoveCamera(Vector3 destination, float duration)
    {
        float time = 0;
        Vector3 startPosition = theCamera.transform.position;
        while (time < duration)
        {
            theCamera.transform.position = Vector3.Lerp(startPosition, destination, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        theCamera.transform.position = destination;
        nextPoint = true;
        waypointCount += 1;
    }

    /// <summary>
    /// Enumerator to rotate the camera over time to face desired location
    /// </summary>
    /// <param name="endValue"></param> the end rotation value
    /// <returns></returns>
    IEnumerator RotateCamera(Quaternion endValue)
    {
        float time = 0;
        Quaternion startValue = theCamera.transform.rotation;
        while (time < rotateTime)
        {
            theCamera.transform.rotation = Quaternion.Lerp(startValue, endValue, time / rotateTime);
            time += Time.deltaTime;
            yield return null;
        }
        theCamera.transform.rotation = endValue;
        camRotated = true;
    }

    #endregion
}
