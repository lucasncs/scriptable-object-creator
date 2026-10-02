# Changelog

All notable changes to this package are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-10-02

### Added

- **Assets > Create > ScriptableObject** menu item that opens a picker for creating an asset of any ScriptableObject class, without a `[CreateAssetMenu]` attribute on each class.
- Direct creation: with a ScriptableObject script selected in the Project window, the menu item creates an instance of that class without opening the picker.
- Two type pickers, both searchable and grouped by namespace:
  - **Dropdown**, a popup like Unity's *Add Component* menu.
  - **Tree View**, a tree of namespaces and classes with keyboard navigation.
- Project settings under **Edit > Project Settings > ScriptableObject Creator**:
  - **Type Picker** to choose the picker.
  - **Auto Close Window** to close the picker after creating an asset, or keep it open as a dockable window.
  - The assemblies whose classes are listed, split into project assemblies and Unity or precompiled ones.
