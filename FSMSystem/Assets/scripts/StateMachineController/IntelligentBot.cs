using UnityEngine;
using Harris.GPC;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace FSMController
{

    [ExecuteAlways]
    public class IntelligentBot : StateMachineController
    {
        //private CompositeFSM _fsm = new CompositeFSM();
        private CompositeFSM _fsm;

        private WaypointsController _waypointsController;

        private string[] letters = { "A", "B", "C", "D" };

        public void Start()
        {
            int i = 0;

            _waypointsController = GetComponent<WaypointsController>();
            _waypointsController.ConnectWaypoints();

            Initialize();
        }

        public override void Initialize()
        {
            Reset();
            _fsm = new CompositeFSM();

            /*foreach (var wp in _waypointsController.Waypoints)
            {
                int i = 1;

                Debug.Log("wp connections count " + wp.Connections.Count);

                foreach (var connection in wp.Connections)
                {
                    //
                    var fsm1 = new FSM();
                    var state1 = new MoveToLocationState(this, wp, connection);
                    var state2 = new IdleState(this);

                    int index1 = fsm1.AddState(state1);
                    int index2 = fsm1.AddState(state2);

                    fsm1.AddTransition(index1, -2, state1.GetExitGuard("Arrived"), null, () => { return 40; });
                    fsm1.AddTransition(index1, index2, state1.GetExitGuard("Arrived"), null, () => { return 60; });
                    fsm1.AddTransition(index2, -2, state2.GetExitGuard("TimeOut"));

                    _fsm.AddLeaf(new LeafFSM(fsm1));

                    i++;
                }

            }

            for (int i = 0; i < _fsm.list.Count; i++)
            {
                CalculateTransitions(_fsm.list[i] as LeafFSM);

                string log = (_fsm.list[i].Name.States[0].state as MoveToLocationState)._start + ", " + (_fsm.list[i].Name.States[0].state as MoveToLocationState)._end + " => (";
                foreach (var transition in (_fsm.list[i] as LeafFSM).Transitions)
                {
                    log += transition + ", ";
                }
                log += ")";
                Debug.Log(log);
            }*/

            var fsm0 = new LeafFSM(new FSM());
            _fsm.AddLeaf(fsm0);

            var fsm1 = new CompositeFSM();
            fsm1.AddLeaf(new LeafFSM(new FSM()));
            fsm1.AddLeaf(new LeafFSM(new FSM()));
            fsm1.AddLeaf(new LeafFSM(new FSM()));

             var fsm1_1 = new CompositeFSM();
            fsm1_1.Add(new LeafFSM(new FSM()));
            fsm1.AddComposite(fsm1_1);
            _fsm.AddComposite(fsm1);


            var fsm2 = new CompositeFSM();
            fsm2.AddLeaf(new LeafFSM(new FSM()));
            fsm2.AddLeaf(new LeafFSM(new FSM()));
            fsm2.AddLeaf(new LeafFSM(new FSM()));
            _fsm.AddComposite(fsm2);

            var fsm3 = new LeafFSM(new FSM());
            _fsm.AddLeaf(fsm3);
            
            AddComposite(_fsm);
            SerializeStateMachine(this, " temp");
            base.Initialize();

        }

        private List<int> GetAllStatesByStartLocation(ConnectedWaypoint start)
        {
            List<int> result = new List<int>();
            for (int i = 0; i < _fsm.list.Count; i++)
            {
                if ((_fsm.list[i].Name.States[0].state as MoveToLocationState)._start == start)
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
    }
}