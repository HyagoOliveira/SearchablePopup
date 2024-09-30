using UnityEditor.IMGUI.Controls;

namespace ActionCode.SearchablePopup.Editor
{
    public class SearchableTreeViewItem : TreeViewItem
    {
        /// <summary>
        /// Unique key to identify this item.
        /// </summary>
        public readonly string key;

        /// <summary>
        /// SearchableTreeViewItem constructor.
        /// </summary>
        /// <param name="id">Unique ID to identify this item.</param>
        /// <param name="depth">Depth of this item.</param>
        /// <param name="displayName">Rendered name of this item.</param>
        /// <param name="key">Unique key to identify this item.</param>
        public SearchableTreeViewItem(int id, int depth, string displayName, string key) :
            base(id, depth, displayName)
        {
            this.key = key;
        }
    }
}
