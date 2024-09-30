# Searchable Popup

* Searchable popups for your Inspector window fields using TreeView API
* Unity minimum version: **2019.3**
* Current version: **1.0.0**
* License: **MIT**

## Summary

You may have an Enum with multiples values and, when serializing it on Inspector, realizing how difficult is to select a value without a searching field.

This package helps on that by adding a Searchable field for this cases.

## How To Use

You can use the ```Searchable``` attribute with a list of ```strings```, ```Enum``` or a ```Dictionary<string, string>```. 

Also, you can set the ```wide``` property to ```true``` to display the popup using the entire Inspector width available.

```csharp
using UnityEngine;
using ActionCode.SearchablePopup;

public class TestSearchPopup : MonoBehaviour
{
    [Tooltip("Select a fruit.")]
    [Searchable("Açaí", "Apple", "Avocado", "Banana", "Blueberry", "Breadfruit", "Cherry", "Coconut", "Cranberry", "Egg Fruit", "Gooseberry", "Grape")]
    public string fruit = "Banana";

    [SerializeField]
    [Tooltip("Select a Platform.")]
    [Searchable(typeof(RuntimePlatform), wide: true)]
    private RuntimePlatform platform = RuntimePlatform.LinuxEditor;
}
```

Which will produce the following results in Inspector window:

![Showcase][showcase]

If a serialized value is not present into the searchable list, a text field will be displayed until the right value is inputed.

![Unlisted Showcase][showcase-unlisted]

## Installation

### Using the Package Registry Server

Follow the instructions inside [here](https://cutt.ly/ukvj1c8) and the package **ActionCode-Searchable Popup** 
will be available for you to install using the **Package Manager** windows.

### Using the Git URL

You will need a **Git client** installed on your computer with the Path variable already set. 

- Use the **Package Manager** "Add package from git URL..." feature and paste this URL: `https://github.com/HyagoOliveira/SearchablePopup.git`

- You can also manually modify you `Packages/manifest.json` file and add this line inside `dependencies` attribute: 

```json
"com.actioncode.searchable-popup":"https://github.com/HyagoOliveira/SearchablePopup.git"
```

---

**Hyago Oliveira**

[GitHub](https://github.com/HyagoOliveira) -
[BitBucket](https://bitbucket.org/HyagoGow/) -
[LinkedIn](https://www.linkedin.com/in/hyago-oliveira/) -
<hyagogow@gmail.com>

[showcase]: /Docs~/searchable-popup-showcase.gif "Searchable Popups"
[showcase-unlisted]: /Docs~/searchable-popup-showcase-unlisted.gif "Unlisted Searchable Popup"