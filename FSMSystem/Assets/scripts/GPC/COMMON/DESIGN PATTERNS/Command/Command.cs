using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Windows.Input;

namespace Harris.GPC
{

    public interface ICommandReceiver
    {
        public void ReceiveCommand(Command command);  

        //event called by keyboard input system to bind receivers to specific input commands 
        public static event Action<string, ICommandReceiver> _onBindReceiver;

        public void BindReceiver(string cmdType)
        {
            Debug.Log("binding  receiver");
            _onBindReceiver?.Invoke(cmdType, this);
        }

    }


    public class Command
    {
        public InputAction Action{get;set;}

        public string Name{get;set;}

        enum ButtonState
        {
            None, Pressed, PressedThisFrame, Released
        }
        private ButtonState buttonState{get;set;}

        public bool Triggered => buttonState == ButtonState.PressedThisFrame;

        public bool Executing => buttonState == ButtonState.Pressed;

        public bool Stopped => buttonState == ButtonState.Released;


		//private ICommandReceiver _receiver;
        //public ICommandReceiver Receiver => _receiver;

        private List<ICommandReceiver> _receivers = new List<ICommandReceiver>();
        public List<ICommandReceiver> Receivers => _receivers;

        public void SetReceiver(ICommandReceiver receiver)
		{
			//_receiver = receiver;
            _receivers.Add(receiver);
		}

        public Command(InputAction action, string name)
        {
            Action = action;
            action.performed += CommandPerformed;
            action.canceled += CommandCanceled;
            buttonState = ButtonState.None;
            Name = name;
        }

        public virtual void Execute()
        {
            if(_receivers != null)
                foreach(var receiver in _receivers)
                    receiver.ReceiveCommand(this);
        }

        public virtual void Update(float dt)
        {
            if(buttonState == ButtonState.Pressed)
            {
                Execute();
            }
        }

        public virtual void CommandPerformed(InputAction.CallbackContext obj)
        {
            buttonState = ButtonState.PressedThisFrame;

            if(_receivers != null)
                //_receiver.ReceiveCommand(this);
                foreach(var receiver in _receivers)
                    receiver.ReceiveCommand(this);
            
            buttonState = ButtonState.Pressed;
        }

        public virtual void CommandCanceled(InputAction.CallbackContext obj)
        {
            buttonState = ButtonState.Released;

            if(_receivers != null)
                //_receiver.ReceiveCommand(this);
                 foreach(var receiver in _receivers)
                    receiver.ReceiveCommand(this);

            buttonState = ButtonState.None;
        }
    }

    public class WASDCommand : Command
    {
        public WASDCommand(InputAction action) : base(action, "") {}

        public override void Execute()
        {
            base.Execute();
            Debug.Log("Move Command executed!");
            if(Receivers != null)
                //_receiver.ReceiveCommand(this);
                 foreach(var receiver in Receivers)
                    receiver.ReceiveCommand(this);
        }
    }
}


