using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;

namespace ScriptableObjectWizard.Settings
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
    /// Project-wide settings for the wizard, stored in ProjectSettings/ScriptableObjectWizardSettings.asset.
    /// </summary>
    public class ScriptableObjectWizardSettings : ScriptableObject
    {
        private const string SettingsPath = "ProjectSettings/ScriptableObjectWizardSettings.asset";

        private static ScriptableObjectWizardSettings _instance;

        [SerializeField]
        private List<string> _assemblyNames = new List<string> { "Assembly-CSharp" };

        [SerializeField]
        private TypePickerStyle _typePicker = TypePickerStyle.Dropdown;

        public static ScriptableObjectWizardSettings Instance
        {
            get
            {
                if (_instance == null) _instance = Load();
                return _instance;
            }
        }

        /// <summary>
        /// Names of the assemblies whose ScriptableObject types are listed by the wizard.
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

        public void Save()
        {
            InternalEditorUtility.SaveToSerializedFileAndForget(new Object[] { this }, SettingsPath, true);
        }

        private static ScriptableObjectWizardSettings Load()
        {
            ScriptableObjectWizardSettings settings = null;
            if (File.Exists(SettingsPath))
            {
                settings = InternalEditorUtility.LoadSerializedFileAndForget(SettingsPath)
                    .OfType<ScriptableObjectWizardSettings>()
                    .FirstOrDefault();
            }

            if (settings == null)
            {
                settings = CreateInstance<ScriptableObjectWizardSettings>();
                settings.Save();
            }

            settings.hideFlags = HideFlags.HideAndDontSave;
            return settings;
        }
    }
}
