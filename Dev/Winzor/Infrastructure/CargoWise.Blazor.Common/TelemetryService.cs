#nullable enable

using System.Collections.Concurrent;
using System.Diagnostics;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Blazor.Common;

static class TelemetryService
{
	public static readonly ActivitySource ActivitySource = new ActivitySource("CargoWise.Blazor.Common");

	[ThreadSafe]
	static readonly ConcurrentDictionary<string, Activity?> assemblyLoadActivities = new();

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Not UI")]
	public static Activity? StartAssemblyLoadActivity(string name, string path)
	{
		var activity = assemblyLoadActivities?.GetOrAdd(name, _ => ActivitySource.CreateActivity("LoadAssembly", ActivityKind.Internal, default(ActivityContext), [new ("name", name), new ("path", path)]));
		return activity?.Start();
	}

	public static void StopAssemblyLoadActivity(string name)
	{
		if (assemblyLoadActivities?.TryRemove(name, out var activity) == true)
		{
			activity?.Dispose();
		}
	}
}
