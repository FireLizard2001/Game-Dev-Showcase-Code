using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace InteractableObjects
{
    public class Network_Breakable : Breakable
    {
        PhotonView PV;

        protected override void Awake()
        {
            base.Awake();
            PV = GetComponent<PhotonView>();
        }

        #region Methods

        [PunRPC]
        void RPC_DestroySelf() => base.DestroySelf();

        public override void DestroySelf() => PV.RPC("RPC_DestroySelf", RpcTarget.All);

        [PunRPC]
        void RPC_Respawn()
        {
            base.Respawn();
        }
        /// <summary> Drop this object </summary>
        public override void Respawn()
        {
            PV.RPC("RPC_Respawn", RpcTarget.All);
        }

        [PunRPC]
        void RPC_Pickup(int heldTransGMid)
        {
            base.Pickup(PhotonView.Find(heldTransGMid).transform.Find("Main Camera").transform.Find("Held Item").transform);
        }

        /// <summary> Pickup and hold an object </summary>
        protected override void Pickup(Transform heldTrans)
        {
            PV.RPC("RPC_Pickup", RpcTarget.All, heldTrans.parent.parent.gameObject.GetPhotonView().ViewID);
        }

        [PunRPC]
        void RPC_Drop()
        {
            base.Drop();
        }
        /// <summary> Drop this object </summary>
        public override void Drop()
        {
            PV.RPC("RPC_Drop", RpcTarget.All);
        }


        [PunRPC]
        void RPC_Throw(Vector3 moveDir, Vector3 camForward)
        {
            base.Drop();

            AkSoundEngine.PostEvent("itemThrow", this.gameObject);

            rb.velocity = (camForward * throwVelocity) + moveDir;
        }

        /// <summary> Throw this object </summary>
        public override void Throw(Vector3 moveDir)
        {
            WasDropped = false;
            Vector3 camForward = GetMyPlayer().GetComponentInChildren<Camera>().transform.forward;
            PV.RPC("RPC_Throw", RpcTarget.All, moveDir, camForward);
        }

        [PunRPC]
        void RPC_Explode()
        {
            base.Explode();
        }
        /// <summary> Explode this object </summary>
        public override void Explode()
        {
            WasDropped = true;
            PV.RPC("RPC_Explode", RpcTarget.All);
        }

        /// <summary> Gets the current player </summary>
        private GameObject GetMyPlayer()
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                if (player.GetComponent<PhotonView>().IsMine)
                {
                    return player;
                }
            }
            return null;
        }

        #endregion
    }
}
