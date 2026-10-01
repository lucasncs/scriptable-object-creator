using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ScriptableObjectWizard.Settings;
using UnityEditor;
using UnityEngine;

namespace ScriptableObjectWizard
{
    /// <summary>
    /// A helper class for instantiating ScriptableObjects in the editor.
    /// </summary>
    public static class ScriptableObjectFactory
    {
        [MenuItem("Assets/Create/ScriptableObject", priority = 1)]
        public static void CreateScriptableObject()
        {
            Type[] types = GetIncludedTypes(typeof(ScriptableObject));
            if (types == null) return;

            switch (ScriptableObjectWizardSettings.Instance.TypePicker)
            {
                case TypePickerStyle.TreeView:
                    ScriptableObjectFactoryTreeWindow.Init(types);
                    break;
                default:
                    ScriptableObjectFactoryWindow.Init(types);
                    break;
            }
        }

        /// <summary>
        /// Returns the creatable subclasses of <paramref name="type"/> in the assemblies selected in the settings,
        /// or null after offering to open the settings when there are none.
        /// </summary>
        private static Type[] GetIncludedTypes(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            ScriptableObjectWizardSettings settings = ScriptableObjectWizardSettings.Instance;

            Type[] allScriptableObjects = GetCreatableTypes()
                .Where(t => t.IsSubclassOf(type) && settings.IsAssemblyIncluded(t.Assembly.GetName().Name))
                .ToArray();

            if (allScriptableObjects.Length == 0)
            {
                if (EditorUtility.DisplayDialog("Scriptable Object Wizard",
                        "No ScriptableObject types were found in the selected assemblies.",
                        "Open Settings", "Cancel"))
                {
                    SettingsService.OpenProjectSettings("Project/Scriptable Object Wizard");
                }

                return null;
            }

            return allScriptableObjects;
        }

        /// <summary>
        /// Returns every concrete ScriptableObject type in the loaded assemblies that can be instantiated as an asset,
        /// excluding editor UI types such as EditorWindow and Editor, and the wizard's own internal types.
        /// </summary>
        internal static IEnumerable<Type> GetCreatableTypes()
        {
            Assembly wizardAssembly = typeof(ScriptableObjectFactory).Assembly;
            return TypeCache.GetTypesDerivedFrom<ScriptableObject>()
                .Where(t => t.Assembly != wizardAssembly)
                .Where(t => !t.IsAbstract && !t.ContainsGenericParameters)
                .Where(t => !typeof(EditorWindow).IsAssignableFrom(t) && !typeof(Editor).IsAssignableFrom(t));
        }
    }
}