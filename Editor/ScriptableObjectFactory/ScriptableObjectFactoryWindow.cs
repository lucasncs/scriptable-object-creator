using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace ScriptableObjectWizard
{
    internal class EndNameEdit : EndNameEditAction
    {
        #region implemented abstract members of EndNameEditAction

        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            AssetDatabase.CreateAsset(EditorUtility.InstanceIDToObject(instanceId),
                AssetDatabase.GenerateUniqueAssetPath(pathName));
        }

        #endregion
    }

    /// <summary>
    /// Scriptable object window.
    /// </summary>
    public class ScriptableObjectFactoryWindow : EditorWindow
    {
        private Type[] _types;
        private int _selectedIndex;
        private AdvancedDropdownState _dropdownState;

        public static void Init(Type[] types)
        {
            var window = GetWindow<ScriptableObjectFactoryWindow>(true, "Create a new ScriptableObject", true);
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
                CreateAsset(selectedName);
            }
        }

        private void OnTypeSelected(int index)
        {
            _selectedIndex = index;
            Repaint();
        }

        private void CreateAsset(string selectedName)
        {
            ScriptableObject asset = CreateInstance(_types[_selectedIndex]);
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                asset.GetInstanceID(),
                CreateInstance<EndNameEdit>(), 
                $"{selectedName}.asset", 
                AssetPreview.GetMiniThumbnail(asset), 
                null);
            Close();
            GUIUtility.ExitGUI();
        }
    }
}