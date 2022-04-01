using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPerson : MonoBehaviour
{

    [Tooltip("Follow target for the Cinemachine Virtual Camera (attached to the player's shoulders)")]
    public GameObject followTarget;
    [Tooltip("Power of the camera rotation in x and y direction for the mouse.")]
    public Vector2 rotationPowerMouse = Vector2.one;
    [Tooltip("Power of the camera rotation in x and y direction for the controller.")]
    public Vector2 rotationPowerController = Vector2.one;
    [Tooltip("Upper and lower bound of camera rotation.")]
    public Vector2 rotationBounds = Vector2.one;

    private Vector2 lookInput = Vector2.zero;
    private bool usingController = false;

    public void SetLookInput(Vector2 newInputValue)
    {
        lookInput = newInputValue;
    }

    // Update the camera based on mouse movement
    void Update()
    {
        // Update roation of the Follow Target based on look input
        Vector2 rotationPower = usingController ? rotationPowerController : rotationPowerMouse;
        followTarget.transform.rotation *= Quaternion.AngleAxis(lookInput.x * rotationPower.x, Vector3.up);
        followTarget.transform.rotation *= Quaternion.AngleAxis(-lookInput.y * rotationPower.y, Vector3.right);

        // Obtain angles to manipulate
        Vector3 angles = followTarget.transform.localEulerAngles;
        angles.z = 0;

        // Clamp the rotation about the x axis
        float xRot = followTarget.transform.transform.localEulerAngles.x;
        if (xRot > 180 && xRot < rotationBounds.y)
        {
            angles.x = rotationBounds.y;
        }
        else if (xRot < 180 && xRot > rotationBounds.x)
        {
            angles.x = rotationBounds.x;
        }

        // Set the final rotation of the Follow Target
        followTarget.transform.localEulerAngles = new Vector3(angles.x, angles.y, 0);

    }

}
