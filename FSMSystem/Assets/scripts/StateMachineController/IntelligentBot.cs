using UnityEngine;
using Harris.GPC;
using System;
using System.Collections.Generic;

namespace FSMController
{

    class IntelligentBot: StateMachineController
    {
        private CompositeFSM _fsm = new CompositeFSM();

        [SerializeField]
        private List<ConnectedWaypoint> _waypoints;

        private string[] letters = {"A","B","C","D"};

        private Dictionary<ConnectedWaypoint, string> _wpDict = new Dictionary<ConnectedWaypoint, string>();
        
        public void Start()
        {
            int i = 0;
            foreach (var wp in _waypoints)
            {
                _wpDict[wp] = letters[i];
                i++;
            }

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
                    //this needs to change!
                    var fsm = new LeafFSM(new MoveToLocationFSM(_wpDict[wp], _wpDict[connection]));

                    _fsm.AddLeaf(fsm);
                }
            }

            for(int i = 0; i < _fsm.list.Count; i++)
            {
                CalculateTransitions(_fsm.list[i] as LeafFSM);

                string log = (_fsm.list[i].Name as MoveToLocationFSM).locationA +", "+ (_fsm.list[i].Name as MoveToLocationFSM).locationB + " => (";
                foreach(var transition in (_fsm.list[i] as LeafFSM).Transitions)
                {
                    log += transition + ", ";
                }
                log += ")";
                Debug.Log(log);
            }

        }

        private List<int> GetAllStatesByStartLocation(string start)
        {
            List<int> result = new List<int>();
            for(int i = 0; i < _fsm.list.Count; i++)
            {
                if((_fsm.list[i].Name as MoveToLocationFSM).locationA == start)
                {
                    var temp = _fsm.list[i].Name as MoveToLocationFSM;
                    result.Add(i);
                }
            }
            return result;
        }

        private void CalculateTransitions(LeafFSM s)
        {
            string to = (s.Name as MoveToLocationFSM).locationB;
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