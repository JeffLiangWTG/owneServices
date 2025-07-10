using System.Runtime.CompilerServices;

namespace CargoWise.NetworkVisualisation.GUI.Extensions;

public static class DiagnosticsExtensions
{
	/// <summary>
	/// Checks the current thread; if WINZOR thread condition is false, notify a failure to be handled by the TraceListeners (DEBUG and RELEASE builds).
	/// </summary>
	/// <param name="methodName">Name of the method the condition is being checked</param>
	public static void AssertInWinzorThread(this IWinzorThreadInfo info, [CallerMemberName] string methodName = "")
		=> System.Diagnostics.Trace.Assert(info.IsWinzorThread, $"{methodName} should be running on Winzor Thread.");

	/// <summary>
	/// Checks the current thread; if RENDER thread condition is false, notify a failure to be handled by the TraceListeners (DEBUG and RELEASE builds).
	/// </summary>
	/// <param name="methodName">Name of the method the condition is being checked</param>
	public static void AssertInRenderThread(this IWinzorThreadInfo info, [CallerMemberName] string methodName = "")
		=> System.Diagnostics.Trace.Assert(info.IsRenderThread, $"{methodName} should be running on Render Thread.");
}
