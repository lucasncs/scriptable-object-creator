using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;

namespace ScriptableObjectCreator.Settings
{
    /// <summary>
    /// How Assets > Create > ScriptableObject lets the user choose the class to create.
    /// </summary>
    public enum TypePickerStyle
    {
        Dropdown,
        TreeView,
    }

    /// <summary>
    /// Project-wide settings for the package, stored in ProjectSettings/ScriptableObjectCreatorSettings.asset.
    /// </summary>
    public class ScriptableObjectCreatorSettings : ScriptableObject
    {
        private const string SETTINGS_PATH = "ProjectSettings/ScriptableObjectCreatorSettings.asset";

        private static ScriptableObjectCreatorSettings _instance;

        [SerializeField] private List<string> _assemblyNames = new List<string> { "Assembly-CSharp" };
        [SerializeField] private TypePickerStyle _typePicker = TypePickerStyle.Dropdown;
        [SerializeField] private bool _autoCloseWindow = true;

        public static ScriptableObjectCreatorSettings Instance
        {
            get
            {
                if (_instance == null) _instance = Load();
                return _instance;
            }
        }

        /// <summary>
        /// Names of the assemblies whose ScriptableObject types are listed by the type pickers.
        /// </summary>
        public IReadOnlyList<string> AssemblyNames => _assemblyNames;

        public TypePickerStyle TypePicker
        {
            get => _typePicker;
            set
            {
                if (_typePicker == value) return;
                _typePicker = value;
                Save();
            }
        }

        /// <summary>
        /// Whether the picker window closes after creating an asset. When true it opens as a utility window,
        /// otherwise as a regular window that can be docked.
        /// </summary>
        public bool AutoCloseWindow
        {
            get => _autoCloseWindow;
            set
            {
                if (_autoCloseWindow == value) return;
                _autoCloseWindow = value;
                Save();
            }
        }

        public bool IsAssemblyIncluded(string assemblyName)
        {
            return _assemblyNames.Contains(assemblyName);
        }

        public void SetAssemblyIncluded(string assemblyName, bool included)
        {
            if (included == IsAssemblyIncluded(assemblyName)) return;

            if (included)
            {
                _assemblyNames.Add(assemblyName);
                _assemblyNames.Sort();
            }
            else
            {
                _assemblyNames.Remove(assemblyName);
            }

            Save();
        }

        private void Save()
        {
            InternalEditorUtility.SaveToSerializedFileAndForget(new Object[] { this }, SETTINGS_PATH, true);
        }

        private static ScriptableObjectCreatorSettings Load()
        {
            ScriptableObjectCreatorSettings settings = null;
            if (File.Exists(SETTINGS_PATH))
            {
                settings = InternalEditorUtility.LoadSerializedFileAndForget(SETTINGS_PATH)
                    .OfType<ScriptableObjectCreatorSettings>()
                    .FirstOrDefault();
            }

            if (settings == null)
            {
                settings = CreateInstance<ScriptableObjectCreatorSettings>();
                settings.Save();
            }

            settings.hideFlags = HideFlags.HideAndDontSave;
            return settings;
        }
    }
}