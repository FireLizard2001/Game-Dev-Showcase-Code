using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerControllerPlus : MonoBehaviour
{
    [Header("Horizontal Movement")]
    [Tooltip("This is more of an acceleration parameter. See below for Max Speed. Strongly recommend not putting this below linear drag (see below).")]
    public float moveSpeed = 10f;
    [HideInInspector]
    public float backupMoveSpeed;
    [SerializeField]
    private Vector2 direction;
    public bool facingRight = true;

    [Header("Vertical Movement")]
    public float jumpStrength = 15f;
    public float jumpLeniency = 0.25f;
    [HideInInspector]
    public float backupJumpLeniency;
    private float jumpTimer;
    public bool wallJumpEnabled = true;
    public float wallJumpStrength = 10f;
    [SerializeField]
    private float wallLeniencyTimerLeft = 0f;
    [SerializeField]
    private float wallLeniencyTimerRight = 0f;
    public bool doubleJumpEnabled = true;
    private int jumps;
    public bool slideOnWalls = true;
    public float slideSpeed = 0.3f;
    private bool preventInfinite = false;  // removes player control (horizontal) to prevent climbing walls via wall jumping
    public GameObject jumpEffect;
    public GameObject landEffect;

    [Header("Components")]
    public Rigidbody2D rb;
    public Animator animator;
    [Tooltip("This should be the ground layer in our tilemap for the stage.")]
    public LayerMask groundLayer;
    [Tooltip("Layer that players can only jump off of but not wall jump.")]
    public LayerMask boxLayer;
    public GameObject playerRep;

    [Header("Player Movement Physics")]
    [Tooltip("Limit the player's maximum move speed using this.")]
    public float maxSpeed = 7f;
    [HideInInspector]
    public float backupMaxSpeed;
    [Tooltip("Friction value for player. Decrease this to make 'ice' hazards or increase it for 'sludge' hazards")]
    public float linearDrag = 4f;
    [Tooltip("Base gravity rate.")]
    public float gravity = 1f;
    [Tooltip("Gravity multiplier when falling.")]
    public float fallStrength = 5f;

    [Header("Collision")]

    public bool onGround = false;
    [SerializeField]
    private bool onStairs = false;
    [SerializeField]
    private float stairTimer = 0f;
    [SerializeField]
    private bool onWallLeft = false;
    [SerializeField]
    private bool onWallRight = false;
    [Tooltip("Should be a bit more than the character's height / 2 (use gizmos to help; make red line touch ground exactly)")]
    public float groundLength = 0.63f;
    [Tooltip("Used for walljumping collision. See gizmos.")]
    public float sideLength = 0.36f;
    [Tooltip("Distance between legs from vertical line in center of player.")]
    public Vector3 colliderOffset;

    [Header("Visuals")]
    public bool enableJumpSqueeze = true;
    public bool enableLandSqueeze = true;

    private PlayerControllerPlus[] playersList;
    public bool isAlive = true;
    public float dragMultiplier;
    public float gravityMultiplier;
    public int currentDiceFaceNum = 5;

    public float killHeight;
    private float fixLandingAudioBug = 1f;

    /// <summary>
    /// Description:
    /// Standard Unity Function called when the script is loaded
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        ResetValuesToDefault();
        // Set up the instance of this
    }

    /// <summary>
    /// Description:
    /// Sets all the input variables to their default values so that nothing weird happens in the game if you accidentally
    /// set them in the editor
    /// Input:
    /// none
    /// Return:
    /// void
    /// </summary>
    void ResetValuesToDefault()
    {
        horizontalMovement = default;
        verticalMovement = default;

        horizontalLookAxis = default;
        verticalLookAxis = default;

        jumpStarted = default;
        jumpHeld = default;

        pauseButton = default;
    }

    [Header("Movement Input")]
    [Tooltip("The horizontal movmeent input of the player.")]
    public float horizontalMovement;
    [Tooltip("The vertical movmeent input of the player.")]
    public float verticalMovement;

    /// <summary>
    /// Description:
    /// Reads and stores the movement input
    /// Input: 
    /// CallbackContext callbackContext
    /// Return:
    /// void (no return)
    /// </summary>
    /// <param name="callbackContext">The context of the movement input</param>
    public void GetMovementInput(InputAction.CallbackContext callbackContext)
    {
        Vector2 movementVector = callbackContext.ReadValue<Vector2>();
        horizontalMovement = movementVector.x;
        verticalMovement = movementVector.y;
    }

    [Header("Jump Input")]
    [Tooltip("Whether a jump was started this frame.")]
    public bool jumpStarted = false;
    [Tooltip("Whether the jump button is being held.")]
    public bool jumpHeld = false;

    /// <summary>
    /// Description:
    /// Reads and stores the jump input
    /// Input: 
    /// CallbackContext callbackContext
    /// Return: 
    /// void (no return)
    /// </summary>
    /// <param name="callbackContext">The context of the jump input</param>
    public void GetJumpInput(InputAction.CallbackContext callbackContext)
    {
        jumpStarted = !callbackContext.canceled;
        jumpHeld = !callbackContext.canceled;
        if (this.isActiveAndEnabled) StartCoroutine("ResetJumpStart");
    }

    /// <summary>
    /// Description
    /// Coroutine that resets the jump started variable after one frame
    /// Input: 
    /// none
    /// Return: 
    /// IEnumerator
    /// </summary>
    /// <returns>Allows this to function as a coroutine</returns>
    private IEnumerator ResetJumpStart()
    {
        yield return new WaitForEndOfFrame();
        jumpStarted = false;
    }

    [Header("Pause Input")]
    [Tooltip("The state of the pause button")]
    public float pauseButton = 0;

    /// <summary>
    /// Description:
    /// Collects pause button input
    /// Input: 
    /// CallbackContext callbackContext
    /// Returns:
    /// void (no return)
    /// </summary>
    /// <param name="callbackContext">The context of the pause input</param>
    public void GetPauseInput(InputAction.CallbackContext callbackContext)
    {
        pauseButton = callbackContext.ReadValue<float>();
    }

    [Header("Mouse Input")]
    [Tooltip("The horizontal mouse input of the player.")]
    public float horizontalLookAxis;
    [Tooltip("The vertical mouse input of the player.")]
    public float verticalLookAxis;

    /// <summary>
    /// Description:
    /// Collects movementInput
    /// Input: 
    /// CallbackContext callbackContext
    /// Return:
    /// void (no return)
    /// </summary>
    /// <param name="callbackContext">The context of the mouse input</param>
    public void GetMouseLookInput(InputAction.CallbackContext callbackContext)
    {
        Vector2 mouseLookVector = callbackContext.ReadValue<Vector2>();
        horizontalLookAxis = mouseLookVector.x;
        verticalLookAxis = mouseLookVector.y;
    }
    void Start()
    {
        dragMultiplier = 1;
        gravityMultiplier = 1;
        backupMoveSpeed = moveSpeed;
        backupMaxSpeed = maxSpeed;
        backupJumpLeniency = jumpLeniency;
        if (doubleJumpEnabled)
        {
            jumps = 2;
        }
        else
        {
            jumps = 1;
        }
        playersList = GameObject.FindObjectsOfType<PlayerControllerPlus>();
        this.gameObject.GetComponent<Health>().teamId = playersList.Length - 1;
        Animator animator = gameObject.transform.Find("PlayerRep/CharAnim").GetComponent<Animator>();
        switch (playersList.Length)
        {
            case 1:
                animator.runtimeAnimatorController = Resources.Load("BluePlayerAnimatorController") as RuntimeAnimatorController;
                break;
            case 2:
                transform.position = new Vector3(transform.position.x + 2, transform.position.y, transform.position.z);
                animator.runtimeAnimatorController = Resources.Load("RedPlayerAnimatorController") as RuntimeAnimatorController;
                break;
            case 3:
                transform.position = new Vector3(transform.position.x + 4, transform.position.y, transform.position.z);
                animator.runtimeAnimatorController = Resources.Load("YellowPlayerAnimatorController") as RuntimeAnimatorController;
                break;
            case 4:
                transform.position = new Vector3(transform.position.x + 6, transform.position.y, transform.position.z);
                animator.runtimeAnimatorController = Resources.Load("PurplePlayerAnimatorController") as RuntimeAnimatorController;
                break;
            default:
                Debug.LogError("Invalid player count!");
                break;
        }

    }
    // Update is called once per frame
    void Update()
    {
        // A silly fix to slow motion bug
        if (moveSpeed < backupMoveSpeed)
        {
            moveSpeed = backupMoveSpeed;
        }
        if (maxSpeed < backupMaxSpeed)
        {
            maxSpeed = backupMaxSpeed;
        }
        if (jumpLeniency != backupJumpLeniency && !GameManagerPlus.isSloMotion)
        {
            jumpLeniency = backupJumpLeniency;
        }
        bool wasOnGround = onGround;
        bool wasOnStairs = onStairs;
        onGround = Physics2D.Raycast(transform.position + colliderOffset, Vector2.down, groundLength, groundLayer) ||
                   Physics2D.Raycast(transform.position - colliderOffset, Vector2.down, groundLength, groundLayer) ||
                   Physics2D.Raycast(transform.position + colliderOffset, Vector2.down, groundLength, boxLayer) ||
                   Physics2D.Raycast(transform.position - colliderOffset, Vector2.down, groundLength, boxLayer); // this does not belong in FixedUpdate()
        onStairs = Physics2D.Raycast(transform.position + colliderOffset, Vector2.down, groundLength, groundLayer) ^
                   Physics2D.Raycast(transform.position - colliderOffset, Vector2.down, groundLength, groundLayer);
        if (stairTimer < 1 && !onStairs)
        {
            stairTimer += Time.deltaTime;
        }
        onWallLeft = Physics2D.Raycast(transform.position + Vector3.down * 0.36f, Vector2.left, sideLength, groundLayer) ||
                     Physics2D.Raycast(transform.position + Vector3.up * 0.36f, Vector2.left, sideLength, groundLayer);
        onWallRight = Physics2D.Raycast(transform.position + Vector3.down * 0.36f, Vector2.right, sideLength, groundLayer) ||
                      Physics2D.Raycast(transform.position + Vector3.up * 0.36f, Vector2.right, sideLength, groundLayer);
        if ((onWallLeft && !onGround))
        {
            wallLeniencyTimerLeft = 0.25f;
        }
        else if ((onWallRight && !onGround))
        {
            wallLeniencyTimerRight = 0.25f;
        }
        if (enableLandSqueeze && !wasOnGround && onGround)
        {
            wallLeniencyTimerLeft = wallLeniencyTimerRight = 0;
            if (doubleJumpEnabled)
            {
                jumps = 2;
            }
            else
            {
                jumps = 1;
            }
            if (!(wasOnStairs || onStairs)) StartCoroutine(JumpSqueeze(1.25f, 0.8f, 0.05f));
            if (fixLandingAudioBug >= 0.5f)
            {
                GameObject.Instantiate(landEffect, transform.position, Quaternion.identity, null);
                fixLandingAudioBug = 0f;
            }

        }
        if (jumpStarted)
        {
            jumpTimer = Time.time + jumpLeniency;
        }
        direction = new Vector2(horizontalMovement, verticalMovement);
        wallLeniencyTimerLeft -= Time.unscaledDeltaTime;
        wallLeniencyTimerRight -= Time.unscaledDeltaTime;
        if (!wasOnStairs && onStairs)
        {
            //rb.gravityScale = 0;
            if (rb.velocity.y > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0);
            }
            else
            {
                //rb.gravityScale = gravity * 1000;
                if (stairTimer > 0.5 && (Mathf.Abs(direction.x) < 0.4 || (direction.x > 0 && rb.velocity.x < 0) || (direction.x < 0 && rb.velocity.x > 0)))
                {
                    //Debug.Log("RAN!!");
                    rb.velocity = Vector2.zero;
                }
            }

            //Debug.Log("Landed on stairs");
        }
        if (wasOnStairs && !onStairs && !(jumpStarted || jumpHeld))
        {
            //rb.gravityScale *= Mathf.Sqrt(2);
            if (rb.velocity.y > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0);
            }
            else
            {
                //rb.gravityScale = gravity * 1000;
            }
            //Debug.Log("Left stairs");
        }
        if (onStairs)
        {
            stairTimer = 0f;
        }

        if ((transform.position.y <= killHeight || Mathf.Abs(transform.position.x) >= 20f) && isAlive && SceneManager.GetActiveScene().name != "Stage 2")
        {
            gameObject.GetComponent<Health>().TakeDamage(999);
        }
        fixLandingAudioBug += Time.deltaTime;
    }

    // Physics stuff here
    // Physics is superior for movement than what MSU controller does
    void FixedUpdate()
    {
        if (!preventInfinite)
        {
            moveCharacter(direction.x);
        }
        else if (rb.velocity.x > 0)
        {
            moveCharacter(direction.x > 0 ? direction.x : 0);
        }
        else if (rb.velocity.x < 0)
        {
            moveCharacter(direction.x < 0 ? direction.x : 0);
        }
        if ((jumpTimer > Time.time && jumps > 0) || (wallJumpEnabled && jumpTimer > Time.time && ((onWallLeft) || (onWallRight))))
        {
            Jump();
        }
        modifyPhysics();
    }
    void moveCharacter(float horizontal)
    {
        if (onStairs)
        {
            rb.AddForce(Vector2.right * horizontal * moveSpeed * Mathf.Sqrt(2));
        }
        else
        {
            rb.AddForce(Vector2.right * horizontal * moveSpeed);
        }


        // Flip character when character is facing opposite direction of input
        if ((horizontal < 0 && facingRight) || (horizontal > 0 && !facingRight))
        {
            Flip();
        }
        if (Mathf.Abs(rb.velocity.x) > maxSpeed)
        {
            rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);
        }
        animator.SetFloat("horizontal", Mathf.Abs(rb.velocity.x));
        animator.SetFloat("vertical", rb.velocity.y);
    }
    void Jump()
    {
        if (wallJumpEnabled && wallLeniencyTimerLeft > 0)
        {
            rb.velocity = new Vector2(0, 0);
            rb.AddForce(1.3f * Vector2.up * wallJumpStrength + Vector2.right * 1.3f * wallJumpStrength, ForceMode2D.Impulse);
            jumpTimer = 0f;
            preventInfinite = true;
            jumps = 0;
            facingRight = true;
            transform.rotation = Quaternion.Euler(0, 0, 0);
            this.gameObject.transform.GetChild(4).rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (wallJumpEnabled && wallLeniencyTimerRight > 0)
        {
            rb.velocity = new Vector2(0, 0);
            rb.AddForce(1.3f * Vector2.up * wallJumpStrength - Vector2.right * 1.3f * wallJumpStrength, ForceMode2D.Impulse);
            jumpTimer = 0f;
            preventInfinite = true;
            jumps = 0;
            facingRight = false;
            transform.rotation = Quaternion.Euler(0, 180, 0);
            this.gameObject.transform.GetChild(4).rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(Vector2.up * jumpStrength, ForceMode2D.Impulse);
            jumpTimer = 0f;
            --jumps;
        }
        if (enableJumpSqueeze) StartCoroutine(JumpSqueeze(0.5f, 1.2f, 0.1f));
        GameObject.Instantiate(jumpEffect, transform.position, Quaternion.identity, null);
    }
    void modifyPhysics()
    {
        bool changingDirections = (direction.x > 0 && rb.velocity.x < 0) || (direction.x < 0 && rb.velocity.x > 0); // fix slow direction changes
        if (onGround)
        {
            if (Mathf.Abs(direction.x) < 0.4 || changingDirections)
            {
                rb.drag = linearDrag * dragMultiplier;
            }
            else
            {
                rb.drag = 0f;
            }
            rb.gravityScale = 0f;
        }
        else
        {
            rb.gravityScale = gravity * gravityMultiplier;
            rb.drag = linearDrag * 0.15f;
            if (rb.velocity.y < 0)
            {
                preventInfinite = false;
                if (slideOnWalls && ((onWallLeft && horizontalMovement < 0) || (onWallRight && horizontalMovement > 0)))
                {
                    rb.gravityScale = slideSpeed * gravityMultiplier;
                }
                else
                {
                    rb.gravityScale = gravity * fallStrength * gravityMultiplier;
                }
            }
            else if (rb.velocity.y > 0 && !jumpHeld)  // wait to apply full gravity until jump button released
            {
                rb.gravityScale = gravity * (fallStrength / 2) * gravityMultiplier;
            }
        }
    }
    void Flip()
    {
        facingRight = !facingRight;
        transform.rotation = Quaternion.Euler(0, facingRight ? 0 : 180, 0); // <3 ternary operator
        this.gameObject.transform.GetChild(4).rotation = Quaternion.Euler(0, 0, 0);

    }
    IEnumerator JumpSqueeze(float xSqueeze, float ySqueeze, float seconds)
    {
        Vector3 originalSize = Vector3.one;
        Vector3 newSize = new Vector3(xSqueeze, ySqueeze, originalSize.z);
        float t = 0f;
        while (t <= 1.0)
        {
            t += Time.unscaledDeltaTime / seconds;
            playerRep.transform.localScale = Vector3.Lerp(originalSize, newSize, t);
            yield return null;
        }
        t = 0f;
        while (t <= 1.0)
        {
            t += Time.unscaledDeltaTime / seconds;
            playerRep.transform.localScale = Vector3.Lerp(newSize, originalSize, t);
            yield return null;
        }

    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + colliderOffset, transform.position + colliderOffset + Vector3.down * groundLength);
        Gizmos.DrawLine(transform.position - colliderOffset, transform.position - colliderOffset + Vector3.down * groundLength);
        Gizmos.DrawLine(transform.position + Vector3.down * 0.36f, transform.position + Vector3.down * 0.36f + Vector3.left * sideLength);
        Gizmos.DrawLine(transform.position + Vector3.up * 0.36f, transform.position + Vector3.up * 0.36f + Vector3.left * sideLength);
        Gizmos.DrawLine(transform.position + Vector3.down * 0.36f, transform.position + Vector3.down * 0.36f + Vector3.right * sideLength);
        Gizmos.DrawLine(transform.position + Vector3.up * 0.36f, transform.position + Vector3.up * 0.36f + Vector3.right * sideLength);
        //Gizmos.DrawLine(transform.position + Vector3.up * (groundLength / 2 - 0.2f), transform.position + Vector3.up * (groundLength / 2 - 0.2f) - Vector3.right * sideLength);
    }
}
