// project armada
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Harris.GPC
{

	[Serializable]
	/// <summary>
	/// State machine manager
	/// </summary>
	public class FSM
	{
		/// <summary>
		/// Transition was taken from one state to another
		/// </summary>
		public event Action<int,int> onTransition = null;

		public event Action<FSM> _onFinished;

		public FSM()
		{
			
		}

		/// <summary>
		/// Explicitly transition to given state (at index)
		/// </summary>
		/// <param name="newStateIndex">State index</param>
		public void SetState(in int newStateIndex)
		{
			if(newStateIndex < 0 || newStateIndex >= _states.Count) { return; }

			int s1 = _currentState;
			int s2 = newStateIndex;

			// exit current state
			BoundState oldState = GetStateAtIndex(_currentState);

			if(_currentState != newStateIndex)
				oldState?.state.Exit();
			m_oldState = oldState?.state;

			// enter new
			_currentState = newStateIndex;
			BoundState newState = GetStateAtIndex(_currentState);
			newState?.state.Enter();

			m_currentState = newState?.state;

			// alert observers of transition(npc transitioned to new state)
			onTransition?.Invoke(s1, s2);
		}

		/// <summary>
		/// Define transition from state A to state B
		/// <param name="stateA">Index of state to transition from</param>
		/// <param name="stateB">Index of state to transition to</param>
		/// <param name="exitGuard">Exit condition eval</param>
		/// <param name="priority">Dynamic weight (not implemented)</param>///
		///  </summary>
		public void AddTransition
		(
			in int stateA,
			in int stateB,
			Func<bool> exitGuard,
			Func<float> priority = null,
			Func<float> weight = null
		)
		{
			// cannot transition to self
			if(stateA == stateB) { return; }

			BoundState s1 = GetStateAtIndex(stateA);
			BoundState s2 = GetStateAtIndex(stateB);

			// null checks
			if(s1 == null || (s2 == null && stateB != -2)|| exitGuard == null) { return; }

			// ensure default priority
			if(priority == null) { priority = GetDefaultPriority; }

			if(weight == null) { weight= GetDefaultWeight; }

			// register transition
			s1.transitions.Add(new Transition
			{
				state = stateB,
				guard = exitGuard,
				priority = priority,
				weight = weight
			});
		}

		/// <summary>
		/// Update execution
		/// </summary>
		public void Tick(in float deltaTime)
		{
			// no active state
			if(_currentState < 0) { Debug.Log("CURRENT STATE IS NULL!"); return; }

			BoundState ctx = _states[_currentState];
			ctx.state.Tick(deltaTime);

			int exitState = -1;

			bool stop = false;

			//does this state have transition weights? if so get random exit from this state
			for (int i = 0; i < ctx.transitions.Count && stop==false; i++)
			{
				Transition t = ctx.transitions[i];
				if (t.weight.Invoke() > 0f)
				{
					Debug.Log("weight > 0");
					exitState = GetRandomExit(_currentState);
					stop = true;
				}
			}

			if(exitState == -1 && stop == false)
			{
				Debug.Log("exit state = -1");
				// check if state has available exits
				exitState = GetAvailableExit(_currentState);
			}

			if (exitState > -1) { SetState(exitState); }
			
			
			//Abort this FSM completely
			if (exitState == -2)
			{
				// exit current state
				BoundState oldState = GetStateAtIndex(_currentState);
				oldState?.state.Exit();
				m_oldState = oldState?.state;

				_onFinished?.Invoke(this);
				_currentState = 0;//IMportant, upon entering this fsm again we want to start at the first state
				return;

			}
		}

		/// <summary>
		/// Late Update execution
		/// </summary>
		public void LateTick(in float deltaTime)
		{
			// no active state
			if(_currentState < 0) { Debug.Log("CURRENT STATE IS NULL!"); return; }

			BoundState ctx = _states[_currentState];
			ctx.state.LateTick(deltaTime);
		}

		/// <summary>
		/// Creates and adds state of given type, returns index of state
		/// </summary>
		/// <param name="s">State instance</param>
		/// <returns>State index</returns>
		public int AddState(FSM_State s)
		{
			_states.Add(new BoundState
			{
				state = s
			});
			
			// use first state as default start
			if(_currentState < 0) { 
				_currentState = 0;
				BoundState curState = GetStateAtIndex(_currentState);
				m_currentState = curState?.state;
			}

			// 
			return _states.Count - 1; // index -> "ID"
		}

		// index of currently executing state
		private int _currentState = -1;
		// states registered with FSM
		private List<BoundState> _states = new List<BoundState>();

		public List<BoundState> States => _states;

		// fallback getter for transition priority
		private static float GetDefaultPriority() => 0f;

		private static float GetDefaultWeight() => 0f;

		private FSM_State m_currentState, m_oldState;

		public FSM_State CurrentState {get => m_currentState; set => m_currentState = value;}
		public FSM_State OldState => m_oldState;

		/// <summary>
		/// Finds first possible transition out of given state
		/// </summary>
		/// <param name="stateIndex"></param>
		/// <returns>Exit state</returns>
		private int GetAvailableExit(in int stateIndex)
		{
			BoundState ctx = GetStateAtIndex(stateIndex);
			if(ctx == null) { return -1; }

			// 
			float bestPriority = float.MaxValue;

			// index in transition list for state
			int transitionIndex = -1;

			for(int i = 0; i < ctx.transitions.Count; i++)
			{
				Transition t = ctx.transitions[i];				//3 transitions

				// eval condition
				if (!t.guard.Invoke()) { continue; }

				// check if priority exceeds current best
				float priority = t.priority.Invoke();
				if(priority < bestPriority)
				{
					bestPriority = priority;
					transitionIndex = i;
				}

				else if(t.state == -2)
				{
					//20 = indicator for transitioning to different fsm
					return -2;
				}
			}

			// available transition was found
			if(transitionIndex > -1)
			{
				return ctx.transitions[transitionIndex].state;
			}

			return -1;
		}


		/// <summary>
		/// Finds random transition out of given state
		/// </summary>
		/// <param name="stateIndex"></param>
		/// <returns>Exit state</returns>

		private int GetRandomExit(in int stateIndex)
		{
			BoundState ctx = GetStateAtIndex(stateIndex);
			if (ctx == null) { return -1; }

			// index in transition list for state
			int transitionIndex = -1;

			//Initialize the statePoll list which will be used to choose a random state
			//It contains the summed weights of all ready transitions
			//The summed weight of all ready transitions mustn't exceed 100
			//but will be less than 100 if the eval condition for this transition fails, because that transition will be skipped
			List<int> statesPoll = new List<int>();

			//Loop through all transitions for this state
			for (int i = 0; i < ctx.transitions.Count; i++)
			{
				Transition t = ctx.transitions[i];

				// eval condition => Is it possible to exit to the new state of the current transition?
				if (!t.guard.Invoke()) { continue; }

				//Get the weight of this transition

				float weight = t.weight.Invoke();   //30% probability, 40% probability
				for (int j = 0; j < weight; j++)
				{
					statesPoll.Add(t.state);
				}
			}

			//choose a random state
			int randomState = UnityEngine.Random.Range(0, statesPoll.Count);
			//return randomState;

			Debug.Log("state poll count: " + statesPoll.Count);

			if (statesPoll.Count > 0)
			{
				Debug.Log("random state: " + statesPoll[randomState]);

				return statesPoll[randomState];
			}

			return -1;
		}

		[Serializable]
		// transition to another state
		public class Transition
		{
			// target state
			public int state = -1;
			// condition before transition can be taken
			public Func<bool> guard = null;
			// priority eval
			public Func<float> priority = null;

			public Func<float> weight = null;

			public FSM transitionFSM = null;
		}

		// information about state and its transitions
		public class BoundState
		{
			public FSM_State state = null;
			public List<Transition> transitions = new List<Transition>();
		}

		// simple indexer
		private BoundState GetStateAtIndex(in int i)
		{
			return i < 0 || i >= _states.Count ? null : _states[i];
		}

		public string ToFile(string fileName, int level)
		{

			string res = "";

			res = indent(level) + "\"states\":\n";
			res += indent(level) + "{\n";

			int j = 1;
			foreach (var boundState in _states)
			{

				res += indent(level);
				res += indent(1);//indent one level in addition
				res += "\"state" + j + "\":\n";

				res += boundState.state.ToFile(fileName, level + 1);

				if (j < _states.Count)
					res += ",\n";
				else
					res += "\n";

				j++;

			}

			res += indent(level) + "}\n";
			return res;
		}

		private string indent(int level)
        {
            string s = "";
            string indentation = "\t";        
            for(int i = 0; i < level; i++)
                s+=indentation;
            return s;
        }
	}
}


namespace Harris.GPC
{
	using System;

	[Serializable]
	/// <summary>
	/// Base class for FSM state
	/// </summary>
	public abstract class FSM_State
	{
		/// <summary>
		/// State entered
		/// </summary>
		public virtual void Enter() { }
		/// <summary>
		/// State updated
		/// </summary>
		/// <param name="deltaTime">A numbery thing</param>
		public virtual void Tick(in float deltaTime) { }

		public virtual void LateTick(in float deltaTime) { }
		/// <summary>
		/// State exited
		/// </summary>
		public virtual void Exit() { }

		//Dictionary of exit conditions
		private IDictionary<string, Func<bool>> exitGuards = new Dictionary<string, Func<bool>>();

		public IDictionary<string, Func<bool>> ExitGuards => exitGuards;

		public void AddExitGuard(string name, Func<bool> exitGuard)
		{
			if (!exitGuards.ContainsKey(name))
			{
				exitGuards.Add(name, exitGuard);
			}
		}

		public Func<bool> GetExitGuard(string name)
		{
			if (!exitGuards.ContainsKey(name)) return null;
			return exitGuards[name];
		}

		public virtual string ToFile(string fileName, int level)
		{
			return "";
		}

		protected string indent(int level)
        {
            string s = "";
            string indentation = "    ";        
            for(int i = 0; i < level; i++)
                s+=indentation;
            return s;
        }
	}
}

namespace Harris.GPC
{
	using System;

	/// <summary>
	/// State wrapper around simple delegates
	/// </summary>
	internal sealed class ActionState : FSM_State
	{
		/// <summary>
		/// Create new state composed of simple delegates
		/// </summary>
		/// <param name="onTick">State ticked</param>
		/// <param name="onEnter">State entered</param>
		/// <param name="onExit">State exited</param>
		public ActionState(Action<float> onTick, Action onEnter = null, Action onExit = null)
		{
			_onTick = onTick;
			_onEnter = onEnter;
			_onExit = onExit;
		}
		public override void Enter() => _onEnter?.Invoke();
		public override void Tick(in float deltaTime) => _onTick?.Invoke(deltaTime);
		public override void Exit() => _onExit?.Invoke();
		private Action<float> _onTick = null;
		private Action _onEnter = null, _onExit = null;
	}
}