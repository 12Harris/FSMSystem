using UnityEngine;
using UnityEditor;
using Harris.GPC;

namespace FSMController
{
    //NOTES: GUI COORDINATES (0,0) represents the top-left corner and Y increases downwards.

    public class StateMachineEditorWindow: EditorWindow
    {
        private StateMachineController _fsmController;
        private Vector2 _hierarchy_offset = Vector2.zero;
        private static IComponent<FSM> _selectedEntry = null;

        //GUIUtility.ScreenToGUIPoint(screenPos);

        private void Awake()
        {
            //_fsmController = null;
        }

        private void OnEnable()
        {
            if(_fsmController == null)
            {
                Debug.Log("finding fsm controller!");
                _fsmController = GameObject.Find("FSMController").GetComponent<StateMachineController>(); // Replace "MyGameObject"
            }

            _fsmController.Root.AssignLevels(0);
            _fsmController.Root.GenerateID(0);

            Vector2 _hierarchy_offset = new Vector2(50,20);
        }


        [MenuItem("StateMachineEditor/Show")]
        public static void ShowWindow()
        {
            GetWindow<StateMachineEditorWindow>("State Machine Editor");
        }

        public static void ShowHieararchy(IComponent<FSM> node, Vector2 pos, int level)
        {
            
            string label = "";

            if(node is CompositeFSM)
            {
                label = (node as CompositeFSM).ID.ToString()+ " " +(node as CompositeFSM).Desc;
                //label = level.ToString()+ " " +(node as CompositeFSM).Desc;
            }
            else
            {
                label = (node as LeafFSM).ID.ToString()+ " " +(node as LeafFSM).Desc;
                //label = level.ToString()+ " " +(node as LeafFSM).Desc;
            }

            if(node == _selectedEntry)
            {
                GUIStyle style = new GUIStyle();
                style.fontStyle = FontStyle.Bold;
                GUI.Label(new Rect(pos, new Vector2(200,20)), label, style);               
            }
            else
            {
                GUI.Label(new Rect(pos, new Vector2(200,20)), label);
            }
            
            if(node is CompositeFSM)
            {
                foreach(var component in (node as CompositeFSM).list)
                {
                    //pos = pos + new Vector2(0,20);
                    pos = new Vector2(20 * (level+1),pos.y+20);
                    ShowHieararchy(component, pos, level+1);
                }
            }

            /*var arr = CompositeFSM.ToArray(node as CompositeFSM);
            label = (node as Composite<FSM>).ID.ToString()+ " " +(node as CompositeFSM).Desc;

            foreach(var item in arr)
            {
                if(item is CompositeFSM)
                {
                    label = (item as Composite<FSM>).ID.ToString()+ " " +(item as CompositeFSM).Desc;
                }
                else
                {
                    label = (item as Component<FSM>).ID.ToString()+ " " +(item as LeafFSM).Desc;
                }
                pos =pos + new Vector2(0,50);
                GUI.Label(new Rect(pos, new Vector2(200,20)), label);
            }*/

        }

        private void OnGUI()
        {
            Debug.Log("root: " +_fsmController.Root );
            ShowHieararchy(_fsmController.Root, _hierarchy_offset,0);

            var _mouseGUICoords = UnityEngine.Event.current.mousePosition;
            //var _mouseGUICoords = GUIUtility.ScreenToGUIPoint(Input.mousePosition);

            if(_mouseGUICoords.x < 100 && _mouseGUICoords.y < 400)
            {
                _selectedEntry= GetEntry(_mouseGUICoords);  
            }
        }

        public IComponent<FSM> GetEntry(Vector2 mousePos)
        {
            Debug.Log("Mouse gui pos: " + mousePos);

            var arr = CompositeFSM.ToArray(_fsmController.Root);

            //calculate index in hierarchy from mouseposy
            var mouseRelativeY = mousePos.y-_hierarchy_offset.y;

            int index = (int)mouseRelativeY/20;

            if(index >= arr.Count) 
                return null;

            //return arr[index];

            if(mousePos.x > arr[index].Level*20 && mousePos.x < arr[index].Level*20 + 50)
                return arr[index];

            Debug.Log("INDEX = " + index);
            Debug.Log("LEVEL = " + arr[index].Level);

            return null;
        }

        private void Update()
        {
            if(_selectedEntry!= null)
            {
                if(_selectedEntry is LeafFSM)
                    Debug.Log("Selected Entry: " + (_selectedEntry as LeafFSM).Desc);
                else
                    Debug.Log("Selected Entry: " + (_selectedEntry as CompositeFSM).Desc);
            }
            Repaint();
        }

    }

}