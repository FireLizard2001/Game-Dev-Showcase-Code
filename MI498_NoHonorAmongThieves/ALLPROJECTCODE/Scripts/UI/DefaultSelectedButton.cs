#region Namespaces
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#endregion


/// <summary>
/// Makes sure a default button is selected at all times. This is needed for controller support
/// </summary>
public class DefaultSelectedButton : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        // If no button is currently selected, make this button selecetd
        if (EventSystem.current.currentSelectedGameObject == null) EventSystem.current.SetSelectedGameObject(this.gameObject);
    }

    public void ClearButtonSelected()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }
}
