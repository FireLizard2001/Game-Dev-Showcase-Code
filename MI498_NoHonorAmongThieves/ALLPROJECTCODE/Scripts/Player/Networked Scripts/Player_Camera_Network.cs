#region Namespaces
using Photon.Pun;
using UnityEngine;
using Inputs;
#endregion

namespace PlayerFunctionality
{
	/// <summary> Controls player camera (Mouse / Right Joystick) </summary>
	public class Player_Camera_Network : Player_Camera
	{
		#region Attributes

		PhotonView PV;                   // Current players photon view

		#endregion
		#region Methods

		protected override void Awake()
		{
			Cursor.lockState = CursorLockMode.Locked;
			_playerBody = transform.parent;
			_controller = GetComponentInParent<CharacterController>();
			//DefaultPos = transform.localPosition;
			Camera.main.fieldOfView = fov;
			_playerInteract = GetComponentInParent<Player_Interact>();
			_playerMovement = GetComponentInParent<Player_Movement>();
			_heldItemTrans = transform.GetChild(0);
			PV = GetComponentInParent<PhotonView>();
			if (!PV.IsMine) { this.gameObject.GetComponent<Camera>().enabled = false; }
		}
		protected override void Update()
		{
			if (!isPaused)
			{
				Shake();

				if (PV.IsMine)
				{
					Look();
				}
				Bobble();
			}
			if (PV.IsMine)
			{
				if (PlayerPrefs.GetFloat("Sensitivity") != sensitivity && PlayerPrefs.GetFloat("Sensitivity") != 0)
				{
					sensitivity = PlayerPrefs.GetFloat("Sensitivity");
				}
				else if (PlayerPrefs.GetFloat("Sensitivity") == 0)
                    sensitivity = 1.0f;
            }
		}
		#endregion
	}
}
