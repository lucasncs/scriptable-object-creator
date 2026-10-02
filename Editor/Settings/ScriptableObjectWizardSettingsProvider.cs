using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.UIElements;

namespace ScriptableObjectWizard.Settings
{
    /// <summary>
    /// Draws the wizard settings under Edit > Project Settings > Scriptable Object Wizard.
    /// </summary>
    internal class ScriptableObjectWizardSettingsProvider : SettingsProvider
    {
        private const string SETTINGS_MENU_PATH = "Project/Scriptable Object Wizard";
        private const string SEARCH_FIELD_CONTROL_NAME = "ScriptableObjectWizardAssemblySearch";

        private struct AssemblyEntry
        {
            public string Name;
            public int TypeCount;
        }

        private List<AssemblyEntry> _projectAssemblies;
        private List<AssemblyEntry> _unityAssemblies;
        private Vector2 _scrollPosition;
        private string _search = string.Empty;
        private readonly InlineSearchField _searchField;
        private bool _showUnityAssemblies;

        private ScriptableObjectWizardSettingsProvider()
            : base(SETTINGS_MENU_PATH, SettingsScope.Project,
                new[] { "ScriptableObject", "Assembly", "Wizard", "Type Picker", "Auto Close Window" })
        {
            _searchField = new InlineSearchField(SEARCH_FIELD_CONTROL_NAME, Repaint);
        }

        [SettingsProvider]
        public static SettingsProvider Create()
        {
            return new ScriptableObjectWizardSettingsProvider();
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            // Assemblies compiled from Unity's own packages are listed with the Unity assemblies, not the project's.
            var projectAssemblyNames = new HashSet<string>(CompilationPipeline.GetAssemblies()
                .Select(a => a.name)
                .Where(name => !IsUnityPackageAssembly(name)));

            List<AssemblyEntry> entries = ScriptableObjectFactory.GetCreatableTypes()
                .GroupBy(t => t.Assembly.GetName().Name)
                .Select(g => new AssemblyEntry { Name = g.Key, TypeCount = g.Count() })
                .OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            _projectAssemblies = entries.Where(e => projectAssemblyNames.Contains(e.Name)).ToList();
            _unityAssemblies = entries.Where(e => !projectAssemblyNames.Contains(e.Name)).ToList();
        }

        private static bool IsUnityPackageAssembly(string assemblyName)
        {
            string asmdefPath = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(assemblyName);
            return asmdefPath != null &&
                   asmdefPath.StartsWith("Packages/com.unity.", StringComparison.OrdinalIgnoreCase);
        }

        public override void OnGUI(string searchContext)
        {
            var settings = ScriptableObjectWizardSettings.Instance;

            settings.TypePicker = (TypePickerStyle)EditorGUILayout.EnumPopup(
                new GUIContent("Type Picker",
                    "How Assets > Create > ScriptableObject lets you choose the class to create."),
                settings.TypePicker);

            settings.AutoCloseWindow = EditorGUILayout.Toggle(
                new GUIContent("Auto Close Window",
                    "Close the picker window after creating an asset. When enabled the window floats as a utility " +
                    "window; when disabled it opens as a regular window that can be docked and stays open."),
                settings.AutoCloseWindow);

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "Select the assemblies whose ScriptableObject types are listed by Assets > Create > ScriptableObject.",
                MessageType.None);
            _search = _searchField.OnGUI(_search);
            EditorGUILayout.Space();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.LabelField("Project Assemblies", EditorStyles.boldLabel);
            DrawAssemblyToggles(settings, _projectAssemblies);

            EditorGUILayout.Space();
            _showUnityAssemblies = EditorGUILayout.Foldout(_showUnityAssemblies,
                $"Unity & Precompiled Assemblies ({_unityAssemblies.Count})", true);
            if (_showUnityAssemblies)
            {
                DrawAssemblyToggles(settings, _unityAssemblies);
            }

            List<string> missing = settings.AssemblyNames
                .Where(n => _projectAssemblies.All(e => e.Name != n) && _unityAssemblies.All(e => e.Name != n))
                .ToList();
            if (missing.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Missing Assemblies", EditorStyles.boldLabel);
                foreach (string assemblyName in missing)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(assemblyName, "No ScriptableObject types found");
                        if (GUILayout.Button("Remove", GUILayout.Width(70)))
                        {
                            settings.SetAssemblyIncluded(assemblyName, false);
                        }
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawAssemblyToggles(ScriptableObjectWizardSettings settings, List<AssemblyEntry> entries)
        {
            using (new EditorGUI.IndentLevelScope())
            {
                foreach (AssemblyEntry entry in entries)
                {
                    if (_search.Length > 0 && entry.Name.IndexOf(_search, StringComparison.OrdinalIgnoreCase) < 0)
                        continue;

                    bool included = settings.IsAssemblyIncluded(entry.Name);
                    var label = new GUIContent($"{entry.Name}  ({entry.TypeCount})");
                    bool toggled = EditorGUILayout.ToggleLeft(label, included);
                    if (toggled != included)
                    {
                        settings.SetAssemblyIncluded(entry.Name, toggled);
                    }
                }
            }
        }
    }
}