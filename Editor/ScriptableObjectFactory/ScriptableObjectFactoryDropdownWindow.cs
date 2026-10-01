using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ScriptableObjectWizard
{
    public class ScriptableObjectFactoryDropdownWindow : EditorWindow
    {
        private Type[] _types;
        private int _selectedIndex;
        private AdvancedDropdownState _dropdownState;

        public static void Init(Type[] types)
        {
            var window = GetWindow<ScriptableObjectFactoryDropdownWindow>(true, "Create a new ScriptableObject", true);
            window._types = types;
            window.ShowPopup();
        }

        private void OnGUI()
        {
            if (_types == null)
            {
                Close();
                GUIUtility.ExitGUI();
            }

            GUILayout.Label("ScriptableObject Class");
            string selectedName = _types[_selectedIndex].Name;
            Rect dropdownRect = GUILayoutUtility.GetRect(new GUIContent(selectedName), EditorStyles.popup);
            if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(selectedName), FocusType.Keyboard, EditorStyles.popup))
            {
                if (_dropdownState == null) _dropdownState = new AdvancedDropdownState();
                new ScriptableObjectTypeDropdown(_dropdownState, _types, OnTypeSelected).Show(dropdownRect);
            }

            if (GUILayout.Button("Create"))
            {
                CreateAsset();
            }
        }

        private void OnTypeSelected(int index)
        {
            _selectedIndex = index;
            Repaint();
        }

        private void CreateAsset()
        {
            ScriptableObjectFactory.StartCreatingAsset(_types[_selectedIndex]);
            Close();
            GUIUtility.ExitGUI();
        }
    }
}