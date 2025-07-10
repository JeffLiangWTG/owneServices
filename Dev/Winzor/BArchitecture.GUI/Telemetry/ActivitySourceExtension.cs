using System.Diagnostics;

namespace WinzorFramework.Telemetry;

public static class ActivitySourceExtension
{
	public static Action WithTracing(this ActivitySource? activitySource, Action action, string traceName, Activity? parent = null)
	{
		if (activitySource is null)
		{
			return action;
		}

		var stopwatch = Stopwatch.StartNew();
		var tracedAction = () =>
		{
			stopwatch.Stop();
			var activity = activitySource?.CreateActivity(traceName, ActivityKind.Internal, parent?.Context ?? default);
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
