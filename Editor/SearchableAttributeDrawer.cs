using UnityEditor;
using UnityEngine;

namespace ActionCode.SearchablePopup.Editor
{
    /// <summary>
    /// Custom drawer for <see cref="SearchableAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(SearchableAttribute))]
    public sealed class SearchableAttributeDrawer : PropertyDrawer
    {
        private SearchablePopupWindow popupWindow;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (popupWindow == null)
            {
                popupWindow = new SearchablePopupWindow(attribute as SearchableAttribute);
            }
            popupWindow.OnGUI(position, property, label);
        }
    }
}
