using UnityEngine;
using Harris.GPC;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Runtime.InteropServices.WindowsRuntime;

namespace FSMController
{

    public class IntelligentBot: StateMachineController
    {
        private CompositeFSM _fsm = new CompositeFSM();

        [SerializeField]
        private List<ConnectedWaypoint> _waypoints;

        private string[] letters = {"A","B","C","D"};

        //private Dictionary<ConnectedWaypoint, string> _wpDict = new Dictionary<ConnectedWaypoint, string>();
        
        //private Dictionary<string, ConnectedWaypoint> _wpDict = new Dictionary<string,ConnectedWaypoint>();
        
        public void Start()
        {
            int i = 0;
            /*foreach (var wp in _waypoints)
            {
                _wpDict[wp] = letters[i];
                i++;
            }*/

            ConnectWaypoints();

            InitializeFSM();

            AddComposite(_fsm);
            StateMachineController.SerializeStateMachine(this, " temp");
            Initialize();
        }

        private void InitializeFSM()
        {
            /*int i = 0;
            int j = 0;
            foreach(var wp in _waypoints)
            {
                j = 0;
                foreach(var connection in wp.Connections)
                {
                    _fsm.AddLeaf(new LeafFSM(new MoveToLocationFSM(_wpDict[wp], _wpDict[connection])));
                    j++;
                }
                i++;
            }*/
            foreach(var wp in _waypoints)
            {
                foreach(var connection in wp.Connections)
                {
                    //
                    var fsm1 = new FSM();
                    var state1 = new MoveToLocationState(this, wp, connection);
                    var state2 = new IdleState(this);

                    int index1 = fsm1.AddState(state1);
                    int index2 = fsm1.AddState(state2);

                    fsm1.AddTransition(index1, -2, state1.GetExitGuard("Arrived"),null, () => { return 30;});
                    fsm1.AddTransition(index1, index2, state1.GetExitGuard("Arrived"),null, () => { return 70;});
                    fsm1.AddTransition(index2, -2, state2.GetExitGuard("TimeOut"));

                    _fsm.AddLeaf(new LeafFSM(fsm1));
                }
            }

            for(int i = 0; i < _fsm.list.Count; i++)
            {
                CalculateTransitions(_fsm.list[i] as LeafFSM);

                string log = (_fsm.list[i].Name.States[0].state as MoveToLocationState)._start +", "+ (_fsm.list[i].Name.States[0].state as MoveToLocationState)._end + " => (";
                foreach(var transition in (_fsm.list[i] as LeafFSM).Transitions)
                {
                    log += transition + ", ";
                }
                log += ")";
                Debug.Log(log);
            }

        }

        private List<int> GetAllStatesByStartLocation(ConnectedWaypoint start)
        {
            List<int> result = new List<int>();
            for(int i = 0; i < _fsm.list.Count; i++)
            {
                if((_fsm.list[i].Name.States[0].state as MoveToLocationState)._start== start)
                {
                    var temp = _fsm.list[i].Name as MoveToLocationFSM;
                    result.Add(i);
                }
            }
            return result;
        }

        private void CalculateTransitions(LeafFSM s)
        {
            ConnectedWaypoint to = (s.Name.States[0].state as MoveToLocationState)._end;
            List<int> temp = GetAllStatesByStartLocation(to);
            foreach (var item in temp)
            {
                s.AddTransition(item);
            }

        }

        private void ConnectWaypoints()
        {
            ConnectedWaypoint current = _waypoints[0];
            foreach(var wp in _waypoints)
            {
                foreach(var other in _waypoints)
                {
                    if(wp == other)
                        continue;
                    if(other.Connections.Contains(wp))
                        continue;
                    wp.TryAddConnection(other);
                }
            }
        }

        public void IntelligentPatrole()
        {

        }
    }
}