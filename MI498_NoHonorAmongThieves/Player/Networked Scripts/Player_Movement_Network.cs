#region Namespaces
using Photon.Pun;
using UnityEngine;
using System.Collections;
using Inputs;
#endregion

namespace PlayerFunctionality
{
    /// <summary> Controls player movement (WASD / Left Joystick) </summary>
    [RequireComponent(typeof(CharacterController))]
    public class Player_Movement_Network : Player_Movement
    {
        #region Attributes

        /* PhotonView */
        PhotonView PV;                   // Current players photon view

        #endregion
        #region Methods

        public override void Awake() {
            base.Awake();
            PV = GetComponent<PhotonView>();
        }

        public override void DoUpdate()
        {
            if (PV.IsMine)
            {
                base.DoUpdate();
                PV.RPC("RPC_SetBool", RpcTarget.All, "isCrouching", IsCrouching(), this.gameObject.GetPhotonView().ViewID);
            }
        }

        public override void AttemptCollision(Collision coll)
        {
            if (PV.IsMine)
            {
                base.AttemptCollision(coll);
            }
        }

        /// <summary> Main functionality for player movement </summary>
        public override void UpdateAnimations(Vector3 moveDirection)
        {
            // Null Check
            if (anim == null) { return; }

            // Update animations
            if (moveDirection.magnitude > 5)
            {
                PV.RPC("RPC_SetTrigger", RpcTarget.All, "MoveTrigger", this.gameObject.GetPhotonView().ViewID);
                PV.RPC("RPC_ResetTrigger", RpcTarget.All, "IdleTrigger", this.gameObject.GetPhotonView().ViewID);
                //_anim.SetTrigger("MoveTrigger");
                //_anim.ResetTrigger("IdleTrigger");
                _idleTimer = 0;
            }
            else
            {
                PV.RPC("RPC_SetTrigger", RpcTarget.All, "IdleTrigger", this.gameObject.GetPhotonView().ViewID);
                PV.RPC("RPC_ResetTrigger", RpcTarget.All, "MoveTrigger", this.gameObject.GetPhotonView().ViewID);
                //_anim.SetTrigger("IdleTrigger");
                //_anim.ResetTrigger("MoveTrigger");

                if (!IsCrouching())
                {
                    _idleTimer += Time.deltaTime;
                    if (_idleTimer >= idleHandsDelay)
                    {
                        PV.RPC("RPC_CrossFade", RpcTarget.All, "Armature_001|Idle2Mischief", 0.1f, this.gameObject.GetPhotonView().ViewID);
                        //_anim.CrossFade("Armature_001|Idle2Mischief", 0.1f);
                        _idleTimer = 0;
                    }
                }
            }
            PV.RPC("RPC_SetBool", RpcTarget.All, "isSprinting", IsSprinting(), this.gameObject.GetPhotonView().ViewID);
            //_anim.SetBool("isSprinting", IsSprinting());
            PV.RPC("RPC_SetBool", RpcTarget.All, "isJumping", IsJumping(), this.gameObject.GetPhotonView().ViewID);
            //_anim.SetBool("isJumping", IsJumping());
        }
        #endregion

        [PunRPC]
		public void RPC_SetTrigger(string trigger, int playerID)
        {
            if (PhotonView.Find(playerID).transform.gameObject.GetComponent<Player_Movement_Network>().anim == null) { return; }
			PhotonView.Find(playerID).transform.gameObject.GetComponent<Player_Movement_Network>().anim.SetTrigger(trigger);

		}

		[PunRPC]
		public void RPC_ResetTrigger(string trigger, int playerID)
		{
            if (PhotonView.Find(playerID).transform.gameObject.GetComponent<Player_Movement_Network>().anim == null) { return; }
            PhotonView.Find(playerID).transform.gameObject.GetComponent<Player_Movement_Network>().anim.ResetTrigger(trigger);
		}

		[PunRPC]
		void RPC_CrossFade(string fade, float len, int playerID)
		{
            if (PhotonView.Find(playerID).transform.gameObject.GetComponent<Player_Movement_Network>().anim == null) { return; }
            PhotonView.Find(playerID).transform.gameObject.GetComponent<Player_Movement_Network>().anim.CrossFade(fade, len);
		}

        [PunRPC]
        public void RPC_SetBool(string name, bool val, int playerID)
        {
            if (PhotonView.Find(playerID).transform.gameObject.GetComponent<Player_Movement_Network>().anim == null) { return; }
            PhotonView.Find(playerID).transform.gameObject.GetComponent<Player_Movement_Network>().anim.SetBool(name, val);
        }
    }
}
