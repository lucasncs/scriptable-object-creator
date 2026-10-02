using System;
using System.Collections.Generic;
using System.Linq;
using ScriptableObjectWizard.Settings;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace ScriptableObjectWizard
{
    public static class ScriptableObjectFactory
    {
        [MenuItem("Assets/Create/ScriptableObject", priority = 1)]
        public static void OpenScriptableObjectCreator()
        {
            Type selectedType = GetSelectedScriptType();
            if (selectedType != null)
            {
                StartCreatingAsset(selectedType);
                return;
            }

            Type[] types = GetIncludedTypesOrPromptSettings();
            if (types == null) return;

            bool autoClose = ScriptableObjectWizardSettings.Instance.AutoCloseWindow;
            switch (ScriptableObjectWizardSettings.Instance.TypePicker)
            {
                case TypePickerStyle.TreeView:
                    ATypePickerWindow.Open<TreeViewTypePickerWindow>(types, autoClose);
                    break;
                case TypePickerStyle.Dropdown:
                default:
                    ATypePickerWindow.Open<DropdownTypePickerWindow>(types, autoClose);
                    break;
            }
        }

        private static Type GetSelectedScriptType()
        {
            var script = Selection.activeObject as MonoScript;
            Type type = script != null ? script.GetClass() : null;
            return type != null && type.IsSubclassOf(typeof(ScriptableObject)) && IsCreatable(type) ? type : null;
        }

        /// <summary>
        /// Creates an instance of <paramref name="type"/> and starts naming its asset in the active Project folder.
        /// </summary>
        internal static void StartCreatingAsset(Type type)
        {
            var asset = ScriptableObject.CreateInstance(type);
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                asset.GetInstanceID(),
                ScriptableObject.CreateInstance<CreateAssetAction>(),
                $"{type.Name}.asset",
                AssetPreview.GetMiniThumbnail(asset),
                null);
        }

        /// <summary>
        /// Returns the creatable ScriptableObject types in the assemblies selected in the settings.
        /// </summary>
        internal static Type[] FindIncludedTypes()
        {
            var settings = ScriptableObjectWizardSettings.Instance;

            return GetCreatableTypes()
                .Where(t => settings.IsAssemblyIncluded(t.Assembly.GetName().Name))
                .ToArray();
        }

        /// <summary>
        /// Returns <see cref="FindIncludedTypes"/>, or null after offering to open the settings when it is empty.
        /// </summary>
        private static Type[] GetIncludedTypesOrPromptSettings()
        {
            Type[] allScriptableObjects = FindIncludedTypes();
            if (allScriptableObjects.Length != 0) return allScriptableObjects;

            if (EditorUtility.DisplayDialog("Scriptable Object Wizard",
                    "No ScriptableObject types were found in the selected assemblies.",
                    "Open Settings", "Cancel"))
            {
                SettingsService.OpenProjectSettings("Project/Scriptable Object Wizard");
            }

            return null;
        }

        /// <summary>
        /// Returns every concrete ScriptableObject type in the loaded assemblies that can be instantiated as an asset,
        /// excluding editor UI types such as EditorWindow and Editor, and the wizard's own internal types.
        /// </summary>
        internal static IEnumerable<Type> GetCreatableTypes()
        {
            return TypeCache.GetTypesDerivedFrom<ScriptableObject>().Where(IsCreatable);
        }

        private static bool IsCreatable(Type type)
        {
            return type.Assembly != typeof(ScriptableObjectFactory).Assembly
                   && !type.IsAbstract && !type.ContainsGenericParameters
                   && !typeof(EditorWindow).IsAssignableFrom(type) && !typeof(Editor).IsAssignableFrom(type);
        }

        /// <summary>
        /// Saves the new instance as an asset once the user confirms its name in the Project window.
        /// </summary>
        private class CreateAssetAction : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                AssetDatabase.CreateAsset(EditorUtility.InstanceIDToObject(instanceId),
                    AssetDatabase.GenerateUniqueAssetPath(pathName));
            }
        }
    }
}