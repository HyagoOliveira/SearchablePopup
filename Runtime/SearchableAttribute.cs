using System;
using UnityEngine;
using System.Collections.Generic;

namespace ActionCode.SearchablePopup
{
    /// <summary>
    /// Use a Searchable Popup for a field in the Inspector window.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class SearchableAttribute : PropertyAttribute
    {
        /// <summary>
        /// Whether the search popup area should be wide.
        /// </summary>
        public readonly bool wide;

        /// <summary>
        /// A list of options to search.
        /// </summary>
        public readonly Dictionary<string, string> options;

        /// <summary>
        /// Use a Searchable Popup for a field.
        /// </summary>
        /// <param name="enumType">Enum type to search.</param>
        /// <param name="wide">Whether the search popup area should be wide.</param>
        public SearchableAttribute(Type enumType, bool wide = false) :
            this(Enum.GetNames(enumType))
        {
            this.wide = wide;
        }

        /// <summary>
        /// Use a Searchable Popup for a field.
        /// </summary>
        /// <param name="wide">Whether the search popup area should be wide.</param>
        /// <param name="options">A list of options to search.</param>
        public SearchableAttribute(bool wide, params string[] options) : this(options)
        {
            this.wide = wide;
        }

        /// <summary>
        /// Use a Searchable Popup for a field.
        /// </summary>
        /// <param name="options">A list of options to search.</param>
        public SearchableAttribute(params string[] options)
        {
            wide = false;
            this.options = new Dictionary<string, string>(options.Length);
            foreach (var entry in options)
            {
                AddOption(entry, entry);
            }
        }

        /// <summary>
        /// Use a Searchable Popup for a field.
        /// </summary>
        /// <param name="options">A key/value pair dictionary containing all options to search.</param>
        /// <param name="wide">Whether the search popup area should be wide.</param>
        public SearchableAttribute(Dictionary<string, string> options, bool wide = false)
        {
            this.wide = wide;
            this.options = options;
        }

        private void AddOption(string key, string value)
        {
            var canAdd = !options.ContainsKey(key);
            if (canAdd) options.Add(key, value);
        }
    }
}
