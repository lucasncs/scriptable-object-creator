using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ScriptableObjectWizard
{
    /// <summary>
    /// Window that lists ScriptableObject types in a searchable tree grouped by namespace.
    /// </summary>
    public class ScriptableObjectFactoryTreeWindow : EditorWindow
    {
        private const string SearchFieldControlName = "ScriptableObjectWizardTypeSearch";

        private ScriptableObjectTypeTreeView _treeView;
        private InlineSearchField _searchField;
        private string _search = string.Empty;
        private bool _focusSearchField;
        private GUIStyle _paddingStyle;

        public static void Init(Type[] types)
        {
            var window = GetWindow<ScriptableObjectFactoryTreeWindow>(true, "Create a new ScriptableObject", true);
            window.minSize = new Vector2(320f, 400f);
            window._treeView = new ScriptableObjectTypeTreeView(new TreeViewState(), types, window.CreateAsset);
            window._searchField = new InlineSearchField(SearchFieldControlName, window.Repaint);
            window._search = string.Empty;
            window._focusSearchField = true;
            window.ShowPopup();
        }

        private void OnGUI()
        {
            // The type list does not survive a domain reload, so close instead of showing an empty window.
            if (_treeView == null)
            {
                Close();
                GUIUtility.ExitGUI();
            }

            if (_paddingStyle == null) _paddingStyle = new GUIStyle { padding = new RectOffset(6, 6, 6, 6) };

            using (new EditorGUILayout.VerticalScope(_paddingStyle))
            {
                HandleSearchFieldKeys();
                DrawSearchField();
                EditorGUILayout.Space();

                Rect treeRect = GUILayoutUtility.GetRect(0f, 100000f, 0f, 100000f);
                _treeView.OnGUI(treeRect);

                Type selectedType = _treeView.SelectedType;
                EditorGUILayout.LabelField(
                    selectedType != null ? selectedType.FullName : "Select a ScriptableObject class",
                    EditorStyles.miniLabel);
                using (new EditorGUI.DisabledScope(selectedType == null))
                {
                    if (GUILayout.Button("Create")) CreateAsset(selectedType);
                }
            }
        }

        private void DrawSearchField()
        {
            if (_focusSearchField && Event.current.type == EventType.Layout)
            {
                _focusSearchField = false;
                _searchField.SetFocus();
            }

            string search = _searchField.OnGUI(_search);
            if (search == _search) return;

            _search = search;
            _treeView.searchString = search;
            if (search.Length > 0) _treeView.SelectFirstType();
        }

        /// <summary>
        /// Lets the keyboard move from the search field into the tree, and create the selected type with Enter.
        /// </summary>
        private void HandleSearchFieldKeys()
        {
            Event current = Event.current;
            if (current.type != EventType.KeyDown || !_searchField.HasFocus) return;

            if (current.keyCode == KeyCode.DownArrow)
            {
                _treeView.SetFocusAndEnsureSelectedItem();
                current.Use();
            }
            else if ((current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter)
                     && _treeView.SelectedType != null)
            {
                current.Use();
                CreateAsset(_treeView.SelectedType);
            }
        }

        private void CreateAsset(Type type)
        {
            ScriptableObjectFactory.StartCreatingAsset(type);
            Close();
            GUIUtility.ExitGUI();
        }
    }
}
