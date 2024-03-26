#region Namespaces
using UnityEngine;
#endregion

namespace InteractableObjects {
    /// <summary> Implements functionality for paintings </summary>
    public class Painting : Interactable
    {
        protected override void Awake()
        {
            base.Awake();
            rb.isKinematic = true;
        }

        protected override void Pickup(Transform heldTrans)
        {
            if (onWall) rb.isKinematic = false;
            base.Pickup(heldTrans);
        }
    }
}
