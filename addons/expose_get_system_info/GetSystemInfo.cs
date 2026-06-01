using System.Collections.Generic;

using Godot;

public static class SystemInfo {
	public static string GetSystemInfo() {
		string distributionName = OS.GetDistributionName();
		if (distributionName.Length == 0) {
			distributionName = OS.GetName();
		}
		if (distributionName.Length == 0) {
			distributionName = "Other";
		}
		string distributionVersion = OS.GetVersionAlias();

		string godotVersion = "Godot v" + Engine.GetVersionInfo()["string"];
		if (Engine.GetVersionInfo()["build"].AsString() != "official") {
			string hash = Engine.GetVersionInfo().GetValueOrDefault("hash", "").AsString();
			hash = hash.Length == 0 ? "unknown" : string.Format("({0:G})", hash.Left(9));
			godotVersion += " " + hash;
		}

		string displaySessionType = "";
		// NOTE: Nearest analog. While `GODOT_LINUXBSD` does exist as a C# pre-processor define, this would only tell us if the engine itself was on BSD. As such, a run-time check has been substituted.
		if (OS.GetName().EndsWith("BSD")) {
			// `Replace` is necessary, because `Capitalize` introduces a whitespace between "x" and "11".
			displaySessionType = DisplayServer.GetName().Capitalize().Replace(" ", "");
		}
		string driverName = RenderingServer.GetCurrentRenderingDriverName().ToLower();
		string renderingMethod = RenderingServer.GetCurrentRenderingMethod().ToLower();

		string renderingDeviceName = RenderingServer.GetVideoAdapterName();

		RenderingDevice.DeviceType deviceType = RenderingServer.GetVideoAdapterType();
		string deviceTypeString = "";
		switch (deviceType) {
			case RenderingDevice.DeviceType.IntegratedGpu:
				deviceTypeString = "integrated";
				break;
			case RenderingDevice.DeviceType.DiscreteGpu:
				deviceTypeString = "dedicated";
				break;
			case RenderingDevice.DeviceType.VirtualGpu:
				deviceTypeString = "virtual";
				break;
			case RenderingDevice.DeviceType.Cpu:
				deviceTypeString = "(software emulation on CPU)";
				break;
			case RenderingDevice.DeviceType.Other:
			case RenderingDevice.DeviceType.Max:
				break; // Can't happen, but silences warning for DEVICE_TYPE_MAX
		}

		string[] videoAdapterDriverInfo = OS.GetVideoAdapterDriverInfo();

		string processorName = OS.GetProcessorName();
		int processorCount = OS.GetProcessorCount();

		string audioDriverName = AudioServer.GetDriverName();
		float mixRate = AudioServer.GetMixRate();

		AudioServer.SpeakerMode speakerMode = AudioServer.GetSpeakerMode();
		string speakerModeString = "";
		switch (speakerMode) {
			case AudioServer.SpeakerMode.ModeStereo:
				speakerModeString = "Stereo/mono";
				break;
			case AudioServer.SpeakerMode.Surround31:
				speakerModeString = "Surround 3.1";
				break;
			case AudioServer.SpeakerMode.Surround51:
				speakerModeString = "Surround 5.1";
				break;
			case AudioServer.SpeakerMode.Surround71:
				speakerModeString = "Surround 7.1";
				break;
		}

		// Prettify
		if (renderingMethod == "forward_plus") {
			renderingMethod = "Forward+";
		} else if (renderingMethod == "mobile") {
			renderingMethod = "Mobile";
		} else if (renderingMethod == "gl_compatibility") {
			renderingMethod = "Compatibility";
		}
		if (driverName == "vulkan") {
			driverName = "Vulkan";
		} else if (driverName == "d3d12") {
			driverName = "Direct3D 12";
		} else if (driverName == "opengl3_angle") {
			driverName = "OpenGL ES 3/ANGLE";
		} else if (driverName == "opengl3_es") {
			driverName = "OpenGL ES 3";
		} else if (driverName == "opengl3") {
			// NOTE: No direct analog (see open issue: https://github.com/godotengine/godot-proposals/issues/8380 )
			// if (OS::get_singleton()->get_gles_over_gl()) {
			// 	driverName = "OpenGL 3";
			// } else {
			// 	driverName = "OpenGL ES 3";
			// }
			driverName = "OpenGL 3";
		} else if (driverName == "metal") {
			driverName = "Metal";
		}

		// Join info.
		List<string> info = new List<string>();
		info.Add(godotVersion);
		string distributionDisplaySessionType = distributionName;
		if (distributionVersion.Length != 0) {
			distributionDisplaySessionType += " " + distributionVersion;
		}
		if (displaySessionType.Length != 0) {
			distributionDisplaySessionType += " on " + displaySessionType;
		}
		info.Add(distributionDisplaySessionType);

		string displayDriverWindowMode = "";
		// NOTE: Nearest analog. While `GODOT_LINUXBSD` does exist as a C# pre-processor define, this would only tell us if the engine itself was on BSD. As such, a run-time check has been substituted.
		if (OS.GetName().EndsWith("BSD")) {
			// `Replace` is necessary, because `Capitalize` introduces a whitespace between "x" and "11".
			displayDriverWindowMode = DisplayServer.GetName().Capitalize().Replace(" ", "") + " display driver";
		}
		if (displayDriverWindowMode.Length != 0) {
			displayDriverWindowMode += ", ";
		}
		displayDriverWindowMode += ((SceneTree) Godot.Engine.GetMainLoop()).Root.GetViewport().GuiEmbedSubwindows ? "Single-window" : "Multi-window";

		if (DisplayServer.GetScreenCount() == 1) {
			displayDriverWindowMode += ", " + DisplayServer.GetScreenCount() + " monitor";
		} else {
			displayDriverWindowMode += ", " + DisplayServer.GetScreenCount() + " monitors";
		}

		info.Add(displayDriverWindowMode);

		info.Add(string.Format("{0:G} ({1:G})", driverName, renderingMethod));

		string graphics = "";
		if (deviceTypeString.Length != 0) {
			graphics = deviceTypeString + " ";
		}
		graphics += renderingDeviceName;
		if (videoAdapterDriverInfo.Length == 2) { // This vector is always either of length 0 or 2.
			string vadName = videoAdapterDriverInfo[0];
			string vadVersion = videoAdapterDriverInfo[1]; // Version could be potentially empty on Linux/BSD.
			if (vadVersion.Length != 0) {
				graphics += string.Format(" ({0:G}; {1:G})", vadName, vadVersion);
			} else if (vadName.Length != 0) {
				graphics += string.Format(" ({0:G})", vadName);
			}
		}
		info.Add(graphics);

		info.Add(string.Format("{0:G} ({1:D} threads)", processorName, processorCount));

		long systemRam = OS.GetMemoryInfo()["physical"].AsInt64();
		if (systemRam > 0) {
			// If the memory info is available, display it.
			// NOTE: string.humanize_size was one of the methods excluded in the C# StringExtensions implementation, as noted here: https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_differences.html#string . As such, it has been re-implemented here.
			string HumanizeSize(long pSize) {
				int magnitude = 0;
				long _div = 1;
				while (pSize > _div * 1024 && magnitude < 6) {
					_div *= 1024;
					magnitude++;
				}

				if (magnitude == 0) {
					return pSize.ToString() + " " + TranslationServer.Translate("B");
				} else {
					string suffix = "";
					switch (magnitude) {
						case 1:
							suffix = TranslationServer.Translate("KiB");
							break;
						case 2:
							suffix = TranslationServer.Translate("MiB");
							break;
						case 3:
							suffix = TranslationServer.Translate("GiB");
							break;
						case 4:
							suffix = TranslationServer.Translate("TiB");
							break;
						case 5:
							suffix = TranslationServer.Translate("PiB");
							break;
						case 6:
							suffix = TranslationServer.Translate("EiB");
							break;
					}

					double divisor = _div;
					// int digits = _humanize_digits(p_size / _div);
					long pNum = pSize / _div;
					int digits;
					if (pNum < 100) {
						digits = 2;
					} else if (pNum < 1024) {
						digits = 1;
					} else {
						digits = 0;
					}
					return (pSize / divisor).ToString().PadDecimals(digits) + " " + suffix;
				}
			}

			info.Add(string.Format("{0:G} memory", HumanizeSize(systemRam)));
		}

		info.Add(string.Format("{0:G} ({1:D} Hz, {2:G})", audioDriverName, (int) mixRate, speakerModeString));

		return string.Join(" - ", info);
	}
}