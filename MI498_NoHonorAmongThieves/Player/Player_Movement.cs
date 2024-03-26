#region Namespaces
using UnityEngine;
using System.Collections;
using Inputs;
#endregion

namespace PlayerFunctionality {
/// <summary> Controls player movement (WASD / Left Joystick) </summary>
[RequireComponent(typeof(CharacterController))]
public class Player_Movement : MonoBehaviour
{
	#region Attributes

		/* Serialized Fields */
		[Header("General Settings")]
			[Tooltip("Speed the player moves")] 
			[SerializeField] [Range(1, 10)] float moveSpeed = 1f;
			
			[Tooltip("Speed the player sprints while holding an item")] 
			[SerializeField] [Range(1, 15)] float slowSprintSpeed = 1.5f;
			
			[Tooltip("Speed the player sprints")] 
			[SerializeField] [Range(1, 20)] float sprintSpeed = 2f;

			[Tooltip("Speed the player jumps")] 
			[SerializeField] [Range(1, 10)] float jumpSpeed = 1f;

			[Tooltip("Forgiveness for jumping when not grounded")] 
			[SerializeField] [Range(0, 2)] float groundedForgiveness = 0.25f;

		[Header("Crouch Settings")]

			[Tooltip("Speed the player crouches")] 
			[SerializeField] [Range(1, 20)] float crouchSpeed = 10f;

			[Tooltip("Position of the camera while standing")] 
			[SerializeField] Vector3 standCamPos;

			[Tooltip("Position of the camera while crouching")] 
			[SerializeField] Vector3 crouchCamPos;

		[Header("Other Settings")]

			[Tooltip("The player's gravity modifier")] 
			[SerializeField] [Range(-10, 0)] float gravity = -9.81f;

			[Tooltip("The player's knockback modifier")] 
			[SerializeField] [Range(1, 10)] float knockbackMass = 1;

			[Tooltip("The knockback amount when hit by thrown breakable")] 
			[SerializeField] [Range(0, 50)] float breakableKnockback = 25;

			[Tooltip("Time to wait between rub hands idle animation")] 
			[SerializeField] [Range(0, 60)] protected float idleHandsDelay = 20;

			[Tooltip("Indicates if the player is paused")]
			public bool isPaused = false;

		/* Private General */
		CharacterController _charController;
		float _speedMultiplier = 2.5f;   // Keeps inspector values cleaner
		float _verticalVelocity;         // Used for gravity / jumping
		bool _isSprinting = false;		 // Is the player currently sprinting?
		bool _sprintHeld = false;        // Is the sprint button being held?
		float _prevYPos;                 // Caches y pos of prev frame
		Vector3 _impact;                 // Used to control knockback
		float _magLimit;                 // Limit for movement during knockback
		float _groundTimer;				 // How long has the player been off the ground?
		bool _jumpInProgress;			 // Is the player in the air due to jumping?

		/* Private Crouching */
		bool _isCrouching = false;       // Determines if player is crouching
		bool _crouchInProgress = false;  // Is the player mid-crouch?
		Vector3 _targetPos;              // Target position for camera
		Vector3 _initPos;                // Initial position for camera
		float _crouchTimer = 0;          // Progress of current crouch

		/* Private Animations */
		protected float _idleTimer = 0;			 // How long has player been idle?

		/* Private References */
		Player_Interact _playerInteract;
		Player_Camera _playerCamera;
		Transform _camTrans;

		/* Animator References */
		public Animator anim;

	#endregion
	#region Methods

		public virtual void Awake()
		{
			_charController = GetComponent<CharacterController>();
			_playerInteract = GetComponent<Player_Interact>();
			_playerCamera = GetComponentInChildren<Player_Camera>();
			_camTrans = _playerCamera.transform;
			_playerCamera.DefaultPos = standCamPos;
		}
		void Start()
		{
			GameInputManager.Instance.JumpPressed += AttemptJump;
			GameInputManager.Instance.CrouchPressed += AttemptCrouch;
			GameInputManager.Instance.SprintDown += SprintDown;
			GameInputManager.Instance.SprintUp += SprintUp;

			_targetPos = standCamPos;
		}
		void Update()
		{
			if (anim == null) { if (GetComponentInChildren<Animator>() != null) { anim = GetComponentInChildren<Animator>(); } }
			bool isKnockback = ApplyKnockback();
			if (isKnockback) return;

			DoUpdate();
		}

		public virtual void DoUpdate()
		{
			AttemptSprint();
			Move();
			Crouch();

			if (_charController.isGrounded)
			{
				_groundTimer = 0;
				if (_jumpInProgress)
					_jumpInProgress = false;
			}
			else
				_groundTimer += Time.deltaTime;
		}

		/// <summary> If hit by an interactable object, get knocked back </summary>
		void OnCollisionEnter(Collision coll)
		{
			AttemptCollision(coll);
		}

		public virtual void AttemptCollision(Collision coll)
		{
			// Not interactable
			if (!coll.gameObject.CompareTag("Breakable")) return;

			// Already being knocked back.
			if (_impact.magnitude > 0.25f) return;

			// Calculate collision direction
			Vector3 dir = coll.contacts[0].point - transform.position;
			dir = new Vector3(dir.x, 0, dir.z);
			dir = -dir.normalized;

			// Apply knockback
			Knockback(dir, breakableKnockback);
		}

		/// <summary> Get this player's movement direction </summary>
		public Vector3 GetMoveDir() => _charController.velocity;

		/// <summary> Is the player currently crouching? </summary>
		public bool IsCrouching() => _isCrouching;

		/// <summary> Is the player mid crouch? </summary>
		public bool IsMidCrouch() => _crouchInProgress;

		/// <summary> Is the player currently in the air? </summary>
		public bool IsJumping() => !_charController.isGrounded;
		
		/// <summary> Is the player currently sprinting? </summary>
		public bool IsSprinting() => _isSprinting;

		/// <summary> Knockback this player </summary>
		/// <param name="dir"> The direction to be knocked back </param>
		/// <param name="force"> How far to get knocked back </param>
		public void Knockback(Vector3 dir, float force)
		{
		   dir.Normalize();

		   // Reflect down force on the ground
		   if (dir.y < 0) dir.y = -dir.y;

		   // Add the knockback
		   _impact += dir * force / knockbackMass;

		   // Set the magnitude limit to 90% of magnitude
		   _magLimit = _impact.magnitude * 0.9f;
		   
		   // Turn off sprinting
		   _isSprinting = false;
		}

		/// <summary> Handles actually knocking back the player </summary>
		/// <returns> true if currently being knocked back </returns>
		bool ApplyKnockback()
		{
			// Stop here if not being knocked back
			if (_impact.magnitude == 0) return false;

			// Apply the impact force
			if (_impact.magnitude > 0.25F)
			{
				_charController.Move(_impact * Time.deltaTime);
			}
			_impact = Vector3.Lerp(_impact, Vector3.zero, 5 * Time.deltaTime);

			// Don't allow player movement
			if (_impact.magnitude > _magLimit) return true;
			else return false;
		}

		/// <summary> Main functionality for player movement </summary>
		protected void Move()
		{
			if (!_charController.enabled) return;
			//if (_playerInteract.IsStunned) return;

			// Get player input from InputManager
			Vector2 moveInput = new Vector2(0f, 0f);
			if (!isPaused && !_playerInteract.IsStunned)
			{
				moveInput = GameInputManager.Instance.MoveInput;
			}

			// Calculate move direction
			Vector3 moveDirection = Vector3.zero;

			moveDirection = transform.forward * moveInput[1] + transform.right * moveInput[0];

			// Reset vertical velocity if grounded
			if (_charController.isGrounded) 
				_verticalVelocity = 0;

			// Or hit their head on object
			else if (!_charController.isGrounded && _prevYPos == transform.position.y) 
				_verticalVelocity = 0;

			// Apply gravity
			_verticalVelocity += gravity * Time.deltaTime;

			// Cache y pos
			_prevYPos = transform.position.y; 

			// Calculate move speed
			float curSpeed = moveSpeed;
			if (_isSprinting && !_playerInteract.IsStunned)
			{
				// Holding an item
				if (_playerInteract.IsHoldingItem())
					curSpeed = slowSprintSpeed;
				else
					curSpeed = sprintSpeed;
			}
				
			// Move the player
			moveDirection *= curSpeed * _speedMultiplier;
			moveDirection.y = _verticalVelocity * moveSpeed * _speedMultiplier;
			_charController.Move(moveDirection * Time.deltaTime);

			// Update the animations
			UpdateAnimations(moveDirection);
		}

		/// <summary> pdates the animation controller </summary>
		public virtual void UpdateAnimations(Vector3 moveDirection)
        {
			// Update animations
			if (moveDirection.magnitude > 5)
			{
				anim.SetTrigger("MoveTrigger");
				anim.ResetTrigger("IdleTrigger");
				_idleTimer = 0;
			}
			else
			{
				anim.SetTrigger("IdleTrigger");
				anim.ResetTrigger("MoveTrigger");

				_idleTimer += Time.deltaTime;
				if (_idleTimer >= idleHandsDelay)
				{
					anim.CrossFade("Armature_001|Idle2Mischief", 0.1f);
					_idleTimer = 0;
				}
			}
			anim.SetBool("isSprinting", IsSprinting());
			anim.SetBool("isJumping", IsJumping());
		}

		/// <summary> Attempts to jump when input is given </summary>
		void AttemptJump()
		{
			// Check if controller is destroyed
			if (_charController == null) return;

			// Check if we are paused
			if (isPaused) return;

			// Check if player is stunned
			if (_playerInteract.IsStunned) return;

			// Don't jump if in the air or mid-crouch
			if (!GroundCheck())
				return;

			// If player is crouching, stand up
			if (_isCrouching)
			{
				AttemptCrouch();
				return;
			}

			_jumpInProgress = true;

			// Set the y value to jump speed
			_verticalVelocity = jumpSpeed;
			Vector3 moveDirection = _charController.velocity;
			moveDirection.y = _verticalVelocity;

			// Move the player
			_charController.Move(moveDirection * Time.deltaTime);
		}

		/// <summary> Attempts to crouch when the input is given
		void AttemptCrouch()
		{
			// Check if controller is destroyed
			if (_charController == null) return;

            // Check if we are paused
            if (isPaused) return;

            // Don't crouch if in the air
            if (!_charController.isGrounded) return;

			// Check if player is stunned
			if (_playerInteract.IsStunned) return;

			// Don't crouch while ceiling above
			if (_playerInteract.CeilingDetected())
			{
				if (_isCrouching) return;
			}

			// Determine initial and target heights
			if (_isCrouching)
			{
				_targetPos = standCamPos;
				_initPos = crouchCamPos;
			}
			else
			{
				_targetPos = crouchCamPos;
				_initPos = standCamPos;
			}
			_crouchInProgress = true;
			_playerCamera.DefaultPos = _targetPos;

			// Toggle Crouch
			_isCrouching = !_isCrouching;
			
			// Turn off sprinting
			if (_isSprinting)
				_isSprinting = false;
		}
		
		/// <summary> Attempts to sprint when the input is given
		void SprintDown() => _sprintHeld = true;
		void SprintUp() => _sprintHeld = false;
		void AttemptSprint()
		{
			// Check if in the air
			if (!_charController.isGrounded) return;

            // Check if we are paused
            if (isPaused) return;

            // Check if player is stunned
            if (_playerInteract.IsStunned) return;
			
			// Check if crouching
			if (_isCrouching) return;
			
			_isSprinting = !_sprintHeld;
		}

		/// <summary> Lerps between standing and crouching height </summary>
		void Crouch()
		{
			// Check if we are currently attempting to crouch
			if (!_crouchInProgress) return;

			// Check if player is stunned
			if (_playerInteract.IsStunned) return;

			// If we are not at the currently desired height
			if (Vector3.Distance(_camTrans.localPosition , _targetPos) > 0.1f)
			{
				_crouchTimer += crouchSpeed * Time.deltaTime;
				_camTrans.localPosition = Vector3.Lerp(_initPos, _targetPos, _crouchTimer);
			}
			else
			{
				_crouchInProgress = false;
				_crouchTimer = 0;
				_camTrans.localPosition = _targetPos;
			}
		}

		/// <summary> Determines if player can jump </summary>
		bool GroundCheck()
		{
			// Is the player airborne due to jumping?
			if (_jumpInProgress) return false;

			// Player is grounded
			if (_charController.isGrounded)
			{
				// Mid-crouch?
				if (_crouchInProgress) return false;
				else return true;
			}

			// Player is within jump forgiveness timer
			if (_groundTimer <= groundedForgiveness)
				return true;
			else
				return false;
		}

	#endregion
}
}
