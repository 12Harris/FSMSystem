using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace Harris.GPC
{

	[AddComponentMenu("CSharpBookCode/Common/Keyboard Input Controller")]

	public abstract class BaseKeyboardInput : BaseInputController
	{

		//Reference to player input actions
		public PlayerInputActions InputActions {get; set;}

		//input actions
		protected InputAction wasdAction;
		protected InputAction spaceBarAction;
		protected InputAction shitfBtnAction;
		protected InputAction lShiftBtnAction;

		private Command wasdCmd;
		public Command WasdCmd => wasdCmd;
        public Vector2 wasdInput => WasdCmd.Action.ReadValue<Vector2>();

		private void ConnectReceiver(string cmdType, ICommandReceiver receiver)
		{
			if(cmdType == "WASDCmd")
				wasdCmd.SetReceiver(receiver);

		}

		private void OnDisable()
		{
			wasdCmd.Action.Disable();
		}


		public virtual void Initialize()
		{
			wasdAction = InputActions.Player.WASD;
			wasdCmd = new WASDCommand(wasdAction);
            wasdAction.Enable();
			ICommandReceiver._onBindReceiver += ConnectReceiver;
		}

		public virtual void Update()
		{
			if(wasdInput != Vector2.zero)
			{
				wasdCmd.Execute();
			}
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(false);
		}

		public void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
			Cursor.visible = true;
		}
	}
}