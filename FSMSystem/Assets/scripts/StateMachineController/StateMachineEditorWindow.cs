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
        public int _componentID;
        public SelectedEntry(Rect r, Texture2D t, int compID)
        {
            _rect = r;
            _tex = t;
            _sel = false;
            _componentID = compID;
        }
    }

    public class StateMachineEditorWindow : EditorWindow
    {

        [SerializeField]
        private StateMachineController _fsmController;

        public StateMachineController FSMController { get => _fsmController; set => _fsmController = value; }
        private Vector2 _hierarchy_offset = new Vector3(0, 20);
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
        public static int current = 0;
        private bool _leftMousePressed = false;
        private bool _rightMousePressed = false;
        private bool _showAddFSMContextMenu = false;
        private Vector2 _mouseGUICoords = Vector2.zero;

        private Rect addFSMContextMenuRect;

        int index = -1;

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

        }


        [MenuItem("StateMachineEditor/Show")]
        public static void ShowWindow()
        {
            GetWindow<StateMachineEditorWindow>("State Machine Editor");
        }

        public static void ShowHieararchy(IComponent<FSM> node, Vector2 pos, int level)
        {
            GUIStyle style = new GUIStyle();
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.white;
            GUI.Label(new Rect(Vector3.zero, new Vector2(200, 20)), "Hierarchy",style);
            ShowHieararchyRec(node, pos, level);
        }

        public static void ShowHieararchyRec(IComponent<FSM> node, Vector2 pos, int level)
        {

            string label = "";

            if (node is CompositeFSM)
            {
                //label = (node as CompositeFSM).ID.ToString() + " " + (node as CompositeFSM).Desc;
                label = (node as CompositeFSM).Desc;

            }
            else if (node is LeafFSM)
            {
                label = (node as LeafFSM).Desc;
            }
            else if (node == null)
            {
                label = "Leaf FSM Data";
            }

            GUI.Label(new Rect(pos, new Vector2(200, 20)), node.ID + ":" + label);

            if (node != null)
            {
                Instance.SelectedEntries[current]._rect = new Rect(20 * level, pos.y + 5, 10, 10);


                if (Instance.SelectedEntries[current]._sel)
                    Instance.SelectedEntries[current]._tex = Instance.SelectedEntryIndicatorResource2;
                else
                    Instance.SelectedEntries[current]._tex = Instance.SelectedEntryIndicatorResource;

                GUI.DrawTexture(Instance.SelectedEntries[current]._rect, Instance.SelectedEntries[current]._tex, ScaleMode.ScaleToFit, true, 1.0F);
            }

            //GUI.DrawTexture(Instance.SelectedEntries[current]._rect, Instance.SelectedEntries[current]._tex, ScaleMode.ScaleToFit, true, 1.0F);

            if (node is CompositeFSM && Instance.SelectedEntries[current]._sel)
            {
                var parentIndex = current;
                foreach (var component in (node as CompositeFSM).list)
                {
                    //pos = pos + new Vector2(0,20);
                    //current++;

                    current = component.ID;
                    pos = new Vector2(20 * (level + 1) + 15, pos.y + 20);
                    ShowHieararchyRec(component, pos, level + 1);

                    if (component is CompositeFSM && Instance.SelectedEntries[component.ID]._sel)
                    {
                        UpdatePosY(component as CompositeFSM, ref pos);
                    }

                    if (component is LeafFSM && Instance.SelectedEntries[current]._sel)
                    {
                        pos = new Vector2(20 * (level + 2) + 15, pos.y + 20);
                        ShowHieararchyRec(null, pos, 0);
                    }
                }
            }
        }

        private static void UpdatePosY(CompositeFSM comp, ref Vector2 pos)
        {
            foreach (var c in (comp as CompositeFSM).list)
            {
                pos.y += 20;
                if (c is CompositeFSM && Instance.SelectedEntries[c.ID]._sel)
                {
                    UpdatePosY(c as CompositeFSM, ref pos);
                }
            }
        }

        private void OnGUI()
        {

            _mouseGUICoords = UnityEngine.Event.current.mousePosition;

            if (UnityEngine.Event.current.type == EventType.MouseDown && UnityEngine.Event.current.button == 0)
            {
                if(_showAddFSMContextMenu &&!addFSMContextMenuRect.Contains(_mouseGUICoords))
                    _showAddFSMContextMenu = false;
                _leftMousePressed = true;
            }
            if (_leftMousePressed && UnityEngine.Event.current.type == EventType.MouseUp && UnityEngine.Event.current.button == 0)
            {
                _leftMousePressed = false;
                OnLeftMouseReleased();
            }

            if (UnityEngine.Event.current.type == EventType.MouseDown && UnityEngine.Event.current.button == 1)
            {
                _rightMousePressed = true;
            }
            if (_rightMousePressed && UnityEngine.Event.current.type == EventType.MouseUp && UnityEngine.Event.current.button == 1)
            {
                _rightMousePressed = false;
                OnRightMouseReleased();
            }


            GUIContent temp = new GUIContent();

            //Display FSMOwners
            temp.text = "Choose FSM Owner";
            DisplayFSMOwnersDropdown(new Rect(150, 5, 150, 20), temp);

            //Display Hierarchy
            if (_fsmController == null)
                return;

            current = 0;
            ShowHieararchy(_fsmController.Root, _hierarchy_offset, 0);

            //Draw Template FSM Dropdown
            temp.text = "Choose FSM Template";
            DrawTemplateFSMDropdown(new Rect(350, 5, 150, 20), temp);

            if (_showAddFSMContextMenu && _selectedEntryIndex > -1)
            {
                //GUIContent contextMenu = new GUIContent();
                //contextMenu.text = "Add FSM";
                //GUI.Label(new Rect(_selectedEntries[_selectedEntryIndex]._rect.x, _selectedEntries[_selectedEntryIndex]._rect.y, 50, 20), contextMenu);

                //public static int Popup(Rect position, GUIContent label, int selectedIndex, GUIContent[] displayedOptions, GUIStyle style = EditorStyles.popup);
                index = -1;
                string[] options = { "New Composite", "New Leaf" };
                addFSMContextMenuRect = new Rect(_selectedEntries[_selectedEntryIndex]._rect.x + 40, _selectedEntries[_selectedEntryIndex]._rect.y, 120, 20);
                index = EditorGUI.Popup(
                addFSMContextMenuRect,
                index,
                options, EditorStyles.popup);

                _leftMousePressed = false;

                Debug.Log("CHOSEN OPTION: " + index);
            }
        }

        public void OnLeftMouseReleased()
        {

            if (_selectedEntryIndex >= 0)
            {
                _selectedEntries[_selectedEntryIndex]._sel = !_selectedEntries[_selectedEntryIndex]._sel;
            }
        }

        public void OnRightMouseReleased()
        {
            _selectedEntry = GetEntry(_mouseGUICoords);
            if (_selectedEntryIndex >= 0)
            {
                Debug.Log("rm released");
                _showAddFSMContextMenu = true;
                //_showAddFSMContextMenu = !_showAddFSMContextMenu;
            }
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
                Instance.FSMController.Initialize();
                Instance.FSMController.Root.AssignLevels(0);
                Instance.FSMController.Root.GenerateID(0);
                Instance.SelectedEntries.Clear();

                var arr = CompositeFSM.ToArray(Instance.FSMController.Root);
                Debug.Log("arr count: " + arr.Count);

                if (Instance.SelectedEntries.Count == 0)
                {
                    Debug.Log("selected entries count = 0");
                    for (int i = 0; i < arr.Count; i++)
                    {
                        Instance.SelectedEntries.Add(new SelectedEntry(new Rect(0, 0, 0, 0), Instance.SelectedEntryIndicatorResource, arr[i].ID));
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

        private int SkipUnselectedComponents(List<IComponent<FSM>> arr, int max, int current)
        {

            if (current < max && arr[current] is CompositeFSM && _selectedEntries[current]._sel == false)
            {
                Debug.Log("CURRENT = " + current + ", MAX = " + max);
                for (int k = 0; k < (arr[current] as CompositeFSM).list.Count; k++)
                {
                    current++;
                    if (current < max && arr[current] is CompositeFSM && _selectedEntries[current]._sel == false)
                        SkipUnselectedComponents(arr, max, current);
                }
            }
            else if(current < max)
            {
                SkipUnselectedComponents(arr, max, current+1);
            }

            return current;
        }

        public IComponent<FSM> GetEntry(Vector2 mousePos)
        {
            Debug.Log("Mouse gui pos: " + mousePos);

            var arr = CompositeFSM.ToArray(_fsmController.Root);

            //calculate index in hierarchy from mouseposy
            /*var mouseRelativeY = mousePos.y - _hierarchy_offset.y;

            int index = (int)mouseRelativeY / 20;

            if (index >= arr.Count)
                return null;

            _selectedEntryIndex = -1;

            int j;
            
            Debug.Log("INDEX = " + index);
            Debug.Log("LEVEL = " + arr[index].Level);


            if (_selectedEntries[index]._rect.Contains(UnityEngine.Event.current.mousePosition))
            {
                //_selectedEntryIndex = _selectedEntries[index]._componentID;
                _selectedEntryIndex = index;
                return arr[_selectedEntryIndex];
            }*/

            var oldSelectedEntryIndex = _selectedEntryIndex;
            for (int i = 0; i < _selectedEntries.Count; i++)
            {
                if (_selectedEntries[i]._rect.Contains(mousePos))
                {
                    _selectedEntryIndex = i;
                    if (_selectedEntryIndex != oldSelectedEntryIndex)
                        onSelectionChanged();
                    return arr[_selectedEntryIndex];
                }

            }
            _selectedEntryIndex = -1;

            return null;
        }

        private void onSelectionChanged()
        {
            //_showAddFSMContextMenu = false;
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

            if (_selectedEntryIndex != -1 && !_selectedEntries[_selectedEntryIndex]._rect.Contains(_mouseGUICoords) && !_showAddFSMContextMenu)
            {
                Debug.Log("sel entry = -1");
                _selectedEntryIndex = -1;
            }

            if (index > -1)
            {
                Debug.Log("showaddmenu = false!");
                _showAddFSMContextMenu = false;
            }

            if (_leftMousePressed && _mouseGUICoords.x <= 100 && _mouseGUICoords.x >= 0 && _mouseGUICoords.y <= 800 && _mouseGUICoords.y >= 0)
            {
                Debug.Log("left mouse pressed!");
                _selectedEntry = GetEntry(_mouseGUICoords);
                //if (_rightMousePressed && _selectedEntryIndex > -1)
                    //_showAddFSMContextMenu = !_showAddFSMContextMenu;
            }

            if (_fsmController != null)
                Repaint();
        }

    }

}