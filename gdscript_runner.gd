extends RichTextLabel

const SystemInfo = preload("res://addons/expose_get_system_info/get_system_info.gd")

func _ready() -> void:
	self.text = "GDScript:\n" + SystemInfo.get_system_info()