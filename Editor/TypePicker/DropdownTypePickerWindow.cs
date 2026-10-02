using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ScriptableObjectCreator
{
    /// <summary>
    /// Window that picks the ScriptableObject class from a searchable dropdown grouped by namespace.
    /// </summary>
    public class DropdownTypePickerWindow : ATypePickerWindow
    {
        private Type _selectedType;
        private AdvancedDropdownState _dropdownState;

        protected override void OnTypesChanged()
        {
            _selectedType = Types[0];
        }

        protected override void DrawContent()
        {
            GUILayout.Label("ScriptableObject Class");
            var selectedName = new GUIContent(_selectedType.Name);
            Rect dropdownRect = GUILayoutUtility.GetRect(selectedName, EditorStyles.popup);
            if (EditorGUI.DropdownButton(dropdownRect, selectedName, FocusType.Keyboard, EditorStyles.popup))
            {
                if (_dropdownState == null) _dropdownState = new AdvancedDropdownState();
                new ScriptableObjectTypeDropdown(_dropdownState, Types, OnTypeSelected).Show(dropdownRect);
            }

            if (GUILayout.Button("Create"))
            {
                CreateAsset(_selectedType);
            }
        }

        private void OnTypeSelected(Type type)
        {
            _selectedType = type;
            Repaint();
        }
    }
}