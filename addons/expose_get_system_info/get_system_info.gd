extends Node

class_name SystemInfo

static func get_system_info():
	var distribution_name = OS.get_distribution_name()
	if distribution_name.is_empty():
		distribution_name = OS.get_name()
	if distribution_name.is_empty():
		distribution_name = "Other"
	var distribution_version = OS.get_version_alias()

	var godot_version = "Godot v" + Engine.get_version_info()["string"]
	if Engine.get_version_info()["build"] != "official":
		var hash = Engine.get_version_info().get("hash", "")
		hash = "unknown" if hash.Length == 0 else "(%s)" % hash.left(9)
		godot_version += " " + hash

	var display_session_type = ""
	# NOTE: Nearest analog. While `GODOT_LINUXBSD` does exist as a C# pre-processor define, this would only tell us if the engine itself was on BSD. As such, a run-time check has been substituted.
	if OS.get_name().ends_with("BSD"):
		# `replace` is necessary, because `capitalize` introduces a whitespace between "x" and "11".
		display_session_type = DisplayServer.get_name().capitalize().replace(" ", "")

	var driver_name = RenderingServer.get_current_rendering_driver_name().to_lower()
	var rendering_method = RenderingServer.get_current_rendering_method().to_lower()

	var rendering_device_name = RenderingServer.get_video_adapter_name()

	var device_type = RenderingServer.get_video_adapter_type()
	var device_type_string = ""

	match device_type:
		RenderingDevice.DeviceType.DEVICE_TYPE_INTEGRATED_GPU:
			device_type_string = "integrated"
		RenderingDevice.DeviceType.DEVICE_TYPE_DISCRETE_GPU:
			device_type_string = "dedicated"
		RenderingDevice.DeviceType.DEVICE_TYPE_VIRTUAL_GPU:
			device_type_string = "virtual"
		RenderingDevice.DeviceType.DEVICE_TYPE_CPU:
			device_type_string = "(software emulation on CPU)"
		RenderingDevice.DeviceType.DEVICE_TYPE_OTHER:
			pass
		RenderingDevice.DeviceType.DEVICE_TYPE_MAX:
			pass # Can't happen, but silences warning for DEVICE_TYPE_MAX

	var video_adapter_driver_info = OS.get_video_adapter_driver_info()

	var processor_name = OS.get_processor_name()
	var processor_counter = OS.get_processor_count()

	var audio_driver_name = AudioServer.get_driver_name()
	var mix_rate = AudioServer.get_mix_rate()

	var speaker_mode = AudioServer.get_speaker_mode()
	var speaker_mode_string = ""
	match speaker_mode:
		AudioServer.SpeakerMode.SPEAKER_MODE_STEREO:
			speaker_mode_string = "Stereo/mono"
		AudioServer.SpeakerMode.SPEAKER_SURROUND_31:
			speaker_mode_string = "Surround 3.1"
		AudioServer.SpeakerMode.SPEAKER_SURROUND_51:
			speaker_mode_string = "Surround 5.1"
		AudioServer.SpeakerMode.SPEAKER_SURROUND_71:
			speaker_mode_string = "Surround 7.1"

	# Prettify
	if rendering_method == "forward_plus":
		rendering_method = "Forward+"
	elif rendering_method == "mobile":
		rendering_method = "Mobile"
	elif rendering_method == "gl_compatibility":
		rendering_method = "Compatibility"
	if driver_name == "vulkan":
		driver_name = "Vulkan"
	elif driver_name == "d3d12":
		driver_name = "Direct3D 12"
	elif driver_name == "opengl3_angle":
		driver_name = "OpenGL ES 3/ANGLE"
	elif driver_name == "opengl3_es":
		driver_name = "OpenGL ES 3"
	elif driver_name == "opengl3":
		# NOTE: No direct analog (see open issue: https://github.com/godotengine/godot-proposals/issues/8380 )
		# if (OS::get_singleton()->get_gles_over_gl()) {
		# 	driverName = "OpenGL 3";
		# } else {
		# 	driverName = "OpenGL ES 3";
		# }
		driver_name = "OpenGL 3";
	elif driver_name == "metal":
		driver_name = "Metal"

	var info = []
	info.push_back(godot_version)
	var distribution_display_session_type = distribution_name
	if not distribution_version.is_empty():
		distribution_display_session_type += " " + distribution_version
	if not display_session_type.is_empty():
		distribution_display_session_type += " on " + display_session_type
	info.push_back(distribution_display_session_type)

	var display_driver_window_mode = ""
	# NOTE: Nearest analog. While `GODOT_LINUXBSD` does exist as a C# pre-processor define, this would only tell us if the engine itself was on BSD. As such, a run-time check has been substituted.
	if OS.get_name().ends_with("BSD"):
		display_driver_window_mode = DisplayServer.get_name().capitalize().replace(" ", "")
	if not display_driver_window_mode.is_empty():
		display_driver_window_mode += ", "
	display_driver_window_mode += "Single-window" if Engine.get_main_loop().root.get_viewport().gui_embed_subwindows else "Multi-window"

	if DisplayServer.get_screen_count() == 1:
		display_driver_window_mode += ", " + str(DisplayServer.get_screen_count()) + " monitor"
	else:
		display_driver_window_mode += ", " + str(DisplayServer.get_screen_count()) + " monitors"

	info.push_back(display_driver_window_mode)

	info.push_back("%s (%s)" % [driver_name, rendering_method])

	var graphics = ""
	if not device_type_string.is_empty():
		graphics = device_type_string + " "
	graphics += rendering_device_name
	if video_adapter_driver_info.size() == 2: # This vector is always either of length 0 or 2.
		var vad_name = video_adapter_driver_info[0]
		var vad_version = video_adapter_driver_info[1] # Version could be potentially empty on Linux/BSD.
		if not vad_version.is_empty():
			graphics += " (%s; %s)" % [vad_name, vad_version]
		elif not vad_name.is_empty():
			graphics += " (%s)" % vad_name

	info.push_back(graphics)

	info.push_back("%s (%d threads)" % [processor_name, processor_counter])

	var system_ram = OS.get_memory_info()["physical"]
	if system_ram > 0:
		# If the memory info is available, display it.
		info.push_back("%s memory" % String.humanize_size(system_ram))

	info.push_back("%s (%d Hz, %s)" % [audio_driver_name, int(mix_rate), speaker_mode_string])

	return " - ".join(info)