using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

namespace Harris.GPC
{
	[AddComponentMenu("CSharpBookCode/Common/Mouse Input Controller")]

	public abstract class BaseMouseInput : BaseInputController
	{
		private Vector2 prevMousePos;
		private Vector2 mouseDelta;

		private float speedX = 0.05f;
		private float speedY = 0.1f;

		//Reference to player input actions
		public PlayerInputActions InputActions {get; set;}

		//input actions
		protected InputAction LeftMouseClickAction;
		protected InputAction RightMouseClickAction;
		protected InputAction MouseLookAction;

		//public Vector2 MousePosition => MouseLookAction.ReadValue<Vector2>();


		private Command leftMouseClickCmd;
		public Command LeftMouseClickCmd => leftMouseClickCmd;

		private Command rightMouseClickCmd;
		public Command RightMouseClickCmd => rightMouseClickCmd;

		private float _mouseDeltaY = 0;
		public float MouseDeltaY =>_mouseDeltaY;

		private void ConnectReceiver(string cmdType, ICommandReceiver receiver)
		{
			Debug.Log("connecting to mouse input");

			if(cmdType == "LeftMouseClickCmd")
			    leftMouseClickCmd.SetReceiver(receiver);

			if(cmdType == "RightMouseClickCmd")
			    rightMouseClickCmd.SetReceiver(receiver);
		}


		public void Start()
		{
			prevMousePos = Input.mousePosition;
		}

		public virtual void Update()
		{
			leftMouseClickCmd.Update(Time.deltaTime);
			rightMouseClickCmd.Update(Time.deltaTime);
		}

		public virtual void Initialize()
		{
			LeftMouseClickAction = InputActions.Player.Fire;
			RightMouseClickAction = InputActions.Player.RFire;

			MouseLookAction = InputActions.Player.Look;

			leftMouseClickCmd = new Command(LeftMouseClickAction, "LeftMouseClickCmd");

			LeftMouseClickAction.Enable();

			rightMouseClickCmd = new Command(RightMouseClickAction, "RightMouseClickCmd");

			RightMouseClickAction.Enable();

			ICommandReceiver._onBindReceiver += ConnectReceiver;

			Debug.Log("init mouse");
		}

		private void OnDisable()
		{
            leftMouseClickCmd.Action.Disable();
		}

		public override void CheckInput()
		{
			// get input data from vertical and horizontal axis and store them internally in vert and horz so we don't
			// have to access them every time we need to relay input data out

			// calculate a percentage amount to use per pixel
			float scalerX = 100f / Screen.width;
			float scalerY = 100f / Screen.height;

			// calculate and use deltas
			_mouseDeltaY = Input.mousePosition.y - prevMousePos.y;
			float mouseDeltaX = Input.mousePosition.x - prevMousePos.x;

			// scale based on screen size
			vert += (_mouseDeltaY  * speedY) * scalerY;
			horz += (mouseDeltaX * speedX) * scalerX;

			// store this mouse position for the next time we're here
			prevMousePos = Input.mousePosition;

			// set up some boolean values for up, down, left and right
			Up = (vert > 0);
			Down = (vert < 0);
			Left = (horz < 0);
			Right = (horz > 0);

			// REMEMBER: To make this work, you need to change some input settings. In Input settings, remove mouse 0 from Fire1 and change mouse 1 in Fire2 to mouse 0

			// get fire / action buttons
			//Fire1 = Input.GetButton("Fire2");
		}

		public void LateUpdate()
		{
			// check inputs each LateUpdate() ready for the next tick
			CheckInput();
		}
	}
}