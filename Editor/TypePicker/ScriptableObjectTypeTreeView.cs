using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ScriptableObjectCreator
{
    /// <summary>
    /// Tree of ScriptableObject types grouped by namespace. Searching shows a flat list of matching types.
    /// </summary>
    internal class ScriptableObjectTypeTreeView : TreeView
    {
        private readonly Type[] _types;
        private readonly Action<Type> _onTypeConfirmed;
        private readonly Texture2D _namespaceIcon;
        private readonly Texture2D _typeIcon;
        private GUIStyle _namespaceStyle;

        public ScriptableObjectTypeTreeView(TreeViewState state, Type[] types, Action<Type> onTypeConfirmed)
            : base(state)
        {
            _types = types;
            _onTypeConfirmed = onTypeConfirmed;
            _namespaceIcon = EditorGUIUtility.IconContent("Folder Icon").image as Texture2D;
            _typeIcon = EditorGUIUtility.IconContent("ScriptableObject Icon").image as Texture2D;
            showBorder = true;
            Reload();
            ExpandAll();
        }

        /// <summary>
        /// The selected type, or null when nothing or a namespace is selected.
        /// </summary>
        public Type SelectedType
        {
            get
            {
                IList<int> selection = GetSelection();
                return selection.Count == 1 ? TypeFromId(selection[0]) : null;
            }
        }

        /// <summary>
        /// Selects the first type among the visible rows, such as the first search result.
        /// </summary>
        public void SelectFirstType()
        {
            TreeViewItem first = GetRows().FirstOrDefault(row => TypeFromId(row.id) != null);
            if (first != null) SetSelection(new[] { first.id }, TreeViewSelectionOptions.RevealAndFrame);
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem(0, -1, "Root") { children = new List<TreeViewItem>() };
            var groups = new Dictionary<string, TreeViewItem>();
            int nextGroupId = -1;

            // Types use positive ids (index + 1) and namespaces negative ones, so the two never collide.
            for (int i = 0; i < _types.Length; i++)
            {
                TreeViewItem parent = GetOrCreateGroup(root, groups, _types[i].Namespace, ref nextGroupId);
                parent.AddChild(new TreeViewItem(i + 1) { displayName = _types[i].Name, icon = _typeIcon });
            }

            SortChildren(root);
            SetupDepthsFromParentsAndChildren(root);
            return root;
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override bool DoesItemMatchSearch(TreeViewItem item, string search)
        {
            return TypeFromId(item.id) != null
                   && item.displayName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            base.RowGUI(args);

            // Search results lose their tree position, so show each type's namespace beside it.
            Type type = TypeFromId(args.item.id);
            if (!hasSearch || type == null || string.IsNullOrEmpty(type.Namespace)) return;

            if (_namespaceStyle == null)
            {
                _namespaceStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
                    { alignment = TextAnchor.MiddleRight };
            }

            Rect rect = args.rowRect;
            rect.xMax -= 4f;
            GUI.Label(rect, type.Namespace, _namespaceStyle);
        }

        protected override void DoubleClickedItem(int id)
        {
            Type type = TypeFromId(id);
            if (type != null)
            {
                _onTypeConfirmed(type);
            }
            else
            {
                SetExpanded(id, !IsExpanded(id));
            }
        }

        protected override void KeyEvent()
        {
            Event current = Event.current;
            if (current.type != EventType.KeyDown) return;
            if (current.keyCode != KeyCode.Return && current.keyCode != KeyCode.KeypadEnter) return;

            Type type = SelectedType;
            if (type == null) return;

            current.Use();
            _onTypeConfirmed(type);
        }

        private Type TypeFromId(int id)
        {
            return id > 0 && id <= _types.Length ? _types[id - 1] : null;
        }

        private TreeViewItem GetOrCreateGroup(TreeViewItem root, Dictionary<string, TreeViewItem> groups,
            string typeNamespace, ref int nextGroupId)
        {
            if (string.IsNullOrEmpty(typeNamespace)) return root;

            TreeViewItem parent = root;
            string path = null;
            foreach (string level in typeNamespace.Split('.'))
            {
                path = path == null ? level : $"{path}.{level}";
                if (!groups.TryGetValue(path, out TreeViewItem group))
                {
                    group = new TreeViewItem(nextGroupId--) { displayName = level, icon = _namespaceIcon };
                    parent.AddChild(group);
                    groups.Add(path, group);
                }

                parent = group;
            }

            return parent;
        }

        /// <summary>
        /// Sorts namespaces before types, each alphabetically, at every level of the tree.
        /// </summary>
        private static void SortChildren(TreeViewItem item)
        {
            if (!item.hasChildren) return;

            item.children = item.children
                .OrderBy(child => child.id > 0)
                .ThenBy(child => child.displayName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (TreeViewItem child in item.children)
            {
                SortChildren(child);
            }
        }
    }
}