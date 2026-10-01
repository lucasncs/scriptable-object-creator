using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ScriptableObjectWizard
{
    /// <summary>
    /// Searchable dropdown listing ScriptableObject types grouped by namespace.
    /// </summary>
    internal class ScriptableObjectTypeDropdown : AdvancedDropdown
    {
        private readonly Type[] _types;
        private readonly Action<int> _onTypeSelected;

        public ScriptableObjectTypeDropdown(AdvancedDropdownState state, Type[] types, Action<int> onTypeSelected)
            : base(state)
        {
            _types = types;
            _onTypeSelected = onTypeSelected;
            minimumSize = new Vector2(250f, 300f);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("ScriptableObject");
            var groups = new Dictionary<string, AdvancedDropdownItem>();

            IEnumerable<int> sortedIndices = Enumerable.Range(0, _types.Length)
                .OrderBy(i => _types[i].FullName, StringComparer.OrdinalIgnoreCase);
            foreach (int index in sortedIndices)
            {
                Type type = _types[index];
                AdvancedDropdownItem parent = GetOrCreateGroup(root, groups, type.Namespace);
                parent.AddChild(new AdvancedDropdownItem(type.Name) { id = index });
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            _onTypeSelected(item.id);
        }

        private static AdvancedDropdownItem GetOrCreateGroup(AdvancedDropdownItem root,
            Dictionary<string, AdvancedDropdownItem> groups, string typeNamespace)
        {
            if (string.IsNullOrEmpty(typeNamespace)) return root;

            AdvancedDropdownItem parent = root;
            string path = null;
            foreach (string level in typeNamespace.Split('.'))
            {
                path = path == null ? level : $"{path}.{level}";
                if (!groups.TryGetValue(path, out AdvancedDropdownItem group))
                {
                    group = new AdvancedDropdownItem(level);
                    parent.AddChild(group);
                    groups.Add(path, group);
                }

                parent = group;
            }

            return parent;
        }
    }
}
