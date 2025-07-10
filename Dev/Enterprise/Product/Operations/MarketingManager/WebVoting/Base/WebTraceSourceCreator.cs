using System;
using System.Collections;
using System.Diagnostics;
using CargoWiseOne.WebInfrastructure;
using GlobalTraceSource = System.Diagnostics.TraceSource;

namespace Enterprise.MarketingManager.WebVoting
{
	public static class WebTraceSourceCreator
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "This is Event Log source name.")]
		static string CargoWiseOneWebSource => WebInfrastructureConstants.EventSourceName;
		static readonly string ApplicationLogName = WebInfrastructureConstants.EventLogName;
		public static readonly string EnableTracingVariable = "EnableTracing";

		public static GlobalTraceSource Initialise(ICollection session)
		{
			if (!FindVariable(session, EnableTracingVariable))
			{
				return null;
			}

			var traceSource = new GlobalTraceSource(CargoWiseOneWebSource);
			var listener = new EventLogTraceListener();
			var log = new EventLog(ApplicationLogName);

			log.Source = CargoWiseOneWebSource;
			listener.EventLog = log;
			traceSource.Listeners.Clear();
			traceSource.Listeners.Add(listener);
			traceSource.Switch.Level = SourceLevels.All;
			return traceSource;
		}

		static bool FindVariable(IEnumerable session, string variableName)
		{
			var sessionList = session.GetEnumerator();
			sessionList.Reset();
			while (sessionList.MoveNext())
			{
				if (string.Equals((string)sessionList.Current, variableName, StringComparison.InvariantCultureIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}
	}
}
