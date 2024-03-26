#region Namespaces
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;
using Photon.Pun;
using PlayerFunctionality;
#endregion

namespace InteractableObjects {
    /// <summary> Base class for interactable objects </summary>
    public abstract class Interactable : MonoBehaviour
    {
        #region Attributes

        static Transform InteractablesParent;

        static GameManager GM;

        /* Serialized Fields */

        [Header("Interactable Settings")]
        [Tooltip("The name of the object")]
        [SerializeField] protected string Name;

        [Tooltip("How long is the  player stunned when hit by this object?")]
        [Range(0, 5)] public float StunDuration = 1;

        [Tooltip("The thrown velocity of the object")]
        [SerializeField] [Range(1, 100)] protected float throwVelocity = 1;

        [Tooltip("The explosion velocity of the object when caught by a guard")]
        [SerializeField] [Range(1, 100)] protected float explodeVelocity = 15;

        [Tooltip("Time until interactable again after throwing")]
        [SerializeField] [Range(0, 5)] protected float interactDelay = 1;

        [Tooltip("Duration to wait between collisions")]
        [SerializeField] [Range(0, 5)] protected float collDelay = 0.5f;

        [Header("Held Item Settings")]
        [Tooltip("The position offset of held item")]
        [SerializeField] protected Vector3 heldPos;

        [Tooltip("The rotation of held item")]
        [SerializeField] protected Vector3 heldRot;

        [Tooltip("The scale of held item")]
        [SerializeField] protected Vector3 heldScale;

        [Header("Interactable References")]
        [Tooltip("Creates noise for guards to hear")]
        [SerializeField] protected GameObject noisePrefab;

        [Tooltip("Instantiated VFX when this object hits anything")]
        [SerializeField] protected GameObject hitVFX;

        [Tooltip("Shimmer gameObject when this interactable needs to be highlighted")]
        [SerializeField] protected GameObject shimmerVFX;

        /* Public */
        public GameObject LastPlayer {get; private set;}
        public bool WasDropped {get; protected set;}

        /* Private */
        Vector3 _initScale;
        float _collCooldown;
        float _interactTimer;

        /* Collector Relevant */
        public int Value { get; private set; } = 0;
        protected bool onWall = true;
        protected Vector3 _initialPosition;
        protected Quaternion _initialRotation;

        /* Protected */
        protected bool isCarried = false;
        protected Transform heldItemTransform;

        /* References */
        protected Rigidbody rb;
        protected BoxCollider boxCollider;
        private int lastID = -1;
        //protected TextMeshPro hoverText;
        //protected TextMeshPro interactText;


        #endregion
        #region Methods

        /// <summary> Get this object's player trans </summary>
        public GameObject GetPlayer() => heldItemTransform.parent.parent.gameObject;

        /// <summary> Interact with this object </summary>
        public virtual Interactable Interact(Transform heldTrans)
        {
            Pickup(heldTrans);
            return this;
        }

        public virtual void DestroySelf() => gameObject.SetActive(false);

        public virtual void Respawn()
        {
            rb.velocity = Vector3.zero;
            gameObject.transform.position = _initialPosition;
            gameObject.transform.rotation = _initialRotation;

            rb.useGravity = false;
            rb.freezeRotation = true;
            rb.velocity = Vector3.zero;

            boxCollider.enabled = true;
        }

        /// <summary> Drop this object </summary>
        public virtual void Drop()
        {
            if (this is Network_Breakable || this is Network_Painting)
            {
                GetComponent<PhotonTransformView>().enabled = true;
                Vector3 worldPos = transform.position;
                GetComponent<PhotonTransformView>().m_UseLocal = false;
                GetComponent<PhotonTransformView>().m_SynchronizePosition = true;
                GetComponent<PhotonTransformView>().m_SynchronizeRotation = true;
                transform.position = worldPos;
            }

            lastID = heldItemTransform.parent.parent.gameObject.GetComponent<PhotonView>().ViewID;

            // Set this players van to not glow
            if (Value == 1)
            {
                if (PlayerPrefs.GetInt("MyCharacter") == heldItemTransform.parent.parent.gameObject.GetComponent<AvatarSetup>().characterValue)
                {
                    GM.MakeVanGlow(PlayerPrefs.GetInt("MyCharacter"), false);
                    GM.UpdatePaintingHolderUI(heldItemTransform.parent.parent.gameObject.GetComponent<AvatarSetup>().characterValue,
                    heldItemTransform.parent.parent.gameObject.GetComponent<Player_Interact_Network>().nickname, false);

                    // chevron handler
                    GetComponent<Target>().enabled = true;
                }
            }

            // Adjust parent, position, rotation
            if (InteractablesParent == null)
            {
                InteractablesParent = new GameObject("Interactables").transform;
            }
            LastPlayer = GetPlayer();
            transform.parent = InteractablesParent;

            Camera cam = GetPlayer().GetComponentInChildren<Camera>();
            transform.position = cam.transform.position + (cam.transform.forward * 1.5f);
            //transform.position += Vector3.up + heldItemTransform.forward;
            
            transform.rotation = Quaternion.Euler(30, heldItemTransform.rotation.eulerAngles.y, 0);
            transform.localScale = _initScale;

            // Enable gravity, rotation
            rb.useGravity = true;
            rb.freezeRotation = false;

            // Enable box collider
            boxCollider.enabled = true;

            // Enable carry text
            isCarried = false;

            if (gameObject.activeSelf)
            {
                _interactTimer = interactDelay;
            }
        }

        /// <summary> Throw this object </summary>
        public virtual void Throw(Vector3 moveDir)
        {
            Drop();

            WasDropped = false;

            AkSoundEngine.PostEvent("itemThrow", this.gameObject);

            // Add velocity
            Camera cam = GetPlayer().GetComponentInChildren<Camera>();

            rb.AddForce((cam.transform.forward * throwVelocity) + moveDir, ForceMode.Impulse);
        }
        
        /// <summary> Explode this object </summary>
        public virtual void Explode()
        {
            Drop();

            // Add velocity in random direction
            var direction = Quaternion.Euler(Random.Range(-180, 180), 0, Random.Range(-180, 180)) * heldItemTransform.up;
            rb.velocity = direction * explodeVelocity;
        }

        /// <summary> Called each frame this object is being looked at </summary>
        public virtual void Hover(Transform player)
        {
            // This object is no longer being looked at
            /*if (player is null || isCarried)
            {
                hoverText.enabled = false;
                interactText.enabled = false;
                return;
            }

            // Object is being looked at, enable text
            hoverText.enabled = true;
            interactText.enabled = true;*/
        }

        /// <summary> Get references to text </summary>
        protected virtual void Awake()
        {
            
            if (GM == null)
            {
                GM = GameObject.Find("GameManager")?.GetComponent<GameManager>();
            }

            // Set Starting position
            _initialPosition = this.transform.position;
            _initialRotation = this.transform.rotation;

            // Get references
            rb = GetComponent<Rigidbody>();
            boxCollider = GetComponent<BoxCollider>();
            TextMeshPro[] textRefs = GetComponentsInChildren<TextMeshPro>();
            //hoverText = textRefs[0];
            //interactText = textRefs[1];

            // Disable text by default
            //hoverText.enabled = false;
            //interactText.enabled = false;

            //Track Global Pos and Rot
            if (this is Network_Breakable || this is Network_Painting)
            {
                GetComponent<PhotonTransformView>().m_UseLocal = false;
                GetComponent<PhotonTransformView>().m_SynchronizePosition = true;
                GetComponent<PhotonTransformView>().m_SynchronizeRotation = true;
            }
        }

        /// <summary> Set the hover and interact text accordingly </summary>
        protected virtual void Start()
        {
            // Set hover text to this object's name + value
            /*hoverText.text = Name;

            // Set interact text
            string tempText = "";

            // If using Mouse + Keyboard
            if (Gamepad.all.Count == 0)
                tempText = "Press <color=green>'E'</color> to interact";

            else
            {
                // Xbox Controller
                if (Gamepad.current is UnityEngine.InputSystem.XInput.XInputController)
                    tempText = "Press <color=green>'X'</color> to interact";

                // Playstation Controller
                else if (Gamepad.current is UnityEngine.InputSystem.DualShock.DualShockGamepad)
                    tempText = "Press <color=green>'[]'</color> to interact";

                // Unknown?
                else
                    tempText = "Press <color=green>'Interact'</color> to interact";
            }

            interactText.text = tempText;*/

            _initScale = transform.localScale;
        }

        void Update() => UpdateInteractionState();
        void UpdateInteractionState()
        {
            // Determine whether CanInteract should be true
            if (_interactTimer != 0)
            {
                _interactTimer -= Time.deltaTime;

                if (_interactTimer <= 0)
                {
                    _interactTimer = 0;

                    if (Value == 0)
                        gameObject.layer = LayerMask.NameToLayer("Interactable");
                    else
                        gameObject.layer = LayerMask.NameToLayer("Player's Paint Goal");

                    LastPlayer = null;
                }
            }
        }

        /// <summary> Determines if this object hit a player </summary>
        protected virtual void OnCollisionEnter(Collision other)
        {
            if (onWall) return;

            // Stun guard if hit
            if (other.gameObject.CompareTag("Guard") && !WasDropped)
            {
                other.gameObject.GetComponent<Pathing_Network>().TriggerStun();

                if (lastID != -1)
                {
                    if (PhotonView.Find(lastID).transform.gameObject.GetComponent<AvatarSetup>().characterValue == PlayerPrefs.GetInt("MyCharacter"))
                    {
                        //AkSoundEngine.PostEvent("stunOpponent", PhotonView.Find(lastID).transform.gameObject);
                    }
                }
            }

            if (_collCooldown > Time.time) return;
            _collCooldown = Time.time + collDelay;

            // Instantiate sound prefab
            if (noisePrefab != null)
            {
                Instantiate(noisePrefab, transform.position, Quaternion.identity);
            }

            // Spawn hit VFX
            if (hitVFX != null && !other.gameObject.CompareTag("Player") && !other.gameObject.CompareTag("Guard"))
            {
                GameObject newHitVFX = Instantiate(hitVFX, transform.position, Quaternion.identity);

                if (this.GetComponent<Painting>() != null)
                {
                    AkSoundEngine.PostEvent("paintingCrash", this.gameObject);
                }

                // Rotation
                newHitVFX.transform.rotation = other.transform.rotation;
                newHitVFX.transform.Rotate(90, 0, 90);

                // Position
                Vector3 contactPoint = other.GetContact(0).point;
                newHitVFX.transform.position = contactPoint;

                // Adjust Position
                Collider otherCollider = other.collider;
                Vector3 otherPosition = otherCollider.transform.position;
                Vector3 otherSize = otherCollider.bounds.size;
                Vector3 modifierPos = Vector3.zero;

                // Top / Bottom
                if (Mathf.Abs(otherPosition.y - contactPoint.y) > otherSize.y / 2)
                {
                    // Top
                    if (contactPoint.y > otherPosition.y)
                        modifierPos = Vector3.up;

                    // Bottom
                    else
                        modifierPos = Vector3.down;
                }

                // Left / Right
                else if (Mathf.Abs(otherPosition.x - contactPoint.x) > otherSize.x / 2)
                {
                    // Right
                    if (contactPoint.x > otherPosition.x)
                        modifierPos = Vector3.right;

                    else
                        modifierPos = Vector3.left;
                }

                // Front / Back
                else if (Mathf.Abs(otherPosition.z - contactPoint.z) > otherSize.z / 2)
                {
                    // Front
                    if (contactPoint.z > otherPosition.z)
                        modifierPos = Vector3.forward;

                    // Back
                    else
                        modifierPos = Vector3.back;
                }

                newHitVFX.transform.position += modifierPos * 0.1f;
            }
            else
            {
                if (GetPlayer().GetComponent<Player_Interact>() != other.gameObject.GetComponent<Player_Interact>())
                {
                    // Hit SFX here
                    AkSoundEngine.PostEvent("hit" , this.gameObject);

                    if (lastID != -1)
                    {
                        if (PhotonView.Find(lastID).transform.gameObject.GetComponent<AvatarSetup>().characterValue == PlayerPrefs.GetInt("MyCharacter"))
                        {
                            //AkSoundEngine.PostEvent("stunOpponent", PhotonView.Find(lastID).transform.gameObject);
                        }
                    }
                }
            }
        }

        /// <summary> Pickup and hold an object </summary>
        protected virtual void Pickup(Transform heldTrans)
        {
            if (this is Network_Breakable || this is Network_Painting)
            {
                GetComponent<PhotonTransformView>().enabled = false;
                //GetComponent<PhotonTransformView>().m_UseLocal = true;
                //GetComponent<PhotonTransformView>().m_SynchronizePosition = false;
                //GetComponent<PhotonTransformView>().m_SynchronizeRotation = false;
            }

            if (onWall)
            {
                onWall = false;
            }

            heldItemTransform = heldTrans;

            if (PlayerPrefs.GetInt("MyCharacter") == heldItemTransform.parent.parent.gameObject.GetComponent<AvatarSetup>().characterValue)
            {
                AkSoundEngine.PostEvent("itemPickup", this.gameObject);
            }

            // Set this players van to glow
            if (Value == 1)
            {
                if(PlayerPrefs.GetInt("MyCharacter") == heldItemTransform.parent.parent.gameObject.GetComponent<AvatarSetup>().characterValue)
                {
                    GM.MakeVanGlow(PlayerPrefs.GetInt("MyCharacter"), true);
                    GM.UpdatePaintingHolderUI(heldItemTransform.parent.parent.gameObject.GetComponent<AvatarSetup>().characterValue,
                    heldItemTransform.parent.parent.gameObject.GetComponent<Player_Interact_Network>().nickname, true);

                    // chevron handler
                    GetComponent<Target>().enabled = false;
                }
            }

            // Set parent to player
            transform.parent = heldItemTransform;


            // Disable gravity, rotation
            rb.useGravity = false;
            rb.freezeRotation = true;
            rb.velocity = Vector3.zero;

            // Disable box collider
            boxCollider.enabled = false;

            // Disable carry text
            isCarried = true;

            // Set object's position
            transform.position = transform.parent.position;
            transform.localPosition += heldPos;
            //transform.position += heldPos;

            // Set object's rotation
            transform.localRotation = Quaternion.Euler(heldRot.x, heldRot.y, heldRot.z);
            //transform.rotation = Quaternion.Euler(heldRot.x, heldRot.y, heldRot.z);

            // Set object's scale
            transform.localScale = heldScale;

            // Change layer
            //_initLayer = LayerMask.LayerToName(gameObject.layer);
            if (Value == 0)
                gameObject.layer = LayerMask.NameToLayer("Default");
            else
                gameObject.layer = LayerMask.NameToLayer("Held Non-Interactable");
        }

        /// <summary>
        /// Either set this to be or create a new collectors item.
        /// </summary>
        public virtual void SetToCollectorsItem()
        {
            Interactable toSet = this;

            if (toSet.isCarried)
            {
                Debug.LogWarning("Attempted to set a carried painting to collectors item, selecting a different one...");
                GM.SelectCollectorPainting();
            }
            else if (toSet.Value == 1)
            {
                Debug.LogWarning("Attempted to set a deposited painting to collectors item, selecting a different one...");
                GM.SelectCollectorPainting();
            }
            else
            {
                Respawn();

                if (shimmerVFX != null)
                {
                    shimmerVFX.SetActive(true);
                }

                toSet.gameObject.SetActive(true);

                toSet.Value = 1;

                gameObject.layer = LayerMask.NameToLayer("Player's Paint Goal");

                foreach (Transform child in toSet.transform.GetComponentsInChildren<Transform>())
                {
                    child.gameObject.layer = LayerMask.NameToLayer("Player's Paint Goal"); ;
                }
                toSet.onWall = true;

                DialogueManager.instance.PlayLocalDialogue("collector_PaintingMarked");

                // Set Chevron on this object
                this.GetComponent<Target>().enabled = true;

                Debug.Log("Set new Player Goal " + this.gameObject.name);
            }
        }
        #endregion
}}
