using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerMovementKeys : MonoBehaviour
{
    // New input system C# manager
    private AutomationGame _playerInput;
    [SerializeField] private Vector2 _moveInput;


    private void Awake()
    {
        _playerInput = new AutomationGame(); // Create new instance of the script
    }
    private void OnEnable()
    {
        _playerInput.Player.Enable();
    }

    private void OnDisable()
    {
        _playerInput.Player.Disable();
    }


    /// <summary>
    /// Uses the new input instance script to read the user input values
    /// Then, applies it to the vector 2 variable
    /// Calls the interface to reference the appropriate function active
    /// Sets velocity in that specific script
    /// </summary>
    private void FixedUpdate()
    {
        _moveInput = _playerInput.Player.WASD.ReadValue<Vector2>();
        GetComponent<IMoveVelocity>().SetVelocity(_moveInput);

        //Allows for modularity and less coding
    }

}
