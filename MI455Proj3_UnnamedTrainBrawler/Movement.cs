using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [Header("Player")] public GameObject player;
    private Feet playerFeet;

    private Rigidbody2D rigid;
    //public PlayerAnimation anim;

    [Header("Customization")] public float moveSpeed;
    public float jumpForce;
    public float gravity;

    [Header("Debug")] public bool horizontalIsHeld;
    public float directionHeld;


    private void Start()
    {
        horizontalIsHeld = false;
        directionHeld = 0;
        rigid = player.GetComponent<Rigidbody2D>();
        playerFeet = player.GetComponentInChildren<Feet>();
    }

    void Update()
    {
        CheckMovement();
        CalculateGravity();
    }

    void CheckMovement()
    {
        if (horizontalIsHeld)
        {
            MoveHorizontally(directionHeld);
        }
        else
        {
            SlowXMovement();
        }
    }

    private void MoveHorizontally(float dir)
    {
        rigid.velocity = new Vector2(moveSpeed * dir, rigid.velocity.y);
        RotateSprite(dir);
        
    }

    private void RotateSprite(float dir)
    {
        if (dir < 0)
        {
            player.transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (dir > 0)
        {
            player.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    private void Jump()
    {
        if (playerFeet.isGrounded)
        {
            rigid.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    private void SlowXMovement()
    {
        rigid.velocity = new Vector2(0, rigid.velocity.y);
    }


    private void CalculateGravity()
    {
        rigid.AddForce(Vector2.down * gravity, ForceMode2D.Force);
    }

    public void ToggleHorizontal(InputAction.CallbackContext context)
    {
        directionHeld = context.ReadValue<Vector2>().x;
        horizontalIsHeld = !context.canceled;
    }

    public void ToggleUp(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Jump();
        }
    }
}