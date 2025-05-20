using UnityEngine;
using Harris.GPC;
using System;

namespace FSMController
{

    class StateMachineExample : StateMachineController
    {
        private CompositeFSM _fsm = new CompositeFSM();

        public void Start()
        {
            /*_fsm.Desc ="Composite 1";
                    
            LeafFSM _subFSM1 =  new LeafFSM(new MoveToLocationFSM("A", "B"));

            LeafFSM _subFSM2 =  new LeafFSM(new MoveToLocationFSM("B", "C"));

            LeafFSM _subFSM3 = new LeafFSM(new IdleFSM());
            
            //_fsm.AddComposite(_temp);

            _fsm.AddLeaf(_subFSM1);
            _fsm.AddLeaf(_subFSM2);
            _fsm.AddLeaf(_subFSM3);
            
            AddComposite(_fsm);
            StateMachineController.SerializeStateMachine(this, " temp");
            //Initialize();*/
        }
    }
}