#region Namespaces
using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using InteractableObjects;
using Inputs;
#endregion

namespace PlayerFunctionality
{
    /// <summary> Allows player to interact with interactable objects </summary>
    public class Player_Interact_Network : Player_Interact
    {

        #region Attributes

        //Network Changes
        PhotonView PV;

        #endregion

        /// <summary> Add Points to this player </summary>
        public override void AddPoint() => PV.RPC("RPC_AddPoint", RpcTarget.All);

        /// <summary> Reset this player's points to 0 </summary>
        public override void ResetPoints() => PV.RPC("RPC_ResetPoints", RpcTarget.All);

        #region Externally Called Functions

        /// <summary> Gets nickname for the current local player </summary>
        private void SetNickname()
        {
            // Make sure its the right player
            if (!PV.IsMine) { return; }

            PV.RPC("RPC_SetNickname", RpcTarget.All, PlayerPrefs.GetString("NickName"));

        }

        /// <summary> This player has been pushed </summary>
        public override void Pushed(Vector3 dir, float force, bool self)
        {
            // Make sure its the right player
            if (!PV.IsMine) { return; }

            PV.RPC("PlayEvent", RpcTarget.All, "playerPush", this.gameObject.GetPhotonView().ViewID);

            base.Pushed(dir, force, self);
        }

        // <summary> This player has been caught by a guard </summary>
        public override void Caught()
        {
            // Make sure its the right player
            if (!PV.IsMine) { return; }

            base.Caught();
        }

        #endregion

        # region Awake Start Update

        protected override void Awake()
        {
            base.Awake();
            PV = GetComponent<PhotonView>();
        }
        protected override void Start()
        {
            base.Start();

            if (PV.IsMine)
            {
                SetNickname();
                GameObject playerModel = GetComponentInChildren<Animator>().gameObject;
                foreach (Transform child in playerModel.transform)
                    child.gameObject.SetActive(false);
            }
        }

        protected override void Update()
        {
            if (PV.IsMine)
            {
                base.Update();
                PV.RPC("RPC_SetBool", RpcTarget.All, "isStunned", IsStunned, this.gameObject.GetPhotonView().ViewID);
            }
        }

        protected override void OnCollisionEnter(Collision other)
        {
            if (PV.IsMine)
            {
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
        }

        #endregion

        #region Attempt Interactions

        public override void Push(Player_Interact _playerInView)
        {
            PV.RPC("RPC_Push", RpcTarget.All, _playerInView.gameObject.GetPhotonView().ViewID);
        }

        /// <summary> Interaction with no object in view </summary>
        public override void AttemptThrow()
        {
            // Make sure its the right player
            if (!PV.IsMine){return;}

            base.AttemptThrow();
        }

        /// <summary> Interaction with object in view </summary>
        public override void AttemptInteract()
        {
            // Make sure its the right player
            if (!PV.IsMine) { return; }

            PV.RPC("RPC_ResetTrigger", RpcTarget.All, "GrabTrigger", this.gameObject.GetPhotonView().ViewID);
            Interactable prevHeld = HeldObject;
            base.AttemptInteract();
            if (prevHeld != HeldObject)
            {
                PV.RPC("RPC_SetTrigger", RpcTarget.All, "GrabTrigger", this.gameObject.GetPhotonView().ViewID);
            }
        }

		/// <summary> Attempts to push a player </summary>
		protected override void AttemptPush()
		{
            // Check if we are paused
            if (isPaused) return;

            if (playerInView is null || lastPushTime > 0)
				return;

			// Check if stunned
			if (IsStunned) return;
            PV.RPC("RPC_ResetTrigger", RpcTarget.All, "PushTrigger", this.gameObject.GetPhotonView().ViewID);

			Push(playerInView);

			lastPushTime = pushCooldown;

            if (PV.IsMine)
            {
                AkSoundEngine.PostEvent("stunOpponent", gameObject);
            }

            PV.RPC("RPC_SetTrigger", RpcTarget.All, "PushTrigger", this.gameObject.GetPhotonView().ViewID);
		}
        #endregion

        #region Networking

        [PunRPC]
        void RPC_AddPoint()
        {
            base.AddPoint();
            AkSoundEngine.PostEvent("pointScored", this.gameObject);
        }

        [PunRPC]
        void RPC_ResetPoints() => base.ResetPoints();

        [PunRPC]
        void RPC_SetNickname(string myName) => base.SetNickname(myName);

        [PunRPC]
        void RPC_Push(int playerID)
        {
            base.Push(PhotonView.Find(playerID).transform.gameObject.GetComponent<Player_Interact_Network>());
        }

        /// <summary>
        /// Call a Wwise Audio Event
        /// </summary>
        /// <param name="eventName">Name of the event</param>
        [PunRPC]
        void PlayEvent(string eventName, int playerID)
        {
            AkSoundEngine.PostEvent(eventName, PhotonView.Find(playerID).transform.gameObject);
        }
        #endregion
    }
}
