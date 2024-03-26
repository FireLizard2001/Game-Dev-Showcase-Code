#region Namespaces
using UnityEngine;
#endregion

namespace InteractableObjects {
/// <summary> Implements functionality for breakable objects </summary>
public class Breakable : Interactable
{
    #region Attributes

        /* Serialized Fields */
        [Header("Breakable Settings")]
            [Tooltip("If velocity is above this limit on collision, break")]
            [SerializeField] [Range(0, 100)] float breakLimit = 0;

        [Header("Breakable References")]
            [Tooltip("Instantiated when this object breaks")]
            [SerializeField] GameObject breakVFX;

        #endregion

        #region Methods

        public override void DestroySelf()
        {
            base.Respawn();
        }

        /// <summary> Determines if this object will be destroyed </summary>
        protected override void OnCollisionEnter(Collision other) 
        {
            base.OnCollisionEnter(other);

            AkSoundEngine.PostEvent("sculptureShatter", this.gameObject);

            // Spawn VFX
            if (breakVFX is not null)
                Instantiate(breakVFX, transform.position, Quaternion.identity);

            // Break SFX here?

            DestroySelf();
        }

    #endregion
}
}
