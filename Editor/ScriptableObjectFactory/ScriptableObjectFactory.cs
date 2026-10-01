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
        public static void CreateScriptableObject()
        {
            Type selectedType = GetSelectedScriptType();
            if (selectedType != null)
            {
                StartCreatingAsset(selectedType);
                return;
            }

            Type[] types = GetIncludedTypes(typeof(ScriptableObject));
            if (types == null) return;

            switch (ScriptableObjectWizardSettings.Instance.TypePicker)
            {
                case TypePickerStyle.TreeView:
                    ScriptableObjectFactoryTreeWindow.Init(types);
                    break;
                case TypePickerStyle.Dropdown:
                default:
                    ScriptableObjectFactoryDropdownWindow.Init(types);
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
                ScriptableObject.CreateInstance<EndNameEdit>(),
                $"{type.Name}.asset",
                AssetPreview.GetMiniThumbnail(asset),
                null);
        }

        private static Type[] GetIncludedTypes(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            var settings = ScriptableObjectWizardSettings.Instance;

            Type[] allScriptableObjects = GetCreatableTypes()
                .Where(t => t.IsSubclassOf(type) && settings.IsAssemblyIncluded(t.Assembly.GetName().Name))
                .ToArray();

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

        private class EndNameEdit : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                AssetDatabase.CreateAsset(EditorUtility.InstanceIDToObject(instanceId),
                    AssetDatabase.GenerateUniqueAssetPath(pathName));
            }
        }
    }
}