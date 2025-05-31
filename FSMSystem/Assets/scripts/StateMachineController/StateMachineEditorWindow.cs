using UnityEngine;
using UnityEditor;
using Harris.GPC;
using System.Collections.Generic;
using System.Linq;
using System;

namespace FSMController
{
    //NOTES: GUI COORDINATES (0,0) represents the top-left corner and Y increases downwards.

    public class SelectedEntry
    {
        public Rect _rect;
        public Texture2D _tex;
        public bool _sel;
        IComponent<FSM> _comp;

        public SelectedEntry(Rect r, Texture2D t, IComponent<FSM> comp)
        {
            _rect = r;
            _tex = t;
            _sel = false;
            _comp = comp;
        }
    }

    public class StateMachineEditorWindow : EditorWindow
    {
        private StateMachineController _fsmController;
        public StateMachineController FSMController { get => _fsmController; set => _fsmController = value; }
        private Vector2 _hierarchy_offset = Vector2.zero;
        private static IComponent<FSM> _selectedEntry = null;
        private static int _selectedEntryIndex = -1;

        //private static int _selectedEntry =
        public static StateMachineEditorWindow Instance;
        private Texture2D _selectedEntryIndicatorResource;
        public Texture2D SelectedEntryIndicatorResource => _selectedEntryIndicatorResource;

        private Texture2D _selectedEntryIndicatorResource2;
        public Texture2D SelectedEntryIndicatorResource2 => _selectedEntryIndicatorResource2;
        private List<SelectedEntry> _selectedEntries = new List<SelectedEntry>();
        public List<SelectedEntry> SelectedEntries => _selectedEntries;
        private static int current = 0;
        private bool _leftMousePressed = false;
        private Vector2 _mouseGUICoords = Vector2.zero;

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

            if (_selectedEntryIndicatorResource2 == null)
                _selectedEntryIndicatorResource2 = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Textures/selector_down.svg", typeof(Texture2D));

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

            /*if (node == _selectedEntry)
            {
                GUIStyle style = new GUIStyle();
                style.fontStyle = FontStyle.Bold;
                GUI.Label(new Rect(pos, new Vector2(200, 20)), label, style);
            }
            else
            {
                GUI.Label(new Rect(pos, new Vector2(200, 20)), label);
            }*/
            GUI.Label(new Rect(pos, new Vector2(200, 20)), label);

    
            Instance.SelectedEntries[current]._rect = new Rect(20 * level, pos.y+5, 10, 10);

            if (node is CompositeFSM)
            {
                if (Instance.SelectedEntries[current]._sel)
                    Instance.SelectedEntries[current]._tex = Instance.SelectedEntryIndicatorResource2;
                else
                    Instance.SelectedEntries[current]._tex = Instance.SelectedEntryIndicatorResource;

                GUI.DrawTexture(Instance.SelectedEntries[current]._rect, Instance.SelectedEntries[current]._tex, ScaleMode.ScaleToFit, true, 1.0F);
            }
            
            //GUI.DrawTexture(Instance.SelectedEntries[current]._rect, Instance.SelectedEntries[current]._tex, ScaleMode.ScaleToFit, true, 1.0F);

            if (node is CompositeFSM && Instance.SelectedEntries[current]._sel)
            {
                foreach (var component in (node as CompositeFSM).list)
                {
                    //pos = pos + new Vector2(0,20);
                    current++;
                    pos = new Vector2(20 * (level + 1) + 15, pos.y + 20);
                    ShowHieararchy(component, pos, level + 1);
                }
            }
        }

        private void OnGUI()
        {

            _mouseGUICoords = UnityEngine.Event.current.mousePosition;

            if (_mouseGUICoords.x < 100 && _mouseGUICoords.y < 400)
            {
                _selectedEntry = GetEntry(_mouseGUICoords);
            }


            if (UnityEngine.Event.current.type == EventType.MouseDown && UnityEngine.Event.current.button == 0)
            {
                _leftMousePressed = true;
            }
            if (_leftMousePressed && UnityEngine.Event.current.type == EventType.MouseUp && UnityEngine.Event.current.button == 0)
            {
                _leftMousePressed = false;
                OnLeftMouseReleased();
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

        public void OnLeftMouseReleased()
        {
            Debug.Log("OKI PRESSED");

            if(_selectedEntryIndex >= 0)
            _selectedEntries[_selectedEntryIndex]._sel = !_selectedEntries[_selectedEntryIndex]._sel;
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
                        Instance.SelectedEntries.Add(new SelectedEntry(new Rect(0, 0, 0, 0), Instance.SelectedEntryIndicatorResource,arr[i]));
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

            //calculate index in hierarchy from mouseposy
            var mouseRelativeY = mousePos.y - _hierarchy_offset.y;

            int index = (int)mouseRelativeY / 20;

            if (index >= arr.Count)
                return null;

            _selectedEntryIndex = -1;

            if (_selectedEntries[index]._rect.Contains(UnityEngine.Event.current.mousePosition))
            {

                _selectedEntryIndex = index;
                return arr[index];
            }

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