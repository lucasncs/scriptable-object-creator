using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ScriptableObjectWizard
{
    /// <summary>
    /// Window that lists ScriptableObject types in a searchable tree grouped by namespace.
    /// </summary>
    public class TreeViewTypePickerWindow : ATypePickerWindow
    {
        private const string SEARCH_FIELD_CONTROL_NAME = "ScriptableObjectWizardTypeSearch";

        private ScriptableObjectTypeTreeView _treeView;
        private InlineSearchField _searchField;
        private string _search = string.Empty;
        private bool _focusSearchField;
        private GUIStyle _paddingStyle;

        private void OnEnable()
        {
            minSize = new Vector2(320f, 400f);
        }

        protected override void OnTypesChanged()
        {
            _treeView = new ScriptableObjectTypeTreeView(new TreeViewState(), Types, CreateAsset);
            _searchField = new InlineSearchField(SEARCH_FIELD_CONTROL_NAME, Repaint);
            _search = string.Empty;
            _focusSearchField = true;
        }

        protected override void DrawContent()
        {
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
    }
}