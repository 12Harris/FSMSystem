using UnityEngine;
using Harris.GPC;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.ProjectWindowCallback;

namespace FSMController
{

    class IdleState : FSM_State
    {

        private IdleFSMData _saveData;
        private float _timer = 0f;
        private IntelligentBot _bot;

        public IdleState(IntelligentBot bot)
        {
            AddExitGuard("TimeOut", () => { return _timer > 2.0f; });
            _bot = bot;
        }

        public override void Enter()
        {
            Debug.Log("Entered idle state");
            _timer = 0f;
            _bot.gameObject.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        }

        public override void Tick(in float dt)
        {
            _timer += dt;
            Debug.Log("Ticking idle state!");
        }

        public override string ToFile(string fileName, int level)
        {
            _saveData = ScriptableObject.CreateInstance<IdleFSMData>();
            _saveData._idleDuration = 2.0f;
            string json = JsonUtility.ToJson(_saveData ,true);

            //reformat the json to match current indentation level
            string[] lines = json.Split(
                new string[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None
            );

            string res = "";
            foreach(var line in lines)
            {
                Debug.Log("LINE: " + line);
                res += indent(level) + line + "\n";
            }

            return res;

            //return json;
        }

        private string indent(int level)
        {
            string s = "";
            string indentation = "    ";        
            for(int i = 0; i < level; i++)
                s+=indentation;
            return s;
        }


    }

    public class MoveToLocationState : FSM_State
    {
        //public string _start;
        //public string _end;

        public ConnectedWaypoint _start;
        public ConnectedWaypoint _end;

        private float _timer = 0f;
        private MoveToLocationFSMData _saveData;

        private bool _arrived = false;

        private IntelligentBot _bot;

        public MoveToLocationState(IntelligentBot bot, ConnectedWaypoint start, ConnectedWaypoint end)
        {
            _start = start;
            _end = end;
            _bot = bot;

            AddExitGuard("Arrived", () => { return _arrived;});

        }

        public override void Enter()
        {
            Debug.Log("Starting at: " + _start);
            _timer = 0f;

        }

        public override void Tick(in float dt)
        {
            _timer += dt;
            Debug.Log("Moving to: " + _end);
            var v = _bot.transform.position;
            v.y = _start.transform.position.y;

            var distanceToDestination = Vector3.Distance(v, _end.transform.position);
            _arrived = distanceToDestination < 0.1f;

            if (_arrived)
                return;

            _bot.gameObject.GetComponent<Rigidbody>().linearVelocity = (_end.transform.position - v).normalized * 5.0f;
        }
        public override void Exit()
        {
            Debug.Log("Arrived at: " + _end);
        }

        public override string ToFile(string fileName, int level)
        {
            _saveData = ScriptableObject.CreateInstance<MoveToLocationFSMData>();
            _saveData._from = _start.transform.position;
            _saveData._to = _end.transform.position;
            string json = JsonUtility.ToJson(_saveData ,true);

            //reformat the json to match current indentation level
            string[] lines = json.Split(
                new string[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None
            );

            string res = "";
            foreach(var line in lines)
            {
                Debug.Log("LINE: " + line);
                res += indent(level) + line + "\n";
            }

            return res;
            //return json;
        }

        private string indent(int level)
        {
            string s = "";
            string indentation = "    ";        
            for(int i = 0; i < level; i++)
                s+=indentation;
            return s;
        }

    }

    [Serializable]
    public class IdleFSM : FSM
    {
        private IdleState _state;

        public IdleFSM()
        {
            _state = new IdleState(null);
            int index = AddState(_state);
        }
    }

    [Serializable]
    public class MoveToLocationFSM: FSM
    {
        public ConnectedWaypoint locationA;
        public ConnectedWaypoint locationB;
        private MoveToLocationState _state;
        public MoveToLocationState State => _state;

        public MoveToLocationFSM(ConnectedWaypoint start,ConnectedWaypoint end)
        {
            locationA = start;
            locationB = end;
            _state = new MoveToLocationState(null,start, end);
            int index = AddState(_state);
            AddTransition(index, -2, _state.GetExitGuard("Arrived"));//Abort this state machine
        }

        public void Initialize(string start, string end)
        {
           /* _state = new MoveToLocationState(start, end);
            int index = AddState(_state);
            AddTransition(index, -2, _state.GetExitGuard("Arrived"));*/
        }

    }

    [Serializable]
    public class CompositeFSM : Composite<FSM>
    {
         //This is the root state machine
        private int current = 0;
        private bool finished = false;
        public string Desc = "CompositeFSM";

        public event Action<IComponent<FSM>> _onFinished;

        private List<int> _transitions = new List<int>();
        public List<int> Transitions => _transitions;

        public CompositeFSM() : base(null)
        {
        }

        public void AddTransition(int newState)
        {
            _transitions.Add(newState);
        }

        public virtual void AddComposite(CompositeFSM component)
        {
            Add(component);
            component._onFinished += HandleAborted;
        }


        /*public virtual void AddLeaf<T,V> (T component) where V: FSM, new() where T: LeafFSM<V>
        {
            Add(component);
            component._onFinished += HandleAborted;
        }*/

        public virtual void AddLeaf(LeafFSM component)
        {
            Add(component);
            component._onFinished += HandleAborted;

            //ERROR: THE CONVERSION IS FALSE
            //if(component as IComponent<T> != null)
                //.Log("Component converted is NOT null!");
        }

        public void HandleAborted(IComponent<FSM> component)
        {
            //make sure that the passed component is the current executing component
            if(component != list[current])
                return;

            if((list[current] as LeafFSM).Transitions.Count == 0)
            {
                //get next state machine if it exists
                current++;

                if(current < list.Count)
                {
                    //start the state machine from its first state
                    var start = list[current].Start();
                    start.Name.SetState(0);

                }            
                else
                {
                    finished = true;
                    _onFinished?.Invoke(this);
                }
            }

            else
            {
                var rand = UnityEngine.Random.Range(0,(list[current] as LeafFSM).Transitions.Count);
                current = (list[current] as LeafFSM).Transitions[rand];
            }
        }

        public override void Update()
        {
            //Debug.Log("List Count = " + list.Count);
            //.Log("Current: " + current);

            //if(list[current] == null)
                //Debug.Log("Current is null!");

            if(!finished)
                list[current].Update();
        }

        public string ToFile(string fileName, int level)
		{
            string res = "";
     
            res = indent(level) + "\"type\": " + "\"CompositeFSM\",\n";

            res += indent(level) + "\"children\":\n";

            res += indent(level) + "{\n";

            int j = 1;
           
            foreach(var comp in list)
            {
                res += indent(level);
                res += indent(1);//indent one level in addition
                    
                res += "\"child" + j + "\":\n";

                if (comp is CompositeFSM)
                    res +=  (comp as CompositeFSM).ToFile(fileName, level+1);
                else
                    res +=  (comp as LeafFSM).ToFile(fileName, level+1);
                
                j++;
            }

            res += indent(level) + "}\n";
            return res;
		}

        private string indent(int level)
        {
            string s = "";
            string indentation = "    ";        
            for(int i = 0; i < level; i++)
                s+=indentation;
            return s;
        }

    }


    [Serializable]
    //LeafFSM only contains a single FSM
    public class LeafFSM: Component<FSM>
    {
        public event Action<IComponent<FSM>> _onFinished;
        public string Desc = "LeafFSM";

        private List<int> _transitions = new List<int>();
        public List<int> Transitions => _transitions;

        public LeafFSM(FSM fsm) : base(fsm)
        {
            //Debug.Log("Leaf created");
            Name._onFinished += HandleAborted;
        }

        public void AddTransition(int newState)
        {
            _transitions.Add(newState);
        }


        public void HandleAborted(FSM fsm)
        {
            if(fsm != Name)
                return;

            //this state machine finished, so signal it to its parent
            _onFinished?.Invoke(this as IComponent<FSM>);
        }

        public override void Update()
        {
            Name.Tick(Time.deltaTime);
        }

        public string ToFile(string fileName, int level)
		{
            string res = "";

            res = indent(level) + "\"type\": " + "\"LeafFSM\",\n";

            res += Name.ToFile(fileName, level);

            return res;

            //return Name.ToFile(fileName);
        }

        private string indent(int level)
        {
            string s = "";
            string indentation = "    ";        
            for(int i = 0; i < level; i++)
                s+=indentation;
            return s;
        }
    }

    //[ExecuteInEditMode]
    //State machine controller is the root for all state machines
    public abstract class StateMachineController : MonoBehaviour
    {
        
        //This is the root state machine
        private CompositeFSM _root;
        public CompositeFSM Root => _root;
        private int current = 0;
        private LeafFSM _executingLeaf = null;

        private void Awake()
        {
            _root = null;
            Debug.Log("state machine awake");
        }

        public void AddComposite(CompositeFSM component)
        {
            if(_root == null)
            {
                _root = new CompositeFSM();
                _root.Desc = "Root";
            }
                
            _root.AddComposite(component);
        }

        public void Initialize()
        {

            Debug.Log("INITIALIZE");
            _executingLeaf = _root.Start() as LeafFSM;//Find first leaf component and enter its state machine

            Debug.Log("Start is: " + _executingLeaf);

            _executingLeaf.Name.SetState(0);
        }

        // Update is called once per frame
        public void Update()
        {
            if(_root == null)
                Debug.Log("Error root is null");

            else
                Debug.Log("root is valid");

            if(_root.list.Count == 0)
                return;
                
            _root.list[current].Update();
        }

        public string Serialize(string filename)
        {
            //int i = 45;
            //ObjectSerializer.SerializeObject(i, filename);
            return (Root as CompositeFSM).ToFile(filename,0);
        }


        public static void SerializeStateMachine(StateMachineController stateMachine, string fileName)
		{
			if (stateMachine== null) {return; }

            string json = stateMachine.Serialize(fileName);

            Debug.Log("Serialized object: \n" + json);
        }
    }
}



/*
    public class StateMachineController : MonoBehaviour
    {
        
        //Compound Patrole state
        private IComponent <FSM> _compositePatrole = new Composite<FSM> (null);
        private int current = 0;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            PatroleFSM _patrole1 = new PatroleFSM();

            _compositePatrole.Add(_patrole1);

            var start = _compositePatrole.Start();//Find first leaf component and enter its state machine
            start.Name.SetState(0);
            //start.Name.CurrentState.Enter();
        }

        // Update is called once per frame
        void Update()
        {
            ((_compositePatrole as Composite<FSM>).list[current] as PatroleFSM).Update();
        }
    }
*/