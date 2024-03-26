using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace InteractableObjects
{
    public class Network_Painting : Painting
    {
        PhotonView PV;


        #region Methods

        protected override void Awake()
        {
            base.Awake();
            PV = GetComponent<PhotonView>();
        }

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
        void RPC_Throw(Vector3 moveDir)
        {
            base.Throw(moveDir);
        }
        /// <summary> Throw this object </summary>
        public override void Throw(Vector3 moveDir)
        {
            WasDropped = false;
            PV.RPC("RPC_Throw", RpcTarget.All, moveDir);
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

        [PunRPC]
        void RPC_SetToCollectorsItem()
        {
            base.SetToCollectorsItem();
            AkSoundEngine.PostEvent("paintingHighlight", this.gameObject);
        }
        /// <summary> Set this to be or create a new collectors item</summary>
        public override void SetToCollectorsItem()
        {
            PV.RPC("RPC_SetToCollectorsItem", RpcTarget.All);
        }
        #endregion
    }
}
