using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
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
    public class ScriptableObjectFactoryWindow : EditorWindow, ISearchWindowProvider
    {
        private string[] _names;
        private Type[] _types;
        private int _selectedIndex;

        public static void Init(Type[] types)
        {
            var window = GetWindow<ScriptableObjectFactoryWindow>(true, "Create a new ScriptableObject", true);
            window._types = types;
            window._names = types.Select(t => t.FullName).ToArray();
            window.ShowPopup();
        }

        private void OnGUI()
        {
            GUILayout.Label("ScriptableObject Class");
            string selectedName = _types[_selectedIndex].Name;
            if (GUILayout.Button($"{selectedName}", EditorStyles.popup))
            {
                SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)),
                    this);
            }

            if (GUILayout.Button("Create"))
            {
                ScriptableObject asset = CreateInstance(_types[_selectedIndex]);
                ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                    asset.GetInstanceID(),
                    CreateInstance<EndNameEdit>(), 
                    $"{selectedName}.asset", 
                    AssetPreview.GetMiniThumbnail(asset), 
                    null);
                Close();
            }
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var list = new List<SearchTreeEntry> { new SearchTreeGroupEntry(new GUIContent("ScriptableObject"), 0) };
            var groups = new List<string>();
            for (int i = 0; i < _names.Length; i++)
            {
                string[] namespaceLevels = _names[i].Split('.');
                var groupName = new StringBuilder();
                for (int j = 0; j < namespaceLevels.Length - 1; j++)
                {
                    groupName.Append(namespaceLevels[j]);
                    string groupNameString = groupName.ToString();
                    if (!groups.Contains(groupNameString))
                    {
                        list.Add(new SearchTreeGroupEntry(new GUIContent(namespaceLevels[j]), j + 1));
                        groups.Add(groupNameString);
                    }

                    groupName.Append("/");
                }

                var entry = new SearchTreeEntry(new GUIContent(namespaceLevels.Last()))
                {
                    level = namespaceLevels.Length, userData = i
                };
                list.Add(entry);
            }

            return list;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            _selectedIndex = (int)searchTreeEntry.userData;
            return true;
        }
    }
}