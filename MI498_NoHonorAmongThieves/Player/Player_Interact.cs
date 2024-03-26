#region Namespaces
using UnityEngine;
using UnityEngine.UI;
using InteractableObjects;
using Inputs;
#endregion

namespace PlayerFunctionality {
	/// <summary> Allows player to interact with interactable objects </summary>
	public class Player_Interact : MonoBehaviour
	{
		#region Attributes

		[Header("Settings")]
		[Tooltip("How far can player interact with objects?")]
		[SerializeField] [Range(1, 100)] int interactRange = 1;

		[Tooltip("How much does movement impact throw velocity?")]
		[SerializeField] [Range(0, 10)] float moveImpact = 1;

		[Tooltip("Duration to wait between successful pushes")]
		[SerializeField] [Range(0, 10)] protected float pushCooldown = 1;

		[Tooltip("How far to knockback players with a push")]
		[SerializeField] [Range(0, 50)] float pushVelocity = 25;

		[Tooltip("Duration to wait between successful throws")]
		[SerializeField] [Range(0, 10)] float throwCooldown = 1;

		[Tooltip("How long to stun player with a push")]
		[SerializeField] [Range(0, 5)] float stunDuration = 1;

		/*[Tooltip("How long to shake camera when initially stunned")]
		[SerializeField] [Range(0, 5)] float stunShakeDuration = 1;*/

		[Tooltip("The nickname for this player")]
		public string nickname = null;

		[Tooltip("Bool for if a player has escaped or not.")]
		public bool escaped = false;

        [Tooltip("Indicates if the player is paused")]
        public bool isPaused = false;

		[Tooltip("Is the player being chased?")]
		public bool isChased = false;

        [Header("References")]
		[Tooltip("Layer Mask for interactable objects")]
		[SerializeField] LayerMask interactMask;

		[Tooltip("Position for picked up objects")]
		[SerializeField] Transform heldItemTransform;

		[Tooltip("This player's reticle")]
		[SerializeField] Image reticle;

		[Tooltip("Available reticles")]
		[SerializeField] Sprite[] reticleList;

		/* Public */
		public int Points { get; private set; }
		public bool IsStunned {get; private set; }
		public Interactable HeldObject { get; private set; }

		/* Protected */
		protected Player_Interact playerInView;
		protected float lastPushTime;

		/* Private */
		Interactable _objectInView;
		Player_Movement _playerMovement;
		float _lastThrowTime;
		float _stunTimer;
		bool _ceilingAbove;

		/* Reticle */
		Vector3 defaultReticle = Vector3.zero;
		Color32 defaultColor = new Color32(255, 255, 255, 100);
		Color32 actionColor = new Color32(255, 255, 255, 255);

		Transform cam;
		#endregion

		#region Single Line

		/// <summary> Add Points to this player </summary>
		public virtual void AddPoint() => Points++;

		/// <summary> Reset this player's points to 0 </summary>
		public virtual void ResetPoints() => Points = 0;

		/// <summary> Get the stun duration of this player <summary>
		public virtual float GetStunDuration() => _stunTimer;

		/// <summary> Get the push cooldown of this player <summary>
		public virtual float GetPushCooldown() => lastPushTime;

		/// <summary> Is there a ceiling currently above the player? </summary>
		public virtual bool CeilingDetected() => _ceilingAbove;
		
		/// <summary> Is the player currently holding an item </summary>
		public virtual bool IsHoldingItem() => HeldObject != null;

		/// <summary> Return a reference to the currently held item/// </summary>
        public virtual Interactable GetHeldItem() => HeldObject;

		/// <summary>
		/// Returns reference to object in view <returns></returns>
        public virtual Interactable GetViewItem() => _objectInView;

        /// <summary> Gets nickname for the current local player </summary>
        protected virtual void SetNickname(string myName) => nickname = myName;

		#endregion

		#region Externally Called

		/// <summary> This player has been pushed </summary>
		/// <param name="dir"> The direction to be knocked back </param>
		/// <param name="force"> How far to get knocked back </param>
		public virtual void Pushed(Vector3 dir, float force, bool self)
		{
			if (!self)
			{
				// Knock the player back
				_playerMovement.Knockback(dir, force);

				// Stun the player
				SetStun(stunDuration);

				// Make player drop item
				Caught();
			}
		}

		// <summary> This player has been caught by a guard </summary>
		public virtual void Caught()
		{
			// If holding item, explode it
			if (HeldObject != null)
			{
				HeldObject.Explode();
				HeldObject = null;

				// Set to default reticle
				ResetReticle();
			}
		}

		/// <summary> Set's the reticle to default </summary>
		public void ResetReticle()
		{
			reticle.sprite = reticleList[0];
			reticle.rectTransform.localScale = defaultReticle;
			reticle.color = defaultColor;
		}

		#endregion

		#region Awake Start Update Collision

		protected virtual void Awake()
		{
			_playerMovement = GetComponent<Player_Movement>();
			cam = GetComponentInChildren<Camera>().transform;
		}
		protected virtual void Start()
		{
			GameInputManager.Instance.InteractPressed += AttemptInteract;
			GameInputManager.Instance.PushPressed += AttemptPush;
			GameInputManager.Instance.ThrowPressed += AttemptThrow;
			IsStunned = false;
			reticle.rectTransform.localScale = defaultReticle;
			reticle.color = defaultColor;
			AkSoundEngine.SetState("gameplayStates", "NotStunned");
		}
		protected virtual void Update()
		{
			Stunned();
			if (IsStunned) return;

			Scan();
			DetectCeiling();

			// Update push cooldown
			if (lastPushTime > 0)
				lastPushTime -= Time.deltaTime;
		}
		protected virtual void OnCollisionEnter(Collision other)
		{
			// Get stunned & drop item if hit by interactable object
			other.gameObject.TryGetComponent<Interactable>(out Interactable interactable);
			if (interactable != null)
			{
				string layer = LayerMask.LayerToName(other.gameObject.layer);
				if (layer == "Interactable") return;
				if (layer == "Player's Paint Goal") return;
				if (interactable.WasDropped) return;

				GameObject lastPlayer = interactable.LastPlayer;
				if (lastPlayer == gameObject) return;

				// Stun
				SetStun(interactable.StunDuration);

				// Drop item
				Caught();
			}
		}

		#endregion

		/// <summary> Initially stuns this player </summary>
		public void SetStun(float stunDur)
		{
			if (stunDur < _stunTimer) return;

			// Camera Shake
			//GetComponentInChildren<Player_Camera>().CameraShake(stunShakeDuration);

			// Set the stun
			_stunTimer = stunDur;

			//AkSoundEngine.PostEvent("stunOpponent" , this.gameObject);
			AkSoundEngine.SetState("gameplayStates", "Stunned");
		}

		/// <summary> Uses raycast to check for objects / players </summary>
		void Scan()
		{
			// Raycast for objects / players
			RaycastHit[] hitArr;
			hitArr = Physics.RaycastAll(cam.position, cam.forward, interactRange, interactMask);
			
			if (hitArr.Length > 0)
			{
				foreach (var hit in hitArr)
				{
					int layer = hit.transform.gameObject.layer;

					// Player hit
					if (layer == LayerMask.NameToLayer("Player"))
					{
						Player_Interact hitPlayer = hit.transform.GetComponent<Player_Interact>();

						if (hitPlayer != this)
							playerInView = hitPlayer;
						else
							playerInView = null;
					}

					// Object hit
					else if (layer == LayerMask.NameToLayer("Interactable") ||
							layer == LayerMask.NameToLayer("Player's Paint Goal"))
					{
						_objectInView = hit.transform.GetComponent<Interactable>();

						// The object has correct layer, but no script attached
						if (_objectInView is null)
						{
							Debug.LogError(hit.transform.name + " has interactable layer, but not the script");
							return;
						}

						_objectInView.Hover(transform);

						// Set to grab reticle
						if (_objectInView is Painting)
							reticle.sprite = reticleList[1];
						else
							reticle.sprite = reticleList[2];
						reticle.rectTransform.localScale = Vector3.one;
						reticle.color = actionColor;

						return;
					}
				}
			}
			else
			{
				// Disable the hover text when nobody is looking at it
				if (_objectInView) 
				{
					if (HeldObject == null)
					{
						// Set to default reticle
						ResetReticle();
					}
					else
					{
						// Update reticle
						if (HeldObject is Painting)
							reticle.sprite = reticleList[3];
						else
							reticle.sprite = reticleList[4];
						reticle.rectTransform.localScale = Vector3.one;
						reticle.color = actionColor;
					}

					_objectInView.Hover(null);
				}

				_objectInView = null;
				playerInView = null;
			}
		}

		/// <summary> Detects if an object is above the player </summary>
		void DetectCeiling()
		{
			RaycastHit hit;
			if (Physics.Raycast(cam.position, cam.up, out hit, 1.5f))
			{
				_ceilingAbove = true;
			}
			else
			{
				_ceilingAbove = false;
			}
		}

		/// <summary> Keeps this player stunned for a duration </summary>
		void Stunned()
		{
			// Not stunned
			if (_stunTimer == 0)
			{
				IsStunned = false;
				AkSoundEngine.SetState("gameplayStates", "NotStunned");
			}

			_stunTimer -= Time.deltaTime;

			// Stunned
			if (_stunTimer > 0)
			{
				if (!IsStunned)
                {
					PostManager.instance.ToggleStun(true);
				}

				IsStunned = true;
				return;
			}

			// No longer stunned
			if (IsStunned)
            {
				PostManager.instance.ToggleStun(false);
			}

			_stunTimer = 0;
			IsStunned = false;
		}

		#region Attempt Interactions

		/// <summary> Attempts to push a player </summary>
		protected virtual void AttemptPush()
		{
            // Check if we are paused
            if (isPaused) return;

            if (playerInView is null || lastPushTime > 0)
				return;

			// Check if stunned
			if (IsStunned) return;

			Push(playerInView);

			lastPushTime = pushCooldown;
		}

		public virtual void Push(Player_Interact _playerInView)
		{
			_playerInView.Pushed(cam.forward, pushVelocity, _playerInView == this);
		}

		/// <summary> Attempts to throw an object </summary>
		public virtual void AttemptThrow()
		{
            // Check if we are paused
            if (isPaused) return;

            // Check cooldown
            if (_lastThrowTime > Time.time) return;

			// Check if stunned
			if (IsStunned) return;

			// Holding object
			if (HeldObject is not null)
			{
				_lastThrowTime = Time.time + throwCooldown;
				HeldObject.Throw(_playerMovement.GetMoveDir() * moveImpact);
				HeldObject = null;

				// Update reticle
				ResetReticle();
			}
		}

		/// <summary> Attempts to interact with an object </summary>
		public virtual void AttemptInteract()
		{
            // Check if we are paused
            if (isPaused) return;

            // If no object in view
            if (_objectInView is null) 
				return;

			Interactable newObject = _objectInView.Interact(heldItemTransform);

			// Object is picked up
			if (newObject is not null)
			{
				// Drop Held Item
				if (HeldObject is not null)
					HeldObject.Drop();

				// NewObject is now held
				HeldObject = newObject;
				_objectInView = null;

				// Update reticle
				if (newObject is Painting)
					reticle.sprite = reticleList[3];
				else
					reticle.sprite = reticleList[4];
				reticle.rectTransform.localScale = Vector3.one;
				reticle.color = actionColor;
			}
		}
		#endregion
	}
}
