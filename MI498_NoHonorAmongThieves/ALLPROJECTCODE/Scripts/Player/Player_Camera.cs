#region Namespaces
using UnityEngine;
using UnityEngine.InputSystem;
using Inputs;
#endregion

namespace PlayerFunctionality {
/// <summary> Controls player camera (Mouse / Right Joystick) </summary>
public class Player_Camera : MonoBehaviour
{
	#region Attributes

		/* Public */
		[HideInInspector] public Vector3 DefaultPos;

		/* Serialized Fields */
		[Header("General Settings")]
			[Tooltip("Look Sensitivity")] [Range(1, 10)]
			[SerializeField] protected float sensitivity = 1.0f;

			[Tooltip("Field of View")] [Range(20, 120)]
			[SerializeField] protected float fov = 60f;
			
			[Tooltip("Enables or Disables head bobble")]
			[SerializeField] protected bool enableBobbing = true;

			[Tooltip("Indicates if the player is paused")]
			public bool isPaused = false;

        [Header("Head Bobble Speed")]

			[Tooltip("How fast to head bobble while walking")]
			[SerializeField] protected float walkingBobbingSpeed = 14f;

			[Tooltip("How fast to head bobble while slow sprinting")]
			[SerializeField] protected float slowSprintingBobbingSpeed = 17.5f;
			
			[Tooltip("How fast to head bobble while sprinting")]
			[SerializeField] protected float sprintingBobbingSpeed = 20f;
			
		[Header("Head Bobble Amount")]

			[Tooltip("How far to head bobble while walking")]
			[SerializeField] protected float walkingBobbingAmount = 0.05f;
			
			[Tooltip("How far to head bobble while slow sprinting")]
			[SerializeField] protected float slowSprintingBobbingAmount = 0.0625f;
			
			[Tooltip("How far to head bobble while sprinting")]
			[SerializeField] protected float sprintingBobbingAmount = 0.075f;

		[Header("Camera Shake Settings")]

			[Tooltip("How hard to shake the camera")]
			[SerializeField] protected float shakeAmount = 0.1f;

		/* Protected */
		protected Transform _playerBody;
		protected CharacterController _controller;
		protected Player_Interact _playerInteract;
		protected Player_Movement _playerMovement;
		protected Transform _heldItemTrans;

		/* Private */
		float _xRotation = 0f;
		float _sensMultiplier = 0.1f;   // Keeps inspector values cleaner
		float _bobbleTimer = 0;
		float _shakeTimer = 0;
		bool _isShaking = false;

	#endregion
	#region Methods

		/// <summary> Causes this camera to shake for a duration </summary>
		/*public void CameraShake(float duration)
		{
			_shakeTimer = duration;
			_isShaking = true;
		}*/

		virtual protected void Awake()
		{
			Cursor.lockState = CursorLockMode.Locked;
			_playerBody = transform.parent;
			_controller = GetComponentInParent<CharacterController>();
			//DefaultPos = transform.localPosition;
			Camera.main.fieldOfView = fov;
			_playerInteract = GetComponentInParent<Player_Interact>();
			_playerMovement = GetComponentInParent<Player_Movement>();
			_heldItemTrans = transform.GetChild(0);
		}
		virtual protected void Start()
		{
			transform.localPosition = DefaultPos;
		}
		virtual protected void Update()
		{
			if (!isPaused)
			{
				Shake();
				Look();
				Bobble();
			}
		}
		
		/// <summary> Calculates and applies player input to camera </summary>
		protected void Look()
		{
			// Get player input from InputManager
			Vector2 cameraInput = GameInputManager.Instance.CameraInput;

			// Determine how far to move in x and y direction
			float camX = cameraInput.x * sensitivity * _sensMultiplier;
			float camY = cameraInput.y * sensitivity * _sensMultiplier;

			if (Gamepad.current != null)
			{
				camX *= 5;
				camY *= 5;
			}

			if (_playerInteract.IsStunned)
			{
				camX *= 0.33f;
				camY *= 0.33f;
			}

			// Adjust rotation accordingly
			_xRotation -= camY;
			_xRotation = Mathf.Clamp(_xRotation, -90, 90);

			// Apply rotations
			transform.localRotation = Quaternion.Euler(_xRotation, 0, 0);
			_playerBody.Rotate(Vector3.up * camX);
		}

		/// <summary> Creates a head bobble effect while moving </summary>
		protected void Bobble()
		{
			// Is bobbing disabled or camera shaking?
			if (!enableBobbing || _playerInteract.IsStunned ||
				 _isShaking || _playerMovement.IsJumping() || _playerMovement.IsMidCrouch()) return;
			
			// Adjust for sprinting
			float bobbleSpeed = walkingBobbingSpeed;
			float bobbleAmount = walkingBobbingAmount;
			if (_playerMovement.IsSprinting())
			{
				if (_playerInteract.IsHoldingItem())
				{
					bobbleSpeed = slowSprintingBobbingSpeed;
					bobbleAmount = slowSprintingBobbingAmount;	
				}
				else
				{
					bobbleSpeed = sprintingBobbingSpeed;
					bobbleAmount = sprintingBobbingAmount;	
				}
			}

			// Moving
			float newY = 0;
			if(Mathf.Abs(_controller.velocity.magnitude) > 0.1f)
			{
				_bobbleTimer += Time.deltaTime * bobbleSpeed;
				newY = DefaultPos.y + Mathf.Sin(_bobbleTimer) * bobbleAmount;
				transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
			}

			// Idle
			else
			{
				_bobbleTimer = 0;
				newY = Mathf.Lerp(transform.localPosition.y, DefaultPos.y, Time.deltaTime * bobbleSpeed);
				transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
			}

			_heldItemTrans.localPosition = new Vector3(_heldItemTrans.localPosition.x, -newY, _heldItemTrans.localPosition.z);
		}

		/// <summary> Creates a camera shake effect </summary>
		protected void Shake()
		{
			if (_shakeTimer == 0) return;

			transform.localPosition = DefaultPos + Random.insideUnitSphere * shakeAmount;
			_shakeTimer -= Time.deltaTime;

			if (_shakeTimer <= 0)
			{
				_shakeTimer = 0;
				transform.localPosition = DefaultPos;
				_isShaking = false;
			}
		}
	#endregion
}
}
