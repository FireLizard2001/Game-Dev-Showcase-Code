using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Base Physics")] [SerializeField]
    private float playerSpeed = 2.0f;

    [SerializeField] private float jumpHeight = 1.0f;
    [SerializeField] private float gravityValue = -9.81f;

    private Rigidbody2D rigid;
    private Vector3 playerVelocity;
    private Feet playerFeet;

    [Header("Movement and Jumping")] private Vector2 move;
    private Vector2 movementInput = Vector2.zero;
    private bool jumped = false;
    private bool hasJumped = false;
    private bool stoppedJumping = false;
    private bool crouched = false;
    private float direction = 1f;

    [Header("Attacking")] public Transform attackPoint;
    public float attackRange = 0.5f;
    public Vector2 airKickKnockback;
    public Vector2 punchKnockback;
    public Vector2 engineKnockback;
    public Vector2 keydropKnockback;
    public LayerMask playerLayer;
    private bool attacked = false;
    private bool hasAttacked = false;
    private bool stoppedAttacking = false;
    private bool airAttacking = false;
    private bool punching = false;

    /// Delay for attacks
    private bool canMove = true;

    public float punchDelay;

    [Header("Make sure attacks only apply once per hit and movement gets disabled for knockback")]
    private bool knockedBack = false;

    private bool damageOnceInAir = false;
    private bool damageOnceCrouching = false;
    private bool damageOncePunching = false;

    [Header("Grab/Throwing")] private bool grabbed = false;
    private bool stoppedGrabbing = false;

    private Animator anim;

    private void Start()
    {
        rigid = gameObject.GetComponent<Rigidbody2D>();
        playerFeet = gameObject.GetComponentInChildren<Feet>();
        anim = gameObject.GetComponent<Animator>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        jumped = context.performed;
        stoppedJumping = context.canceled;
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        crouched = context.performed;
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        attacked = context.performed;
        stoppedAttacking = context.canceled;
    }

    public void OnGrabThrow(InputAction.CallbackContext context)
    {
        grabbed = context.performed;
        stoppedGrabbing = context.canceled;
    }


    // Physics based stuff goes here
    void FixedUpdate()
    {
        if (!knockedBack && canMove)
        {
            Movement();
            Jump();
        }

        ApplyPhysics();
        HandleGravity();
    }


    // Non-Physics Based Stuff Goes Here
    private void Update()
    {
        DamageChecks();
        if (canMove && !knockedBack)
        {
            Attack();
        }
    }

    void DamageChecks()
    {
        if (playerFeet.isGrounded && damageOnceInAir)
        {
            damageOnceInAir = false;
        }
    }


    void HandleGravity()
    {
        // This is when the player lands on the ground
        if (playerFeet.isGrounded && playerVelocity.y < 0)
        {
            knockedBack = false;
            airAttacking = false;
            anim.SetBool("isJumping", false);
            ///Stops player velocity once on ground. Sets back to 0
            playerVelocity.y = 0f;
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
    }

    void Movement()
    {
        ///Controls horizontal movement of player. Only allow while grounded and not crouching
        //if (playerFeet.isGrounded)
        //{
        move = new Vector2(movementInput.x, 0);
        if (movementInput.x != 0)
        {
            direction = movementInput.x;
            RotateSprite();
        }

        if (move.x != 0)
        {
            anim.SetBool("isWalking", true);
        }
        else
        {
            anim.SetBool("isWalking", false);
        }
        //}
    }

    private void RotateSprite()
    {
        if (direction < 0)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (direction > 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    void Jump()
    {
        // Changes the height position of the player.
        if (jumped && playerFeet.isGrounded && !hasJumped)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
            hasJumped = true;
            anim.SetBool("isJumping", true);
            // Move attack point down while in air for kicks
            attackPoint.localPosition =
                new Vector3(attackPoint.localPosition.x, attackPoint.localPosition.y - 0.3f, 0f);
        }

        // Check when player lands after jumping
        if (stoppedJumping && playerFeet.isGrounded && hasJumped)
        {
            hasJumped = false;
            // Move attack point back up
            attackPoint.localPosition =
                new Vector3(attackPoint.localPosition.x, attackPoint.localPosition.y + 0.3f, 0f);
        }
    }

    void ApplyPhysics()
    {
        if (!knockedBack && canMove)
        {
            rigid.velocity = new Vector2(move.x * Time.deltaTime * playerSpeed, playerVelocity.y);
        }
        else
        {
            rigid.velocity = new Vector2(playerVelocity.x, playerVelocity.y);
        }
    }

    void Attack()
    {
        if (!hasAttacked)
        {
            AirAttack();
            PunchAttack();
        }

        if (stoppedAttacking && !airAttacking && !punching)
        {
            hasAttacked = false;
        }
    }

    void AirAttack()
    {
        if (!playerFeet.isGrounded && attacked)
        {
            airAttacking = true;
        }

        if (airAttacking)
        {
            // Detect other player
            Collider2D[] hitPlayer = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);

            // Damage objects in hitPlayer array
            foreach (Collider2D player in hitPlayer)
            {
                if (player.tag != "Player" && player.tag != "Engine")
                {
                    break;
                }

                airAttacking = false;
                hasAttacked = true;
                if (!playerFeet.isGrounded && !damageOnceInAir && player.tag != "Engine")
                {
                    damageOnceInAir = true;
                    //Debug.Log("AirKick");
                    //Drop key
                    DropKeyFromPlayer(player.gameObject);
                }

                if (player.tag == "Engine")
                {
                    DropKeyFromEngine(player.gameObject, ref damageOnceInAir);
                }
            }
        }
    }

    void PunchAttack()
    {
        if (playerFeet.isGrounded && !crouched && !airAttacking && attacked && !hasJumped)
        {
            punching = true;
        }

        if (punching)
        {
            // Did a punch, remove horizontal velocity
            rigid.velocity = new Vector2(0f, 0f);
            playerVelocity = new Vector2(0f, 0f);
            StartCoroutine(PunchDelay());

            hasAttacked = true;

            // Detect other player
            Collider2D[] hitPlayer = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);

            foreach (Collider2D player in hitPlayer)
            {
                if (player.tag != "Player" && player.tag != "Engine")
                {
                    break;
                }

                if (!damageOncePunching && player.tag != "Engine")
                {
                    damageOncePunching = true;
                    //Debug.Log("Punch");
                    DropKeyFromPlayer(player.gameObject);
                }

                if (player.tag == "Engine")
                {
                    DropKeyFromEngine(player.gameObject, ref damageOncePunching);
                }
            }
        }
    }

    IEnumerator PunchDelay()
    {
        canMove = false;
        yield return new WaitForSeconds(punchDelay);
        canMove = true;
        damageOncePunching = false;
        punching = false;
    }


    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    private void DropKeyFromEngine(GameObject player, ref bool isCombat)
    {
        isCombat = true;
        KeyManager keyManager = GameObject.FindWithTag("KeyManager").GetComponent<KeyManager>();
        TrainEngine trainEngine = player.gameObject.GetComponent<TrainEngine>();
        if (trainEngine.keyInserted && (keyManager.getIDByPlayer(this.gameObject) == trainEngine.engineID))
        {
            trainEngine.keyInserted = false;
            keyManager.DropKey(player.gameObject, new Vector3(engineKnockback.x * direction, engineKnockback.y, 0f));
        }
    }

    private void DropKeyFromPlayer(GameObject player)
    {
        KeyManager keyManager = GameObject.FindWithTag("KeyManager").GetComponent<KeyManager>();
        if (keyManager.player_holding == keyManager.getIDByPlayer(player.gameObject))
        {
            keyManager.DropKey(player.gameObject,
                new Vector3(keydropKnockback.x * -direction, keydropKnockback.y, 0f));
        }

        player.GetComponent<PlayerInputHandler>().knockedBack = true;
        player.GetComponent<PlayerInputHandler>().playerVelocity =
            new Vector3(keydropKnockback.x * direction, keydropKnockback.y, 0f);
    }
}