using System;
using UnityEditor;
using UnityEngine;

namespace ScriptableObjectCreator
{
    /// <summary>
    /// Base for the windows that let the user pick a ScriptableObject class and create an asset of it.
    /// </summary>
    public abstract class ATypePickerWindow : EditorWindow
    {
        [SerializeField] private bool _autoClose;

        /// <summary>
        /// The classes the user can pick from.
        /// </summary>
        protected Type[] Types { get; private set; }

        /// <summary>
        /// Opens the window, as a utility window that closes after creating an asset when <paramref name="autoClose"/>
        /// is true, or as a regular window that stays open otherwise.
        /// </summary>
        internal static void Open<T>(Type[] types, bool autoClose) where T : ATypePickerWindow
        {
            var window = GetWindow<T>(autoClose, Constants.DISPLAY_NAME, true);
            window._autoClose = autoClose;
            window.SetTypes(types);
        }

        /// <summary>
        /// Called when <see cref="Types"/> is set, so the window can rebuild its picker.
        /// </summary>
        protected abstract void OnTypesChanged();

        /// <summary>
        /// Draws the picker. <see cref="Types"/> is always set when this is called.
        /// </summary>
        protected abstract void DrawContent();

        protected void CreateAsset(Type type)
        {
            ScriptableObjectFactory.StartCreatingAsset(type);
            if (!_autoClose) return;

            Close();
            GUIUtility.ExitGUI();
        }

        private void OnGUI()
        {
            // The type list does not survive a domain reload, so find it again; close if nothing is left to create.
            if (Types == null)
            {
                Type[] types = ScriptableObjectFactory.FindIncludedTypes();
                if (types.Length == 0)
                {
                    Close();
                    GUIUtility.ExitGUI();
                }

                SetTypes(types);
            }

            DrawContent();
        }

        private void SetTypes(Type[] types)
        {
            Types = types;
            OnTypesChanged();
        }
    }
}