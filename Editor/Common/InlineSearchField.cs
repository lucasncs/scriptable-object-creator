using System;
using UnityEditor;
using UnityEngine;

namespace ScriptableObjectWizard
{
    /// <summary>
    /// A search field with the magnifier icon on the left and the clear button inside its right edge.
    /// </summary>
    internal class InlineSearchField
    {
        private readonly string _controlName;
        private readonly Action _repaint;
        private GUIStyle _fieldStyle;
        private bool _refocus;

        public InlineSearchField(string controlName, Action repaint)
        {
            _controlName = controlName;
            _repaint = repaint;
        }

        public bool HasFocus => GUI.GetNameOfFocusedControl() == _controlName;

        /// <summary>
        /// Gives keyboard focus to the field. Call from OnGUI before the field is drawn.
        /// </summary>
        public void SetFocus()
        {
            EditorGUI.FocusTextInControl(_controlName);
        }

        public string OnGUI(string text)
        {
            GUIStyle baseStyle = GUI.skin.FindStyle("SearchTextField") ?? EditorStyles.textField;
            GUIStyle cancelStyle =
                GUI.skin.FindStyle(text.Length > 0 ? "SearchCancelButton" : "SearchCancelButtonEmpty")
                ?? GUIStyle.none;

            Rect rect = GUILayoutUtility.GetRect(GUIContent.none, baseStyle, GUILayout.ExpandWidth(true));
            float buttonWidth = cancelStyle.fixedWidth > 0 ? cancelStyle.fixedWidth : 14f;
            float buttonHeight = cancelStyle.fixedHeight > 0 ? cancelStyle.fixedHeight : rect.height;
            var buttonRect = new Rect(rect.xMax - buttonWidth - 2f, rect.y + (rect.height - buttonHeight) * 0.5f,
                buttonWidth, buttonHeight);

            // Handle the clear click before the text field, which would otherwise take the mouse event.
            Event current = Event.current;
            if (text.Length > 0 && current.type == EventType.MouseDown && buttonRect.Contains(current.mousePosition))
            {
                // A focused field keeps its own edit buffer, so drop focus to apply the clear and restore it next pass.
                _refocus = HasFocus;
                text = string.Empty;
                GUIUtility.keyboardControl = 0;
                current.Use();
                _repaint();
            }
            else if (_refocus && current.type == EventType.Layout)
            {
                _refocus = false;
                SetFocus();
            }

            if (_fieldStyle == null || _fieldStyle.name != baseStyle.name)
            {
                _fieldStyle = new GUIStyle(baseStyle);
                _fieldStyle.padding.right += (int)buttonWidth + 2;
            }

            GUI.SetNextControlName(_controlName);
            text = EditorGUI.TextField(rect, text, _fieldStyle);

            if (current.type == EventType.Repaint)
            {
                cancelStyle.Draw(buttonRect, GUIContent.none, false, false, false, false);
            }

            return text;
        }
    }
}