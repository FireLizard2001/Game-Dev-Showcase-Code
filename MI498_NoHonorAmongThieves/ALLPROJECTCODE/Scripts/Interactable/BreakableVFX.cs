#region Namespaces
using UnityEngine;
#endregion

namespace InteractableObjects{
/// <summary> Destroys this VFX after an interval </summary>
public class BreakableVFX : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] [Range(0, 5)] float destroyInterval = 1;

    void Start() => Destroy(gameObject, destroyInterval);
}
}
