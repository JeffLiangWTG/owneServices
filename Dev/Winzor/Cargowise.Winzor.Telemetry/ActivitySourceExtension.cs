using System.Diagnostics;

namespace CargoWise.Winzor.Telemetry;
public static class ActivitySourceExtension
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
	public static Action WithTracing(this ActivitySource? activitySource, Action action, string traceName)
	{
		if (activitySource is null)
		{
			return action;
		}

		var stopwatch = Stopwatch.StartNew();
		var activity = activitySource?.CreateActivity(traceName, ActivityKind.Internal, Activity.Current?.Context ?? default);
		var tracedAction = () =>
		{
			stopwatch.Stop();
			activity?.AddTag("thread", Thread.CurrentThread.Name);
			activity?.AddTag("delay", stopwatch.ElapsedMilliseconds);
			using (activity?.Start())
			{
				action();
			}
		};

		return tracedAction;
	}
}
