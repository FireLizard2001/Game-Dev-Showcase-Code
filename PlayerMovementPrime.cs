using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementPrime : MonoBehaviour
{
    public Rigidbody2D rb;
    private Animator anim;
    public GameManager GM;

    public AudioSource audioFootstep;
    public AudioSource audioJump;

    public bool isAlive = true;

    private float movementInputDirection;
    public float currentVelocity;
    public float moveSpeed = 10f;
    public float forceFrictionMagnitude = 0.3f;
    public float groundHorizontalAccelerationForce = 0.2f;
    public float maxHorizontalVelocity = 3f;
    public float jumpVelocity = 5f;
    public float smallJumpMultiplier = 2.5f;
    public float gravityUpMultiplier = 2f;
    public float gravityDownMultiplier = 2f;
    public float velocityToApplyGravity;
    public float airDragMultiplier;
    public float maxAirMovementSpeed;
    public float airAcceleration;

    private int directionFaced;

    private bool isFacingLeft;
    private bool isRunning;
    public bool isGrounded;

    public Transform groundCheck;

    public float groundCheckBoxHeight = 1f;
    public float groundCheckBoxLength = 1f;
    public float groundCheckBoxDepth = 1f;
    public float groundCheckBoxHorizontalOffset = 0f;
    
    public LayerMask whatIsGround;

    // Start is called before the first frame update
    //Sets the components of the rigid body and calls the animator function
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame

    //Calls the function to the determine direction, jump, update the animation
    //and checks the surroundings of the character every frame.
    void Update()
    {
        if (isAlive)
        {
            DetermineDirection();
            JumpUp();
        }
        IsRunning();
        UpdateAnimation();
        CheckSurroundings();

        if (isRunning)
        {
            if (!audioFootstep.isPlaying && isGrounded == true)
            {
                audioFootstep.Play();
            }
        }
        else
        {
            audioFootstep.Stop();
        }
    }


    //Moved the apply movement function to fixed update as it 
    //works with physics and applies movement to the character with 
    //physics components and functions
    private void FixedUpdate()
    {
        if (isAlive)
        {
            ApplyMovement();
        }
        ApplyGroundFriction();
    }

    //Flips the char sprite depending on the direction faced variable
    //Swapping the 1 and -1 reverses the direction faced.
    public void Flip()
    {
        if(directionFaced == -1 && !isFacingLeft)
        { 
            transform.Rotate(0.0f, 180.0f, 0.0f);
            isFacingLeft = !isFacingLeft;
        }
        else if(directionFaced == 1 && isFacingLeft)
        {
            transform.Rotate(0.0f, 180.0f, 0.0f);
            isFacingLeft = !isFacingLeft;
        }
    }

    //Draws gizmos for the collider onto the character
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(238, 238, 0, 1);
        Gizmos.DrawCube(new Vector2(transform.position.x + groundCheckBoxHorizontalOffset, transform.position.y - groundCheckBoxHeight/2 - 0.005f), new Vector2(groundCheckBoxLength, 0.01f)); //draws a cube
    }


    //Function checks the surroundings of the character to tell if the player is grounded or not
    //based on a layer mask which encompasses the ground.
    public void CheckSurroundings()
    {
        isGrounded = Physics2D.OverlapArea(new Vector2((transform.position.x - groundCheckBoxLength / 2), transform.position.y - groundCheckBoxHeight / 2) , 
            (new Vector2((transform.position.x + groundCheckBoxLength / 2), transform.position.y - groundCheckBoxHeight / 2 - 0.01f)), whatIsGround);
    }



    //This function applies movement to the player. It applies it in the horizontal and verticle.
    //It changes the acceleration, max velocity allowed, drag / frictional force and direction based on
    //if the character is grounded or not and the directionFaced variable.
    public void ApplyMovement()
    {
        movementInputDirection = Input.GetAxis("Horizontal");

        //Movement on the ground

        // Make smooth directional change. If traveling right and switching to left, set current velocity to 0 and vice versa.
        if ((rb.velocity.x < 0 && Input.GetAxisRaw("Horizontal") == 1) || (rb.velocity.x > 0 && Input.GetAxisRaw("Horizontal") == -1))
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        // On Horizontal button press, if absolute velocity is less than the maximum
        if (Input.GetAxisRaw("Horizontal") != 0 && (Mathf.Abs(rb.velocity.x) < maxHorizontalVelocity) && isGrounded)
        { 
            // Apply acceleration to velocity in the direction faced by the player
            rb.velocity = new Vector2(rb.velocity.x + groundHorizontalAccelerationForce * directionFaced * Time.deltaTime, rb.velocity.y);

            // Checks to see if adding acceleration goes over maxvelocity and then tops off at mex velocity if so
            if (Mathf.Abs(rb.velocity.x) > maxHorizontalVelocity)
            {
                rb.velocity = new Vector2(maxHorizontalVelocity * directionFaced, rb.velocity.y);
            }
        }
        // If velocity is at max and direction is being pressed, maintain max velocity
        else if(Input.GetAxisRaw("Horizontal") != 0 && isGrounded)
        {
            rb.velocity = new Vector2(maxHorizontalVelocity * directionFaced, rb.velocity.y);
        }

        currentVelocity = rb.velocity.x;

        //Movement in the air
        // vertical component
        if (!isGrounded)
        {
            //make into function to appease Shawn - delete before putting on resume please :( (Lol you know you'll forget to -Other Coder)
            if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
            {
                rb.velocity += Vector2.up * (Physics2D.gravity.y) * (smallJumpMultiplier - 1) * Time.deltaTime;
            }
            else if (0 < rb.velocity.y && rb.velocity.y < velocityToApplyGravity)
            {
                rb.velocity += Vector2.up * (Physics2D.gravity.y) * (gravityUpMultiplier - 1) * Time.deltaTime;
            }
            else if (rb.velocity.y < 0)
            {
                rb.velocity += Vector2.up * (Physics2D.gravity.y) * (gravityDownMultiplier - 1) * Time.deltaTime;
            }

            //edits in this function for in air movement here on
            //call x component function
            if(Input.GetAxisRaw("Horizontal") != 0 && (Mathf.Abs(rb.velocity.x) < maxAirMovementSpeed))
            {
                rb.velocity = new Vector2(rb.velocity.x + airAcceleration * directionFaced * Time.deltaTime, rb.velocity.y);

            }
            if(Input.GetAxisRaw("Horizontal") == 0) {
                rb.velocity = new Vector2(rb.velocity.x + (-rb.velocity.x * airDragMultiplier), rb.velocity.y);
            }
        }
    }

    // Constantly apply friction when on ground including on player death
    public void ApplyGroundFriction()
    {
        // If no directional input being pressed
        if ((Input.GetAxisRaw("Horizontal") == 0 || !isAlive) && rb.velocity.x != 0 && isGrounded)
        {
            // Applies Frictional Force / deceleration to horizontal movement.
            rb.velocity = new Vector2(rb.velocity.x + (-rb.velocity.x * forceFrictionMagnitude), rb.velocity.y);

        }
    }


    //This function determines the direction faced of the character
    //based on the input from the player and calls the Flip() function
    //if and only if the direction faced is the opposite of the raw horizontal input.
    public void DetermineDirection()
    {
        // Determines direction faced by player and flips animation if need be.
        if (Input.GetAxisRaw("Horizontal") < 0)
        {
            directionFaced = -1;
            Flip();
        }
        else if (Input.GetAxisRaw("Horizontal") > 0)
        {
            directionFaced = 1;
            Flip();
        }
    }

    public void IsRunning()
    {
        // Checks to see if stationary or not
        if (Input.GetAxisRaw("Horizontal") != 0 && isAlive)
        {
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }
    }


    //This function applies the jump action onto the character
    //if they are grounded and they input the "Jump" key.
    public void JumpUp()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);
        }
    }

    


    //This function updates the animation of the character sprite.
    //The parameters must be named the same in the editor
    private void UpdateAnimation()
    {
        anim.SetBool("isRunning", isRunning);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.velocity.y / jumpVelocity);
    }
}
