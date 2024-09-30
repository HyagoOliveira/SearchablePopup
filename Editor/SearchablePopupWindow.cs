using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace ActionCode.SearchablePopup.Editor
{
    /// <summary>
    /// A popup search window with auto-complete.
    /// </summary>
    public sealed class SearchablePopupWindow : PopupWindowContent
    {
        /// <summary>
        /// Whether the item search popup area should be wide.
        /// </summary>
        public bool Wide { get; set; }

        /// <summary>
        /// The OnGUI Serialized Property.
        /// </summary>
        public SerializedProperty Property { get; private set; }

        /// <summary>
        /// Action executed when a confirmation is made.
        /// </summary>
        public Action<SearchableTreeViewItem> OnConfirmSelection { get; set; }

        /// <summary>
        /// Action executed when the search value is changed.
        /// </summary>
        public Action<string> OnChangeSearch { get; set; }

        /// <summary>
        /// Whether the search popup is open.
        /// </summary>
        public bool IsOpen { get; private set; }

        /// <summary>
        /// Current search value.
        /// </summary>
        public string SearchValue => searchableView?.searchString;

        private Vector2 size;
        private SearchableTreeView searchableView;

        private readonly Dictionary<string, string> options;

        private const string SESSION_STATE_KEY_PREFIX = "SPW";

        /// <summary>
        /// Creates a Searchable popup window using the given options dictionary.
        /// </summary>
        /// <param name="options">Options dictionary to search.</param>
        public SearchablePopupWindow(Dictionary<string, string> options)
        {
            this.options = options;
            size = base.GetWindowSize();
        }

        /// <summary>
        /// Creates a Searchable popup window using the given searchable attribute.
        /// </summary>
        /// <param name="attribute">Searchable attribute.</param>
        public SearchablePopupWindow(SearchableAttribute attribute) : this(attribute.options)
        {
            Wide = attribute.wide;
        }

        /// <summary>
        /// Creates a Searchable popup window with no options.
        /// </summary>
        public SearchablePopupWindow() : this(new Dictionary<string, string>())
        {
        }

        /// <summary>
        /// Adds the given values to searchable options.
        /// </summary>
        /// <param name="values">A list of texts values.</param>
        public void AddOptions(params string[] values)
        {
            foreach (var value in values)
            {
                AddOption(value);
            }
        }

        /// <summary>
        /// Adds the given options to searchable options.
        /// </summary>
        /// <param name="options">A key/value pair dictionary containing all options to search.</param>
        public void AddOptions(Dictionary<string, string> options)
        {
            foreach (var option in options)
            {
                AddOption(option.Key, option.Value);
            }
        }

        /// <summary>
        /// Adds the given value to searchable options.
        /// </summary>
        /// <param name="value">A text value.</param>
        public void AddOption(string value)
        {
            AddOption(value, value);
        }

        /// <summary>
        /// Adds the given key and value to searchable options.
        /// </summary>
        /// <param name="key">The key from this option.</param>
        /// <param name="value">A text value.</param>
        public void AddOption(string key, string value)
        {
            var canAdd = !options.ContainsKey(key);
            if (canAdd)
            {
                options.Add(key, value);
                if (searchableView != null)
                {
                    searchableView.AddItem(key, value);
                }
            }
        }

        /// <summary>
        /// Clears all the entries in searchable options.
        /// </summary>
        public void ClearOptions()
        {
            options.Clear();
        }

        /// <summary>
        /// Whether has options.
        /// </summary>
        /// <returns></returns>
        public bool HasOptions() => options.Count > 0;

        /// <summary>
        /// The size of the popup window.
        /// </summary>
        /// <returns>The size of the popup window.</returns>
        public override Vector2 GetWindowSize()
        {
            return size;
        }

        /// <summary>
        /// Callback when the popup window is opened.
        /// </summary>
        public override void OnOpen()
        {
            base.OnOpen();
            IsOpen = true;
        }

        /// <summary>
        /// Callback when the popup window is closed.
        /// </summary>
        public override void OnClose()
        {
            base.OnClose();
            IsOpen = false;
        }

        /// <summary>
        /// Callback for drawing GUI controls for the popup window.
        /// </summary>
        /// <param name="position">The rectangle to draw the GUI inside.</param>
        public override void OnGUI(Rect position)
        {
            searchableView.OnGUI(position);
            if (searchableView.HasEnterSelection) Close();
        }

        /// <summary>
        /// Draws the auto-complete searchable field.
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the property GUI.</param>
        /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
        /// <param name="label">The label of this property.</param>
        public void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Property = property;

            EditorGUI.BeginProperty(position, label, property);
            var fieldPosition = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            var openSearchPopup = DisplayField(fieldPosition);
            EditorGUI.EndProperty();

            if (openSearchPopup)
            {
                CreateSearchableView();
                var searchArea = Wide ? position : fieldPosition;
                size.x = searchArea.width;
                PopupWindow.Show(searchArea, this);
            }
        }

        /// <summary>
        /// Closes the popup.
        /// </summary>
        public void Close()
        {
            GUIUtility.hotControl = 0;
            editorWindow.Close();
            SaveSearchableState();
        }

        private void CreateSearchableView()
        {
            var searchableState = LoadSearchableState(GetAssetId());

            searchableView = new SearchableTreeView(searchableState, options);
            searchableView.OnChangeSearch += OnSearchChange;
            searchableView.OnConfirmSelection += OnSelectionConfirm;
        }

        private void SaveSearchableState()
        {
            SaveSearchableState(GetAssetId(), searchableView.state);
        }

        private string GetAssetId()
        {
            var id = Property != null ? $"{Property.serializedObject.targetObject}-{Property.displayName}" : string.Empty;
            return id;
        }

        private void OnSearchChange(string value)
        {
            OnChangeSearch?.Invoke(value);
            SaveSearchableState();
        }

        private void OnSelectionConfirm(SearchableTreeViewItem selectedItem)
        {
            if (Property != null)
            {
                ApplyPropertyValue(Property, selectedItem.key);
            }

            OnConfirmSelection?.Invoke(selectedItem);
            SaveSearchableState();
        }

        private bool DisplayField(Rect position)
        {
            var value = GetPropertyValue(out bool drawDropdown);
            if (drawDropdown)
            {
                return EditorGUI.DropdownButton(position, new GUIContent(value), FocusType.Passive);
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                value = EditorGUI.TextField(position, value);
                if (EditorGUI.EndChangeCheck()) ApplyPropertyValue(value);
            }

            return false;
        }

        private string GetPropertyValue(out bool drawDropdown)
        {
            drawDropdown = true;
            if (Property == null) return string.Empty;

            var key = GetPropertyValue(Property);
            var containsKey = options.ContainsKey(key);
            drawDropdown = string.IsNullOrEmpty(key) || containsKey;

            return containsKey ? options[key] : key;
        }

        private void ApplyPropertyValue(string value)
        {
            if (Property == null) return;
            ApplyPropertyValue(Property, value);
        }

        private static string GetPropertyValue(SerializedProperty property)
        {
            var value = string.Empty;

            if (property.propertyType == SerializedPropertyType.String)
            {
                value = property.stringValue;
            }
            else if (property.propertyType == SerializedPropertyType.Enum)
            {
                value = property.enumNames[property.enumValueIndex];
            }

            return value;
        }

        private static void ApplyPropertyValue(SerializedProperty property, string value)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                property.stringValue = value;
            }
            else if (property.propertyType == SerializedPropertyType.Enum)
            {
                var enumNames = property.enumNames;
                for (int i = 0; i < enumNames.Length; i++)
                {
                    if (enumNames[i].Equals(value))
                    {
                        property.enumValueIndex = i;
                        break;
                    }
                }
            }

            property.serializedObject.ApplyModifiedProperties();
        }

        private static TreeViewState LoadSearchableState(string id)
        {
            var treeViewState = new TreeViewState();
            var jsonState = SessionState.GetString(SESSION_STATE_KEY_PREFIX + id, "");
            var hasSavedJson = !string.IsNullOrEmpty(jsonState);
            if (hasSavedJson) JsonUtility.FromJsonOverwrite(jsonState, treeViewState);

            return treeViewState;
        }

        private static void SaveSearchableState(string id, TreeViewState state)
        {
            SessionState.SetString(SESSION_STATE_KEY_PREFIX + id, JsonUtility.ToJson(state));
        }
    }
}
