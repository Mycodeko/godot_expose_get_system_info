# Expose get_system_info
Exposes the internal method (`EditorNode::_get_system_info()`) used by Godot to create the System Info string from the editor UI at `Help - Copy System Info`.

## Usage
### Installation
1. Download the source code zip archive [here](https://github.com/Mycodeko/godot_expose_get_system_info/releases/latest).
2. Extract the `expose_get_system_info` folder in `godot_expose_get_system_info-X.X.X/addons` into `<PROJECT_DIRECTORY>/addons` (should result in `<PROJECT_DIRECTORY>/addons/expose_get_system_info`).

### C#
- Call ```csharp SystemInfo.GetSystemInfo()```.

### GDScript
- Add ```gdscript const SystemInfo = preload("res://addons/expose_get_system_info/get_system_info.gd")``` to the top of your script's file, then call ```gdscript SystemInfo.get_system_info()```