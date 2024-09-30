using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;

namespace ActionCode.SearchablePopup.Editor
{
    public sealed class SearchableTreeView : TreeView
    {
        /// <summary>
        /// True if a confirmation is made.
        /// </summary>
        public bool HasEnterSelection { get; private set; }

        /// <summary>
        /// The last selected item. It going to be null if none.
        /// </summary>
        public SearchableTreeViewItem LastSelectedItem { get; private set; }

        /// <summary>
        /// Action executed when a confirmation is made.
        /// </summary>
        public Action<SearchableTreeViewItem> OnConfirmSelection { get; set; }

        /// <summary>
        /// Action executed when the search value is changed.
        /// </summary>
        public Action<string> OnChangeSearch { get; set; }

        private bool hasEmptyItem;

        private readonly TreeViewItem root;
        private readonly SearchField searchField;
        private readonly GUIStyle mouseHoverStyle;

        private const int DEFAULT_DEPTH = 0;
        private const int EMPTY_ITEM_ID = 0;

        /// <summary>
        /// Creates a searchable tree view using the given state.
        /// </summary>
        /// <param name="serializableState">TreeView state (expanded items, selection etc).</param>
        public SearchableTreeView(TreeViewState serializableState) :
            base(serializableState)
        {
            searchField = new SearchField();
            mouseHoverStyle = new GUIStyle("selectionRect");
            root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };

            showBorder = true;
            showAlternatingRowBackgrounds = true;

            searchField.SetFocus();
            searchField.downOrUpArrowKeyPressed += SetFocusAndEnsureSelectedItem;

            AddEmptyItem();
        }

        /// <summary>
        /// Creates a searchable tree view using the given state and option dictionary.
        /// </summary>
        /// <param name="serializableState">TreeView state (expanded items, selection etc).</param>
        /// <param name="options">An option dictionary.</param>
        public SearchableTreeView(TreeViewState serializableState, Dictionary<string, string> options) :
            this(serializableState)
        {
            AddItems(options);
        }

        /// <summary>
        /// Creates a searchable tree view using the given option dictionary.
        /// </summary>
        /// <param name="options">An option dictionary.</param>
        public SearchableTreeView(Dictionary<string, string> options) :
            this(new TreeViewState(), options)
        {
        }

        /// <summary>
        /// Adds a new item to the TreeView and reloads it.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="displayName"></param>
        public void AddItem(string key, string displayName)
        {
            RemoveEmptyItemIfAny();
            var index = root.hasChildren ? root.children.Count : 0;
            root.AddChild(new SearchableTreeViewItem(++index, DEFAULT_DEPTH, displayName, key));
            Reload();
        }

        /// <summary>
        /// Adds news items to the TreeView and reloads it.
        /// </summary>
        /// <param name="options">An option dictionary.</param>
        public void AddItems(Dictionary<string, string> options)
        {
            RemoveEmptyItemIfAny();
            var index = root.hasChildren ? root.children.Count : 0;
            foreach (var option in options)
            {
                var key = option.Key;
                var displayName = option.Value;
                root.AddChild(new SearchableTreeViewItem(++index, DEFAULT_DEPTH, displayName, key));
            }
            Reload();
        }

        /// <summary>
        /// Clears the TreeView and reloads it. 
        /// </summary>
        public void Clear()
        {
            root.children.Clear();
            Reload();
        }

        /// <summary>
        /// This is the overridden GUI method of the TreeView, where the <see cref="SearchableTreeViewItem"/> are processed and drawn.
        /// </summary>
        /// <param name="position">Position where the TreeView is rendered</param>
        public override void OnGUI(Rect position)
        {
            const int border = 4;
            const int topPadding = 12;
            const int searchHeight = 20;
            const int remainTop = topPadding + searchHeight + border;
            var searchPosition = new Rect(border, topPadding, position.width - border * 2, searchHeight);
            var itemsPosition = new Rect(border, topPadding + searchHeight + border, position.width - border * 2, position.height - remainTop - border);

            EditorGUI.BeginChangeCheck();
            searchString = searchField.OnGUI(searchPosition, searchString);
            var hasChangedSearch = EditorGUI.EndChangeCheck();

            base.OnGUI(itemsPosition);

            if (hasChangedSearch) OnSearchChange();

            HandleKeyboard();
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            base.RowGUI(args);
            HandleMouseHovering(args.rowRect);
        }

        protected override TreeViewItem BuildRoot()
        {
            SetupDepthsFromParentsAndChildren(root);
            return root;
        }

        protected override bool CanMultiSelect(TreeViewItem item) => false;

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            // Just one selection can be made since CanMultiSelect is false.
            var index = selectedIds[0];
            LastSelectedItem = FindItem(index, root) as SearchableTreeViewItem;
            var hasSelection = LastSelectedItem != null;

            if (hasSelection)
            {
                if (IsMouseSelection()) HasEnterSelection = true;
                OnSelectionConfirm();
            }

            base.SelectionChanged(selectedIds);
        }

        private void RemoveEmptyItemIfAny()
        {
            if (hasEmptyItem) RemoveEmptyItem();
        }

        private void AddEmptyItem()
        {
            root.AddChild(new TreeViewItem(EMPTY_ITEM_ID, DEFAULT_DEPTH, string.Empty));
            hasEmptyItem = true;
            Reload();
        }

        private void RemoveEmptyItem()
        {
            root.children.RemoveAt(EMPTY_ITEM_ID);
            hasEmptyItem = false;
        }

        private void HandleKeyboard()
        {
            var currentEvent = Event.current;
            var hitFinishKey =
                currentEvent.keyCode == KeyCode.Return ||
                currentEvent.keyCode == KeyCode.Escape ||
                currentEvent.keyCode == KeyCode.KeypadEnter;
            var wasKeyboardSelection = currentEvent.type == EventType.KeyUp && hitFinishKey;

            var confirmSelection = wasKeyboardSelection && LastSelectedItem != null;
            if (confirmSelection)
            {
                HasEnterSelection = true;
                OnSelectionConfirm();
            }
        }

        private void HandleMouseHovering(Rect position)
        {
            var currentEvent = Event.current;
            if (currentEvent.type == EventType.Repaint)
            {
                var isMouseEnterResult = position.Contains(currentEvent.mousePosition);
                if (isMouseEnterResult)
                {
                    mouseHoverStyle.Draw(position, isHover: true, isActive: false, on: false, hasKeyboardFocus: false);
                    RepaintFocusedWindow();
                }
            }
        }

        private void OnSearchChange()
        {
            OnChangeSearch?.Invoke(searchString);
        }

        private void OnSelectionConfirm()
        {
            OnConfirmSelection?.Invoke(LastSelectedItem);
        }

        private static bool IsMouseSelection()
        {
            return Event.current.type == EventType.MouseDown;
        }

        private static void RepaintFocusedWindow()
        {
            if (EditorWindow.focusedWindow != null)
            {
                EditorWindow.focusedWindow.Repaint();
            }
        }
    }
}
