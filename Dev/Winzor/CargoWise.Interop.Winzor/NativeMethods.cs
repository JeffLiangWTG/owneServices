using System;
using System.Runtime.InteropServices;

namespace CargoWise.Interop;

public static class NativeMethods
{
	public const int GuiResourcesThreshold = 0;
	public static int GetWindowHandlesForCurrentProcess() => 0;

	public static class WindowStyles
	{
		public const int WS_DISABLED = 134217728;
	}
}
