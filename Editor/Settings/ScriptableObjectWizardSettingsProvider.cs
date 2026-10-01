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
        private List<AssemblyEntry> _otherAssemblies;
        private Vector2 _scrollPosition;
        private string _search = string.Empty;
        private GUIStyle _searchFieldStyle;
        private bool _refocusSearchField;
        private bool _showOtherAssemblies;

        private ScriptableObjectWizardSettingsProvider()
            : base(SETTINGS_MENU_PATH, SettingsScope.Project, new[] { "ScriptableObject", "Assembly", "Wizard", "Type Picker" })
        {
        }

        [SettingsProvider]
        public static SettingsProvider Create()
        {
            return new ScriptableObjectWizardSettingsProvider();
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            var projectAssemblyNames = new HashSet<string>(CompilationPipeline.GetAssemblies().Select(a => a.name));

            List<AssemblyEntry> entries = ScriptableObjectFactory.GetCreatableTypes()
                .GroupBy(t => t.Assembly.GetName().Name)
                .Select(g => new AssemblyEntry { Name = g.Key, TypeCount = g.Count() })
                .OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            _projectAssemblies = entries.Where(e => projectAssemblyNames.Contains(e.Name)).ToList();
            _otherAssemblies = entries.Where(e => !projectAssemblyNames.Contains(e.Name)).ToList();
        }

        public override void OnGUI(string searchContext)
        {
            ScriptableObjectWizardSettings settings = ScriptableObjectWizardSettings.Instance;

            settings.TypePicker = (TypePickerStyle)EditorGUILayout.EnumPopup(
                new GUIContent("Type Picker", "How Assets > Create > ScriptableObject lets you choose the class to create."),
                settings.TypePicker);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "Select the assemblies whose ScriptableObject types are listed by Assets > Create > ScriptableObject.",
                MessageType.None);
            _search = DrawSearchField(_search);
            EditorGUILayout.Space();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.LabelField("Project Assemblies", EditorStyles.boldLabel);
            DrawAssemblyToggles(settings, _projectAssemblies);

            EditorGUILayout.Space();
            _showOtherAssemblies = EditorGUILayout.Foldout(_showOtherAssemblies,
                $"Unity & Precompiled Assemblies ({_otherAssemblies.Count})", true);
            if (_showOtherAssemblies)
            {
                DrawAssemblyToggles(settings, _otherAssemblies);
            }

            List<string> missing = settings.AssemblyNames
                .Where(n => _projectAssemblies.All(e => e.Name != n) && _otherAssemblies.All(e => e.Name != n))
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

        /// <summary>
        /// Draws a search field with the magnifier icon on the left and the clear button inside its right edge.
        /// </summary>
        private string DrawSearchField(string text)
        {
            GUIStyle fieldStyle = GUI.skin.FindStyle("SearchTextField") ?? EditorStyles.textField;
            GUIStyle cancelStyle = GUI.skin.FindStyle(text.Length > 0 ? "SearchCancelButton" : "SearchCancelButtonEmpty")
                                   ?? GUIStyle.none;

            Rect rect = GUILayoutUtility.GetRect(GUIContent.none, fieldStyle, GUILayout.ExpandWidth(true));
            float buttonWidth = cancelStyle.fixedWidth > 0 ? cancelStyle.fixedWidth : 14f;
            float buttonHeight = cancelStyle.fixedHeight > 0 ? cancelStyle.fixedHeight : rect.height;
            var buttonRect = new Rect(rect.xMax - buttonWidth - 2f, rect.y + (rect.height - buttonHeight) * 0.5f,
                buttonWidth, buttonHeight);

            Event current = Event.current;
            if (text.Length > 0 && current.type == EventType.MouseDown && buttonRect.Contains(current.mousePosition))
            {
                _refocusSearchField = GUI.GetNameOfFocusedControl() == SEARCH_FIELD_CONTROL_NAME;
                text = string.Empty;
                GUIUtility.keyboardControl = 0;
                current.Use();
                Repaint();
            }
            else if (_refocusSearchField && current.type == EventType.Layout)
            {
                _refocusSearchField = false;
                EditorGUI.FocusTextInControl(SEARCH_FIELD_CONTROL_NAME);
            }

            if (_searchFieldStyle == null || _searchFieldStyle.name != fieldStyle.name)
            {
                _searchFieldStyle = new GUIStyle(fieldStyle);
                _searchFieldStyle.padding.right += (int)buttonWidth + 2;
            }

            GUI.SetNextControlName(SEARCH_FIELD_CONTROL_NAME);
            text = EditorGUI.TextField(rect, text, _searchFieldStyle);

            if (current.type == EventType.Repaint)
            {
                cancelStyle.Draw(buttonRect, GUIContent.none, false, false, false, false);
            }

            return text;
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
