using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Harris.GPC;

namespace PhotonTest
{

	[AddComponentMenu("CSharpBookCode/Common/Mouse Game Input Controller")]

	public class MouseInput : BaseMouseInput, ICommandReceiver
	{
		private bool _firstUpdate = true;

		private bool _rightMouseWasPressed = false;
		private bool _rightMouseWasReleased = false;
		private bool _rightMouseIsPressed = false;

		public override  void Update()
		{
			base.Update();
			
			if(_firstUpdate)
			{
				((ICommandReceiver)this).BindReceiver("LeftMouseClickCmd");
				((ICommandReceiver)this).BindReceiver("RightMouseClickCmd");
				_firstUpdate = false;
			}
			
		}

		public void ReceiveCommand(Command cmd)
		{
			if(cmd.Name == "RightMouseClickCmd")
			{
				_rightMouseWasPressed = cmd.Triggered;
				_rightMouseWasReleased = cmd.Stopped;
				_rightMouseIsPressed = cmd.Executing;
			}
		}
	}
}