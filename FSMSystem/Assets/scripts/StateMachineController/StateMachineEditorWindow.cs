using UnityEngine;
using UnityEditor;
using Harris.GPC;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.Rendering;

namespace FSMController
{
    //NOTES: GUI COORDINATES (0,0) represents the top-left corner and Y increases downwards.

    public class SelectedEntry
    {
        public Rect _rect;
        public Texture2D _tex;
        public bool _sel;

        public SelectedEntry(Rect r, Texture2D t)
        {
            _rect = r;
            _tex = t;
            _sel = false;
        }
    }

    public class StateMachineEditorWindow : EditorWindow
    {
        private StateMachineController _fsmController;
        public StateMachineController FSMController { get => _fsmController; set => _fsmController = value; }
        private Vector2 _hierarchy_offset = Vector2.zero;
        private static IComponent<FSM> _selectedEntry = null;

        public static StateMachineEditorWindow Instance;
        private Texture2D _selectedEntryIndicatorResource;
        public Texture2D SelectedEntryIndicatorResource => _selectedEntryIndicatorResource;
        private List<SelectedEntry> _selectedEntries = new List<SelectedEntry>();
        public List<SelectedEntry> SelectedEntries => _selectedEntries;
        private static int current = 0;

        //GUIUtility.ScreenToGUIPoint(screenPos);

        private void Awake()
        {
            //_fsmController = null;
        }

        private void OnEnable()
        {
            if (Instance == null)
                Instance = this;

            if (_selectedEntryIndicatorResource == null)
                _selectedEntryIndicatorResource = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Textures/selector_right.svg", typeof(Texture2D));

            Vector2 _hierarchy_offset = new Vector2(50, 20);
        }


        [MenuItem("StateMachineEditor/Show")]
        public static void ShowWindow()
        {
            GetWindow<StateMachineEditorWindow>("State Machine Editor");
        }

        public static void ShowHieararchy(IComponent<FSM> node, Vector2 pos, int level)
        {

            string label = "";

            if (node is CompositeFSM)
            {
                //label = (node as CompositeFSM).ID.ToString() + " " + (node as CompositeFSM).Desc;
                label = (node as CompositeFSM).Desc;

            }
            else
            {
                label = (node as LeafFSM).Desc;
            }

            if (node == _selectedEntry)
            {
                GUIStyle style = new GUIStyle();
                style.fontStyle = FontStyle.Bold;
                GUI.Label(new Rect(pos, new Vector2(200, 20)), label, style);
            }
            else
            {
                GUI.Label(new Rect(pos, new Vector2(200, 20)), label);
            }

    
            Instance.SelectedEntries[current]._rect = new Rect(20 * level, pos.y+5, 10, 10);
            GUI.DrawTexture(Instance.SelectedEntries[current]._rect, Instance.SelectedEntries[current]._tex, ScaleMode.ScaleToFit, true, 1.0F);


            if (node is CompositeFSM)
            {
                foreach (var component in (node as CompositeFSM).list)
                {
                    //pos = pos + new Vector2(0,20);
                    current++;
                    pos = new Vector2(20 * (level + 1) + 15, pos.y + 20);
                    ShowHieararchy(component, pos, level + 1);
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

            var _mouseGUICoords = UnityEngine.Event.current.mousePosition;
            //var _mouseGUICoords = GUIUtility.ScreenToGUIPoint(Input.mousePosition);

            if (_mouseGUICoords.x < 100 && _mouseGUICoords.y < 400)
            {
                _selectedEntry = GetEntry(_mouseGUICoords);
            }

            GUIContent temp = new GUIContent();

            //Display FSMOwners
            temp.text = "Choose FSM Owner";
            DisplayFSMOwnersDropdown(new Rect(150, 5, 150, 20), temp);

            //Draw Template FSM Dropdown
            if (_fsmController == null)
                return;

            Debug.Log("root: " + _fsmController.Root);
            current = 0;
            ShowHieararchy(_fsmController.Root, _hierarchy_offset, 0);

            //GUIContent temp = new GUIContent();
            temp.text = "Choose FSM Template";
            DrawTemplateFSMDropdown(new Rect(350, 5, 150, 20), temp);
        }


        public static void DrawTemplateFSMDropdown(Rect position, GUIContent label)
        {
            if (!EditorGUI.DropdownButton(position, label, FocusType.Passive))
            {
                return;
            }

            void handleItemClicked(object parameter)
            {
                Debug.Log(parameter);
            }

            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Advanced Patrole FSM"), false, handleItemClicked, "Advanced Patrole FSM");
            menu.AddItem(new GUIContent("Item 2"), false, handleItemClicked, "Item 2");
            menu.AddItem(new GUIContent("Item 3"), false, handleItemClicked, "Item 3");
            menu.DropDown(position);
        }


        public static void HandleFSMOwnerChosen(object fsmController)
        {

            Instance.FSMController = fsmController as StateMachineController;
            if (Instance.FSMController != null)
            {
                Instance.FSMController.Root.AssignLevels(0);
                Instance.FSMController.Root.GenerateID(0);

                var arr = CompositeFSM.ToArray(Instance.FSMController.Root);

                if (Instance.SelectedEntries.Count == 0)
                {
                    for (int i = 0; i < arr.Count; i++)
                    {
                        Instance.SelectedEntries.Add(new SelectedEntry(new Rect(0, 0, 0, 0), Instance.SelectedEntryIndicatorResource));
                    }
                }
                Instance.SelectedEntries[0]._sel = true;
            }
        }

        public static void DisplayFSMOwnersDropdown(Rect position, GUIContent label)
        {
            if (!EditorGUI.DropdownButton(position, label, FocusType.Passive))
            {
                return;
            }

            GenericMenu menu = new GenericMenu();

            //Grab all state machine owners in scene
            Dictionary<GameObject, StateMachineController> scriptsDict = GetAllStateMachineOwners();

            foreach (KeyValuePair<GameObject, StateMachineController> kvp in scriptsDict)
            {
                menu.AddItem(new GUIContent(kvp.Key.name), false, HandleFSMOwnerChosen, kvp.Value);
            }
            menu.AddItem(new GUIContent("None"), false, HandleFSMOwnerChosen, null);

            menu.DropDown(position);
        }

        public static Dictionary<GameObject, StateMachineController> GetAllStateMachineOwners()
        {
            Dictionary<GameObject, StateMachineController> scriptsDict = new();
            var allGameObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            foreach (var go in allGameObjects)
            {
                var stateMachine = go.GetComponent<StateMachineController>();

                if (stateMachine != null)
                    scriptsDict.Add(go, stateMachine);
            }
            return scriptsDict;
        }

        public IComponent<FSM> GetEntry(Vector2 mousePos)
        {
            Debug.Log("Mouse gui pos: " + mousePos);

            var arr = CompositeFSM.ToArray(_fsmController.Root);

            /*for (int i = 0; i < arr.Count; i++)
            {
                if (_selectedEntries[i]._sel == true)
                    continue;
                _selectedEntries[i]._sel = false;
            }
            if (_selectedEntries[0]._sel == false)
                _selectedEntries[0]._sel = true;*/

            //calculate index in hierarchy from mouseposy
            var mouseRelativeY = mousePos.y - _hierarchy_offset.y;

            int index = (int)mouseRelativeY / 20;

            if (index >= arr.Count)
                return null;

            //return arr[index];

            if (mousePos.x > arr[index].Level * 20 && mousePos.x < arr[index].Level * 20 + 50)
                return arr[index];

            Debug.Log("INDEX = " + index);
            Debug.Log("LEVEL = " + arr[index].Level);

            return null;
        }

        private void Update()
        {
            if (_selectedEntry != null)
            {
                if (_selectedEntry is LeafFSM)
                    Debug.Log("Selected Entry: " + (_selectedEntry as LeafFSM).Desc);
                else
                    Debug.Log("Selected Entry: " + (_selectedEntry as CompositeFSM).Desc);
            }
            Repaint();
        }

    }

}